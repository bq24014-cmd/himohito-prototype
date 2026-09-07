using System;

namespace HimoHito
{
    /// <summary>One visual-only gesture per successful, grounded bridge build.</summary>
    internal sealed class WeavePoseState
    {
        private const float FrameDuration = 0.06f;
        private const int FrameCount = 6;
        private float elapsed;
        private bool firstDrawing;

        public bool IsActive { get; private set; }
        public bool FacingLeft { get; private set; }
        public int FrameIndex => IsActive ? Math.Min(FrameCount - 1, (int)(elapsed / FrameDuration)) : -1;

        public void Begin(bool eligible, bool facingLeft)
        {
            Reset();
            if (!eligible) return;
            FacingLeft = facingLeft;
            IsActive = firstDrawing = true;
        }

        public void Advance(float deltaTime, bool eligible)
        {
            if (deltaTime <= 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime)) return;
            if (!eligible) { Reset(); return; }
            if (!IsActive) return;
            // Q runs in Update; keep the neutral entry for its first LateUpdate.
            if (firstDrawing) { firstDrawing = false; return; }
            elapsed += Math.Min(deltaTime, 0.05f);
            if (elapsed >= FrameDuration * FrameCount) Reset();
        }

        public void Reset()
        {
            IsActive = firstDrawing = false;
            elapsed = 0f;
        }
    }
}
