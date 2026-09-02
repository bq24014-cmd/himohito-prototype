using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Restarts section eight when the player falls into the pit or when an
    /// overlong generated bridge physically overlaps a spike.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class SectionEightPitHazard : MonoBehaviour
    {
        private Collider2D detectionArea;
        private MainStageRespawnOnFall respawn;
        private Collider2D playerCollider;

        private void Awake()
        {
            detectionArea = GetComponent<Collider2D>();
            CachePlayer();
        }

        private void FixedUpdate()
        {
            CachePlayer();
            if (detectionArea == null || !detectionArea.enabled ||
                respawn == null)
            {
                return;
            }

            if (playerCollider != null && playerCollider.enabled &&
                Physics2D.Distance(
                    detectionArea,
                    playerCollider).isOverlapped)
            {
                respawn.FailCurrentSection();
                return;
            }

            GeneratedRopePlatform[] platforms =
                FindObjectsByType<GeneratedRopePlatform>(
                    FindObjectsSortMode.None);
            foreach (GeneratedRopePlatform platform in platforms)
            {
                if (platform == null ||
                    !platform.TryGetComponent(out Collider2D platformCollider) ||
                    !platformCollider.enabled)
                {
                    continue;
                }

                if (Physics2D.Distance(
                        detectionArea,
                        platformCollider).isOverlapped)
                {
                    respawn.FailCurrentSection();
                    return;
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryFailFor(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryFailFor(other);
        }

        private void TryFailFor(Collider2D other)
        {
            CachePlayer();
            if (respawn == null || other == null)
            {
                return;
            }
            if (other.GetComponentInParent<MainStageRespawnOnFall>() == respawn ||
                other.TryGetComponent(out GeneratedRopePlatform _))
            {
                respawn.FailCurrentSection();
            }
        }

        private void CachePlayer()
        {
            if (respawn == null)
            {
                respawn = FindFirstObjectByType<MainStageRespawnOnFall>();
            }
            if (respawn != null && playerCollider == null)
            {
                playerCollider = respawn.GetComponent<Collider2D>();
            }
        }
    }
}
