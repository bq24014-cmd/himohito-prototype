using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HimoHito
{
    /// <summary>
    /// Completes the main stage when the player lands on the final green platform.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class MainStageGoalZone : MonoBehaviour
    {
        private const float ClearRevealDelay = 1.35f;

        public bool IsClear { get; private set; }
        public bool IsCompleting => isClearPending || IsClear;
        private bool isClearPending;
        private bool clearNavigationStarted;

        private void Update()
        {
            if (!IsClear || clearNavigationStarted) return;
            if (Input.GetKeyDown(KeyCode.Escape)) ReturnToStageSelectionFromClear();
            else if (Input.GetKeyDown(KeyCode.R)) RetryAfterClear();
        }

        public void RetryAfterClear()
        {
            if (!IsClear || clearNavigationStarted) return;
            clearNavigationStarted = true;
            HimoHitoAudioSettings.Save();
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void ReturnToStageSelectionFromClear()
        {
            if (!IsClear || clearNavigationStarted) return;
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

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (IsClear ||
                isClearPending ||
                collision.rigidbody == null ||
                collision.rigidbody.position.y < transform.position.y ||
                !collision.rigidbody.TryGetComponent(out RopeResource _))
            {
                return;
            }

            if (!GoalChestPresentation.ReadyToClear(collision, GetComponent<Collider2D>(),
                MainStageSectionTenSetup.GoalMarkerName)) return;

            if (collision.rigidbody.TryGetComponent(out RopeController ropeController))
            {
                ropeController.DetachAndRefund();
            }

            collision.rigidbody.linearVelocity = Vector2.zero;
            collision.rigidbody.angularVelocity = 0f;
            collision.rigidbody.simulated = false;

            isClearPending = true;
            collision.rigidbody.GetComponent<RopeBodyVisual>()?.PlayGoalPose(ClearRevealDelay,
                GameObject.Find(MainStageSectionTenSetup.GoalMarkerName)?.transform);
            PrototypeAudioFeedback audioFeedback =
                collision.rigidbody.GetComponent<PrototypeAudioFeedback>();
            StartCoroutine(CompleteClearSequence(audioFeedback));
        }

        private IEnumerator CompleteClearSequence(
            PrototypeAudioFeedback audioFeedback)
        {
            yield return new WaitForSecondsRealtime(ClearRevealDelay);
            audioFeedback?.PlayClearRevealed();
            IsClear = true;
            isClearPending = false;
            StageProgress.MarkSceneCleared(gameObject.scene.path);
        }
    }
}
