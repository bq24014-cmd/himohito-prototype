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

            GUILayout.BeginArea(new Rect(22f, 18f, 500f, 204f), GUI.skin.box);
            GUILayout.Label("HIMOHITO / GRAYBOX PROTOTYPE", titleStyle);

            if (ropeResource != null)
            {
                GUILayout.Label(
                    $"ROPE  {ropeResource.CurrentLength:0.0} / {ropeResource.MaximumLength:0.0}",
                    lengthStyle);
            }

            if (ropeController != null)
            {
                GUILayout.Label(
                    $"LENGTH  {ropeController.SelectedRopeLength} / " +
                    $"{ropeController.MaximumSelectableRopeLength}    W: +1 / S: -1",
                    bodyStyle);
            }

            string state;
            if (runController != null && runController.Outcome != PrototypeRunController.RunOutcome.Playing)
            {
                state = runController.Outcome == PrototypeRunController.RunOutcome.Clear
                    ? "CLEAR — press R to restart"
                    : runController.IsAutomaticRespawnPending
                        ? "FAILED — respawning..."
                        : "FAILED — press R to restart";
            }
            else
            {
                state = ropeController != null && ropeController.IsAttached
                    ? $"ATTACHED {ropeController.ActiveRopeLength:0.0} — release E: {ropeController.ReleaseRefundRate:P0} returns"
                    : "READY — aim with arrow keys and hold E";
            }
            GUILayout.Label(state, bodyStyle);
            if (playerBody != null)
            {
                GUILayout.Label($"SPEED  {playerBody.linearVelocity.magnitude:0.0}", bodyStyle);
            }
            GUILayout.Label("Move: A / D    Jump: Space    Aim: Left / Right arrows", bodyStyle);
            GUILayout.Label("Snap aim: Up / Down arrows    Rope: Hold E", bodyStyle);
            GUILayout.Label("Restart: R    Mouse aiming is optional", bodyStyle);
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
            GUILayout.Label("W / S  次に使うヒモの長さを選ぶ", startImportantStyle);
            GUILayout.Space(20f);
            GUILayout.Label("A / D  左右移動     Space  ジャンプ", startControlStyle);
            GUILayout.Label("← / →  照準を動かす     ↑ / ↓  真上・真下へ合わせる", startControlStyle);
            GUILayout.Label("E 長押し  ヒモを掛ける", startControlStyle);
            GUILayout.Label("E を離す  勢いを保って飛ぶ", startControlStyle);
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
