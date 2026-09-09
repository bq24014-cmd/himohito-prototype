using System.Collections.Generic;
using UnityEngine;

namespace HimoHito
{
    /// <summary>Presentation only, notified AFTER a checkpoint accepts a landing.</summary>
    [DisallowMultipleComponent]
    public sealed class SectionArrivalFeedback : MonoBehaviour
    {
        private const float NoticeDuration = 2.1f;
        private readonly Dictionary<int, CheckpointYarnFlag> flags = new();
        private PrototypeRunController tutorial;
        private MainStageRespawnOnFall mainStage;
        private MainStageGoalZone goal;
        private StageOverlayControls overlay;
        private TutorialSectionGuide guide;
        private Rigidbody2D body;
        private float noticeElapsed = NoticeDuration;
        private string noticeText;
        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;

        private void Awake()
        {
            tutorial = GetComponent<PrototypeRunController>();
            mainStage = GetComponent<MainStageRespawnOnFall>();
            body = GetComponent<Rigidbody2D>();
            overlay = FindFirstObjectByType<StageOverlayControls>();
            guide = GetComponent<TutorialSectionGuide>();
            goal = FindFirstObjectByType<MainStageGoalZone>();
        }

        public static void Notify(GameObject player, int section, Vector2 respawn, Collider2D floor)
        {
            if (player == null || floor == null || section < 2 || section > 10) return;
            if (!player.TryGetComponent(out SectionArrivalFeedback feedback))
                feedback = player.AddComponent<SectionArrivalFeedback>();
            feedback.ShowArrival(section, respawn, floor);
        }

        private void ShowArrival(int section, Vector2 respawn, Collider2D floor)
        {
            if (flags.ContainsKey(section)) return;
            Bounds bounds = floor.bounds;
            float size = Mathf.Min(1f, bounds.size.x / 1.5f);
            float x = Mathf.Clamp(respawn.x + 2f,
                bounds.min.x + .25f * size, bounds.max.x - 1.1f * size);
            var flagObject = new GameObject($"Reached Section {section} Yarn Flag")
            {
                hideFlags = HideFlags.DontSave
            };
            // Keep it on the actual accepted floor, not a raycast-selected hook.
            flagObject.transform.position = new Vector3(x, bounds.max.y - .04f, floor.transform.position.z);
            flagObject.transform.localScale = Vector3.one * size;
            var flag = flagObject.AddComponent<CheckpointYarnFlag>();
            flag.Initialize(section);
            flags.Add(section, flag);
            noticeText = $"第{section}区間";
            noticeElapsed = 0f;
        }

        private bool IsPlaying =>
            (tutorial == null || tutorial.Outcome == PrototypeRunController.RunOutcome.Playing) &&
            (mainStage == null || !mainStage.IsFailureVisible) &&
            (goal == null || !goal.IsCompleting) && (body == null || body.simulated);

        private bool IsCovered => Time.timeScale <= 0f ||
            (overlay != null && overlay.IsOverlayVisible) || (guide != null && guide.IsVisible);

        private void Update()
        {
            if (!IsPlaying)
            {
                noticeElapsed = NoticeDuration;
                return;
            }
            if (IsCovered) return;
            Advance(Time.deltaTime);
        }

        private void Advance(float deltaTime)
        {
            if (deltaTime <= 0f) return;
            noticeElapsed = Mathf.Min(NoticeDuration, noticeElapsed + deltaTime);
            foreach (var flag in flags.Values)
                if (flag != null) flag.Advance(deltaTime);
        }

        private void OnGUI()
        {
            if (!IsPlaying || IsCovered || noticeElapsed >= NoticeDuration) return;
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 28, fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter, wordWrap = false
                };
                subtitleStyle = new GUIStyle(titleStyle) { fontSize = 13, fontStyle = FontStyle.Normal };
                HimoHitoGuiTheme.ApplyToStyles(titleStyle, subtitleStyle);
                titleStyle.normal.textColor = subtitleStyle.normal.textColor = new Color(.12f, .08f, .19f);
            }
            float alpha = Mathf.SmoothStep(0f, 1f, noticeElapsed / .22f) *
                (1f - Mathf.SmoothStep(0f, 1f, (noticeElapsed - 1.55f) / .55f));
            Rect safe = Screen.safeArea;
            float width = Mathf.Min(286f, Mathf.Max(0f, safe.width - 32f));
            if (width < 100f || safe.height < 90f) return;
            // Below the compact HUD on narrow windows; never outside the safe area.
            float y = Screen.height - safe.yMax + (safe.width < 1020f ? 198f : 22f);
            y = Mathf.Min(y, Screen.height - safe.yMin - 86f);
            var panel = new Rect(safe.xMin + (safe.width - width) * .5f, y, width, 70f);
            Color previousColor = GUI.color;
            int previousDepth = GUI.depth;
            GUI.depth = -5;
            GUI.color = new Color(previousColor.r, previousColor.g, previousColor.b, previousColor.a * alpha);
            if (HimoHitoUiParts.IsAvailable) HimoHitoUiParts.DrawWoodButton(panel);
            else GUI.Box(panel, GUIContent.none);
            GUI.Label(new Rect(panel.x, panel.y + 5f, panel.width, 38f), noticeText, titleStyle);
            GUI.Label(new Rect(panel.x, panel.y + 40f, panel.width, 22f), "復帰地点を更新", subtitleStyle);
            GUI.color = previousColor;
            GUI.depth = previousDepth;
        }

        // R retries keep the reached trail; only a new tutorial run clears it.
        public void ClearTrail()
        {
            foreach (var flag in flags.Values)
                if (flag != null)
                {
                    flag.gameObject.SetActive(false);
                    flag.Release();
                    if (Application.isPlaying) Destroy(flag.gameObject);
                    else DestroyImmediate(flag.gameObject);
                }
            flags.Clear();
            noticeElapsed = NoticeDuration;
        }

        private void OnDestroy() => ClearTrail();
    }
}
