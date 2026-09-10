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
        private static readonly Color SpentColor = new Color(1f, 0.48f, 0.65f, 1f);
        private const float TrailDuration = 0.4f;
        private const float LabelDuration = 0.95f;
        private const float LowRopeThreshold = 0.20f;
        private const float KnotPulsePeriod = 2.4f;
        private static RopeResource spendingResource;
        private static float spendStartedAt, startRatio, remainingAfterSpend, capacityAtSpend, displayedCost;
        private static string costText;
        private static GUIStyle costStyle;

        private readonly struct SpendFrame
        {
            public readonly float KnotRatio, TrailAlpha, LabelAlpha;
            public SpendFrame(float knotRatio, float trailAlpha = 0f, float labelAlpha = 0f)
            { KnotRatio = knotRatio; TrailAlpha = trailAlpha; LabelAlpha = labelAlpha; }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetFlash()
        {
            spendingResource = null;
            costText = null;
            costStyle = null;
        }

        // Explicit success notification: a restore or temporary attachment is
        // not a permanent spend, even when it changes the displayed amount.
        public static void NotifyPlatformBuilt(RopeResource resource, float spentLength)
        {
            if (resource == null || spentLength <= 0f || float.IsNaN(spentLength) ||
                float.IsInfinity(spentLength)) return;
            float now = Time.time;
            float before = resource.CurrentLength + spentLength;
            float from = Mathf.Clamp01(before / resource.MaximumLength);
            float cost = spentLength;
            // Rapid successful builds continue from the visible marker and combine their labels.
            if (spendingResource == resource && now >= spendStartedAt &&
                now - spendStartedAt < LabelDuration &&
                Mathf.Approximately(capacityAtSpend, resource.MaximumLength) &&
                Mathf.Approximately(remainingAfterSpend, before))
            {
                from = Mathf.Max(from, AnimatedRatio(now));
                cost += displayedCost;
            }
            spendingResource = resource;
            spendStartedAt = now;
            startRatio = from;
            remainingAfterSpend = resource.CurrentLength;
            capacityAtSpend = resource.MaximumLength;
            displayedCost = cost;
            costText = "−" + cost.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture);
        }

        public static void ClearFlash(RopeResource resource)
        {
            // Keep the existing restore/reset hook; it now clears the whole spend presentation.
            if (spendingResource == resource) spendingResource = null;
        }

        private static float AnimatedRatio(float now) => Mathf.Lerp(startRatio,
            remainingAfterSpend / capacityAtSpend,
            Mathf.SmoothStep(0f, 1f, (now - spendStartedAt) / TrailDuration));

        private static SpendFrame Sample(RopeResource resource, float now)
        {
            float ratio = Mathf.Clamp01(resource.NormalizedLength);
            if (spendingResource != resource) return new SpendFrame(ratio);
            float elapsed = now - spendStartedAt;
            if (elapsed < 0f || elapsed >= LabelDuration ||
                !Mathf.Approximately(resource.CurrentLength, remainingAfterSpend) ||
                !Mathf.Approximately(resource.MaximumLength, capacityAtSpend))
            {
                spendingResource = null;
                return new SpendFrame(ratio);
            }
            return new SpendFrame(AnimatedRatio(now),
                .55f * (1f - Mathf.SmoothStep(0f, 1f, elapsed / TrailDuration)),
                1f - Mathf.SmoothStep(0f, 1f, (elapsed - .65f) / .3f));
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
            DrawAtRect(ropeResource, outerRect, Time.time);
        }

        private static void DrawAtRect(RopeResource ropeResource, Rect slot, float now)
        {
            // A permanent gutter avoids a moving gauge width or overlap with neighbouring HUD rows.
            float gutter = Mathf.Min(48f, slot.width * .22f);
            Rect outerRect = new Rect(slot.x, slot.y, Mathf.Max(0f, slot.width - gutter), slot.height);
            Rect trackRect = new Rect(
                outerRect.x + 20f,
                outerRect.y + 9f,
                Mathf.Max(0f, outerRect.width - 40f),
                12f);

            float remainingRatio = Mathf.Clamp01(ropeResource.NormalizedLength);
            SpendFrame frame = Sample(ropeResource, now);
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
                GUI.color = EvaluateFillColor(remainingRatio);
                GUI.DrawTexture(fillRect, Texture2D.whiteTexture);

            }

            if (frame.TrailAlpha > 0f && frame.KnotRatio > remainingRatio)
            {
                Rect trail = trackRect;
                trail.x += trackRect.width * remainingRatio;
                trail.width = trackRect.width * (frame.KnotRatio - remainingRatio);
                GUI.color = new Color(SpentColor.r, SpentColor.g, SpentColor.b,
                    previousColor.a * frame.TrailAlpha);
                GUI.DrawTexture(trail, Texture2D.whiteTexture);
            }
            GUI.color = previousColor;

            if (HimoHitoUiParts.IsAvailable)
            {
                HimoHitoUiParts.DrawGaugeFrame(outerRect);

                float knotSize = 19f;
                float knotX = Mathf.Lerp(
                    trackRect.x,
                    trackRect.xMax,
                    frame.KnotRatio) - knotSize * 0.5f;
                // Low-rope breathing is retained while the marker slides.
                Color knotColor = previousColor;
                knotColor.a *= EvaluateKnotAlpha(remainingRatio, now);
                GUI.color = knotColor;
                HimoHitoUiParts.DrawKnot(new Rect(
                    knotX,
                    outerRect.y + (outerRect.height - knotSize) * 0.5f,
                    knotSize,
                    knotSize));
            }
            if (frame.LabelAlpha > 0f)
            {
                if (costStyle == null)
                {
                    costStyle = new GUIStyle(GUI.skin.label) {
                        fontSize = 15, fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter, clipping = TextClipping.Clip,
                        padding = new RectOffset(), margin = new RectOffset(), wordWrap = false
                    };
                    costStyle.normal.textColor = SpentColor;
                    HimoHitoGuiTheme.ApplyToStyles(costStyle);
                }
                GUI.color = new Color(previousColor.r, previousColor.g, previousColor.b,
                    previousColor.a * frame.LabelAlpha);
                GUI.Label(new Rect(outerRect.xMax + 2f, slot.y + 3f,
                    Mathf.Max(0f, gutter - 2f), 24f), costText, costStyle);
            }
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
