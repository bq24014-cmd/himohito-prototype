using System.Globalization;
using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Keeps the IMGUI screens on the same type family specified by the
    /// presentation instead of relying on an OS-installed Japanese font.
    /// </summary>
    public static class HimoHitoGuiTheme
    {
        private const string RegularFontPath =
            "Fonts/MPlusRounded1c-Regular";
        private const string BoldFontPath =
            "Fonts/MPlusRounded1c-Bold";

        private static Font regularFont;
        private static Font boldFont;

        public static void ApplyToSkin(GUISkin skin)
        {
            if (skin == null || RegularFont == null)
            {
                return;
            }

            skin.font = RegularFont;
        }

        public static void ApplyToStyles(params GUIStyle[] styles)
        {
            foreach (GUIStyle style in styles)
            {
                ApplyToStyle(style);
            }
        }

        public static string FormatRopeValue(float value)
        {
            return value
                .ToString("0.0", CultureInfo.InvariantCulture)
                .PadLeft(4);
        }

        private static Font RegularFont => regularFont != null
            ? regularFont
            : regularFont = Resources.Load<Font>(RegularFontPath);

        private static Font BoldFont => boldFont != null
            ? boldFont
            : boldFont = Resources.Load<Font>(BoldFontPath);

        private static void ApplyToStyle(GUIStyle style)
        {
            if (style == null)
            {
                return;
            }

            bool isBold = style.fontStyle == FontStyle.Bold ||
                style.fontStyle == FontStyle.BoldAndItalic;
            bool isItalic = style.fontStyle == FontStyle.Italic ||
                style.fontStyle == FontStyle.BoldAndItalic;

            Font selectedFont = isBold ? BoldFont : RegularFont;
            if (selectedFont == null)
            {
                return;
            }

            style.font = selectedFont;
            style.fontStyle = isItalic ? FontStyle.Italic : FontStyle.Normal;
        }
    }
}
