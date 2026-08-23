using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Marker component. The rope may attach only to colliders with this component.
    /// </summary>
    public sealed class HookPoint : MonoBehaviour
    {
        [SerializeField] private bool useFixedAttachmentPoint;
        [SerializeField] private Vector2 localAttachmentPoint;

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
            return changed;
        }
    }
}
