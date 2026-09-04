using UnityEngine;

namespace HimoHito
{
    /// <summary>Compact HUD for the ten-section stage defined by the 0829 manual.</summary>
    public sealed class MainStageHud : MonoBehaviour
    {
        private RopeResource ropeResource;
        private RopeController ropeController;
        private Rigidbody2D playerBody;
        private RopePlatformBuilder platformBuilder;
        private MainStagePreview preview;
        private MainStageRespawnOnFall respawn;
        private MainStageGoalZone goal;
        private Texture2D endingBackground;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle ropeStyle;
        private GUIStyle accentStyle;
        private GUIStyle clearTitleStyle;
        private GUIStyle clearBodyStyle;
        private int displayedSection;
        private float sectionTitleUntil;
        private float stageStartedAt;
        private float clearElapsedSeconds = -1f;

        private void Awake()
        {
            ropeResource = FindFirstObjectByType<RopeResource>();
            ropeController = FindFirstObjectByType<RopeController>();
            platformBuilder = FindFirstObjectByType<RopePlatformBuilder>();
            preview = FindFirstObjectByType<MainStagePreview>();
            respawn = FindFirstObjectByType<MainStageRespawnOnFall>();
            goal = FindFirstObjectByType<MainStageGoalZone>();
            endingBackground = Resources.Load<Texture2D>(
                "Art/HimoHitoEndingBackground-v1");
            if (ropeController != null)
            {
                playerBody = ropeController.GetComponent<Rigidbody2D>();
            }
            displayedSection = respawn != null ? respawn.CurrentSection : 1;
            sectionTitleUntil = Time.unscaledTime + 1.2f;
            stageStartedAt = Time.time;
        }

        private void Start()
        {
            goal ??= FindFirstObjectByType<MainStageGoalZone>();
        }

        private void OnGUI()
        {
            HimoHitoGuiTheme.ApplyToSkin(GUI.skin);
            goal ??= FindFirstObjectByType<MainStageGoalZone>();
            EnsureStyles();
            if (goal != null && goal.IsClear)
            {
                DrawClearScreen();
                return;
            }

            UpdateSectionTitle();
            DrawSectionTitle();

            GUILayout.BeginArea(new Rect(22f, 18f, 520f, 440f), GUI.skin.box);
            GUILayout.Label("ヒモヒト / 本編ステージ", titleStyle);
            GUILayout.Label($"第{(respawn != null ? respawn.CurrentSection : 1)}区間", bodyStyle);

            if (ropeResource != null)
            {
                GUILayout.Label(
                    $"ヒモ残量  {HimoHitoGuiTheme.FormatRopeValue(ropeResource.CurrentLength)} / " +
                    HimoHitoGuiTheme.FormatRopeValue(ropeResource.MaximumLength),
                    ropeStyle);
                RopeResourceGauge.Draw(ropeResource);
            }

            if (ropeController != null)
            {
                GUILayout.Label(
                    $"次に使う長さ  {ropeController.SelectedRopeLength} / " +
                    $"{ropeController.MaximumSelectableRopeLength}",
                    bodyStyle);
                GUILayout.Label("W：長くする　S：短くする", bodyStyle);
                GUILayout.Label(
                    ropeController.IsAttached
                        ? "接続中：Eで解除 / Qでこの長さを永久消費して足場化"
                        : "矢印で狙い、Eで接続",
                    accentStyle);
            }

            if (preview != null && preview.IsPreviewing)
            {
                GUILayout.Label("ステージ全体を確認中", accentStyle);
            }
            else if (respawn != null && respawn.IsRopeExhausted)
            {
                GUILayout.Label("ヒモが不足しています — Rで区間の最初へ", accentStyle);
            }
            else if (respawn != null &&
                     respawn.CurrentSection == 4 &&
                     platformBuilder != null)
            {
                DrawSectionFourGuide();
            }
            else if (respawn != null &&
                     respawn.CurrentSection == 5 &&
                     platformBuilder != null)
            {
                DrawSectionFiveGuide();
            }
            else if (respawn != null &&
                     respawn.CurrentSection == 6 &&
                     platformBuilder != null)
            {
                DrawSectionSixGuide();
            }
            else if (respawn != null &&
                     respawn.CurrentSection == 7 &&
                     platformBuilder != null)
            {
                DrawSectionSevenGuide();
            }
            else if (respawn != null &&
                     respawn.CurrentSection == 9 &&
                     platformBuilder != null)
            {
                DrawSectionNineGuide();
            }
            else if (respawn != null &&
                     respawn.CurrentSection == 10 &&
                     platformBuilder != null)
            {
                DrawSectionTenGuide();
            }
            else
            {
                GUILayout.Label("おもちゃ箱のゴールを目指す", accentStyle);
            }

            if (playerBody != null)
            {
                GUILayout.Label($"速度  {playerBody.linearVelocity.magnitude:0.0}", bodyStyle);
            }
            GUILayout.Label("A / D：移動　Space：ジャンプ", bodyStyle);
            GUILayout.Label("矢印：照準　E：接続／解除", bodyStyle);
            if (platformBuilder != null && platformBuilder.IsPlatformBuildingUnlocked)
            {
                GUILayout.Label("Q：接続中のヒモを足場化", bodyStyle);
            }
            if (respawn != null && respawn.CurrentSection == 9)
            {
                GUILayout.Label("F：2本が集まるHookを外して1本にまとめる", bodyStyle);
            }
            GUILayout.Label("R：現在の区間から再挑戦", bodyStyle);
            GUILayout.EndArea();
        }

