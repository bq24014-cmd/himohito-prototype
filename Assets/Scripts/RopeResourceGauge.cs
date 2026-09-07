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
                GUI.color = EvaluateFillColor(remainingRatio);
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
            HimoHitoUiParts.DrawKnot(new Rect(
                knotX,
                outerRect.y + (outerRect.height - knotSize) * 0.5f,
                knotSize,
                knotSize));
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
