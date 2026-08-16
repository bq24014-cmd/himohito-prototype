using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Starts the focused two-move rope-allocation experiment and saves its restart state.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class MainStageSectionEightCheckpoint : MonoBehaviour
    {
        [SerializeField] private Vector2 respawnPosition;
        [SerializeField, Min(1f)] private float experimentRopeLength = 10.2f;

        public bool IsReached { get; private set; }

        public void Configure(Vector2 position, float ropeLength)
        {
            respawnPosition = position;
            experimentRopeLength = Mathf.Max(1f, ropeLength);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (IsReached ||
                collision.rigidbody == null ||
                collision.rigidbody.position.y <= transform.position.y)
            {
                return;
            }

            MainStageRespawnOnFall respawn =
                collision.rigidbody.GetComponent<MainStageRespawnOnFall>();
            if (respawn != null && respawn.TryStartSectionEight(
                    respawnPosition,
                    experimentRopeLength))
            {
                IsReached = true;
            }
        }
    }
}
