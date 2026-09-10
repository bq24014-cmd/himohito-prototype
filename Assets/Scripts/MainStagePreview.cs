using System.Collections;
using UnityEngine;

namespace HimoHito
{
    /// <summary>Opening tour shared by Tutorial and MainStage. Gameplay resumes at the start.</summary>
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Camera))]
    public sealed class MainStagePreview : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Transform previewTarget;
        [SerializeField, Min(0f)] private float holdDuration = 0.7f;
        [SerializeField, Min(1f)] private float panSpeed = 12f;

        private static MainStagePreview activePreview;
        private Camera previewCamera;
        private HorizontalCameraFollow cameraFollow;
        private Rigidbody2D playerBody;
        private PlayerMover playerMover;
        private RopeController ropeController;
        private Vector3 startView;
        private Vector3 goalView;
        private float gameplayOrthographicSize;
        private float elapsed;
        private float travelDuration;
        private float rampDuration;
        private int beganFrame;
        private bool hasStarted;
        private bool finishPending;
        private bool bodyWasSimulated;
        private bool moverWasEnabled;
        private bool ropeWasEnabled;
        private bool followWasEnabled;
        private GUIStyle hintStyle;

        public bool IsPreviewing { get; private set; }
        public static bool IsActive => activePreview != null && activePreview.IsPreviewing;

        public void Configure(Transform playerTransform, Transform targetTransform)
        {
            if (IsPreviewing) return;
            player = playerTransform;
            previewTarget = targetTransform;
        }

        public static bool PlayFor(GameObject playerObject)
        {
            Camera camera = Camera.main;
            if (camera == null || playerObject == null || IsActive) return false;
            MainStagePreview preview = camera.GetComponent<MainStagePreview>();
            if (preview == null) preview = camera.gameObject.AddComponent<MainStagePreview>();
            preview.Configure(playerObject.transform, null);
            preview.enabled = true;
            return preview.BeginPreview();
        }

        private void Awake()
        {
            previewCamera = GetComponent<Camera>();
            cameraFollow = GetComponent<HorizontalCameraFollow>();
        }

        private IEnumerator Start()
        {
            // Let checkpoints, camera follow and background parallax initialize at the player.
            yield return null;
            if (hasStarted) yield break;
            if (player == null)
            {
                MainStageRespawnOnFall respawn = FindFirstObjectByType<MainStageRespawnOnFall>();
                player = respawn != null ? respawn.transform : null;
            }
            // Tutorial starts explicitly after the title's "Start" input.
            if (player != null && player.GetComponent<MainStageRespawnOnFall>() != null)
                BeginPreview();
        }

        private bool BeginPreview()
        {
            if (IsActive || player == null) return false;
            previewCamera = GetComponent<Camera>();
            cameraFollow = GetComponent<HorizontalCameraFollow>();
            playerBody = player.GetComponent<Rigidbody2D>();
            playerMover = player.GetComponent<PlayerMover>();
            ropeController = player.GetComponent<RopeController>();
            // Disabling RopeController detaches a live rope. Never tour during a swing.
            if (playerBody == null || (ropeController != null && ropeController.IsAttached))
                return false;

            string markerName = player.GetComponent<PrototypeRunController>() != null
                ? TutorialSectionFourSetup.GoalMarkerName
                : MainStageSectionTenSetup.GoalMarkerName;
            GameObject goal = GameObject.Find(markerName);
            Transform target = goal != null ? goal.transform : previewTarget;
            if (target == null) return false;

            cameraFollow?.ResetFraming();
            startView = new Vector3(playerBody.position.x, transform.position.y, transform.position.z);
            goalView = new Vector3(target.position.x, startView.y, startView.z);
            float distance = Vector3.Distance(goalView, startView);
            if (distance < 0.1f) return false;
            gameplayOrthographicSize = previewCamera.orthographicSize;
            float cruiseDuration = distance / Mathf.Max(1f, panSpeed);
            rampDuration = Mathf.Min(1f, cruiseDuration);
            travelDuration = cruiseDuration + rampDuration;

            StageStartTransition.ReleasePlayerForPreview(player.gameObject);
            bodyWasSimulated = playerBody.simulated;
            moverWasEnabled = playerMover != null && playerMover.enabled;
            ropeWasEnabled = ropeController != null && ropeController.enabled;
            followWasEnabled = cameraFollow != null && cameraFollow.enabled;
            if (cameraFollow != null) cameraFollow.enabled = false;
            playerBody.simulated = false;
            if (playerMover != null) playerMover.enabled = false;
            if (ropeController != null) ropeController.enabled = false;

            elapsed = 0f;
            beganFrame = Time.frameCount;
            finishPending = false;
            hasStarted = true;
            IsPreviewing = true;
            activePreview = this;
            transform.position = goalView;
            return true;
        }

        private void Update()
        {
            if (!IsPreviewing) return;
            if (StageStartTransition.IsActive)
            {
                // Tour is already parked at the goal; don't spend its hold/travel time under cloth.
                beganFrame = Time.frameCount;
                return;
            }
            if (playerBody == null || previewCamera == null)
            {
                RestoreState();
                return;
            }
            // The Enter that starts Tutorial must not also skip its tour.
            bool skip = Time.frameCount > beganFrame + 1
                && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)
                    || Input.GetKeyDown(KeyCode.KeypadEnter));
            Advance(Time.unscaledDeltaTime, skip);
        }

        private void Advance(float deltaTime, bool skip)
        {
            if (!IsPreviewing || finishPending) return;
            elapsed += Mathf.Max(0f, deltaTime);
            float travelTime = Mathf.Max(0f, elapsed - holdDuration);
            float progress = PanProgress(travelTime, travelDuration, rampDuration);
            transform.position = Vector3.Lerp(goalView, startView, progress);
            // No zoom pumping or vertical swaying during the tour.
            previewCamera.orthographicSize = gameplayOrthographicSize;
            if (skip || travelTime >= travelDuration) finishPending = true;
        }

        private void LateUpdate()
        {
            // Restore after every input Update, so skip-Space cannot also trigger a jump.
            if (finishPending) RestoreState();
        }

        private static float PanProgress(float time, float duration, float ramp)
        {
            if (time <= 0f) return 0f;
            if (time >= duration) return 1f;
            float area = duration - ramp;
            if (time < ramp)
                return (0.5f * time - ramp / (2f * Mathf.PI)
                    * Mathf.Sin(Mathf.PI * time / ramp)) / area;
            if (time > duration - ramp)
            {
                float remaining = duration - time;
                return 1f - (0.5f * remaining - ramp / (2f * Mathf.PI)
                    * Mathf.Sin(Mathf.PI * remaining / ramp)) / area;
            }
            return (time - 0.5f * ramp) / area;
        }

        private void RestoreState()
        {
            if (!IsPreviewing) return;
            transform.position = startView;
            if (previewCamera != null) previewCamera.orthographicSize = gameplayOrthographicSize;
            if (playerBody != null) playerBody.simulated = bodyWasSimulated;
            if (playerMover != null) playerMover.enabled = moverWasEnabled;
            if (ropeController != null) ropeController.enabled = ropeWasEnabled;
            if (cameraFollow != null)
            {
                cameraFollow.enabled = followWasEnabled;
                if (followWasEnabled) cameraFollow.ResetFraming();
            }
            IsPreviewing = false;
            finishPending = false;
            if (activePreview == this) activePreview = null;
        }

        private void OnDisable() => RestoreState();
        private void OnDestroy() => RestoreState();

        private void OnGUI()
        {
            if (!IsPreviewing || StageStartTransition.IsActive) return;
            if (hintStyle == null)
            {
                hintStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 17,
                    wordWrap = true
                };
                hintStyle.normal.textColor = new Color(1f, 0.94f, 0.82f);
                HimoHitoGuiTheme.ApplyToStyles(hintStyle);
            }
            Rect safe = Screen.safeArea;
            float width = Mathf.Min(560f, Mathf.Max(1f, safe.width - 32f));
            Rect rect = new Rect(safe.x + (safe.width - width) * 0.5f,
                Mathf.Max(0f, Screen.height - safe.yMin - 80f), width, 54f);
            GUI.Box(rect, GUIContent.none);
            GUI.Label(rect, "ゴールからスタートへ\nSpace / Enter　スキップ", hintStyle);
        }
    }
}
