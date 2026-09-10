using UnityEngine;
using UnityEngine.SceneManagement;

namespace HimoHito
{
    /// <summary>Owns only the menu-to-stage curtain. The existing tour owns its player freeze.</summary>
    [DefaultExecutionOrder(9000)]
    public sealed class StageStartTransition : MonoBehaviour
    {
        internal const float ThreadDuration = 0.22f;
        internal const float CloseDuration = 0.58f;
        internal const float OpenDuration = 0.62f;
        private enum Phase { Thread, Closing, Covered, Loading, Opening }
        private static StageStartTransition active;
        public static bool IsActive => active != null && active.isActiveAndEnabled;

        private readonly StageStartCurtainView view = new StageStartCurtainView();
        private PrototypeRunController source;
        private string targetPath, stageTitle;
        private Vector2 origin;
        private Phase phase;
        private float elapsed;
        private int coveredFrame, loadedFrame;
        private bool finished, destinationSeen;
        private AsyncOperation load;
        private GameObject destinationPlayer;
        private Rigidbody2D heldBody;
        private PlayerMover heldMover;
        private RopeController heldRope;
        private bool bodyWasSimulated, moverWasEnabled, ropeWasEnabled;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => active = null;

        internal static bool Begin(PrototypeRunController owner, StageCatalog.Entry stage, Vector2 normalizedOrigin)
        {
            if (!Application.isPlaying || IsActive || owner == null || stage == null || !stage.available)
                return false;
            GameObject root = new GameObject("Stage start cloth curtain");
            DontDestroyOnLoad(root);
            StageStartTransition transition = root.AddComponent<StageStartTransition>();
            transition.source = owner;
            transition.targetPath = stage.scenePath;
            transition.stageTitle = stage.title;
            transition.origin = normalizedOrigin;
            active = transition;
            SceneManager.sceneLoaded += transition.OnSceneLoaded;
            return true;
        }

        private void Update()
        {
            if (finished) return;
            // A scene replaced externally must never retain the old menu's curtain.
            if (source == null && phase != Phase.Loading && phase != Phase.Opening)
            {
                Finish();
                return;
            }
            elapsed += Mathf.Max(0, Time.unscaledDeltaTime);
            switch (phase)
            {
                case Phase.Thread:
                    if (elapsed >= ThreadDuration) Enter(Phase.Closing);
                    break;
                case Phase.Closing:
                    if (elapsed >= CloseDuration)
                    {
                        Enter(Phase.Covered);
                        coveredFrame = Time.frameCount;
                    }
                    break;
                case Phase.Covered:
                    // Keep an entire opaque rendered frame before any scene activation.
                    if (Time.frameCount > coveredFrame + 1) CommitStart();
                    break;
                case Phase.Loading:
                    if (load != null && !load.isDone) break;
                    if (Time.frameCount <= loadedFrame + 2) break;
                    if (!destinationSeen)
                    {
                        Fail("ステージを開けませんでした。もう一度お試しください。");
                        break;
                    }
                    if (!MainStagePreview.IsActive && destinationPlayer != null)
                        MainStagePreview.PlayFor(destinationPlayer);
                    // Missing tour markers must not leave controls or an opaque curtain stuck.
                    if (!MainStagePreview.IsActive)
                        Debug.LogWarning("ステージ紹介の対象がないため、布を開いて通常プレイへ戻します。");
                    Enter(Phase.Opening);
                    break;
                case Phase.Opening:
                    if (elapsed >= OpenDuration) Finish();
                    break;
            }
        }

        private void CommitStart()
        {
            if (source == null) { Finish(); return; }
            if (targetPath == StageCatalog.TitleScenePath)
            {
                destinationPlayer = source.gameObject;
                source.CommitTutorialStart();
                if (!MainStagePreview.IsActive) HoldPlayer(destinationPlayer);
                Enter(Phase.Opening);
                return;
            }
            // Revalidate after animation in case the destination became unavailable.
            if (!Application.CanStreamedLevelBeLoaded(targetPath))
            {
                Fail("ステージが見つかりません。Build Settingsを確認してください。");
                return;
            }
            Time.timeScale = 1f;
            HimoHitoAudioSettings.Save();
            Enter(Phase.Loading);
            loadedFrame = Time.frameCount;
            try
            {
                load = SceneManager.LoadSceneAsync(targetPath, LoadSceneMode.Single);
                if (load == null) Fail("ステージを読み込めませんでした。");
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
                Fail("ステージを読み込めませんでした。");
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Additive) return;
            if (phase != Phase.Loading || scene.path != targetPath)
            {
                Finish();
                return;
            }
            destinationSeen = true;
            loadedFrame = Time.frameCount;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                PlayerMover mover = root.GetComponentInChildren<PlayerMover>();
                if (mover == null) continue;
                destinationPlayer = mover.gameObject;
                HoldPlayer(destinationPlayer);
                break;
            }
        }

        private void HoldPlayer(GameObject player)
        {
            heldBody = player.GetComponent<Rigidbody2D>();
            heldMover = player.GetComponent<PlayerMover>();
            heldRope = player.GetComponent<RopeController>();
            bodyWasSimulated = heldBody != null && heldBody.simulated;
            moverWasEnabled = heldMover != null && heldMover.enabled;
            ropeWasEnabled = heldRope != null && heldRope.enabled;
            if (heldBody != null) heldBody.simulated = false;
            if (heldMover != null) heldMover.enabled = false;
            if (heldRope != null) heldRope.enabled = false;
        }

        // Called immediately before the tour snapshots control state, never a frame earlier.
        internal static void ReleasePlayerForPreview(GameObject player)
        {
            if (IsActive && active.destinationPlayer == player) active.ReleaseHold();
        }

        private void ReleaseHold()
        {
            if (heldBody != null) heldBody.simulated = bodyWasSimulated;
            if (heldMover != null) heldMover.enabled = moverWasEnabled;
            if (heldRope != null) heldRope.enabled = ropeWasEnabled;
            heldBody = null;
            heldMover = null;
            heldRope = null;
        }

        private void Enter(Phase next) { phase = next; elapsed = 0; }

        private void Fail(string message)
        {
            if (source != null) source.ReportStageStartError(message);
            ReleaseHold();
            Enter(Phase.Opening);
        }

        private void Finish()
        {
            if (finished) return;
            finished = true;
            ReleaseHold();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (active == this) active = null;
            Destroy(gameObject);
        }

        private void OnDisable() => Finish();
        private void OnDestroy()
        {
            ReleaseHold();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (active == this) active = null;
        }

        private void OnGUI()
        {
            if (finished) return;
            float thread = phase == Phase.Thread ? Smooth(elapsed / ThreadDuration) : 1;
            float cover = phase == Phase.Thread ? 0 : phase == Phase.Closing ? Smooth(elapsed / CloseDuration) : 1;
            float reveal = phase == Phase.Opening ? Smooth(elapsed / OpenDuration) : 0;
            view.Draw(Screen.width, Screen.height, origin, thread, cover, reveal, stageTitle, phase == Phase.Loading);
        }

        private static float Smooth(float value) => Mathf.SmoothStep(0, 1, value);
    }
}
