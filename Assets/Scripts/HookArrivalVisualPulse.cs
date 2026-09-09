using UnityEngine;

namespace HimoHito
{
    /// <summary>Brief recoil on a Hook's visual child only; never moves its physics anchor.</summary>
    [DisallowMultipleComponent]
    public sealed class HookArrivalVisualPulse : MonoBehaviour
    {
        private const float Duration = 0.42f;
        private const float PullDistance = 0.06f;
        private const float TiltDegrees = 6f;
        private const float ScaleExpansion = 0.04f;
        private Vector3 restingPosition;
        private Vector3 restingScale;
        private Quaternion restingRotation;
        private Vector3 pullDirection;
        private float tilt;
        private float elapsed;
        private bool playing;

        public void Play(Vector2 ropeOrigin)
        {
            Restore();
            // This component is for an art-only child, never a gameplay object.
            if (GetComponent<HookPoint>() != null ||
                GetComponentInChildren<Collider2D>() != null ||
                GetComponentInChildren<Rigidbody2D>() != null)
                return;
            restingPosition = transform.localPosition;
            restingScale = transform.localScale;
            restingRotation = transform.localRotation;
            Vector2 direction = ropeOrigin - (Vector2)transform.position;
            pullDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.down;
            Vector3 parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
            float xScale = Mathf.Max(0.01f, Mathf.Abs(parentScale.x));
            float yScale = Mathf.Max(0.01f, Mathf.Abs(parentScale.y));
            // Wide/thin Hook parents amplify child rotation into unwanted sprite shear.
            float tiltCompensation = Mathf.Min(xScale, yScale) / Mathf.Max(xScale, yScale);
            Vector3 localPull = transform.parent != null
                ? transform.parent.InverseTransformDirection(pullDirection) : pullDirection;
            tilt = -localPull.x * TiltDegrees * tiltCompensation * Mathf.Sign(parentScale.x * parentScale.y);
            elapsed = 0f;
            playing = true;
        }

        private void LateUpdate()
        {
            Advance(Time.deltaTime);
        }

        private void Advance(float deltaTime)
        {
            if (!playing || deltaTime <= 0f) return;
            elapsed += deltaTime;
            float t = Mathf.Clamp01(elapsed / Duration);
            if (t >= 1f)
            {
                Restore();
                return;
            }

            // One soft tug followed by a small recoil. No continuous shaking or tracking.
            float pull;
            if (t < 0.28f)
                pull = Mathf.SmoothStep(0f, 1f, t / 0.28f);
            else
            {
                float release = (t - 0.28f) / 0.72f;
                pull = Mathf.Cos(release * Mathf.PI) * (1f - Mathf.SmoothStep(0f, 1f, release));
            }
            Vector3 worldOffset = pullDirection * (pull * PullDistance);
            Vector3 localOffset = transform.parent != null
                ? transform.parent.InverseTransformVector(worldOffset)
                : worldOffset;
            transform.localPosition = restingPosition + localOffset;
            transform.localRotation = restingRotation * Quaternion.Euler(0f, 0f, tilt * pull);
            float scale = 1f + ScaleExpansion * Mathf.Max(0f, pull);
            transform.localScale = new Vector3(restingScale.x * scale,
                restingScale.y * scale, restingScale.z);
        }

        private void OnDisable() => Restore();
        private void OnDestroy() => Restore();

        private void Restore()
        {
            if (!playing) return;
            transform.localPosition = restingPosition;
            transform.localScale = restingScale;
            transform.localRotation = restingRotation;
            playing = false;
        }
    }
}
