using System;

namespace HimoHito
{
    /// <summary>A single, interruptible balance reaction after stopping at an edge.</summary>
    internal sealed class EdgeBalancePoseState
    {
        private static readonly double[] Durations = { .12, .14, .18, .16, .14 };
        private int candidateSide;
        private double stableElapsed;
        private double frameElapsed;
        private double missingElapsed;
        private double returnElapsed;
        private int frame;

        public bool IsVisible { get; private set; }
        public bool IsReturning { get; private set; }
        public int EdgeSide { get; private set; }
        public int FrameIndex => !IsVisible ? -1 : IsReturning ? (returnElapsed < .1 ? 4 : 5) : frame;

        public void Reset()
        {
            IsVisible = IsReturning = false;
            EdgeSide = candidateSide = frame = 0;
            stableElapsed = frameElapsed = missingElapsed = returnElapsed = 0;
        }

        public void Advance(int openSide, float deltaTime, bool eligible)
        {
            if (deltaTime <= 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime)) return;
            if (!eligible) { Reset(); return; }
            if (openSide != -1 && openSide != 1) openSide = 0;
            double step = Math.Min(deltaTime, .05);
            if (!IsVisible)
            {
                if (openSide == 0) { Reset(); return; }
                if (openSide != candidateSide) { candidateSide = openSide; stableElapsed = 0; }
                stableElapsed += step;
                if (stableElapsed < .18) return;
                IsVisible = true;
                EdgeSide = openSide;
                frame = 0;
                return;
            }
            missingElapsed = openSide == EdgeSide ? 0 : missingElapsed + step;
            if (missingElapsed >= .1) IsReturning = true;
            if (IsReturning)
            {
                returnElapsed += step;
                if (returnElapsed >= .22) Reset();
                return;
            }
            // No perpetual wobble at an edge. Hold the last stable drawing.
            if (frame >= Durations.Length) return;
            frameElapsed += step;
            if (frameElapsed >= Durations[frame]) { frame++; frameElapsed = 0; }
        }

        public static int ClassifySupport(bool center, bool left, bool right)
        {
            if (!center || left == right) return 0;
            return left ? 1 : -1;
        }
    }
}
