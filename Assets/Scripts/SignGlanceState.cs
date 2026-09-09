using UnityEngine;

namespace HimoHito
{
    /// <summary>A single, interruptible glance per approach; never writes the rope aim.</summary>
    internal sealed class SignGlanceState
    {
        private int targetId;
        private float waiting, elapsed;
        private bool consumed;
        public float Blend { get; private set; }

        public void Advance(int nearbyTarget, bool eligible, bool input, float dt)
        {
            if (dt <= 0f) return;
            if (nearbyTarget != targetId)
            {
                targetId = nearbyTarget;
                waiting = elapsed = Blend = 0f;
                consumed = false;
            }
            if (nearbyTarget == 0 || !eligible || input)
            {
                waiting = elapsed = Blend = 0f;
                return;
            }
            if (consumed && elapsed <= 0f) return;
            if (!consumed)
            {
                waiting += dt;
                if (waiting < 1.6f) return;
                consumed = true;
            }
            elapsed += dt;
            Blend = Mathf.SmoothStep(0f, 1f, elapsed / .22f) *
                (1f - Mathf.SmoothStep(0f, 1f, (elapsed - .85f) / .3f));
            if (elapsed >= 1.15f) { elapsed = Blend = 0f; }
        }
    }
}
