using UnityEngine;

namespace HimoHito
{
    /// <summary>Tab opens the compact control sheet; Esc pauses the run.</summary>
    public sealed class StageOverlayControls : MonoBehaviour
    {
        private bool showHelp;
        private bool isPaused;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                showHelp = !showHelp;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                isPaused = !isPaused;
                Time.timeScale = isPaused ? 0f : 1f;
            }
        }

        private void OnDisable()
        {
            if (isPaused)
            {
                Time.timeScale = 1f;
                isPaused = false;
            }
        }

        private void OnGUI()
        {
            EnsureStyles();
            GUI.Label(new Rect(Screen.width - 215f, 12f, 200f, 28f),
                "Tab：操作確認　Esc：ポーズ", bodyStyle);

            if (!showHelp && !isPaused)
            {
                return;
            }

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
            GUILayout.Label(isPaused ? "PAUSE" : "操作方法", titleStyle);
            GUILayout.Space(18f);
            GUILayout.Label("A / D：移動・振り子を加速　　Space：ジャンプ", bodyStyle);
            GUILayout.Label("W / S：次に使う長さを増減", bodyStyle);
            GUILayout.Label("矢印キー：照準　　E：接続／解除", bodyStyle);
            GUILayout.Label("Q：接続中のヒモを足場にする（この時だけ永久消費）", bodyStyle);
            GUILayout.Label("F：2本が集まるHookを外して1本にまとめる", bodyStyle);
            GUILayout.Label("R：現在の区間から再挑戦", bodyStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label(isPaused
                ? "Escで再開"
                : "Tabで閉じる", titleStyle);
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
        }
    }
}
