using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Marks the run clear when the player reaches the green goal platform.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class GoalZone : MonoBehaviour
    {
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.rigidbody == null)
            {
                return;
            }

            if (collision.rigidbody.position.y < transform.position.y)
            {
                return;
            }

            PrototypeRunController runController =
                collision.rigidbody.GetComponent<PrototypeRunController>();
            if (runController != null)
            {
                runController.MarkClear();
            }
        }
    }
}
