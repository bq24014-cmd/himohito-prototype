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
        [SerializeField] private string airChainGroup;
        [SerializeField, Min(0)] private int airChainOrder;

        public bool IsAirChainStep =>
            !string.IsNullOrEmpty(airChainGroup) && airChainOrder > 0;
        public string AirChainGroup => airChainGroup;
        public int AirChainOrder => airChainOrder;

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

        public bool ConfigureAirChainStep(string groupName, int order)
        {
            string nextGroup = order > 0 ? groupName : string.Empty;
            int nextOrder = string.IsNullOrEmpty(nextGroup) ? 0 : order;
            bool changed = airChainGroup != nextGroup ||
                           airChainOrder != nextOrder;
            airChainGroup = nextGroup;
            airChainOrder = nextOrder;
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
