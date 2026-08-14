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

        public bool IsReached { get; private set; }

        public void Configure(Vector2 position)
        {
            respawnPosition = position;
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
            if (respawn != null && respawn.TryReachMidpoint(respawnPosition))
            {
                IsReached = true;
            }
        }
    }
}
