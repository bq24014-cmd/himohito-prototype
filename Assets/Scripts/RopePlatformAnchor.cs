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
        [SerializeField] private Vector2 requiredOrigin;
        [SerializeField, Min(0.1f)] private float originTolerance = 0.85f;
        [SerializeField, Min(1)] private int requiredRopeLength = 5;

        public int RequiredRopeLength => requiredRopeLength;

        public bool Configure(
            Vector2 origin,
            float tolerance,
            int ropeLength)
        {
            float safeTolerance = Mathf.Max(0.1f, tolerance);
            int safeLength = Mathf.Max(1, ropeLength);
            bool changed = requiredOrigin != origin ||
                           !Mathf.Approximately(
                               originTolerance,
                               safeTolerance) ||
                           requiredRopeLength != safeLength;
            requiredOrigin = origin;
            originTolerance = safeTolerance;
            requiredRopeLength = safeLength;
            return changed;
        }

        public bool CanBuildFrom(Vector2 playerPosition, float ropeLength)
        {
            return Vector2.Distance(playerPosition, requiredOrigin) <=
                       originTolerance &&
                   Mathf.Abs(ropeLength - requiredRopeLength) <= 0.05f;
        }
    }
}
