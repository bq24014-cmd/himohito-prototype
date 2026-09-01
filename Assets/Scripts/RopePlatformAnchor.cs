using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Marks a yellow Hook that is intended for making a permanent rope
    /// platform. It can require a specific starting area and rope length.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HookPoint))]
    public sealed class RopePlatformAnchor : MonoBehaviour
    {
        [SerializeField, Min(1)] private int requiredRopeLength = 5;

        public int RequiredRopeLength => requiredRopeLength;

        public bool Configure(int ropeLength)
        {
            int safeLength = Mathf.Max(1, ropeLength);
            bool changed = requiredRopeLength != safeLength;
            requiredRopeLength = safeLength;
            return changed;
        }

        public bool CanBuildWith(float ropeLength)
        {
            return Mathf.Abs(ropeLength - requiredRopeLength) <= 0.05f;
        }
    }
}
