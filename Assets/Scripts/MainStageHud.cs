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
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle ropeStyle;
        private GUIStyle accentStyle;
        private GUIStyle clearTitleStyle;
        private GUIStyle clearBodyStyle;
        private int displayedSection;
        private float sectionTitleUntil;

        private void Awake()
        {
            ropeResource = FindFirstObjectByType<RopeResource>();
            ropeController = FindFirstObjectByType<RopeController>();
            platformBuilder = FindFirstObjectByType<RopePlatformBuilder>();
            preview = FindFirstObjectByType<MainStagePreview>();
            respawn = FindFirstObjectByType<MainStageRespawnOnFall>();
            goal = FindFirstObjectByType<MainStageGoalZone>();
            if (ropeController != null)
            {
                playerBody = ropeController.GetComponent<Rigidbody2D>();
            }
            displayedSection = respawn != null ? respawn.CurrentSection : 1;
            sectionTitleUntil = Time.unscaledTime + 1.2f;
        }

        private void OnGUI()
        {
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
                    $"ヒモ残量  {ropeResource.CurrentLength:0.0} / {ropeResource.MaximumLength:0.0}",
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
            if (respawn != null && respawn.CurrentSection >= 9)
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
            bool hasCorrectLength = ropeController != null &&
                Mathf.Abs(
                    ropeController.ActiveRopeLength -
                    MainStageSectionFourSetup.BridgeRopeLength) <= 0.05f;
            GUILayout.Label(
                isGreenAnchor
                    ? hasCorrectLength
                        ? "緑フックへ接続中：Qで2つの緑フックを結ぶ"
                        : "Eで解除し、W/Sで長さ5にして緑フックへ再接続"
                    : "左右の緑フックを確認し、長さ5で右の緑フックへ接続",
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
                    out RopePlatformAnchor anchor) &&
                anchor.RequiredRopeLength ==
                    MainStageSectionFiveSetup.BridgeRopeLength;
            GUILayout.Label(
                isShadowAnchor
                    ? "緑フックへ接続中：Qで遮光する足場を作る（消費6）"
                    : "左棚へ登り、長さ6で右の緑フックへ接続",
                accentStyle);
        }

        private void DrawSectionSixGuide()
        {
            HookPoint activeHook = ropeController != null
                ? ropeController.ActiveHookPoint
                : null;
            if (activeHook != null &&
                activeHook.name ==
                    MainStageSectionSixSetup.UpperBridgeEndHookName)
            {
                GUILayout.Label(
                    "上ルート：Qで長さ5を消費し、安全な橋を作る",
                    accentStyle);
                return;
            }

            if (activeHook != null &&
                (activeHook.name == MainStageSectionSixSetup.LowerHookAName ||
                 activeHook.name == MainStageSectionSixSetup.LowerHookBName ||
                 activeHook.name == MainStageSectionSixSetup.LowerHookCName))
            {
                GUILayout.Label(
                    "下ルート：Eで離し、次の青フックへつなぐ（消費0）",
                    accentStyle);
                return;
            }

            GUILayout.Label(
                "上：長さ5の緑フックで安全橋　下：長さ4で青フック3連続",
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
            GUI.color = new Color(0.035f, 0.04f, 0.085f, 0.99f);
            GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none);
            GUI.color = previous;
            GUILayout.BeginArea(new Rect(0f, 0f, Screen.width, Screen.height));
            GUILayout.FlexibleSpace();
            GUILayout.Label("MAIN STAGE CLEAR", clearTitleStyle);
            if (ropeResource != null)
            {
                GUILayout.Label($"残ったヒモ　{ropeResource.CurrentLength:0.0}", clearBodyStyle);
            }
            if (platformBuilder != null)
            {
                GUILayout.Label($"編んだ足場　{platformBuilder.GeneratedPlatformCount}", clearBodyStyle);
            }
            if (respawn != null)
            {
                GUILayout.Label($"補充　{respawn.RefillCount} 回", clearBodyStyle);
            }
            GUILayout.Label("R　本編を最初から再挑戦", clearBodyStyle);
            GUILayout.FlexibleSpace();
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
                normal = { textColor = new Color(0.33f, 1f, 0.76f) }
            };
            clearBodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
                normal = { textColor = Color.white }
            };
        }
    }
}
