using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Reveals the lower-route recovery stairs only when the player presses F
    /// beside the toy switch. Returning to the section-three start bank hides
    /// the stairs again so the next swing path stays clear.
    /// </summary>
    public sealed class MainStageSectionThreeRecoverySwitch : MonoBehaviour
    {
        private static readonly Color OffColor =
            new Color(1f, 0.72f, 0.18f);
        private static readonly Color OnColor =
            new Color(0.36f, 0.94f, 0.72f);

        [SerializeField] private MainStageSectionThreeRecoveryStairs stairs;
        [SerializeField] private SolidSprite visual;
        private PlayerMover nearbyPlayer;
        private PlayerMover trackedPlayer;

        private void Awake()
        {
            RestoreReferences();
        }

        private void OnEnable()
        {
            RestoreReferences();
        }

        // Loading a saved scene does not run the editor's Configure pass.
        private void RestoreReferences()
        {
            if (visual == null) visual = GetComponent<SolidSprite>();
            if (stairs == null)
            {
                foreach (GameObject candidate in Resources.FindObjectsOfTypeAll<GameObject>())
                {
                    if (candidate.scene == gameObject.scene &&
                        candidate.name == MainStageSectionThreeSetup.LowDeadEndName)
                    {
                        if (!candidate.TryGetComponent(out stairs))
                            stairs = candidate.AddComponent<MainStageSectionThreeRecoveryStairs>();
                        break;
                    }
                }
            }
            SetVisualState(stairs != null && stairs.IsRevealed);
        }

        public void Configure(GameObject lowDeadEnd, SolidSprite switchVisual)
        {
            visual = switchVisual;
            stairs = lowDeadEnd != null
                ? lowDeadEnd.GetComponent<
                    MainStageSectionThreeRecoveryStairs>()
                : null;
            SetVisualState(stairs != null && stairs.IsRevealed);
        }

        private void Update()
        {
            if (MainStagePreview.IsActive) return;

            if (stairs == null || visual == null) RestoreReferences();
            if (trackedPlayer == null)
            {
                trackedPlayer = Object.FindFirstObjectByType<PlayerMover>();
            }

            if (Input.GetKeyDown(KeyCode.F)) TryActivate();

            if (trackedPlayer != null &&
                stairs != null &&
                stairs.IsRevealed &&
                trackedPlayer.transform.position.x <=
                    MainStageSectionTwoSetup.LandingRightEdge + 0.1f &&
                trackedPlayer.transform.position.y >= -5.2f)
            {
                stairs.Hide();
                SetVisualState(false);
            }
        }

        private void TryActivate()
        {
            if (MainStagePreview.IsActive || nearbyPlayer == null || !nearbyPlayer.enabled ||
                Time.timeScale <= 0f || stairs == null || stairs.IsRevealed) return;
            // Activation is immediate. The character gesture is presentation only.
            stairs.Reveal();
            SetVisualState(true);
            nearbyPlayer.GetComponent<RopeBodyVisual>()?.PlaySwitchPressPose(transform);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerMover player = other.GetComponentInParent<PlayerMover>();
            if (player != null)
            {
                nearbyPlayer = player;
                trackedPlayer = player;
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            // Recover proximity after a script reload while already inside.
            OnTriggerEnter2D(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            PlayerMover player = other.GetComponentInParent<PlayerMover>();
            if (player != null && player == nearbyPlayer)
            {
                nearbyPlayer = null;
            }
        }

        private void OnGUI()
        {
            if (nearbyPlayer == null ||
                stairs == null ||
                stairs.IsRevealed)
            {
                return;
            }

            HimoHitoGuiTheme.ApplyToSkin(GUI.skin);
            GUIStyle style = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold
            };
            HimoHitoGuiTheme.ApplyToStyles(style);
            GUI.Box(
                new Rect(
                    (Screen.width - 360f) * 0.5f,
                    Screen.height - 92f,
                    360f,
                    48f),
                "F：帰り道のスイッチを押す",
                style);
        }

        private void SetVisualState(bool isOn)
        {
            if (visual != null)
            {
                visual.Color = isOn ? OnColor : OffColor;
                RecoverySwitchVisual.Ensure(gameObject);
            }
        }

    }
}
