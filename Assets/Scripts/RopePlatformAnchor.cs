using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Marks a special Hook that is intended for making a permanent rope
    /// platform. A paired Hook can fix both ends of the generated platform.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HookPoint))]
    public sealed class RopePlatformAnchor : MonoBehaviour
    {
        [SerializeField, Min(1)] private int requiredRopeLength = 5;
        [SerializeField] private string pairedAnchorName;

        public int RequiredRopeLength => requiredRopeLength;

        public bool Configure(int ropeLength, string pairedObjectName = "")
        {
            int safeLength = Mathf.Max(1, ropeLength);
            string safePairedName = pairedObjectName ?? string.Empty;
            bool changed = requiredRopeLength != safeLength ||
                           pairedAnchorName != safePairedName;
            requiredRopeLength = safeLength;
            pairedAnchorName = safePairedName;
            return changed;
        }

        public bool CanBuildWith(float ropeLength)
        {
            return Mathf.Abs(ropeLength - requiredRopeLength) <= 0.05f;
        }

        public bool TryGetPairedAnchor(out Vector2 anchorPosition)
        {
            anchorPosition = default;
            if (string.IsNullOrWhiteSpace(pairedAnchorName))
            {
                return false;
            }

            foreach (GameObject candidate in
                     Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (candidate != gameObject &&
                    candidate.scene == gameObject.scene &&
                    candidate.activeInHierarchy &&
                    candidate.name == pairedAnchorName &&
                    candidate.TryGetComponent(out HookPoint pairedHook))
                {
                    anchorPosition = pairedHook.GetAttachmentPoint(
                        candidate.transform.position);
                    return true;
                }
            }

            return false;
        }
    }
}
