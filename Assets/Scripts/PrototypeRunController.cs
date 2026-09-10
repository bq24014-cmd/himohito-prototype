using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace HimoHito
{
    /// <summary>
    /// Connects the graybox into one complete run: play, clear or fail, then restart.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(RopeResource), typeof(RopeController))]
    [RequireComponent(typeof(RopePlatformBuilder))]
    public sealed class PrototypeRunController : MonoBehaviour
    {
        public const float TutorialRopeCapacity = 99f;
        private const float ClearRevealDelay = 1.35f;

        public enum RunOutcome
        {
            WaitingToStart,
            Playing,
            Clearing,
            Clear,
            Failed
        }

        public enum RunFailureReason
        {
            None,
            Fell,
            RopeExhausted
        }

        [SerializeField] private float fallThreshold = -9f;
        [SerializeField, Min(0.01f)] private float minimumUsableRopeLength = 1f;
        [FormerlySerializedAs("startFromSectionThreeForDevelopment")]
        [SerializeField] private bool startFromSectionFourForDevelopment;

        private Rigidbody2D body;
        private RopeResource ropeResource;
        private RopeController ropeController;
        private RopePlatformBuilder platformBuilder;
        private PlayerMover playerMover;
        private StageOverlayControls overlayControls;
        private TutorialSectionGuide tutorialSectionGuide;
        private int titleEnteredFrame;
        private bool clearNavigationStarted;
        public bool IsStageSelectionOpen => Outcome == RunOutcome.WaitingToStart;
        public int SelectedStageIndex { get; private set; }
        public string StageSelectionError { get; private set; } = string.Empty;
        private Vector2 startPosition;
        private float startRopeCapacity;
        private float startRopeLength;
        private int startSelectedRopeLength;
        private int startTutorialSection = 1;
        private Vector2 checkpointPosition;
        private float checkpointRopeLength;
        private int checkpointSelectedRopeLength;
        private RopePlatformBuilder.PlatformState[] checkpointPlatformStates;

        public RunOutcome Outcome { get; private set; } = RunOutcome.WaitingToStart;
        public RunFailureReason FailureReason { get; private set; } = RunFailureReason.None;
        public bool IsFallUnravelling => FallUnravelVisual.IsPlayingFor(gameObject);
        public int CurrentTutorialSection { get; private set; } = 1;
        public const int TutorialSectionCount = 4;
        public string CurrentTutorialObjective => CurrentTutorialSection switch
        {
            1 => "長さと向きを選び、ヒモを掛けて振る",
            2 => "長さを選び、中央のトゲを越える",
            3 => "地形同士に掛けたヒモを足場にする",
            4 => "中央のHookを外し、1本の足場でゴールする",
            _ => string.Empty
        };

        private void Awake()
        {
            TutorialSectionOneSetup.ApplyCurrentScene();
            TutorialSectionTwoSetup.ApplyCurrentScene();
            TutorialSectionThreeSetup.ApplyCurrentScene();
            TutorialSectionFourSetup.ApplyCurrentScene();
            TutorialFirstSectionVisuals.Apply(gameObject);

            body = GetComponent<Rigidbody2D>();
            ropeResource = GetComponent<RopeResource>();
            ropeController = GetComponent<RopeController>();
            platformBuilder = GetComponent<RopePlatformBuilder>();
            if (platformBuilder == null)
            {
                platformBuilder = gameObject.AddComponent<RopePlatformBuilder>();
            }
            playerMover = GetComponent<PlayerMover>();
            overlayControls = FindFirstObjectByType<StageOverlayControls>();
            if (overlayControls == null)
            {
                overlayControls = gameObject.AddComponent<StageOverlayControls>();
            }
            tutorialSectionGuide = GetComponent<TutorialSectionGuide>();
            if (tutorialSectionGuide == null)
            {
                tutorialSectionGuide =
                    gameObject.AddComponent<TutorialSectionGuide>();
            }
            ropeResource.RestoreCapacityAndCurrent(
                TutorialRopeCapacity,
                TutorialRopeCapacity);
            if (startFromSectionFourForDevelopment)
            {
                startTutorialSection = 4;
                CurrentTutorialSection = 4;
                body.position =
                    TutorialSectionThreeSetup.LandingRespawnPosition;
                ropeController.RestoreSelectedRopeLength(
                    TutorialSectionFourSetup.PlatformRopeLength);
            }
            else
            {
                body.position = TutorialSectionOneSetup.StartRespawnPosition;
                ropeController.RestoreSelectedRopeLength(
                    TutorialSectionOneSetup.StartingRopeLength);
            }
            Physics2D.SyncTransforms();
            startPosition = body.position;
            startRopeCapacity = ropeResource.MaximumLength;
            startRopeLength = ropeResource.CurrentLength;
            startSelectedRopeLength = ropeController.SelectedRopeLength;

        }

        private void Start()
        {
            EnterStartScreen();
            checkpointPosition = startPosition;
            CaptureCheckpointState();
        }

        private void Update()
        {
            if (StageStartTransition.IsActive) return;
            if (clearNavigationStarted) return;
            if (MainStagePreview.IsActive) return;
            if (IsFallUnravelling) return;

            if (Outcome == RunOutcome.WaitingToStart)
            {
                UpdateStartScreen();
                return;
            }

            if (Outcome == RunOutcome.Clear && Input.GetKeyDown(KeyCode.Escape))
            {
                ReturnToStageSelectionFromClear();
                return;
            }

            if (Outcome == RunOutcome.Clear &&
                (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            {
                ContinueAfterClear();
                return;
            }

            if (Outcome == RunOutcome.Clearing)
            {
                return;
            }

            if (Outcome == RunOutcome.Failed &&
                Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadScene("Tutorial");
                return;
            }

            if (tutorialSectionGuide != null &&
                tutorialSectionGuide.IsVisible)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                if (Outcome == RunOutcome.Clear)
                {
                    RestartTutorial();
                }
                else
                {
                    RestartFromCheckpoint();
                }
                return;
            }

            if (Outcome != RunOutcome.Playing)
            {
                return;
            }

            bool fell = body.position.y < fallThreshold;
            bool cannotUseRope = !ropeController.IsAttached &&
                                 ropeResource.CurrentLength < minimumUsableRopeLength;
            if (fell)
            {
                Finish(RunOutcome.Failed, RunFailureReason.Fell);
                return;
            }

            if (cannotUseRope)
            {
                Finish(RunOutcome.Failed, RunFailureReason.RopeExhausted);
            }
        }

        private void EnterStartScreen()
        {
            titleEnteredFrame = Time.frameCount;
            ropeController.DetachAndRefund();
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.simulated = false;
            playerMover.enabled = false;
            ropeController.enabled = false;
            FailureReason = RunFailureReason.None;
            Outcome = RunOutcome.WaitingToStart;
            SelectedStageIndex = 0;
            StageSelectionError = string.Empty;
        }

        private void UpdateStartScreen()
        {
            // The stage map is the only home screen. Returning from a scene or
            // closing help must not reuse that frame's key as Start or Quit.
            if (!CanUseTitleMenu())
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape)) QuitFromTitle();
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                SelectStage(SelectedStageIndex - 1);
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                SelectStage(SelectedStageIndex + 1);
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                StartSelectedStage();
        }

        public bool CanUseTitleMenu() => Outcome == RunOutcome.WaitingToStart && !StageStartTransition.IsActive &&
            (!Application.isPlaying || Time.frameCount > titleEnteredFrame) &&
            !Input.GetKeyDown(KeyCode.Tab) &&
            (overlayControls == null || (!overlayControls.IsOverlayVisible &&
                !overlayControls.HelpInputConsumedThisFrame));

        public void OpenStageSelection()
        {
            if (!CanUseTitleMenu()) return;
            SelectStage(SelectedStageIndex);
        }

        public void OpenMenuHelp()
        {
            if (!CanUseTitleMenu()) return;
            overlayControls?.OpenMenuHelp();
        }

        public void SelectStage(int index)
        {
            if (!CanUseTitleMenu() || !IsStageSelectionOpen) return;
            SelectedStageIndex = Mathf.Clamp(index, 0, StageCatalog.Entries.Count - 1);
            StageSelectionError = string.Empty;
        }

        public bool StartSelectedStage()
        {
            if (!CanUseTitleMenu() || !IsStageSelectionOpen) return false;
            StageCatalog.Entry stage = StageCatalog.Entries[SelectedStageIndex];
            if (!stage.available) { StageSelectionError = "このステージは追加予定です。今はまだあそべません。"; return false; }
            if (stage.scenePath != StageCatalog.TitleScenePath && !Application.CanStreamedLevelBeLoaded(stage.scenePath))
            {
                StageSelectionError = "ステージが見つかりません。Build Settingsを確認してください。";
                return false;
            }
            return StageStartTransition.Begin(this, stage,
                StageSelectionView.SelectedIslandOrigin(SelectedStageIndex, StageCatalog.Entries.Count,
                    Screen.width, Screen.height));
        }

        public void BeginGameFromTitle()
        {
            if (!CanUseTitleMenu()) return;
            CommitTutorialStart();
        }

        internal void ReportStageStartError(string message) => StageSelectionError = message;

        internal void CommitTutorialStart()
        {
            if (Outcome != RunOutcome.WaitingToStart) return;
            Time.timeScale = 1f;
            body.simulated = true;
            playerMover.enabled = true;
            ropeController.enabled = true;
            Outcome = RunOutcome.Playing;
            MainStagePreview.PlayFor(gameObject);
        }

        public void QuitFromTitle()
        {
            if (!CanUseTitleMenu()) return;
            HimoHitoAudioSettings.Save();
            Time.timeScale = 1f;
            QuitApplication();
        }

        private static void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void MarkClear()
        {
            bool tutorialStepsComplete = gameObject.scene.name != "Tutorial" ||
                                         CurrentTutorialSection >= TutorialSectionCount;
            if (Outcome == RunOutcome.Playing && tutorialStepsComplete)
            {
                BeginClearSequence();
            }
        }

        private void BeginClearSequence()
        {
            ropeController.DetachAndRefund();
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.simulated = false;
            FailureReason = RunFailureReason.None;
            Outcome = RunOutcome.Clearing;

            GetComponent<RopeBodyVisual>()?.PlayGoalPose(ClearRevealDelay,
                GameObject.Find(TutorialSectionFourSetup.GoalMarkerName)?.transform);

            StartCoroutine(CompleteClearSequence());
        }

        private IEnumerator CompleteClearSequence()
        {
            yield return new WaitForSecondsRealtime(ClearRevealDelay);
            GetComponent<PrototypeAudioFeedback>()?.PlayClearRevealed();
            Outcome = RunOutcome.Clear;
            StageProgress.MarkSceneCleared(gameObject.scene.path);
        }

        public void TryReachTutorialSection(
            int sectionNumber,
            Vector2 respawnPosition,
            int startingRopeLength)
        {
            bool canReachCheckpointWhileAttached =
                CurrentTutorialSection == 1 && sectionNumber == 2;
            if (Outcome != RunOutcome.Playing ||
                (ropeController.IsAttached && !canReachCheckpointWhileAttached) ||
                sectionNumber <= CurrentTutorialSection ||
                sectionNumber > TutorialSectionCount)
            {
                return;
            }

            CurrentTutorialSection = sectionNumber;
            checkpointPosition = respawnPosition;
            // Keep the player's selection across sections instead of applying the lesson preset.
            CaptureCheckpointState();
        }

        private void Finish(
            RunOutcome outcome,
            RunFailureReason failureReason = RunFailureReason.None)
        {
            ropeController.DetachAndRefund();
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.simulated = false;
            FailureReason = failureReason;
            Outcome = outcome;
            if (failureReason == RunFailureReason.Fell)
            {
                playerMover.enabled = false;
                ropeController.enabled = false;
                FallUnravelVisual.Play(gameObject);
            }
            if (failureReason == RunFailureReason.RopeExhausted)
            {
                GetComponent<PrototypeAudioFeedback>()?.PlayRopeExhausted();
            }
        }

        private void RestartFromCheckpoint()
        {
            body.simulated = true;
            ropeController.DetachAndRefund();
            RestoreCheckpointState();
            body.position = checkpointPosition;
            // Also reset the interpolated presentation before camera/reveal code runs.
            transform.position = new Vector3(checkpointPosition.x, checkpointPosition.y, transform.position.z);
            ResetMotionAndResume();
            RespawnWeaveVisual.Play(gameObject);
        }

        private void RestartTutorial()
        {
            GetComponent<SectionArrivalFeedback>()?.ClearTrail();
            body.simulated = true;
            ropeController.DetachAndRefund();
            ropeResource.RestoreCapacityAndCurrent(
                startRopeCapacity,
                startRopeLength);
            platformBuilder.ClearPlatforms();

            CurrentTutorialSection = startTutorialSection;
            checkpointPosition = startPosition;
            body.position = startPosition;
            transform.position = new Vector3(startPosition.x, startPosition.y, transform.position.z);
            ropeController.RestoreSelectedRopeLength(
                startSelectedRopeLength);
            CaptureCheckpointState();
            ResetMotionAndResume();
            MainStagePreview.PlayFor(gameObject);
        }

        private void CaptureCheckpointState()
        {
            checkpointRopeLength = ropeResource.CurrentLength;
            checkpointSelectedRopeLength = ropeController.SelectedRopeLength;
            checkpointPlatformStates = platformBuilder.CapturePlatformStates();
        }

        private void RestoreCheckpointState()
        {
            ropeResource.RestoreCurrentLength(checkpointRopeLength);
            ropeController.RestoreSelectedRopeLength(checkpointSelectedRopeLength);
            platformBuilder.RestorePlatformStates(checkpointPlatformStates);
        }

        private void ResetMotionAndResume()
        {
            GetComponent<PrototypeAudioFeedback>()?.StopRopeExhaustedAudio();
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            playerMover.enabled = true;
            ropeController.enabled = true;
            FailureReason = RunFailureReason.None;
            Outcome = RunOutcome.Playing;
            Camera.main?.GetComponent<HorizontalCameraFollow>()?.ResetFraming();
        }

        public void ReturnToStageSelectionFromClear()
        {
            if (Outcome != RunOutcome.Clear || clearNavigationStarted) return;
            if (!Application.CanStreamedLevelBeLoaded(StageCatalog.TitleScenePath))
            {
                Debug.LogError("ステージ選択へ戻れません。TutorialをBuild Settingsに登録してください。");
                return;
            }
            clearNavigationStarted = true;
            HimoHitoAudioSettings.Save();
            Time.timeScale = 1f;
            SceneManager.LoadScene(StageCatalog.TitleScenePath);
        }

        public void ContinueAfterClear()
        {
            if (Outcome != RunOutcome.Clear || clearNavigationStarted) return;
            LoadNextStage();
        }

        private void LoadNextStage()
        {
            const string MainStageSceneName = "MainStage";
            if (Application.CanStreamedLevelBeLoaded(MainStageSceneName))
            {
                clearNavigationStarted = true;
                HimoHitoAudioSettings.Save();
                Time.timeScale = 1f;
                SceneManager.LoadScene(MainStageSceneName);
                return;
            }

            Debug.LogError(
                $"本編シーンを読み込めません。Build Settingsに" +
                $"{MainStageSceneName}が登録されているか確認してください。");
        }
    }
}
