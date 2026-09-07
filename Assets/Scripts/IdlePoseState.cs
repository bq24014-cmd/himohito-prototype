using System;

namespace HimoHito
{
    /// <summary>Occasional idle gesture; presentation only, always interruptible.</summary>
    internal sealed class IdlePoseState
    {
        private const double WaitBeforeIdle = 1.6;
        private const double RestDuration = 4.2;
        private static readonly int[] Frames = { 5, 0, 1, 2, 1, 0, 3, 4, 5 };
        private static readonly double[] Durations = { .16, .20, .18, .10, .12, .18, .16, .16, .20 };
        private const double GestureDuration = 1.46;
        private double quietElapsed;
        private double cycleElapsed;

        public bool IsVisible { get; private set; }
        public int FrameIndex
        {
            get
            {
                if (!IsVisible) return -1;
                double end = 0;
                for (int i = 0; i < Frames.Length; i++)
                {
                    end += Durations[i];
                    if (cycleElapsed < end) return Frames[i];
                }
                return 5; // Hold neutral between gestures, never blink continuously.
            }
        }

        public void Reset()
        {
            IsVisible = false;
            quietElapsed = cycleElapsed = 0;
        }

        public void Advance(float deltaTime, bool eligible)
        {
            // Pausing freezes the current drawing, even if controls are disabled
            // for an overlay. The first live frame rechecks eligibility.
            if (deltaTime <= 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime)) return;
            if (!eligible) { Reset(); return; }
            // A hitch must not run an entire gesture while no frame is drawn.
            double step = Math.Min(deltaTime, .1);
            if (!IsVisible)
            {
                quietElapsed += step;
                if (quietElapsed >= WaitBeforeIdle) IsVisible = true;
                return;
            }
            cycleElapsed = (cycleElapsed + step) % (GestureDuration + RestDuration);
        }
    }
}
