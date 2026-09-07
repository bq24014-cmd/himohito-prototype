using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Draws the remaining rope as a color-changing bar without storing its own value.
    /// </summary>
    public static class RopeResourceGauge
    {
        private static readonly Color TrackColor = new Color(0.08f, 0.09f, 0.14f, 1f);
        private static readonly Color LowColor = new Color(1f, 0.28f, 0.24f, 1f);
        private static readonly Color MiddleColor = new Color(1f, 0.78f, 0.2f, 1f);
        private static readonly Color HighColor = new Color(0.25f, 0.9f, 0.58f, 1f);
        private static readonly Color FlashColor = new Color(1f, 0.98f, 0.94f, 1f);
        private const float FlashDuration = 0.22f;
        private const float LowRopeThreshold = 0.20f;
        private const float KnotPulsePeriod = 2.4f;
        private static RopeResource flashingResource;
        private static float flashStartedAt;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetFlash()
        {
            flashingResource = null;
            flashStartedAt = 0f;
        }

        // Explicit success notification: a restore or temporary attachment is
        // not a permanent spend, even when it changes the displayed amount.
        public static void NotifyPlatformBuilt(RopeResource resource)
        {
            flashingResource = resource;
            flashStartedAt = Time.time;
        }

        public static void ClearFlash(RopeResource resource)
        {
            if (flashingResource == resource) flashingResource = null;
        }

        private static float FlashStrength(RopeResource resource)
        {
            if (flashingResource != resource) return 0f;
            float elapsed = Time.time - flashStartedAt;
            if (elapsed < 0f || elapsed >= FlashDuration)
            {
                flashingResource = null;
                return 0f;
            }
            return 0.8f * (1f - Mathf.SmoothStep(0f, 1f, elapsed / FlashDuration));
        }

        public static void Draw(RopeResource ropeResource)
        {
            if (ropeResource == null)
            {
                return;
            }

            Rect outerRect = GUILayoutUtility.GetRect(
                1f,
                30f,
                GUILayout.ExpandWidth(true),
                GUILayout.Height(30f));
            Rect trackRect = new Rect(
                outerRect.x + 20f,
                outerRect.y + 9f,
                Mathf.Max(0f, outerRect.width - 40f),
                12f);

            float remainingRatio = Mathf.Clamp01(ropeResource.NormalizedLength);
            Color previousColor = GUI.color;

            if (!HimoHitoUiParts.IsAvailable)
            {
                GUI.color = Color.black;
                GUI.DrawTexture(outerRect, Texture2D.whiteTexture);
            }

            GUI.color = TrackColor;
            GUI.DrawTexture(trackRect, Texture2D.whiteTexture);

            if (remainingRatio > 0f)
            {
                Rect fillRect = trackRect;
                fillRect.width *= remainingRatio;
                GUI.color = Color.Lerp(EvaluateFillColor(remainingRatio),
                    FlashColor, FlashStrength(ropeResource));
                GUI.DrawTexture(fillRect, Texture2D.whiteTexture);

            }

            GUI.color = previousColor;

            if (!HimoHitoUiParts.IsAvailable)
            {
                return;
            }

            HimoHitoUiParts.DrawGaugeFrame(outerRect);

            float knotSize = 19f;
            float knotX = Mathf.Lerp(
                trackRect.x,
                trackRect.xMax,
                remainingRatio) - knotSize * 0.5f;
            // Only the marker breathes; retain its position and never make it disappear.
            Color knotColor = previousColor;
            knotColor.a *= EvaluateKnotAlpha(remainingRatio, Time.time);
            GUI.color = knotColor;
            HimoHitoUiParts.DrawKnot(new Rect(
                knotX,
                outerRect.y + (outerRect.height - knotSize) * 0.5f,
                knotSize,
                knotSize));
            GUI.color = previousColor;
        }

        private static float EvaluateKnotAlpha(float remainingRatio, float time)
        {
            // Ease in over 20% -> 15%, avoiding a sudden visual jump at the threshold.
            float strength = Mathf.SmoothStep(0f, 1f,
                (LowRopeThreshold - remainingRatio) / 0.05f);
            float wave = 0.5f - 0.5f * Mathf.Cos(time * 2f * Mathf.PI / KnotPulsePeriod);
            return 1f - 0.4f * strength * wave;
        }

        private static Color EvaluateFillColor(float remainingRatio)
        {
            if (remainingRatio < 0.5f)
            {
                return Color.Lerp(LowColor, MiddleColor, remainingRatio * 2f);
            }

            return Color.Lerp(MiddleColor, HighColor, (remainingRatio - 0.5f) * 2f);
        }
    }
}