        private void DrawSectionFourGuide()
        {
            if (platformBuilder.GeneratedPlatformCount > 0)
            {
                GUILayout.Label(
                    "橋を渡り、柱の上から青フックへ長さ9で接続",
                    accentStyle);
                return;
            }

            bool isGreenAnchor = ropeController != null &&
                ropeController.ActiveHookPoint != null &&
                ropeController.ActiveHookPoint.TryGetComponent(
                    out RopePlatformAnchor _);
            bool canBuild = platformBuilder.CanBuildCurrentPlatform;
            GUILayout.Label(
                isGreenAnchor
                    ? canBuild
                        ? $"緑フックへ接続中：Qで足場化（消費{platformBuilder.CurrentPlatformCost:0}）"
                        : "ヒモが対岸フックまで届いていない：Eで外してWで長くする"
                    : "左右の緑フックを確認し、対岸まで届く長さで接続",
                accentStyle);
        }

        private void DrawSectionFiveGuide()
        {
            GameObject lightObject = GameObject.Find(
                MainStageSectionFiveSetup.LightSpotName);
            bool isBlocked = lightObject != null &&
                lightObject.TryGetComponent(
                    out PlatformOccludedLightHazard hazard) &&
                hazard.IsBlocked;
            if (isBlocked)
            {
                GUILayout.Label(
                    "遮光成功：右棚から青フックへ長さ7で接続",
                    accentStyle);
                return;
            }

            bool isShadowAnchor = ropeController != null &&
                ropeController.ActiveHookPoint != null &&
                ropeController.ActiveHookPoint.TryGetComponent(
                    out RopePlatformAnchor _);
            GUILayout.Label(
                isShadowAnchor
                    ? platformBuilder.CanBuildCurrentPlatform
                        ? $"緑フックへ接続中：Qで遮光足場を作る（消費{platformBuilder.CurrentPlatformCost:0}）"
                        : "ヒモが対岸フックまで届いていない：Eで外してWで長くする"
                    : "左棚へ登り、対岸まで届く長さで右の緑フックへ接続",
                accentStyle);
        }

