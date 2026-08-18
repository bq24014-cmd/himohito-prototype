using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Saves the chosen branch outcome when the player reaches the main-stage midpoint.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class MainStageCheckpoint : MonoBehaviour
    {
        private const float ReachedMessageDuration = 2f;
        private static readonly Color MarkerColor = new Color(0.25f, 0.85f, 1f);

        [SerializeField] private Vector2 respawnPosition;
        [SerializeField, Min(0)] private int checkpointWeaveThreads;
        [SerializeField] private bool grantsSectionSevenBridge;

        private GameObject marker;
        private GUIStyle locationStyle;
        private GUIStyle reachedStyle;
        private float reachedMessageUntil = float.NegativeInfinity;

        public bool IsReached { get; private set; }

        private void Awake()
        {
            CreateMarker();
        }

        public void Configure(
            Vector2 position,
            int weaveThreads,
            bool grantsBridge)
        {
            respawnPosition = position;
            checkpointWeaveThreads = Mathf.Max(0, weaveThreads);
            grantsSectionSevenBridge = grantsBridge;
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (IsReached ||
                collision.rigidbody == null ||
                collision.rigidbody.position.y <= transform.position.y)
            {
                return;
            }

            MainStageRespawnOnFall respawn =
                collision.rigidbody.GetComponent<MainStageRespawnOnFall>();
            if (respawn != null && respawn.TryReachMidpoint(
                    respawnPosition,
                    checkpointWeaveThreads,
                    grantsSectionSevenBridge))
            {
                IsReached = true;
                reachedMessageUntil = Time.unscaledTime + ReachedMessageDuration;
            }
        }

        private void OnGUI()
        {
            EnsureStyles();
            DrawLocationLabel();

            if (!IsReached || Time.unscaledTime > reachedMessageUntil)
            {
                return;
            }

            float width = Mathf.Min(520f, Screen.width - 40f);
            Rect messageRect = new Rect(
                (Screen.width - width) * 0.5f,
                Screen.height * 0.18f,
                width,
                58f);
            Color previousColor = GUI.color;
            GUI.color = new Color(0.03f, 0.06f, 0.11f, 0.92f);
            GUI.Box(messageRect, GUIContent.none);
            GUI.color = previousColor;
            GUI.Label(
                messageRect,
                "チェックポイント更新 — ここから再開",
                reachedStyle);
        }

        private void CreateMarker()
        {
            marker = new GameObject($"{name} Marker");
            marker.transform.position = transform.position + Vector3.up * 1.05f;
            marker.transform.localScale = new Vector3(0.18f, 1.2f, 1f);

            SpriteRenderer renderer = marker.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 4;
            SolidSprite visual = marker.AddComponent<SolidSprite>();
            visual.Color = MarkerColor;
        }

        private void DrawLocationLabel()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            Vector3 screenPosition = mainCamera.WorldToScreenPoint(
                transform.position + Vector3.up * 1.85f);
            if (screenPosition.z <= 0f)
            {
                return;
            }

            const float width = 180f;
            Rect labelRect = new Rect(
                screenPosition.x - width * 0.5f,
                Screen.height - screenPosition.y - 18f,
                width,
                36f);
            GUI.Label(labelRect, "中間地点", locationStyle);
        }

        private void EnsureStyles()
        {
            if (locationStyle != null)
            {
                return;
            }

            locationStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = MarkerColor }
            };
            reachedStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                normal = { textColor = MarkerColor }
            };
        }

        private void OnDestroy()
        {
            if (marker != null)
            {
                Destroy(marker);
            }
        }
    }
}
