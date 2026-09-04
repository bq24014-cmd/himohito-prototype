using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Makes a rail behave as a one-way floor: the player can pass upward
    /// through it, but can stand on its upper surface while falling.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D), typeof(PlatformEffector2D))]
    public sealed class OneWayRailPlatform : MonoBehaviour
    {
        [SerializeField, Range(90f, 180f)] private float surfaceArc = 160f;
        [SerializeField, Min(0f)] private float groundingTolerance = 0.12f;

        private BoxCollider2D platformCollider;

        private void Awake()
        {
            ConfigureComponents();
        }

        private void Reset()
        {
            ConfigureComponents();
        }

        private void OnValidate()
        {
            ConfigureComponents();
        }

        public bool CanSupport(Collider2D playerCollider)
        {
            if (playerCollider == null)
            {
                return false;
            }

            ConfigureComponents();
            return playerCollider.bounds.min.y >=
                platformCollider.bounds.max.y - groundingTolerance;
        }

        public void RestoreAfterRestart()
        {
            enabled = true;
            ConfigureComponents();
        }

        private void ConfigureComponents()
        {
            if (!TryGetComponent(out platformCollider))
            {
                return;
            }

            platformCollider.isTrigger = false;
            platformCollider.usedByEffector = true;

            PlatformEffector2D effector = GetComponent<PlatformEffector2D>();
            effector.enabled = true;
            effector.useOneWay = true;
            effector.useOneWayGrouping = true;
            effector.surfaceArc = surfaceArc;
            effector.rotationalOffset = 0f;
        }
    }
}
