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
        private const float ClearRevealDelay = 0.45f;

        public bool IsClear { get; private set; }
        public bool IsCompleting => isClearPending || IsClear;
        private bool isClearPending;

        private void Update()
        {
            if (IsClear && Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (IsClear ||
                isClearPending ||
                collision.rigidbody == null ||
                collision.rigidbody.position.y < transform.position.y ||
                !collision.rigidbody.TryGetComponent(out RopeResource _))
            {
                return;
            }

            if (collision.rigidbody.TryGetComponent(out RopeController ropeController))
            {
                ropeController.DetachAndRefund();
            }

            collision.rigidbody.linearVelocity = Vector2.zero;
            collision.rigidbody.angularVelocity = 0f;
            collision.rigidbody.simulated = false;

            isClearPending = true;
            PrototypeAudioFeedback audioFeedback =
                collision.rigidbody.GetComponent<PrototypeAudioFeedback>();
            audioFeedback?.PlayGoalChestOpened();
            StartCoroutine(CompleteClearSequence(audioFeedback));
        }

        private IEnumerator CompleteClearSequence(
            PrototypeAudioFeedback audioFeedback)
        {
            yield return new WaitForSecondsRealtime(ClearRevealDelay);
            audioFeedback?.PlayClearRevealed();
            IsClear = true;
            isClearPending = false;
        }
    }
}
