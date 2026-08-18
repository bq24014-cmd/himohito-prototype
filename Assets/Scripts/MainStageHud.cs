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
        private WeaveResource weaveResource;
        private MainStagePreview preview;
        private MainStageRespawnOnFall respawnController;
        private MainStageGoalZone goalZone;
        private WeaveFrame weaveFrame;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle ropeStyle;
        private GUIStyle resultStyle;
        private GUIStyle clearTitleStyle;
        private GUIStyle clearBodyStyle;

        private void Awake()
        {
            ropeResource = FindFirstObjectByType<RopeResource>();
            ropeController = FindFirstObjectByType<RopeController>();
            weaveResource = FindFirstObjectByType<WeaveResource>();
            preview = FindFirstObjectByType<MainStagePreview>();
            respawnController = FindFirstObjectByType<MainStageRespawnOnFall>();
            MainStageSectionSixSetup.EnsureCreated();
            MainStageSectionSevenSetup.EnsureCreated();
            MainStageSectionEightSetup.EnsureCreated();
            MainStageSectionNineSetup.EnsureCreated();
            GameObject sectionTenTarget = MainStageSectionTenSetup.EnsureCreated();
            if (sectionTenTarget != null)
            {
                goalZone = sectionTenTarget.GetComponent<MainStageGoalZone>();
            }
            weaveFrame = FindFirstObjectByType<WeaveFrame>();
            if (preview != null && ropeResource != null && sectionTenTarget != null)
            {
                preview.Configure(ropeResource.transform, sectionTenTarget.transform);
            }

            if (ropeController != null)
            {
                playerBody = ropeController.GetComponent<Rigidbody2D>();
            }
        }

        private void OnGUI()
        {
            EnsureStyles();
            if (goalZone != null && goalZone.IsClear)
            {
                DrawClearScreen();
                return;
            }

            GUILayout.BeginArea(new Rect(22f, 18f, 520f, 380f), GUI.skin.box);
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

            if (weaveResource != null)
            {
                GUILayout.Label($"編み糸  {weaveResource.CurrentThreads}個", bodyStyle);
            }

            if (weaveFrame != null && weaveFrame.IsPlayerInRange && !weaveFrame.IsCompleted)
            {
                GUILayout.Label(
                    !weaveFrame.HasRoutePermission
                        ? "橋を編めるのは上ルートを攻略した場合だけ"
                        : weaveFrame.RemainingThreads == 0
                        ? "Q：編み糸3個で橋を作る"
                        : $"橋を作るには編み糸があと{weaveFrame.RemainingThreads}個必要",
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
            else if (respawnController != null && respawnController.HasReachedSectionTen)
            {
                GUILayout.Label(
                    "第10区間 — 1つのHookで大きく振り、緑のゴールへ飛ぶ",
                    resultStyle);
            }
            else if (respawnController != null && respawnController.HasReachedSectionNine)
            {
                GUILayout.Label(
                    "第9区間 — 赤い障害物が上がるタイミングを観察して進む",
                    resultStyle);
            }
            else if (respawnController != null && respawnController.HasReachedSectionEight)
            {
                GUILayout.Label(
                    "第8区間 — 1手目だけで使い切らず、次のHook分を残す",
                    resultStyle);
            }
            else if (respawnController != null && respawnController.HasReachedMidpoint)
            {
                GUILayout.Label(
                    "第7区間 — 編み糸で橋を作るか、ヒモで谷を渡る",
                    resultStyle);
            }
            else
            {
                GUILayout.Label(GetEarlySectionHint(), resultStyle);
            }

            if (playerBody != null)
            {
                GUILayout.Label($"速度  {playerBody.linearVelocity.magnitude:0.0}", bodyStyle);
            }

            GUILayout.Label("移動：A / D　ジャンプ：Space", bodyStyle);
            GUILayout.Label("照準：矢印キー　ヒモ：E長押し", bodyStyle);
            GUILayout.Label("落下またはR：現在のチェックポイントから再開", bodyStyle);
            GUILayout.EndArea();
        }

        private string GetSectionTitle()
        {
            if (respawnController != null)
            {
                if (respawnController.HasReachedSectionTen)
                {
                    return "第10区間　自分のヒモで大きく飛ぶ";
                }
                if (respawnController.HasReachedSectionNine)
                {
                    return "第9区間　動く障害物の安全な瞬間を読む";
                }
                if (respawnController.HasReachedSectionEight)
                {
                    return "第8区間　2手先までヒモを配分する";
                }
                if (respawnController.HasReachedMidpoint)
                {
                    return "第6〜7区間　長さと資源を選んで進む";
                }
            }

            float playerX = playerBody != null ? playerBody.position.x : float.NegativeInfinity;
            if (playerX < 8f)
            {
                return "第1区間　基本の振り子で着地する";
            }
            if (playerX < 30f)
            {
                return "第2区間　歩いて次の配置を観察する";
            }
            if (playerX < 50f)
            {
                return "第3区間　届く長さと飛べる長さを考える";
            }
            return "第4〜5区間　使う資源を選ぶ上下分岐";
        }

        private string GetEarlySectionHint()
        {
            float playerX = playerBody != null ? playerBody.position.x : float.NegativeInfinity;
            if (playerX < 8f)
            {
                return "Hookへヒモを掛け、振り子で最初の着地床へ進む";
            }
            if (playerX < 30f)
            {
                return "足場を歩き、次に使うヒモの長さを考える";
            }
            if (playerX < 50f)
            {
                return "Hookへ届く最短より、着地に必要な長さを選ぶ";
            }
            return "上：ヒモを編み糸に変える　下：ヒモを温存する";
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
            if (respawnController != null)
            {
                GUILayout.Label(
                    respawnController.CanUseSectionSevenBridge
                        ? "選んだ道　上ルート（編んだ橋を使う道）"
                        : "選んだ道　下ルート（ヒモを温存する道）",
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
