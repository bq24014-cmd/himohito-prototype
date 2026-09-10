using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Marks the start of a section and applies its resource lower bound.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class MainStageCheckpoint : MonoBehaviour
    {
        [SerializeField, Range(2, 10)] private int sectionNumber = 2;
        [SerializeField] private Vector2 respawnPosition;
        [SerializeField, Min(0f)] private float minimumRopeAfterCheckpoint;
        private bool reached;
        private Collider2D checkpointCollider;
        private readonly CheckpointLandingGate landingGate = new CheckpointLandingGate();

        public void Configure(int section, Vector2 position, float lowerBound)
        {
            sectionNumber = Mathf.Clamp(section, 2, 10);
            respawnPosition = position;
            minimumRopeAfterCheckpoint = Mathf.Max(0f, lowerBound);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (reached || !IsPlayerLandingOnTop(collision))
            {
                return;
            }

            MainStageRespawnOnFall respawn =
                collision.rigidbody.GetComponent<MainStageRespawnOnFall>();
            if (respawn != null && respawn.TryReachSection(
                    sectionNumber,
                    respawnPosition,
                    minimumRopeAfterCheckpoint))
            {
                reached = true;
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
