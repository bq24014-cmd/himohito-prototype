using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Keeps the floor-collision comparison quick to repeat without changing
    /// the tutorial or main-stage restart rules.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(RopeController), typeof(RopeResource))]
    public sealed class CollisionExperimentController : MonoBehaviour
    {
        [SerializeField] private float fallThreshold = -10f;

        private Rigidbody2D body;
        private RopeController ropeController;
        private RopeResource ropeResource;
        private Vector2 startPosition;

        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            ropeController = GetComponent<RopeController>();
            ropeResource = GetComponent<RopeResource>();
            startPosition = body.position;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R) || body.position.y < fallThreshold)
            {
                RestartExperiment();
            }
        }

        private void OnGUI()
        {
            EnsureStyles();
            Rect panel = new Rect(Screen.width - 430f, 20f, 410f, 158f);
            GUI.Box(panel, GUIContent.none);
            GUI.Label(
                new Rect(panel.x + 16f, panel.y + 12f, 380f, 32f),
                "床の当たり判定 実験",
                titleStyle);
            GUI.Label(
                new Rect(panel.x + 16f, panel.y + 48f, 380f, 96f),
                "青いレール：下から通過／上から着地\n" +
                "灰色の固い板：振り子中も衝突\n" +
                "R：実験開始地点へ戻る",
                bodyStyle);
        }

        private void RestartExperiment()
        {
            ropeController.DetachAndRefund();
            ropeResource.ResetToMaximum();
            body.position = startPosition;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold
            };
            titleStyle.normal.textColor = new Color(0.33f, 1f, 0.76f);

            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                wordWrap = true
            };
            bodyStyle.normal.textColor = Color.white;
        }
    }
}