        private void DrawSectionSixGuide()
        {
            if (ropeController != null &&
                ropeController.IsAirChainReconnectOpen)
            {
                GUILayout.Label(
                    "空中再接続受付中：射程内でEを押すと次の青フックへ接続",
                    accentStyle);
                return;
            }

            HookPoint activeHook = ropeController != null
                ? ropeController.ActiveHookPoint
                : null;
            if (activeHook != null &&
                activeHook.name ==
                    MainStageSectionSixSetup.UpperBridgeEndHookName)
            {
                GUILayout.Label(
                    platformBuilder.CanBuildCurrentPlatform
                        ? $"上ルート：Qで長さ{platformBuilder.CurrentPlatformCost:0}を消費し、安全橋を作る"
                        : "上ルート：ヒモが対岸フックまで届いていない",
                    accentStyle);
                return;
            }

            if (activeHook != null &&
                (activeHook.name == MainStageSectionSixSetup.LowerHookAName ||
                 activeHook.name == MainStageSectionSixSetup.LowerHookBName ||
                 activeHook.name == MainStageSectionSixSetup.LowerHookCName))
            {
                GUILayout.Label(
                    "下ルート：Eで離し、射程内でもう一度E（次の青フックを自動選択）",
                    accentStyle);
                return;
            }

            GUILayout.Label(
                $"上：長さ{MainStageSectionSixSetup.UpperBridgeRopeLength}の緑フックで安全橋　" +
                $"下：長さ{MainStageSectionSixSetup.LowerRouteRopeLength}で青フック3連続",
                accentStyle);
        }

        private void DrawSectionSevenGuide()
        {
            bool hasFirstBridge = platformBuilder.HasPlatformBetween(
                MainStageSectionSevenSetup.BridgeStartHookPosition,
                MainStageSectionSevenSetup.BridgeEndHookPosition);
            if (!hasFirstBridge)
            {
                bool isBridgeAnchor = ropeController != null &&
                    ropeController.ActiveHookPoint != null &&
                    ropeController.ActiveHookPoint.name ==
                        MainStageSectionSevenSetup.BridgeEndHookName;
                GUILayout.Label(
                    isBridgeAnchor
                        ? platformBuilder.CanBuildCurrentPlatform
                            ? $"緑フックへ接続中：Qで長さ{platformBuilder.CurrentPlatformCost:0}の足場を作る"
                            : "ヒモが対岸フックまで届いていない"
                        : "対岸まで届く長さで中段の緑フックへ接続し、Qで足場化",
                    accentStyle);
                return;
            }

            if (playerBody != null &&
                playerBody.position.x <
                    MainStageSectionSevenSetup.UpperShelfPosition.x - 0.5f)
            {
                GUILayout.Label(
                    "作った足場で中段へ渡り、中段から上段へジャンプ",
                    accentStyle);
                return;
            }

            GUILayout.Label(
                "上段から長さ5で青フックへ接続し、右の床へ振り渡る",
                accentStyle);
        }

        private void DrawSectionNineGuide()
        {
            if (MainStageSectionNineSetup.HasMergedPlatform(platformBuilder))
            {
                GUILayout.Label(
                    "統合成功：深くなった1本の足場で梁の下を通る",
                    accentStyle);
                return;
            }

            bool hasLeft =
                MainStageSectionNineSetup.HasLeftPlatform(platformBuilder);
            bool hasRight =
                MainStageSectionNineSetup.HasRightPlatform(platformBuilder);
            if (hasLeft && hasRight)
            {
                GUILayout.Label(
                    "2本のままでは梁に当たる：中央の青フックを狙ってFで外す",
                    accentStyle);
                return;
            }

            HookPoint activeHook = ropeController != null
                ? ropeController.ActiveHookPoint
                : null;
            GUILayout.Label(
                activeHook != null &&
                (activeHook.name == MainStageSectionNineSetup.CenterHookName ||
                 activeHook.name == MainStageSectionNineSetup.RightAnchorName)
                    ? platformBuilder.CanBuildCurrentPlatform
                        ? $"接続中：Qで長さ{platformBuilder.CurrentPlatformCost:0}の足場を作る"
                        : "ヒモが対岸フックまで届いていない"
                    : hasLeft
                        ? "中央から右の緑フックへ届く長さで接続し、Qで2本目を作る"
                        : "左岸から中央の青フックへ届く長さで接続し、Qで1本目を作る",
                accentStyle);
        }

