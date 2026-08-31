using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Gives a dedicated Hook a fixed attachment position. Other solid colliders
    /// use the actual raycast hit position as their rope anchor.
    /// </summary>
    public sealed class HookPoint : MonoBehaviour
    {
        [SerializeField] private bool useFixedAttachmentPoint;
        [SerializeField] private Vector2 localAttachmentPoint;

        private void Awake()
        {
            ConfigureNonSolidColliders();
        }

        public Vector2 GetAttachmentPoint(Vector2 raycastHitPoint)
        {
            return useFixedAttachmentPoint
                ? transform.TransformPoint(localAttachmentPoint)
                : raycastHitPoint;
        }

        public bool ConfigureFixedAttachmentPoint(Vector2 localPoint)
        {
            bool changed = !useFixedAttachmentPoint ||
                           localAttachmentPoint != localPoint;
            useFixedAttachmentPoint = true;
            localAttachmentPoint = localPoint;
            changed |= ConfigureNonSolidColliders();
            return changed;
        }

        private bool ConfigureNonSolidColliders()
        {
            bool changed = false;
            foreach (Collider2D hookCollider in GetComponents<Collider2D>())
            {
                if (!hookCollider.isTrigger)
                {
                    hookCollider.isTrigger = true;
                    changed = true;
                }
            }
            return changed;
        }
    }
}
