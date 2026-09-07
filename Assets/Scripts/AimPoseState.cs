using System;

namespace HimoHito
{
    /// <summary>Grounded gaze only; reads the aim without changing any controls.</summary>
    internal sealed class AimPoseState
    {
        private static readonly double[] Boundaries = { -55.0, -17.5, 20.0, 60.0 };
        private const double AngleHysteresis = 5.0;
        private double frameElapsed;
        private double facingElapsed;
        private double filteredAngle;
        private int level = 2;

        public bool IsVisible { get; private set; }
        public bool FacingLeft { get; private set; }
        public bool IsLookingForward => IsVisible && level == 2;
        public int FrameIndex => IsVisible ? level : -1;

        public void Reset()
        {
            IsVisible = false;
            FacingLeft = false;
            frameElapsed = facingElapsed = filteredAngle = 0;
            level = 2;
        }

        public void Advance(float x, float y, bool currentFacingLeft,
            float deltaTime, bool eligible)
        {
            // Overlays freeze this drawing; eligibility is rechecked on resume.
            if (!Finite(deltaTime) || deltaTime <= 0f) return;
            if (!eligible) { Reset(); return; }
            if (!Finite(x) || !Finite(y) || (double)x * x + (double)y * y < .0001) return;
            if (!IsVisible)
            {
                // The persistent guide direction, not a key press, owns the
                // gaze. Also follow it after spawning or coming to a stop.
                IsVisible = true;
                FacingLeft = currentFacingLeft;
                facingElapsed = .12;
            }

            double step = Math.Min(deltaTime, .05);
            frameElapsed += step;
            facingElapsed += step;
            double angle = Math.Atan2(y, Math.Abs(x)) * 180 / Math.PI;
            filteredAngle += (angle - filteredAngle) * (1 - Math.Exp(-step / .08));

            // Keep the last facing near vertical. Tiny x sign changes at the
            // overhead aim must not mirror the whole character back and forth.
            double horizontal = x / Math.Sqrt((double)x * x + (double)y * y);
            if (Math.Abs(horizontal) >= .18 && facingElapsed >= .12)
            {
                bool left = horizontal < 0;
                if (left != FacingLeft) { FacingLeft = left; facingElapsed = 0; }
            }

            if (frameElapsed >= .08)
            {
                int next = level;
                if (level < 4 && filteredAngle > Boundaries[level] + AngleHysteresis) next++;
                else if (level > 0 && filteredAngle < Boundaries[level - 1] - AngleHysteresis) next--;
                if (next != level) { level = next; frameElapsed = 0; }
            }
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