        private void DrawSectionTenGuide()
        {
            if (MainStageSectionTenSetup.HasFinalPlatform(platformBuilder))
            {
                GUILayout.Label(
                    "最後の橋を渡り、おもちゃ箱へ進む",
                    accentStyle);
                return;
            }

            bool isFinalAnchor = ropeController != null &&
                ropeController.ActiveHookPoint != null &&
                ropeController.ActiveHookPoint.name ==
                    MainStageSectionTenSetup.RightAnchorName;
            GUILayout.Label(
                isFinalAnchor
                    ? platformBuilder.CanBuildCurrentPlatform
                        ? $"接続中：Qで長さ{platformBuilder.CurrentPlatformCost:0}を最後の橋にする"
                        : "ヒモが対岸フックまで届いていない"
                    : "頭上にフックはない。対岸まで届く長さで右岸の緑フックへ接続",
                accentStyle);
        }

        private void UpdateSectionTitle()
        {
            int current = respawn != null ? respawn.CurrentSection : 1;
            if (current == displayedSection)
            {
                return;
            }

            displayedSection = current;
            sectionTitleUntil = Time.unscaledTime + 1.2f;
        }

        private void DrawSectionTitle()
        {
            if (Time.unscaledTime > sectionTitleUntil)
            {
                return;
            }

            GUI.Label(
                new Rect(0f, Screen.height * 0.17f, Screen.width, 72f),
                $"第{displayedSection}区間",
                clearTitleStyle);
        }

        private void DrawClearScreen()
        {
            Color previous = GUI.color;
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
            GUI.color = previous;

            if (clearElapsedSeconds < 0f)
            {
                clearElapsedSeconds = Mathf.Max(0f, Time.time - stageStartedAt);
            }

            GUILayout.BeginArea(new Rect(
                0f,
                Screen.height * 0.075f,
                Screen.width,
                Screen.height * 0.42f));
            GUILayout.Label("CLEAR", clearTitleStyle);
            if (ropeResource != null)
            {
                GUILayout.Label(
                    $"残ったヒモ　{ropeResource.CurrentLength:0.0} / " +
                    $"{ropeResource.MaximumLength:0.0}",
                    clearBodyStyle);
            }
            if (platformBuilder != null)
            {
                GUILayout.Label(
                    $"編んだ足場　{platformBuilder.GeneratedPlatformCount} 本",
                    clearBodyStyle);
            }
            if (respawn != null)
            {
                GUILayout.Label($"補充　{respawn.RefillCount} 回", clearBodyStyle);
            }
            GUILayout.Label(
                $"かかった時間　{FormatElapsed(clearElapsedSeconds)}",
                clearBodyStyle);
            GUILayout.Space(16f);
            GUILayout.Label("R　本編を最初から再挑戦", clearBodyStyle);
            GUILayout.EndArea();
        }

        private static string FormatElapsed(float seconds)
        {
            int totalSeconds = Mathf.FloorToInt(seconds);
            int minutes = totalSeconds / 60;
            int remainingSeconds = totalSeconds % 60;
            return $"{minutes}分{remainingSeconds:00}秒";
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
                normal = { textColor = new Color(0.33f, 1f, 0.76f) }
            };
            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                normal = { textColor = Color.white }
            };
            ropeStyle = new GUIStyle(bodyStyle)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.78f, 0.2f) }
            };
            accentStyle = new GUIStyle(bodyStyle)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = new Color(0.44f, 0.92f, 1f) }
            };
            clearTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 48,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.36f, 0.56f) }
            };
            clearBodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
                normal = { textColor = new Color(1f, 0.86f, 0.66f) }
            };

            HimoHitoGuiTheme.ApplyToStyles(
                titleStyle,
                bodyStyle,
                ropeStyle,
                accentStyle,
                clearTitleStyle,
                clearBodyStyle);
        }
    }
}
