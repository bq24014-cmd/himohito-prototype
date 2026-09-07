using System;

namespace HimoHito
{
    /// <summary>Samples a visual-only celebration inside the owner's existing clear delay.</summary>
    internal sealed class GoalPoseState
    {
        private float duration;
        private float elapsed;
        public bool IsVisible { get; private set; }
        public bool FacingLeft { get; private set; }
        public bool IsComplete => IsVisible && elapsed >= duration;
        public int FrameIndex => IsVisible ? Math.Min(5, (int)(elapsed / duration * 6f)) : -1;

        public bool Begin(float revealDuration, bool facingLeft)
        {
            // Collision callbacks must not restart an already running finish.
            if (IsVisible || revealDuration <= 0f ||
                float.IsNaN(revealDuration) || float.IsInfinity(revealDuration)) return false;
            duration = revealDuration;
            elapsed = 0f;
            FacingLeft = facingLeft;
            IsVisible = true;
            return true;
        }

        public void Sample(float elapsedSinceGoal, bool eligible)
        {
            if (!eligible) { Reset(); return; }
            if (!IsVisible || elapsedSinceGoal < 0f ||
                float.IsNaN(elapsedSinceGoal) || float.IsInfinity(elapsedSinceGoal)) return;
            // Absolute unscaled elapsed time matches WaitForSecondsRealtime.
            // Never lengthen the clear delay to finish frames after a render hitch.
            elapsed = Math.Min(duration, Math.Max(elapsed, elapsedSinceGoal));
        }

        public void Reset()
        {
            IsVisible = false;
            elapsed = duration = 0f;
        }
    }
}
