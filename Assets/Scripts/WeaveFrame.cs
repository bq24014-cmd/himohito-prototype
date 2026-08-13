using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Builds one predefined platform when the player spends enough weave threads with Q.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class WeaveFrame : MonoBehaviour
    {
        [SerializeField, Min(1)] private int requiredThreads = 2;
        [SerializeField] private GameObject wovenPlatform;

        private WeaveResource weaveResource;
        private bool playerInRange;

        public int RequiredThreads => requiredThreads;
        public int RemainingThreads => weaveResource == null
            ? requiredThreads
            : Mathf.Max(0, requiredThreads - weaveResource.CurrentThreads);
        public bool IsPlayerInRange => playerInRange;
        public bool IsCompleted { get; private set; }

        private void Awake()
        {
            weaveResource = FindFirstObjectByType<WeaveResource>();
            ApplyPlatformState();
        }

        public void Configure(GameObject platform, int threadCost)
        {
            wovenPlatform = platform;
            requiredThreads = Mathf.Max(1, threadCost);
            ApplyPlatformState();
        }

        private void Update()
        {
            if (!playerInRange || IsCompleted || !Input.GetKeyDown(KeyCode.Q))
            {
                return;
            }

            if (weaveResource != null && weaveResource.TrySpend(requiredThreads))
            {
                IsCompleted = true;
                ApplyPlatformState();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<WeaveResource>() != null)
            {
                playerInRange = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponentInParent<WeaveResource>() != null)
            {
                playerInRange = false;
            }
        }

        public void ResetWeave()
        {
            IsCompleted = false;
            playerInRange = false;
            ApplyPlatformState();
        }

        private void ApplyPlatformState()
        {
            if (wovenPlatform != null)
            {
                wovenPlatform.SetActive(IsCompleted);
            }
        }
    }
}
