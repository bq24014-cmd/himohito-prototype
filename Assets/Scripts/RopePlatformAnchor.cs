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
        [SerializeField, Min(1)] private int maximumRopeLength = 5;
        [SerializeField] private string pairedAnchorName;

        public int RequiredRopeLength => requiredRopeLength;
        public int MaximumRopeLength => maximumRopeLength;

        public bool Configure(int ropeLength, string pairedObjectName = "")
        {
            int safeLength = Mathf.Max(1, ropeLength);
            string safePairedName = pairedObjectName ?? string.Empty;
            bool changed = requiredRopeLength != safeLength ||
                           maximumRopeLength != safeLength ||
                           pairedAnchorName != safePairedName;
            requiredRopeLength = safeLength;
            maximumRopeLength = safeLength;
            pairedAnchorName = safePairedName;
            return changed;
        }

        public bool ConfigureRange(
            int minimumLength,
            int maximumLength,
            string pairedObjectName = "")
        {
            int safeMinimum = Mathf.Max(1, minimumLength);
            int safeMaximum = Mathf.Max(safeMinimum, maximumLength);
            string safePairedName = pairedObjectName ?? string.Empty;
            bool changed = requiredRopeLength != safeMinimum ||
                           maximumRopeLength != safeMaximum ||
                           pairedAnchorName != safePairedName;
            requiredRopeLength = safeMinimum;
            maximumRopeLength = safeMaximum;
            pairedAnchorName = safePairedName;
            return changed;
        }

        public bool CanBuildWith(float ropeLength)
        {
            return ropeLength >= requiredRopeLength - 0.05f &&
                   ropeLength <= maximumRopeLength + 0.05f;
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
