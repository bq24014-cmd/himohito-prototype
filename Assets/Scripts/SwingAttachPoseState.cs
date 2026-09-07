using System;

namespace HimoHito
{
    /// <summary>One visual handoff per airborne attachment; never delays the joint.</summary>
    internal sealed class SwingAttachPoseState
    {
        private const float Duration = 0.28f;
        private float elapsed;
        private int lastSequence;
        private bool wasEligible;

        public bool IsActive { get; private set; }
        public float Blend
        {
            get
            {
                float t = Math.Min(1f, elapsed / Duration);
                return t * t * (3f - 2f * t);
            }
        }

        // True only on a new handoff: the caller captures its current artwork.
        public bool Advance(int attachmentSequence, bool eligible, float deltaTime)
        {
            if (deltaTime <= 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime)) return false;
            if (!eligible) { Reset(); return false; }
            if (!wasEligible || lastSequence != attachmentSequence)
            {
                wasEligible = IsActive = true;
                lastSequence = attachmentSequence;
                elapsed = 0f;
                return true;
            }
            if (IsActive)
            {
                elapsed = Math.Min(Duration, elapsed + Math.Min(deltaTime, 0.05f));
                if (elapsed >= Duration) IsActive = false;
            }
            return false;
        }

        public void Reset()
        {
            IsActive = wasEligible = false;
            elapsed = 0f;
        }
    }
}
