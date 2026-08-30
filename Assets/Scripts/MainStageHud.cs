using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Displays only the information needed to test the current main-stage section.
    /// </summary>
    public sealed class MainStageHud : MonoBehaviour
    {
        private RopeResource ropeResource;
        private RopeController ropeController;
        private Rigidbody2D playerBody;
        private RopePlatformBuilder platformBuilder;
        private MainStagePreview preview;
        private MainStageRespawnOnFall respawnController;
        private MainStageGoalZone goalZone;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle ropeStyle;
        private GUIStyle resultStyle;
        private GUIStyle guideStyle;
        private GUIStyle guideCurrentStyle;
        private GUIStyle guideCompleteStyle;
        private GUIStyle clearTitleStyle;
        private GUIStyle clearBodyStyle;

        private void Awake()
        {
            ropeResource = FindFirstObjectByType<RopeResource>();
            ropeController = FindFirstObjectByType<RopeController>();
            platformBuilder = FindFirstObjectByType<RopePlatformBuilder>();
            preview = FindFirstObjectByType<MainStagePreview>();
            respawnController = FindFirstObjectByType<MainStageRespawnOnFall>();
            MainStageMidpointSetup.EnsureCreated();
            MainStageSectionSixSetup.EnsureCreated();
            MainStageSectionSevenSetup.EnsureCreated();
            MainStageSectionEightSetup.DisableLegacyObjects();
            MainStageSectionNineSetup.EnsureCreated();
            GameObject sectionTenTarget = MainStageSectionTenSetup.EnsureCreated();
            if (sectionTenTarget != null)
            {
                goalZone = sectionTenTarget.GetComponent<MainStageGoalZone>();
            }
            if (preview != null && ropeResource != null && sectionTenTarget != null)
            {
                preview.Configure(ropeResource.transform, sectionTenTarget.transform);
            }

            if (ropeResource != null)
            {
                MainStageVisuals.Apply(ropeResource.gameObject);
            }

            if (ropeController != null)
            {
                playerBody = ropeController.GetComponent<Rigidbody2D>();
            }
        }

        private void OnGUI()
        {
            if (platformBuilder == null)
            {
                platformBuilder = FindFirstObjectByType<RopePlatformBuilder>();
            }

            EnsureStyles();
            if (goalZone != null && goalZone.IsClear)
            {
                DrawClearScreen();
                return;
            }

            GUILayout.BeginArea(new Rect(22f, 18f, 520f, 500f), GUI.skin.box);
            GUILayout.Label("ヒモヒト / 本編ステージ", titleStyle);
            GUILayout.Label(GetSectionTitle(), bodyStyle);

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
                GUILayout.Label("W：使う長さを1増やす　S：1減らす", bodyStyle);
            }

            if (platformBuilder != null &&
                platformBuilder.IsPlatformBuildingUnlocked &&
                ropeController != null)
            {
                GUILayout.Label(
                    platformBuilder.CanBuildCurrentPlatform
                        ? $"Q：現在のヒモ {platformBuilder.CurrentPlatformCost:0.0} を足場にする"
                        : "この長さでは、足場化した後のヒモが足りません",
                    resultStyle);
            }

            if (preview != null && preview.IsPreviewing)
            {
                GUILayout.Label("ステージ確認中 — Landingからスタートへ戻ります", resultStyle);
            }
            else if (respawnController != null && respawnController.IsRopeExhausted)
            {
                GUILayout.Label("ヒモが尽きました — Rで現在の区間から再挑戦", resultStyle);
            }
            else
            {
                GUILayout.Label(
                    IsSectionEightActive()
                        ? "ヒモ足場で光を遮って右の床へ進む"
                        : "ゴールを目指す",
                    resultStyle);
            }

            if (IsSectionEightActive())
            {
                DrawSectionEightGuide();
            }

            if (playerBody != null)
            {
                GUILayout.Label($"速度  {playerBody.linearVelocity.magnitude:0.0}", bodyStyle);
            }

            GUILayout.Label("移動：A / D　ジャンプ：Space", bodyStyle);
            GUILayout.Label("照準：矢印キー　ヒモ：Eで接続／解除", bodyStyle);
            if (platformBuilder != null && platformBuilder.IsPlatformBuildingUnlocked)
            {
                GUILayout.Label("足場化：照準を合わせてQ", bodyStyle);
            }
            GUILayout.Label("落下またはR：現在のチェックポイントから再開", bodyStyle);
            GUILayout.EndArea();
        }

        private bool IsSectionEightActive()
        {
            return respawnController != null &&
                respawnController.HasReachedSectionNine &&
                !respawnController.HasReachedSectionTen;
        }

        private void DrawSectionEightGuide()
        {
            bool hasShield = platformBuilder != null &&
                platformBuilder.GeneratedPlatformCount > 0;
            bool isAttached = ropeController != null && ropeController.IsAttached;

            GUILayout.Space(4f);
            GUILayout.Label("第8区間の攻略", resultStyle);
            DrawGuideStep(
                1,
                "光が少し暗くなる向きでQを押し、ヒモ足場を作る",
                hasShield,
                !hasShield);
            DrawGuideStep(
                2,
                "足場の影から、上の青いHookへEで接続する",
                isAttached,
                hasShield && !isAttached);
            DrawGuideStep(
                3,
                "光を避けながら振り、右の青い床へ着地する",
                false,
                hasShield && isAttached);
        }

        private void DrawGuideStep(
            int stepNumber,
            string instruction,
            bool isComplete,
            bool isCurrent)
        {
            string marker = isComplete ? "✓" : isCurrent ? "▶" : "・";
            GUIStyle style = isComplete
                ? guideCompleteStyle
                : isCurrent
                    ? guideCurrentStyle
                    : guideStyle;
            GUILayout.Label($"{marker} {stepNumber}. {instruction}", style);
        }

        private string GetSectionTitle()
        {
            if (respawnController != null)
            {
                if (respawnController.HasReachedSectionTen)
                {
                    return "第9区間";
                }
                if (respawnController.HasReachedSectionNine)
                {
                    return "第8区間";
                }
                if (respawnController.HasReachedMidpoint)
                {
                    return playerBody != null && playerBody.position.x < 96f
                        ? "第6区間"
                        : "第7区間";
                }
            }

            float playerX = playerBody != null ? playerBody.position.x : float.NegativeInfinity;
            if (playerX < 8f)
            {
                return "第1区間";
            }
            if (playerX < 30f)
            {
                return "第2区間";
            }
            if (playerX < 50f)
            {
                return "第3区間";
            }
            return "第4〜5区間";
        }

        private void DrawClearScreen()
        {
            Color previousColor = GUI.color;
            GUI.color = new Color(0.035f, 0.04f, 0.085f, 0.99f);
            GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none);
            GUI.color = previousColor;

            GUILayout.BeginArea(new Rect(0f, 0f, Screen.width, Screen.height));
            GUILayout.FlexibleSpace();
            GUILayout.Label("MAIN STAGE CLEAR", clearTitleStyle);
            GUILayout.Space(18f);
            GUILayout.Label("自分のヒモで、最後まで飛び切りました", clearBodyStyle);
            GUILayout.Space(28f);
            if (ropeResource != null)
            {
                GUILayout.Label(
                    $"残ったヒモ　{ropeResource.CurrentLength:0.0} / " +
                    $"{ropeResource.MaximumLength:0.0}",
                    clearBodyStyle);
            }
            GUILayout.Space(28f);
            GUILayout.Label("プロトタイプはここで終了です", clearBodyStyle);
            GUILayout.Label("R　本編ステージを最初から再挑戦", clearBodyStyle);
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
            resultStyle = new GUIStyle(bodyStyle)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.44f, 0.92f, 1f) }
            };
            guideStyle = new GUIStyle(bodyStyle)
            {
                fontSize = 14,
                wordWrap = true,
                normal = { textColor = new Color(0.74f, 0.78f, 0.88f) }
            };
            guideCurrentStyle = new GUIStyle(guideStyle)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.78f, 0.2f) }
            };
            guideCompleteStyle = new GUIStyle(guideStyle)
            {
                normal = { textColor = new Color(0.33f, 1f, 0.76f) }
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
