using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Builds section three from slide 22 of the 0829 stage manual.
    /// The upper Hook reaches the high shelf with length 8. The lower Hook
    /// reaches only a low dead end with length 6, where stairs allow retreat.
    /// </summary>
    public static class MainStageSectionThreeSetup
    {
        public const string LowerHookName = "Main Hook 3";
        public const string UpperHookName = "Main Hook 3 Upper";
        public const string LowDeadEndName =
            "Main Section 3 Low Dead End";
        public const string ReturnStepAName =
            "Main Section 3 Return Step A";
        public const string ReturnStepBName =
            "Main Section 3 Return Step B";
        public const string ReturnStepCName =
            "Main Section 3 Return Step C";
        public const string ReturnStepDName =
            "Main Section 3 Return Step D";
        public const string RecoverySwitchName =
            "Section 3 Recovery Switch";
        public const string HighShelfName = "Main Landing 3";

        // Slide 22 local coordinates are translated so the start bank's
        // right edge (6.6, 3.1) matches the current world edge (33, -4.35).
        public static readonly Vector2 LowerHookPosition =
            new Vector2(38.4f, -2.25f);
        public static readonly Vector2 UpperHookPosition =
            new Vector2(38.4f, 0.15f);
        public static readonly Vector2 HookSize =
            new Vector2(1.6f, 0.45f);
        public static readonly Vector2 LowDeadEndPosition =
            new Vector2(43.7f, -11.05f);
        public static readonly Vector2 LowDeadEndSize =
            new Vector2(3f, 10f);
        public static readonly Vector2 ReturnStepAPosition =
            new Vector2(41.3f, -11.05f);
        public static readonly Vector2 ReturnStepASize =
            new Vector2(1.8f, 10f);
        public static readonly Vector2 ReturnStepBPosition =
            new Vector2(39.5f, -10.775f);
        public static readonly Vector2 ReturnStepBSize =
            new Vector2(1.8f, 10.55f);
        public static readonly Vector2 ReturnStepCPosition =
            new Vector2(37.7f, -10.5f);
        public static readonly Vector2 ReturnStepCSize =
            new Vector2(1.8f, 11.1f);
        public static readonly Vector2 ReturnStepDPosition =
            new Vector2(35.9f, -10.225f);
        public static readonly Vector2 ReturnStepDSize =
            new Vector2(1.8f, 11.65f);
        public static readonly Vector2 RecoverySwitchPosition =
            new Vector2(44.1f, -5.75f);
        public static readonly Vector2 RecoverySwitchSize =
            new Vector2(0.9f, 0.35f);
        public static readonly Vector2 HighShelfPosition =
            new Vector2(49.6f, -7.15f);
        public static readonly Vector2 HighShelfSize =
            new Vector2(8f, 10f);
        public static readonly Vector2 HighShelfRespawnPosition =
            new Vector2(46.6f, -1.45f);

        private static readonly Color HookColor =
            new Color(0.298f, 0.765f, 1f);

        public static bool ApplyCurrentScene()
        {
            bool changed = false;
            changed |= EnsureHook(LowerHookName, LowerHookPosition);
            changed |= EnsureHook(UpperHookName, UpperHookPosition);
            changed |= EnsureTerrain(
                LowDeadEndName,
                LowDeadEndPosition,
                LowDeadEndSize);
            changed |= EnsureTerrain(
                ReturnStepAName,
                ReturnStepAPosition,
                ReturnStepASize);
            changed |= EnsureTerrain(
                ReturnStepBName,
                ReturnStepBPosition,
                ReturnStepBSize);
            changed |= EnsureTerrain(
                ReturnStepCName,
                ReturnStepCPosition,
                ReturnStepCSize);
            changed |= EnsureTerrain(
                ReturnStepDName,
                ReturnStepDPosition,
                ReturnStepDSize);
            changed |= EnsureTerrain(
                HighShelfName,
                HighShelfPosition,
                HighShelfSize);

            GameObject lowDeadEnd = FindSceneObject(LowDeadEndName);
            if (lowDeadEnd != null)
            {
                if (!lowDeadEnd.TryGetComponent(
                        out MainStageSectionThreeRecoveryStairs recovery))
                {
                    recovery = lowDeadEnd.AddComponent<
                        MainStageSectionThreeRecoveryStairs>();
                    changed = true;
                }
                recovery.Configure();
            }

            changed |= EnsureRecoverySwitch(lowDeadEnd);

            GameObject highShelf = FindSceneObject(HighShelfName);
            if (highShelf != null)
            {
                if (!highShelf.TryGetComponent(
                        out MainStageCheckpoint checkpoint))
                {
                    checkpoint = highShelf.AddComponent<MainStageCheckpoint>();
                    changed = true;
                }
                checkpoint.Configure(
                    4,
                    HighShelfRespawnPosition,
                    45f);
            }
            return changed;
        }

        private static bool EnsureRecoverySwitch(GameObject lowDeadEnd)
        {
            bool changed = false;
            GameObject recoverySwitch = FindSceneObject(RecoverySwitchName);
            if (recoverySwitch == null)
            {
                recoverySwitch = new GameObject(RecoverySwitchName);
                changed = true;
            }
            if (!recoverySwitch.activeSelf)
            {
                recoverySwitch.SetActive(true);
                changed = true;
            }
            changed |= SetTransform(
                recoverySwitch,
                RecoverySwitchPosition,
                RecoverySwitchSize);

            if (!recoverySwitch.TryGetComponent(out SpriteRenderer _))
            {
                recoverySwitch.AddComponent<SpriteRenderer>();
                changed = true;
            }
            if (!recoverySwitch.TryGetComponent(out SolidSprite visual))
            {
                visual = recoverySwitch.AddComponent<SolidSprite>();
                changed = true;
            }
            if (!recoverySwitch.TryGetComponent(out BoxCollider2D trigger))
            {
                trigger = recoverySwitch.AddComponent<BoxCollider2D>();
                changed = true;
            }
            if (!trigger.isTrigger)
            {
                trigger.isTrigger = true;
                changed = true;
            }
            Vector2 triggerSize = new Vector2(3f, 5f);
            if (trigger.size != triggerSize)
            {
                trigger.size = triggerSize;
                changed = true;
            }
            if (!recoverySwitch.TryGetComponent(
                    out MainStageSectionThreeRecoverySwitch controller))
            {
                controller = recoverySwitch.AddComponent<
                    MainStageSectionThreeRecoverySwitch>();
                changed = true;
            }
            controller.Configure(lowDeadEnd, visual);
            return changed;
        }

        // Compatibility entry point used by earlier editor utilities.
        public static GameObject EnsureCreated()
        {
            ApplyCurrentScene();
            return FindSceneObject(UpperHookName);
        }

        private static bool EnsureHook(
            string objectName,
            Vector2 position)
        {
            bool changed = false;
            GameObject hook = FindSceneObject(objectName);
            if (hook == null)
            {
                hook = new GameObject(objectName);
                changed = true;
            }
            if (!hook.activeSelf)
            {
                hook.SetActive(true);
                changed = true;
            }
            changed |= SetTransform(hook, position, HookSize);

            if (!hook.TryGetComponent(out SpriteRenderer _))
            {
                hook.AddComponent<SpriteRenderer>();
                changed = true;
            }
            if (!hook.TryGetComponent(out SolidSprite visual))
            {
                visual = hook.AddComponent<SolidSprite>();
                changed = true;
            }
            if (visual.Color != HookColor)
            {
                visual.Color = HookColor;
                changed = true;
            }
            if (!hook.TryGetComponent(out BoxCollider2D collider))
            {
                collider = hook.AddComponent<BoxCollider2D>();
                changed = true;
            }
            if (collider.size != Vector2.one)
            {
                collider.size = Vector2.one;
                changed = true;
            }
            if (collider.isTrigger)
            {
                collider.isTrigger = false;
                changed = true;
            }
            if (!hook.TryGetComponent(out HookPoint hookPoint))
            {
                hookPoint = hook.AddComponent<HookPoint>();
                changed = true;
            }
            changed |= hookPoint.ConfigureFixedAttachmentPoint(Vector2.zero);
            return changed;
        }

        private static bool EnsureTerrain(
            string objectName,
            Vector2 position,
            Vector2 size)
        {
            bool changed = false;
            GameObject terrain = FindSceneObject(objectName);
            if (terrain == null)
            {
                terrain = new GameObject(objectName);
                changed = true;
            }
            if (!terrain.activeSelf)
            {
                terrain.SetActive(true);
                changed = true;
            }
            changed |= SetTransform(terrain, position, size);

            if (!terrain.TryGetComponent(out SpriteRenderer _))
            {
                terrain.AddComponent<SpriteRenderer>();
                changed = true;
            }
            if (!terrain.TryGetComponent(out SolidSprite _))
            {
                terrain.AddComponent<SolidSprite>();
                changed = true;
            }
            if (!terrain.TryGetComponent(out BoxCollider2D collider))
            {
                collider = terrain.AddComponent<BoxCollider2D>();
                changed = true;
            }
            if (collider.size != Vector2.one)
            {
                collider.size = Vector2.one;
                changed = true;
            }
            if (collider.isTrigger)
            {
                collider.isTrigger = false;
                changed = true;
            }
            if (!terrain.TryGetComponent(out SolidSwingSurface _))
            {
                terrain.AddComponent<SolidSwingSurface>();
                changed = true;
            }
            return changed;
        }

        private static bool SetTransform(
            GameObject target,
            Vector2 position,
            Vector2 size)
        {
            bool changed = false;
            Vector3 targetPosition = new Vector3(position.x, position.y, 0f);
            Vector3 targetScale = new Vector3(size.x, size.y, 1f);
            if (target.transform.position != targetPosition)
            {
                target.transform.position = targetPosition;
                changed = true;
            }
            if (target.transform.localScale != targetScale)
            {
                target.transform.localScale = targetScale;
                changed = true;
            }
            return changed;
        }

        private static GameObject FindSceneObject(string objectName)
        {
            foreach (GameObject candidate in
                     Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (candidate.scene.IsValid() &&
                    candidate.scene.name == "MainStage" &&
                    candidate.name == objectName)
                {
                    return candidate;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Keeps the lower-route recovery stairs out of both valid swing paths.
    /// They become solid and visible only after the player presses the switch
    /// on the low dead end, where they are needed to prevent a soft lock.
    /// </summary>
    public sealed class MainStageSectionThreeRecoveryStairs : MonoBehaviour
    {
        private bool revealed;

        public bool IsRevealed => revealed;

        public void Configure()
        {
            if (Application.isPlaying && !revealed)
            {
                SetStairsActive(false);
            }
        }

        private void Awake()
        {
            Configure();
        }

        public void Reveal()
        {
            if (revealed)
            {
                return;
            }

            revealed = true;
            SetStairsActive(true);
        }

        public void Hide()
        {
            if (!revealed)
            {
                SetStairsActive(false);
                return;
            }

            revealed = false;
            SetStairsActive(false);
        }

        private static void SetStairsActive(bool active)
        {
            SetActive(MainStageSectionThreeSetup.ReturnStepAName, active);
            SetActive(MainStageSectionThreeSetup.ReturnStepBName, active);
            SetActive(MainStageSectionThreeSetup.ReturnStepCName, active);
            SetActive(MainStageSectionThreeSetup.ReturnStepDName, active);
        }

        private static void SetActive(string objectName, bool active)
        {
            foreach (GameObject candidate in
                     Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (candidate.scene.IsValid() &&
                    candidate.scene.name == "MainStage" &&
                    candidate.name == objectName)
                {
                    candidate.SetActive(active);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Reveals the lower-route recovery stairs only when the player presses F
    /// beside the toy switch. Returning to the section-three start bank hides
    /// the stairs again so the next swing path stays clear.
    /// </summary>
    public sealed class MainStageSectionThreeRecoverySwitch : MonoBehaviour
    {
        private static readonly Color OffColor =
            new Color(1f, 0.72f, 0.18f);
        private static readonly Color OnColor =
            new Color(0.36f, 0.94f, 0.72f);

        private MainStageSectionThreeRecoveryStairs stairs;
        private SolidSprite visual;
        private PlayerMover nearbyPlayer;
        private PlayerMover trackedPlayer;

        public void Configure(GameObject lowDeadEnd, SolidSprite switchVisual)
        {
            visual = switchVisual;
            stairs = lowDeadEnd != null
                ? lowDeadEnd.GetComponent<
                    MainStageSectionThreeRecoveryStairs>()
                : null;
            SetVisualState(stairs != null && stairs.IsRevealed);
        }

        private void Update()
        {
            if (trackedPlayer == null)
            {
                trackedPlayer = Object.FindFirstObjectByType<PlayerMover>();
            }

            if (nearbyPlayer != null &&
                stairs != null &&
                !stairs.IsRevealed &&
                Input.GetKeyDown(KeyCode.F))
            {
                stairs.Reveal();
                SetVisualState(true);

                RopeResource resource =
                    nearbyPlayer.GetComponentInParent<RopeResource>();
                if (resource != null)
                {
                    MainStageVisuals.Apply(resource.gameObject);
                }
            }

            if (trackedPlayer != null &&
                stairs != null &&
                stairs.IsRevealed &&
                trackedPlayer.transform.position.x <= 33.1f &&
                trackedPlayer.transform.position.y >= -5.2f)
            {
                stairs.Hide();
                SetVisualState(false);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerMover player = other.GetComponentInParent<PlayerMover>();
            if (player != null)
            {
                nearbyPlayer = player;
                trackedPlayer = player;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            PlayerMover player = other.GetComponentInParent<PlayerMover>();
            if (player != null && player == nearbyPlayer)
            {
                nearbyPlayer = null;
            }
        }

        private void OnGUI()
        {
            if (nearbyPlayer == null ||
                stairs == null ||
                stairs.IsRevealed)
            {
                return;
            }

            GUIStyle style = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold
            };
            GUI.Box(
                new Rect(
                    (Screen.width - 360f) * 0.5f,
                    Screen.height - 92f,
                    360f,
                    48f),
                "F：帰り道のスイッチを押す",
                style);
        }

        private void SetVisualState(bool isOn)
        {
            if (visual != null)
            {
                visual.Color = isOn ? OnColor : OffColor;
            }
        }
    }
}
