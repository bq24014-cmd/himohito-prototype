using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Completes the main stage when the player lands on the final green platform.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class MainStageGoalZone : MonoBehaviour
    {
        public bool IsClear { get; private set; }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (IsClear ||
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
            IsClear = true;
        }
    }
}
