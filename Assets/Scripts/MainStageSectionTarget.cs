using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Marks the temporary end of a main-stage section during incremental development.
    /// </summary>
    public sealed class MainStageSectionTarget : MonoBehaviour
    {
        public bool IsReached { get; private set; }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.TryGetComponent(out RopeResource _) &&
                collision.transform.position.y > transform.position.y)
            {
                IsReached = true;
            }
        }
    }
}
