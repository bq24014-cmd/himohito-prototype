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
        }
    }
}
