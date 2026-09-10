using System;

namespace HimoHito
{
    /// <summary>One short, pausable weave per selected stage; independent of IMGUI event count.</summary>
    internal sealed class StageSelectionWeave
    {
        public const float TraceDuration = .32f;
        public const float TotalDuration = .4f;
        private string selectedStageId;
        private bool observed, hasTimestamp, wasActive;
        private double elapsed, lastTimestamp;

        public float TraceProgress => Smooth(elapsed / TraceDuration);
        public float KnotProgress => Smooth((elapsed - TraceDuration) / (TotalDuration - TraceDuration));

        public void Observe(string stageId, double now, bool active)
        {
            if (!observed || !string.Equals(selectedStageId, stageId, StringComparison.Ordinal))
            {
                selectedStageId = stageId;
                observed = true;
                elapsed = 0;
                hasTimestamp = false;
            }

            bool validTime = !double.IsNaN(now) && !double.IsInfinity(now);
            if (validTime)
            {
                // Both ends must be active: closing help never catches up its hidden time.
                if (hasTimestamp && active && wasActive && now > lastTimestamp)
                    elapsed = Math.Min(TotalDuration, elapsed + (now - lastTimestamp));

                // Repeated or out-of-order IMGUI samples must not count an interval twice.
                lastTimestamp = hasTimestamp ? Math.Max(lastTimestamp, now) : now;
            }
            hasTimestamp = validTime;
            wasActive = active;
        }

        private static float Smooth(double value)
        {
            float t = (float)Math.Max(0, Math.Min(1, value));
            return t * t * (3f - 2f * t);
        }
    }
}
