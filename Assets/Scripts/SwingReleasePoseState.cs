using System;

namespace HimoHito
{
    /// <summary>Visual follow-through after a manual airborne release. No physics writes.</summary>
    internal sealed class SwingReleasePoseState
    {
        private const float SettleDuration = 0.24f;
        private const float HandoffDuration = 0.18f;
        private const float FrameHold = 0.08f;
        private float elapsed;
        private int startFrame;
        private bool firstDrawing;

        public bool IsActive { get; private set; }
        public bool FacingLeft { get; private set; }
        public bool UseAirbornePose => IsActive && elapsed >= SettleDuration;
        public float SettleBlend
        {
            get
            {
                float t = Math.Max(0f, Math.Min(1f, elapsed / SettleDuration));
                return t * t * (3f - 2f * t);
            }
        }
        public float HandoffBlend
        {
            get
            {
                float t = Math.Max(0f, Math.Min(1f, (elapsed - SettleDuration) / HandoffDuration));
                return t * t * (3f - 2f * t);
            }
        }
        public int FrameIndex
        {
            get
            {
                int neutral = startFrame <= 2 ? 2 : 3;
                int steps = Math.Min(Math.Abs(neutral - startFrame), (int)(elapsed / FrameHold));
                return startFrame + Math.Sign(neutral - startFrame) * steps;
            }
        }

        public void Begin(int displayedFrame, bool facingLeft, bool eligible)
        {
            Reset();
            if (!eligible || displayedFrame < 0 || displayedFrame >= 6) return;
            startFrame = displayedFrame;
            FacingLeft = facingLeft;
            IsActive = firstDrawing = true;
        }

        public void Advance(float deltaTime, bool eligible)
        {
            if (deltaTime <= 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime)) return;
            if (!eligible) { Reset(); return; }
            if (!IsActive) return;
            // Retain the exact last swing drawing for the first release frame.
            if (firstDrawing) { firstDrawing = false; return; }
            elapsed += Math.Min(deltaTime, 0.05f);
            if (elapsed >= SettleDuration + HandoffDuration) IsActive = false;
        }

        public void Reset()
        {
            IsActive = firstDrawing = false;
            elapsed = 0f;
            startFrame = 2;
        }
    }
}
