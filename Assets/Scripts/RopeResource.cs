using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// The single source of truth for the player's remaining rope length.
    /// UI and rope actions read this component instead of keeping separate values.
    /// </summary>
    public sealed class RopeResource : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maximumLength = 12f;
        [SerializeField, Min(0f)] private float currentLength = 12f;

        public float MaximumLength => maximumLength;
        public float CurrentLength => currentLength;
        public float NormalizedLength => maximumLength <= 0f ? 0f : currentLength / maximumLength;

        private void Awake()
        {
            currentLength = Mathf.Clamp(currentLength, 0f, maximumLength);

            if (TryGetComponent(out SpriteRenderer _) &&
                !TryGetComponent(out RopeBodyVisual _))
            {
                gameObject.AddComponent<RopeBodyVisual>();
            }
        }

        private void OnValidate()
        {
            maximumLength = Mathf.Max(1f, maximumLength);
            currentLength = Mathf.Clamp(currentLength, 0f, maximumLength);
        }

        public bool TrySpend(float amount)
        {
            if (amount <= 0f || amount > currentLength)
            {
                return false;
            }

            currentLength -= amount;
            return true;
        }

        public void Refund(float amount)
        {
            currentLength = Mathf.Clamp(currentLength + Mathf.Max(0f, amount), 0f, maximumLength);
        }

        public void ResetToMaximum()
        {
            currentLength = maximumLength;
        }

        public void RestoreCurrentLength(float amount)
        {
            currentLength = Mathf.Clamp(amount, 0f, maximumLength);
        }
    }
}
