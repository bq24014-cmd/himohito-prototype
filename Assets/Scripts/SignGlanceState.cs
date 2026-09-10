using UnityEngine;

namespace HimoHito
{
    /// <summary>Explicit reading gaze, retained until gameplay resumes; never writes rope aim.</summary>
    internal sealed class SignGlanceState
    {
        private float elapsed;
        public bool IsActive { get; private set; }
        public bool IsReading { get; private set; }
        public Vector2 Direction { get; private set; }
        public float Blend => IsActive ? 1f : 0f;
        public float Tilt { get; private set; }

        public void Begin(Vector2 direction)
        {
            if (float.IsNaN(direction.x) || float.IsNaN(direction.y) ||
                float.IsInfinity(direction.x) || float.IsInfinity(direction.y) ||
                direction.sqrMagnitude < .0001f) return;
            Direction = direction.normalized;
            IsActive = IsReading = true;
            elapsed = Tilt = 0f;
        }

        public void EndReading() { IsReading = false; elapsed = Tilt = 0f; }

        public void Reset() { IsActive = IsReading = false; elapsed = Tilt = 0f; }

        public void Advance(bool eligible, bool gameplayInput, float dt)
        {
            if (dt <= 0f || float.IsNaN(dt) || float.IsInfinity(dt) || !IsActive) return;
            if (!eligible || gameplayInput)
            {
                Reset();
                return;
            }
            if (IsReading) return;
            elapsed = Mathf.Min(2f, elapsed + Mathf.Min(dt, .1f));
            // Straighten the head after reading, but do not turn back on a timer.
            Tilt = Mathf.SmoothStep(0f, 1f, (elapsed - .15f) / .45f) *
                (1f - Mathf.SmoothStep(0f, 1f, (elapsed - 1f) / .45f));
        }
    }
}
