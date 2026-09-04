using UnityEngine;

namespace HimoHito
{
    /// <summary>Tab opens the compact control sheet; Esc pauses the run.</summary>
    public sealed class StageOverlayControls : MonoBehaviour
    {
        private bool showHelp;
        private bool isPaused;
        private PrototypeRunController runController;
        private Texture2D helpBackground;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle helpTitleStyle;
        private GUIStyle helpBodyStyle;
        private GUIStyle helpKeyStyle;

        public bool IsHelpVisible => showHelp;

        private void Awake()
        {
            runController = FindFirstObjectByType<PrototypeRunController>();
            helpBackground = Resources.Load<Texture2D>(
                "Art/HimoHitoControlsBackground-v1");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                showHelp = !showHelp;
                ApplyPauseState();
            }

            bool isOnTitleScreen = runController != null &&
                runController.Outcome == PrototypeRunController.RunOutcome.WaitingToStart;
            if (isOnTitleScreen)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                isPaused = !isPaused;
                ApplyPauseState();
            }
        }

        private void OnDisable()
        {
            if (isPaused || showHelp)
            {
                Time.timeScale = 1f;
                isPaused = false;
                showHelp = false;
            }
        }

        private void OnGUI()
        {
            // Lower IMGUI depth values are drawn in front. Keep the help sheet
            // above the title HUD regardless of component creation order.
            GUI.depth = -1000;
            HimoHitoGuiTheme.ApplyToSkin(GUI.skin);
            EnsureStyles();
            bool isOnTitleScreen = runController != null &&
                runController.Outcome == PrototypeRunController.RunOutcome.WaitingToStart;
            if (!isOnTitleScreen)
            {
                GUI.Label(new Rect(Screen.width - 215f, 12f, 200f, 28f),
                    "Tab：操作確認　Esc：ポーズ", bodyStyle);
            }

            if (!showHelp && !isPaused)
            {
                return;
            }

            if (showHelp)
            {
                DrawHelpScreen();
                return;
            }

            DrawPauseScreen();
        }

        private void DrawHelpScreen()
        {
            Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
            Color previous = GUI.color;
            if (helpBackground != null)
            {
                GUI.DrawTexture(
                    screenRect,
                    helpBackground,
                    ScaleMode.ScaleAndCrop,
                    true);
            }
            else
            {
                GUI.color = new Color(0.94f, 0.85f, 0.70f, 1f);
                GUI.Box(screenRect, GUIContent.none);
            }
            GUI.color = previous;

            float width = Mathf.Min(Screen.width * 0.56f, 980f);
            float height = Mathf.Min(Screen.height * 0.65f, 700f);
            GUILayout.BeginArea(new Rect(
                (Screen.width - width) * 0.5f,
                Screen.height * 0.145f,
                width,
                height));
            GUILayout.Label("操作方法", helpTitleStyle);
            GUILayout.Space(18f);
            GUILayout.Label(
                "ヒモを掛けて振り、必要な場所では足場として残します",
                helpBodyStyle);
            GUILayout.Space(28f);

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(width * 0.48f));
            DrawHelpRow("A / D", "移動・振り子を加速");
            DrawHelpRow("Space", "ジャンプ");
            DrawHelpRow("W / S", "次に使う長さを変更");
            DrawHelpRow("矢印キー", "照準方向を変更");
            GUILayout.EndVertical();
            GUILayout.Space(width * 0.04f);
            GUILayout.BeginVertical(GUILayout.Width(width * 0.48f));
            DrawHelpRow("E", "ヒモを接続・解除");
            DrawHelpRow("Q", "接続中のヒモを足場化");
            DrawHelpRow("F", "中央Hookを外してまとめる");
            DrawHelpRow("R", "現在の区間から再挑戦");
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.Label("Tab　閉じる", helpKeyStyle);
            GUILayout.EndArea();
        }

        private void DrawHelpRow(string key, string description)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(key, helpKeyStyle, GUILayout.Width(112f));
            GUILayout.Label(description, helpBodyStyle);
            GUILayout.EndHorizontal();
            GUILayout.Space(12f);
        }

        private void DrawPauseScreen()
        {
            Color previous = GUI.color;
            GUI.color = new Color(0.03f, 0.035f, 0.075f, 0.94f);
            GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none);
            GUI.color = previous;

            float width = Mathf.Min(720f, Screen.width - 40f);
            float height = 430f;
            GUILayout.BeginArea(new Rect(
                (Screen.width - width) * 0.5f,
                (Screen.height - height) * 0.5f,
                width,
                height), GUI.skin.box);
            GUILayout.Label("PAUSE", titleStyle);
            GUILayout.Space(18f);
            GUILayout.Label("A / D：移動・振り子を加速　　Space：ジャンプ", bodyStyle);
            GUILayout.Label("W / S：次に使う長さを増減", bodyStyle);
            GUILayout.Label("矢印キー：照準　　E：接続／解除", bodyStyle);
            GUILayout.Label("Q：接続中のヒモを足場にする（この時だけ永久消費）", bodyStyle);
            GUILayout.Label("F：2本が集まるHookを外して1本にまとめる", bodyStyle);
            GUILayout.Label("R：現在の区間から再挑戦", bodyStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label("Escで再開", titleStyle);
            GUILayout.EndArea();
        }

        private void ApplyPauseState()
        {
            Time.timeScale = isPaused || showHelp ? 0f : 1f;
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.35f, 1f, 0.78f) }
            };
            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 17,
                wordWrap = true,
                normal = { textColor = Color.white }
            };

            Color ink = new Color(0.11f, 0.08f, 0.20f);
            helpTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 34,
                fontStyle = FontStyle.Bold,
                normal = { textColor = ink }
            };
            helpBodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 18,
                wordWrap = true,
                normal = { textColor = ink }
            };
            helpKeyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.78f, 0.20f, 0.38f) }
            };

            HimoHitoGuiTheme.ApplyToStyles(
                titleStyle,
                bodyStyle,
                helpTitleStyle,
                helpBodyStyle,
                helpKeyStyle);
        }
    }
}
