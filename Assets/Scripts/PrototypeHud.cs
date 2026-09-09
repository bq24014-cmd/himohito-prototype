using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// A dependency-free HUD for the prototype. OnGUI keeps setup small.
    /// </summary>
    public sealed class PrototypeHud : MonoBehaviour
    {
        [SerializeField] private RopeResource ropeResource;
        [SerializeField] private RopeController ropeController;
        [SerializeField] private Rigidbody2D playerBody;
        [SerializeField] private PrototypeRunController runController;
        [SerializeField] private RopePlatformBuilder platformBuilder;

        private Camera mainCamera;
        private Collider2D playerCollider;
        private Texture2D titleBackground;
        private Texture2D endingBackground;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle lengthStyle;
        private GUIStyle startTitleStyle;
        private GUIStyle startObjectiveStyle;
        private GUIStyle startImportantStyle;
        private GUIStyle startControlStyle;
        private GUIStyle startPromptStyle;
        private GUIStyle weavePromptTitleStyle;
        private GUIStyle weavePromptBodyStyle;
        private GUIStyle weavePromptActionStyle;
        private GUIStyle clearTitleStyle;
        private GUIStyle clearBodyStyle;
        private GUIStyle clearPromptStyle;

        private void Awake()
        {
            if (ropeResource == null)
            {
                ropeResource = FindFirstObjectByType<RopeResource>();
            }

            if (ropeController == null)
            {
                ropeController = FindFirstObjectByType<RopeController>();
            }

            if (playerBody == null && ropeController != null)
            {
                playerBody = ropeController.GetComponent<Rigidbody2D>();
            }

            mainCamera = Camera.main;
            if (playerBody != null)
            {
                playerCollider = playerBody.GetComponent<Collider2D>();
            }

            if (runController == null)
            {
                runController = FindFirstObjectByType<PrototypeRunController>();
            }

            if (platformBuilder == null)
            {
                platformBuilder = FindFirstObjectByType<RopePlatformBuilder>();
            }

            titleBackground = Resources.Load<Texture2D>(
                "Art/HimoHitoTitleBackground-v1");
            endingBackground = Resources.Load<Texture2D>(
                "Art/HimoHitoEndingBackground-v1");
        }

        private void OnGUI()
        {
            if (MainStagePreview.IsActive) return;

            HimoHitoGuiTheme.ApplyToSkin(GUI.skin);

            if (platformBuilder == null)
            {
                platformBuilder = FindFirstObjectByType<RopePlatformBuilder>();
            }

            EnsureStyles();

            if (runController != null &&
                runController.Outcome == PrototypeRunController.RunOutcome.WaitingToStart)
            {
                DrawStartScreen();
                return;
            }

            if (runController != null &&
                runController.Outcome == PrototypeRunController.RunOutcome.Clear)
            {
                DrawClearScreen();
                return;
            }

            if (runController != null &&
                runController.Outcome == PrototypeRunController.RunOutcome.Failed)
            {
                if (!runController.IsFallUnravelling) DrawFailureScreen();
                return;
            }

            GUILayout.BeginArea(new Rect(22f, 18f, Mathf.Min(360f, Screen.width - 44f), 174f), GUI.skin.box);
            GUILayout.Label("チュートリアル　" + $"第{(runController != null ? runController.CurrentTutorialSection : 1)}区間 / {PrototypeRunController.TutorialSectionCount}", titleStyle);
            if (ropeResource != null)
            {
                GUILayout.Label(
                    $"ヒモ残量  {HimoHitoGuiTheme.FormatRopeValue(ropeResource.CurrentLength)} / " +
                    HimoHitoGuiTheme.FormatRopeValue(ropeResource.MaximumLength), lengthStyle);
                RopeResourceGauge.Draw(ropeResource);
            }
            if (ropeController != null)
            {
                GUILayout.Label($"次に使う長さ  {ropeController.SelectedRopeLength} / " +
                    $"{ropeController.MaximumSelectableRopeLength}" +
                    (ropeController.IsAttached ? "　接続中" : ""), bodyStyle);
            }
            GUILayout.Label("Tab　操作説明", bodyStyle);
            GUILayout.EndArea();

            DrawWeavePrompt();
        }

        private void DrawFailureScreen()
        {
            bool fell = runController.FailureReason ==
                PrototypeRunController.RunFailureReason.Fell;
            HimoHitoFailureScreen.Draw(
                fell ? "足を踏み外しました" : "ヒモが尽きました",
                fell
                    ? "少しだけ休んで、同じ区間からもう一度。"
                    : "使う長さを選び直せば、まだ先へ進めます。",
                "R　この区間から再挑戦");
        }

        private void DrawSectionFourGuide()
        {
            if (runController == null ||
                runController.CurrentTutorialSection != 4 ||
                platformBuilder == null)
            {
                return;
            }

            if (TutorialSectionFourSetup.HasMergedPlatform(platformBuilder))
            {
                GUILayout.Label(
                    "1本になった足場を歩き、梁の下をくぐってゴール",
                    bodyStyle);
                return;
            }

            bool hasLeft =
                TutorialSectionFourSetup.HasLeftPlatform(platformBuilder);
            bool hasRight =
                TutorialSectionFourSetup.HasRightPlatform(platformBuilder);
            if (hasLeft && hasRight)
            {
                GUILayout.Label(
                    "2本のままでは梁に当たる。中央の青フックを狙ってF",
                    bodyStyle);
            }
            else if (hasLeft)
            {
                GUILayout.Label(
                    "中央まで歩き、右の緑フックへ長さ6でE → Q",
                    bodyStyle);
            }
            else
            {
                GUILayout.Label(
                    "中央の青フックへ長さ6でE → Q",
                    bodyStyle);
            }
        }

        private void DrawWeavePrompt()
        {
            if (platformBuilder == null ||
                ropeController == null ||
                !ropeController.IsAttached ||
                platformBuilder.CurrentPlatformCost <= 0f)
            {
                return;
            }

            if (playerBody == null || mainCamera == null)
            {
                return;
            }

            Vector3 promptWorldPosition = playerBody.position;
            if (playerCollider != null)
            {
                promptWorldPosition.y = playerCollider.bounds.max.y;
            }

            Vector3 screenPoint = mainCamera.WorldToScreenPoint(promptWorldPosition);
            if (screenPoint.z <= 0f)
            {
                return;
            }

            float panelWidth = Mathf.Min(360f, Screen.width - 24f);
            const float panelHeight = 148f;
            const float screenMargin = 12f;
            float panelX = Mathf.Clamp(
                screenPoint.x - panelWidth * 0.5f,
                screenMargin,
                Screen.width - panelWidth - screenMargin);
            float panelY = Mathf.Clamp(
                Screen.height - screenPoint.y - panelHeight - 18f,
                screenMargin,
                Screen.height - panelHeight - screenMargin);
            Rect panel = new Rect(
                panelX,
                panelY,
                panelWidth,
                panelHeight);

            Color previousColor = GUI.color;
            GUI.color = new Color(0.32f, 0.16f, 0.48f, 0.96f);
            GUI.Box(panel, GUIContent.none);
            GUI.color = previousColor;

            Rect contentArea = new Rect(
                panel.x + 10f,
                panel.y + 7f,
                panel.width - 20f,
                panel.height - 14f);
            GUILayout.BeginArea(contentArea);
            GUILayout.Label("この照準方向へヒモ足場を作る", weavePromptTitleStyle);
            GUILayout.Label(
                $"永久に使う長さ  {platformBuilder.CurrentPlatformCost:0.0}",
                weavePromptBodyStyle);

            bool lacksReserve = ropeResource != null &&
                ropeResource.CurrentLength - platformBuilder.CurrentPlatformCost <
                platformBuilder.MinimumRopeReserve - 0.001f;
            string actionMessage;
            if (platformBuilder.CanBuildCurrentPlatform)
            {
                actionMessage = "Q：この方向へヒモ足場を作る";
            }
            else if (lacksReserve)
            {
                actionMessage =
                    $"足場にできません\nヒモを{platformBuilder.MinimumRopeReserve:0.0}以上残してください";
            }
            else if (runController != null &&
                runController.CurrentTutorialSection == 4)
            {
                actionMessage =
                    "この接続先では足場を作れません\nEで外し、右の緑フックを狙ってください";
            }
            else if (ropeController.ActiveHookPoint != null &&
                ropeController.ActiveHookPoint.TryGetComponent(
                    out RopePlatformAnchor _))
            {
                actionMessage =
                    "ヒモが対岸フックまで届いていません\nEで外し、Wで長くしてください";
            }
            else
            {
                actionMessage = "この接続先では足場を作れません";
            }
            GUILayout.Label(actionMessage, weavePromptActionStyle);
            GUILayout.EndArea();
        }

        private void DrawStartScreen()
        {
            Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
            Color previousColor = GUI.color;
            if (titleBackground != null)
            {
                GUI.DrawTexture(
                    screenRect,
                    titleBackground,
                    ScaleMode.ScaleAndCrop,
                    true);
            }
            else
            {
                GUI.color = new Color(0.04f, 0.05f, 0.11f, 1f);
                GUI.Box(screenRect, GUIContent.none);
            }

            float shadeWidth = Mathf.Min(Screen.width * 0.58f, 1040f);
            GUI.color = new Color(0.025f, 0.025f, 0.075f, 0.72f);
            GUI.Box(new Rect(0f, 0f, shadeWidth, Screen.height), GUIContent.none);
            GUI.color = previousColor;

            float panelWidth = Mathf.Min(760f, shadeWidth - 72f);
            float panelHeight = Mathf.Min(650f, Screen.height - 72f);
            Rect panel = new Rect(
                Mathf.Max(42f, shadeWidth * 0.09f),
                (Screen.height - panelHeight) * 0.5f,
                panelWidth,
                panelHeight);

            GUILayout.BeginArea(panel);
            GUILayout.FlexibleSpace();
            GUILayout.Label("HIMOHITO", startObjectiveStyle);
            GUILayout.Label("ヒモヒト", startTitleStyle);
            GUILayout.Space(20f);
            GUILayout.Label(
                "自分の体であるヒモを伸ばして進み、\n必要な場所では足場として編むアクションパズル",
                startControlStyle);
            GUILayout.Space(54f);
            Rect startButtonRow = GUILayoutUtility.GetRect(
                1f,
                58f,
                GUILayout.ExpandWidth(true),
                GUILayout.Height(58f));
            Rect startButton = new Rect(
                startButtonRow.x,
                startButtonRow.y,
                Mathf.Min(390f, startButtonRow.width),
                startButtonRow.height);
            HimoHitoUiParts.DrawWoodButtonLabel(
                startButton,
                "Enter　はじめる",
                startImportantStyle);
            GUILayout.Label("Tab　操作説明", startPromptStyle);
            GUILayout.Label("Esc　終了", startPromptStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label(
                "制作：bq24014-cmd　／　Unity 6.3 LTS　／　使用素材はREADME参照",
                bodyStyle);
            GUILayout.EndArea();
        }

        private void DrawClearScreen()
        {
            Color previousColor = GUI.color;
            Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
            if (endingBackground != null)
            {
                GUI.DrawTexture(
                    screenRect,
                    endingBackground,
                    ScaleMode.ScaleAndCrop,
                    true);
            }
            else
            {
                GUI.color = new Color(0.035f, 0.04f, 0.085f, 0.99f);
                GUI.Box(screenRect, GUIContent.none);
            }
            GUI.color = previousColor;

            GUILayout.BeginArea(new Rect(
                0f,
                Screen.height * 0.1f,
                Screen.width,
                Screen.height * 0.34f));
            GUILayout.Label("チュートリアルクリア", clearTitleStyle);
            GUILayout.Space(18f);
            GUILayout.Label("ヒモを掛ける・長さを選ぶ・足場にする・まとめるを習得しました", clearBodyStyle);
            GUILayout.Space(42f);
            Rect clearButtonRow = GUILayoutUtility.GetRect(
                1f,
                62f,
                GUILayout.ExpandWidth(true),
                GUILayout.Height(62f));
            Rect clearButton = new Rect(
                clearButtonRow.x + Mathf.Max(0f, (clearButtonRow.width - 480f) * 0.5f),
                clearButtonRow.y,
                Mathf.Min(480f, clearButtonRow.width),
                clearButtonRow.height);
            HimoHitoUiParts.DrawWoodButtonLabel(
                clearButton,
                "Enter　本編ステージへ",
                clearPromptStyle);
            GUILayout.EndArea();
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.35f, 1f, 0.78f) }
            };
            lengthStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 26,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.82f, 0.28f) }
            };
            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                normal = { textColor = Color.white }
            };
            startTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 64,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.36f, 0.56f) }
            };
            startObjectiveStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.72f, 0.79f, 1f) }
            };
            startImportantStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.82f, 0.28f) }
            };
            startControlStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 21,
                wordWrap = true,
                normal = { textColor = Color.white }
            };
            startPromptStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.65f, 0.72f, 1f) }
            };
            weavePromptTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = new Color(0.86f, 0.72f, 1f) }
            };
            weavePromptBodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                wordWrap = true,
                normal = { textColor = Color.white }
            };
            weavePromptActionStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = new Color(1f, 0.86f, 0.34f) }
            };
            clearTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 58,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.36f, 0.56f) }
            };
            clearBodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
                normal = { textColor = new Color(1f, 0.86f, 0.66f) }
            };
            clearPromptStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 25,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.82f, 0.28f) }
            };

            HimoHitoGuiTheme.ApplyToStyles(
                titleStyle,
                lengthStyle,
                bodyStyle,
                startTitleStyle,
                startObjectiveStyle,
                startImportantStyle,
                startControlStyle,
                startPromptStyle,
                weavePromptTitleStyle,
                weavePromptBodyStyle,
                weavePromptActionStyle,
                clearTitleStyle,
                clearBodyStyle,
                clearPromptStyle);
        }
    }
}
