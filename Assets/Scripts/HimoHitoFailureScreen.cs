using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Shared full-screen presentation for a failed tutorial or main-stage run.
    /// The illustration contains no baked text so the Japanese copy stays editable.
    /// </summary>
    public static class HimoHitoFailureScreen
    {
        private const string BackgroundResourcePath =
            "Art/HimoHitoFailureBackground-v1";

        private static Texture2D background;
        private static GUIStyle eyebrowStyle;
        private static GUIStyle titleStyle;
        private static GUIStyle bodyStyle;
        private static GUIStyle promptStyle;

        public static void Draw(
            string title,
            string message,
            string retryPrompt)
        {
            EnsureResources();

            Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
            Color previous = GUI.color;
            if (background != null)
            {
                GUI.DrawTexture(
                    screenRect,
                    background,
                    ScaleMode.ScaleAndCrop,
                    true);
            }
            else
            {
                GUI.color = new Color(0.035f, 0.04f, 0.085f, 1f);
                GUI.Box(screenRect, GUIContent.none);
            }

            float shadeWidth = Mathf.Min(Screen.width * 0.62f, 1120f);
            GUI.color = new Color(0.025f, 0.02f, 0.065f, 0.58f);
            GUI.Box(new Rect(0f, 0f, shadeWidth, Screen.height), GUIContent.none);
            GUI.color = previous;

            float panelWidth = Mathf.Min(780f, shadeWidth - 72f);
            float panelHeight = Mathf.Min(520f, Screen.height * 0.62f);
            GUILayout.BeginArea(new Rect(
                Mathf.Max(42f, shadeWidth * 0.08f),
                Screen.height * 0.14f,
                panelWidth,
                panelHeight));
            GUILayout.Label("もう一度、編み直そう", eyebrowStyle);
            GUILayout.Space(12f);
            GUILayout.Label(title, titleStyle);
            GUILayout.Space(22f);
            GUILayout.Label(message, bodyStyle);
            GUILayout.FlexibleSpace();
            Rect retryButtonRow = GUILayoutUtility.GetRect(
                1f,
                58f,
                GUILayout.ExpandWidth(true),
                GUILayout.Height(58f));
            Rect retryButton = new Rect(
                retryButtonRow.x,
                retryButtonRow.y,
                Mathf.Min(470f, retryButtonRow.width),
                retryButtonRow.height);
            HimoHitoUiParts.DrawWoodButtonLabel(
                retryButton,
                retryPrompt,
                promptStyle);
            GUILayout.Label("Esc　ステージ選択へ", bodyStyle);
            GUILayout.EndArea();
        }

        private static void EnsureResources()
        {
            if (background == null)
            {
                background = Resources.Load<Texture2D>(
                    BackgroundResourcePath);
            }

            if (titleStyle != null)
            {
                return;
            }

            eyebrowStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.70f, 0.78f, 1f) }
            };
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 54,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = new Color(1f, 0.36f, 0.56f) }
            };
            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 22,
                wordWrap = true,
                normal = { textColor = new Color(1f, 0.88f, 0.74f) }
            };
            promptStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 26,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.82f, 0.28f) }
            };

            HimoHitoGuiTheme.ApplyToStyles(
                eyebrowStyle,
                titleStyle,
                bodyStyle,
                promptStyle);
        }
    }
}
