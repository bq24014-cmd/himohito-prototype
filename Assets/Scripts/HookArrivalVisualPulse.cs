using UnityEngine;

namespace HimoHito
{
    /// <summary>Brief recoil on a Hook's visual child only; never moves its physics anchor.</summary>
    [DisallowMultipleComponent]
    public sealed class HookArrivalVisualPulse : MonoBehaviour
    {
        private const float Duration = 0.28f;
        private const float ScaleExpansion = 0.14f;
        private Vector3 restingPosition;
        private Vector3 restingScale;
        private float elapsed;
        private bool playing;

        public void Play()
        {
            Restore();
            restingPosition = transform.localPosition;
            restingScale = transform.localScale;
            elapsed = 0f;
            playing = true;
        }

        private void LateUpdate()
        {
            if (!playing) return;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Duration);
            if (t >= 1f)
            {
                Restore();
                return;
            }

            // A small world-space displacement keeps differently scaled rings consistent.
            float offset = Mathf.Sin(t * Mathf.PI * 3f) * (1f - t) * 0.045f;
            Vector3 worldOffset = Vector3.down * offset;
            Vector3 localOffset = transform.parent != null
                ? transform.parent.InverseTransformVector(worldOffset)
                : worldOffset;
            transform.localPosition = restingPosition + localOffset;
            // Smooth rise and fall, with zero slope at both ends. Scale only the visual child.
            float swell = Mathf.Sin(t * Mathf.PI);
            float scale = 1f + ScaleExpansion * swell * swell;
            transform.localScale = new Vector3(restingScale.x * scale,
                restingScale.y * scale, restingScale.z);
        }

        private void OnDisable() => Restore();

        private void Restore()
        {
            if (!playing) return;
            transform.localPosition = restingPosition;
            transform.localScale = restingScale;
            playing = false;
        }
    }
}
