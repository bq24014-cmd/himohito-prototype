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
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle ropeStyle;
        private GUIStyle resultStyle;

        private void Awake()
        {
            ropeResource = FindFirstObjectByType<RopeResource>();
            ropeController = FindFirstObjectByType<RopeController>();
            weaveResource = FindFirstObjectByType<WeaveResource>();
            preview = FindFirstObjectByType<MainStagePreview>();
            respawnController = FindFirstObjectByType<MainStageRespawnOnFall>();
            GameObject sectionSixTarget = MainStageSectionSixSetup.EnsureCreated();
            if (preview != null && ropeResource != null && sectionSixTarget != null)
            {
                preview.Configure(ropeResource.transform, sectionSixTarget.transform);
            }

            if (ropeController != null)
            {
                playerBody = ropeController.GetComponent<Rigidbody2D>();
            }
        }

        private void OnGUI()
        {
            EnsureStyles();
            GUILayout.BeginArea(new Rect(22f, 18f, 520f, 380f), GUI.skin.box);
            GUILayout.Label("ヒモヒト / 本編ステージ", titleStyle);
            GUILayout.Label(
                respawnController != null && respawnController.HasReachedMidpoint
                    ? "第6区間　長さを選んで障害物を越える"
                    : "第4〜5区間　使う資源を選ぶ上下分岐",
                bodyStyle);

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

            if (preview != null && preview.IsPreviewing)
            {
                GUILayout.Label("ステージ確認中 — Landingからスタートへ戻ります", resultStyle);
            }
            else if (respawnController != null && respawnController.HasReachedMidpoint)
            {
                GUILayout.Label(
                    "中間チェックポイント到達 — 長すぎるヒモは赤い障害物に注意",
                    resultStyle);
            }
            else
            {
                GUILayout.Label("上：ヒモを編み糸に変える　下：ヒモを温存する", resultStyle);
            }

            if (playerBody != null)
            {
                GUILayout.Label($"速度  {playerBody.linearVelocity.magnitude:0.0}", bodyStyle);
            }

            GUILayout.Label("移動：A / D　ジャンプ：Space", bodyStyle);
            GUILayout.Label("照準：矢印キー　ヒモ：E長押し", bodyStyle);
            GUILayout.Label("合流後の落下は中間チェックポイントから再開", bodyStyle);
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
        }
    }
}
