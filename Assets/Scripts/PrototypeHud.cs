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
        [SerializeField] private WeaveResource weaveResource;
        [SerializeField] private WeaveFrame weaveFrame;

        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle lengthStyle;
        private GUIStyle startTitleStyle;
        private GUIStyle startObjectiveStyle;
        private GUIStyle startImportantStyle;
        private GUIStyle startControlStyle;
        private GUIStyle startPromptStyle;

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

            if (runController == null)
            {
                runController = FindFirstObjectByType<PrototypeRunController>();
            }

            if (weaveResource == null)
            {
                weaveResource = FindFirstObjectByType<WeaveResource>();
            }

            if (weaveFrame == null)
            {
                weaveFrame = FindFirstObjectByType<WeaveFrame>();
            }
        }

        private void OnGUI()
        {
            EnsureStyles();

            if (runController != null &&
                runController.Outcome == PrototypeRunController.RunOutcome.WaitingToStart)
            {
                DrawStartScreen();
                return;
            }

            GUILayout.BeginArea(new Rect(22f, 18f, 520f, 390f), GUI.skin.box);
            GUILayout.Label("ヒモヒト / プロトタイプ", titleStyle);

            if (ropeResource != null)
            {
                GUILayout.Label(
                    $"ヒモ残量  {ropeResource.CurrentLength:0.0} / {ropeResource.MaximumLength:0.0}",
                    lengthStyle);
            }

            if (ropeController != null)
            {
                GUILayout.Label(
                    $"次に使う長さ  {ropeController.SelectedRopeLength} / " +
                    $"{ropeController.MaximumSelectableRopeLength}",
                    bodyStyle);
                GUILayout.Label("W：使う長さを1増やす", bodyStyle);
                GUILayout.Label("S：使う長さを1減らす", bodyStyle);
            }

            if (weaveResource != null)
            {
                GUILayout.Label($"編み糸  {weaveResource.CurrentThreads}個", bodyStyle);
            }

            if (weaveFrame != null && weaveFrame.IsPlayerInRange && !weaveFrame.IsCompleted)
            {
                string weaveMessage = weaveFrame.RemainingThreads == 0
                    ? "Q：編み糸2個で足場を編む"
                    : $"足場まで編み糸があと{weaveFrame.RemainingThreads}個必要";
                GUILayout.Label(weaveMessage, bodyStyle);
            }

            string state;
            if (runController != null && runController.Outcome != PrototypeRunController.RunOutcome.Playing)
            {
                state = runController.Outcome == PrototypeRunController.RunOutcome.Clear
                    ? "クリア — Rで再挑戦"
                    : runController.IsAutomaticRespawnPending
                        ? "失敗 — 自動で戻ります..."
                        : "失敗 — Rで再挑戦";
            }
            else
            {
                state = ropeController != null && ropeController.IsAttached
                    ? $"ヒモ接続中 {ropeController.ActiveRopeLength:0.0} — Eを離すと{ropeController.ReleaseRefundRate:P0}戻る"
                    : "準備完了 — 矢印キーで狙い、Eを長押し";
            }
            GUILayout.Label(state, bodyStyle);
            if (playerBody != null)
            {
                GUILayout.Label($"速度  {playerBody.linearVelocity.magnitude:0.0}", bodyStyle);
            }
            GUILayout.Label("移動：A / D    ジャンプ：Space", bodyStyle);
            GUILayout.Label("照準：← / →    真上・真下：↑ / ↓", bodyStyle);
            GUILayout.Label("ヒモ：E長押し    再挑戦：R", bodyStyle);
            GUILayout.Label("編む：編み枠の近くでQ", bodyStyle);
            GUILayout.Label("マウス照準も使用可能", bodyStyle);
            GUILayout.EndArea();
        }

        private void DrawStartScreen()
        {
            Color previousColor = GUI.color;
            GUI.color = new Color(0.04f, 0.05f, 0.11f, 0.98f);
            GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none);
            GUI.color = previousColor;

            float panelWidth = Mathf.Min(880f, Screen.width - 40f);
            float panelHeight = Mathf.Min(620f, Screen.height - 40f);
            Rect panel = new Rect(
                (Screen.width - panelWidth) * 0.5f,
                (Screen.height - panelHeight) * 0.5f,
                panelWidth,
                panelHeight);

            GUILayout.BeginArea(panel, GUI.skin.box);
            GUILayout.Space(24f);
            GUILayout.Label("HIMOHITO", startTitleStyle);
            GUILayout.Label("緑色のゴールを目指す", startObjectiveStyle);
            GUILayout.Space(22f);
            GUILayout.Label("重要", startImportantStyle);
            GUILayout.Label("W：次に使うヒモの長さを1増やす", startImportantStyle);
            GUILayout.Label("S：次に使うヒモの長さを1減らす", startImportantStyle);
            GUILayout.Space(20f);
            GUILayout.Label("A / D  左右移動     Space  ジャンプ", startControlStyle);
            GUILayout.Label("← / →  照準を動かす     ↑ / ↓  真上・真下へ合わせる", startControlStyle);
            GUILayout.Label("E 長押し  ヒモを掛ける", startControlStyle);
            GUILayout.Label("E を離す  勢いを保って飛ぶ", startControlStyle);
            GUILayout.Label("Q  編み糸2個で指定された足場を編む", startControlStyle);
            GUILayout.Label("R  最初から再挑戦", startControlStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label("キーボードの何かのキーを押して開始", startPromptStyle);
            GUILayout.Space(24f);
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
                alignment = TextAnchor.MiddleCenter,
                fontSize = 42,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.35f, 1f, 0.78f) }
            };
            startObjectiveStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            startImportantStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.82f, 0.28f) }
            };
            startControlStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                normal = { textColor = Color.white }
            };
            startPromptStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.65f, 0.72f, 1f) }
            };
        }
    }
}
