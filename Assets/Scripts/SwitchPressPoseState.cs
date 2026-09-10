using System;

namespace HimoHito
{
    /// <summary>One interruptible six-drawing gesture after an actual recovery-switch activation.</summary>
    public sealed class SwitchPressPoseState
    {
        private float elapsed;
        private bool firstDrawing;
        public bool IsActive { get; private set; }
        public bool FacingLeft { get; private set; }
        public int FrameIndex => IsActive ? Math.Min(5, (int)(elapsed / .08f)) : -1;

        public void Begin(bool eligible, bool facingLeft)
        {
            Reset();
            if (!eligible) return;
            FacingLeft = facingLeft;
            IsActive = firstDrawing = true;
        }

        public void Advance(float delta, bool eligible)
        {
            if (delta <= 0f || float.IsNaN(delta) || float.IsInfinity(delta)) return;
            if (!eligible) { Reset(); return; }
            if (!IsActive) return;
            if (firstDrawing) { firstDrawing = false; return; }
            elapsed += Math.Min(delta, .05f);
            if (elapsed >= .48f) Reset();
        }

        public void Reset() { elapsed = 0f; IsActive = firstDrawing = false; }
    }
}
