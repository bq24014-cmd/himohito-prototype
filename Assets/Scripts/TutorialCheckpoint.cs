using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Saves tutorial progress after the player lands.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class TutorialCheckpoint : MonoBehaviour
    {
        [SerializeField, Range(2, 4)] private int sectionNumber = 2;
        [SerializeField] private Vector2 respawnPosition;
        [SerializeField, Range(1, 14)] private int startingRopeLength = 6;

        private Collider2D checkpointCollider;
        private readonly CheckpointLandingGate landingGate = new CheckpointLandingGate();

        public int SectionNumber => sectionNumber;
        public Vector2 RespawnPosition => respawnPosition;

        public void Configure(int section, Vector2 position, int selectedLength)
        {
            sectionNumber = Mathf.Clamp(section, 2, 4);
            respawnPosition = position;
            startingRopeLength = Mathf.Clamp(selectedLength, 1, 14);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (!IsPlayerLandingOnTop(collision))
            {
                return;
            }

            PrototypeRunController runController =
                collision.rigidbody.GetComponent<PrototypeRunController>();
            if (runController != null)
            {
                int previousSection = runController.CurrentTutorialSection;
                runController.TryReachTutorialSection(
                    sectionNumber,
                    respawnPosition,
                    startingRopeLength);
                if (runController.CurrentTutorialSection > previousSection)
                    SectionArrivalFeedback.Notify(collision.rigidbody.gameObject,
                        sectionNumber, respawnPosition, checkpointCollider);
            }
        }

        private bool IsPlayerLandingOnTop(Collision2D collision)
        {
            checkpointCollider ??= GetComponent<Collider2D>();
            return landingGate.Evaluate(collision, checkpointCollider, respawnPosition, Time.fixedTime);
        }

        private void OnCollisionExit2D(Collision2D collision) => landingGate.Exit(collision.rigidbody);
        private void OnDisable() => landingGate.Reset();
    }
}
