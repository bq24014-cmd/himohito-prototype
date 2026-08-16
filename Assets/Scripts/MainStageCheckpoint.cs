using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Saves the chosen branch outcome when the player reaches the main-stage midpoint.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class MainStageCheckpoint : MonoBehaviour
    {
        [SerializeField] private Vector2 respawnPosition;
        [SerializeField, Min(0)] private int checkpointWeaveThreads;
        [SerializeField] private bool grantsSectionSevenBridge;

        public bool IsReached { get; private set; }

        public void Configure(
            Vector2 position,
            int weaveThreads,
            bool grantsBridge)
        {
            respawnPosition = position;
            checkpointWeaveThreads = Mathf.Max(0, weaveThreads);
            grantsSectionSevenBridge = grantsBridge;
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
            if (respawn != null && respawn.TryReachMidpoint(
                    respawnPosition,
                    checkpointWeaveThreads,
                    grantsSectionSevenBridge))
            {
                IsReached = true;
            }
        }
    }
}
