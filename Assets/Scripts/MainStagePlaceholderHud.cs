using UnityEngine;
using UnityEngine.SceneManagement;

namespace HimoHito
{
    /// <summary>
    /// Makes the empty main-stage scene visibly distinct until its layout is designed.
    /// </summary>
    public sealed class MainStagePlaceholderHud : MonoBehaviour
    {
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreatePlaceholderForMainStage()
        {
            if (SceneManager.GetActiveScene().name != "MainStage" ||
                FindFirstObjectByType<MainStagePlaceholderHud>() != null)
            {
                return;
            }

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.04f, 0.085f);

            new GameObject("Main Stage Placeholder")
                .AddComponent<MainStagePlaceholderHud>();
        }

        private void OnGUI()
        {
            EnsureStyles();
            Color previousColor = GUI.color;
            GUI.color = new Color(0.035f, 0.04f, 0.085f, 1f);
            GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none);
            GUI.color = previousColor;

            GUILayout.BeginArea(new Rect(0f, 0f, Screen.width, Screen.height));
            GUILayout.FlexibleSpace();
            GUILayout.Label("本編ステージ", titleStyle);
            GUILayout.Space(20f);
            GUILayout.Label("ここから先のステージは次に一緒に設計します", bodyStyle);
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
                alignment = TextAnchor.MiddleCenter,
                fontSize = 52,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.35f, 1f, 0.78f) }
            };
            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22,
                normal = { textColor = Color.white }
            };
        }
    }
}
