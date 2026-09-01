using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Keeps a rail walkable while detached, but removes only the player's
    /// collision while a rope attachment is active so it cannot block an arc.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class SwingPassThroughRailPlatform : MonoBehaviour
    {
        private Collider2D railCollider;
        private Collider2D playerCollider;
        private RopeController playerRope;
        private bool isIgnoringPlayer;

        private void Awake()
        {
            railCollider = GetComponent<Collider2D>();
            ResolvePlayer();
            UpdatePlayerCollision();
        }

        private void FixedUpdate()
        {
            if (playerRope == null || playerCollider == null)
            {
                ResolvePlayer();
            }
            UpdatePlayerCollision();
        }

        private void ResolvePlayer()
        {
            playerRope = FindFirstObjectByType<RopeController>();
            if (playerRope != null)
            {
                playerCollider = playerRope.GetComponent<Collider2D>();
            }
        }

        private void UpdatePlayerCollision()
        {
            if (railCollider == null || playerCollider == null)
            {
                return;
            }

            bool isBridgeBuildingAttachment =
                playerRope != null &&
                playerRope.ActiveHookPoint != null &&
                playerRope.ActiveHookPoint.TryGetComponent(
                    out RopePlatformAnchor _);
            bool shouldIgnore =
                playerRope != null &&
                playerRope.IsAttached &&
                !isBridgeBuildingAttachment;
            if (shouldIgnore == isIgnoringPlayer)
            {
                return;
            }

            Physics2D.IgnoreCollision(
                railCollider,
                playerCollider,
                shouldIgnore);
            isIgnoringPlayer = shouldIgnore;
        }

        private void OnDisable()
        {
            RestorePlayerCollision();
        }

        private void OnDestroy()
        {
            RestorePlayerCollision();
        }

        private void RestorePlayerCollision()
        {
            if (!isIgnoringPlayer ||
                railCollider == null || playerCollider == null)
            {
                return;
            }

            Physics2D.IgnoreCollision(
                railCollider,
                playerCollider,
                false);
            isIgnoringPlayer = false;
        }
    }
}
