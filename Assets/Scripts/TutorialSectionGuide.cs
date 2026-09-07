using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Places a readable sign at the beginning of every tutorial section and
    /// pauses the run while its illustrated guide is open.
    /// </summary>
    public sealed class TutorialSectionGuide : MonoBehaviour
    {
        private const float InteractionRange = 3.25f;
        private const string SignPrefix = "Tutorial Guide Sign T";
        private const string SignArtworkPath = "Art/TutorialGuideSign-v1";
        private const string SignArtworkName = "Picture-book Guide Sign Visual";
        private const string HelpBackgroundPath =
            "Art/HimoHitoControlsBackground-v1";
        private const string PlayerTexturePath = "Art/HimoHitoPlayer-v1";

        private static readonly Vector2[] SignPositions =
        {
            new Vector2(-6.1f, 1.35f),
            new Vector2(10.1f, 1.35f),
            new Vector2(25.1f, 1.35f),
            new Vector2(37.1f, 1.35f)
        };

        private static readonly string[] SectionTitles =
        {
            "掛けて、振って、渡る",
            "長さを、選ぶ",
            "編む――初めて、体が減る",
            "外して、1本にする"
        };

        private static readonly string[] SectionSummaries =
        {
            "青いHookへヒモを掛け、歩き出して振り子になり、対岸へ渡ります。",
            "長さで振り子の底が変わります。トゲを避ける長さを選びます。",
            "頭上にHookがない谷です。対岸の緑フックへ掛けたヒモを足場にします。",
            "2本の足場を中央で外し、梁の下を通れる1本の橋へまとめます。"
        };

        private static readonly string[] SectionSteps =
        {
            "1　W / Sで長さ、矢印キーで向きを調整\n2　青いHookへ照準を合わせて E\n3　右へ歩いて振り子になり、対岸へ着地",
            "1　長さ8では底が下がり、トゲに当たる\n2　Eで外し、W / Sで長さ6～7を選ぶ\n3　もう一度 Eで掛け、トゲの上を渡る",
            "1　対岸の緑フックへ長さ6で E\n2　接続中に Qでヒモを足場へ変える\n3　残量を6使ってできた橋を歩いて渡る",
            "1　左岸→中央→右岸へ長さ6の足場を2本作る\n2　中央の青いHookへ照準を合わせる\n3　Fで中央を外し、深くたわんだ1本の橋を渡る"
        };

        private static Texture2D ringTexture;

        private PrototypeRunController runController;
        private StageOverlayControls overlayControls;
        private PlayerMover playerMover;
        private RopeController ropeController;
        private RopePlatformBuilder platformBuilder;
        private PrototypeAudioFeedback audioFeedback;
        private Rigidbody2D playerBody;
        private Camera mainCamera;
        private Texture2D helpBackground;
        private Sprite playerSprite;
        private bool wasPlayerMoverEnabled;
        private bool wasRopeControllerEnabled;
        private bool wasPlatformBuilderEnabled;
        private bool disabledGameplay;
        private int openSection;
        private GUIStyle sectionLabelStyle;
        private GUIStyle promptStyle;
        private GUIStyle titleStyle;
        private GUIStyle summaryStyle;
        private GUIStyle stepStyle;
        private GUIStyle diagramLabelStyle;
        private GUIStyle closeStyle;

        public bool IsVisible => openSection > 0;

        private void Awake()
        {
            EnsureSigns();
            runController = GetComponent<PrototypeRunController>();
            if (runController == null)
            {
                runController = FindFirstObjectByType<PrototypeRunController>();
            }
            playerMover = GetComponent<PlayerMover>();
            ropeController = GetComponent<RopeController>();
            platformBuilder = GetComponent<RopePlatformBuilder>();
            audioFeedback = GetComponent<PrototypeAudioFeedback>();
            playerBody = GetComponent<Rigidbody2D>();
            overlayControls = FindFirstObjectByType<StageOverlayControls>();
            mainCamera = Camera.main;
            helpBackground = Resources.Load<Texture2D>(HelpBackgroundPath);
            playerSprite = TutorialFirstSectionVisuals.LoadProcessedToySprite(
                PlayerTexturePath);
        }

        private void Update()
        {
            if (runController == null || playerBody == null)
            {
                return;
            }

            if (IsVisible)
            {
                if (runController.Outcome !=
                    PrototypeRunController.RunOutcome.Playing ||
                    runController.CurrentTutorialSection != openSection)
                {
                    CloseGuide();
                    return;
                }

                if (Input.GetKeyDown(KeyCode.Z))
                {
                    CloseGuide();
                }
                return;
            }

            if (runController.Outcome !=
                PrototypeRunController.RunOutcome.Playing)
            {
                return;
            }

            if (overlayControls == null)
            {
                overlayControls = FindFirstObjectByType<StageOverlayControls>();
            }
            if (overlayControls != null && overlayControls.IsOverlayVisible)
            {
                return;
            }

            int section = runController.CurrentTutorialSection;
            if (IsNearSign(section) && Input.GetKeyDown(KeyCode.Z))
            {
                OpenGuide(section);
            }
        }

        private void OnDisable()
        {
            if (IsVisible)
            {
                CloseGuide();
            }
        }

        private void OnGUI()
        {
            GUI.depth = -1100;
            HimoHitoGuiTheme.ApplyToSkin(GUI.skin);
            EnsureStyles();

            if (IsVisible)
            {
                DrawGuide();
                return;
            }

            if (runController == null ||
                runController.Outcome !=
                    PrototypeRunController.RunOutcome.Playing ||
                (overlayControls != null &&
                    overlayControls.IsOverlayVisible) ||
                !IsNearSign(runController.CurrentTutorialSection))
            {
                return;
            }

            DrawReadPrompt(runController.CurrentTutorialSection);
        }

        public static bool EnsureSigns()
        {
            bool changed = false;
            for (int index = 0; index < SignPositions.Length; index++)
            {
                int section = index + 1;
                string signName = SignPrefix + section;
                GameObject sign = SceneObjectLookup.Find(signName, "Tutorial");
                if (sign == null)
                {
                    sign = new GameObject(signName);
                    changed = true;
                }

                Vector3 position = new Vector3(
                    SignPositions[index].x,
                    SignPositions[index].y,
                    0f);
                if (sign.transform.position != position)
                {
                    sign.transform.position = position;
                    changed = true;
                }
                if (!sign.activeSelf)
                {
                    sign.SetActive(true);
                    changed = true;
                }

                changed |= EnsureSignPiece(
                    sign.transform,
                    "Wooden Post",
                    new Vector2(0f, -0.72f),
                    new Vector2(0.20f, 1.45f),
                    new Color(0.38f, 0.18f, 0.09f),
                    3);
                changed |= EnsureSignPiece(
                    sign.transform,
                    "Wooden Board",
                    new Vector2(0f, 0.22f),
                    new Vector2(1.78f, 1.05f),
                    new Color(0.88f, 0.43f, 0.13f),
                    4);
                changed |= EnsureSignPiece(
                    sign.transform,
                    "Inset Panel",
                    new Vector2(0f, 0.22f),
                    new Vector2(1.45f, 0.72f),
                    new Color(0.12f, 0.08f, 0.22f),
                    5);
                changed |= EnsureSignPiece(
                    sign.transform,
                    "Pink Guide Mark",
                    new Vector2(0f, 0.22f),
                    new Vector2(0.72f, 0.16f),
                    new Color(1f, 0.36f, 0.56f),
                    6);
                changed |= EnsureSignPiece(
                    sign.transform,
                    "Blue Guide Mark",
                    new Vector2(0f, 0.46f),
                    new Vector2(0.44f, 0.14f),
                    new Color(0.24f, 0.56f, 0.88f),
                    6);
                changed |= EnsureSignArtwork(sign.transform);
            }
            return changed;
        }

        private static bool EnsureSignArtwork(Transform parent)
        {
            bool changed = false;
            Sprite artwork = Resources.Load<Sprite>(SignArtworkPath);
            Transform visual = parent.Find(SignArtworkName);
            if (visual == null)
            {
                GameObject visualObject = new GameObject(SignArtworkName);
                visual = visualObject.transform;
                visual.SetParent(parent, false);
                changed = true;
            }

            bool artworkAvailable = artwork != null;
            if (visual.gameObject.activeSelf != artworkAvailable)
            {
                visual.gameObject.SetActive(artworkAvailable);
                changed = true;
            }

            if (artworkAvailable)
            {
                if (!visual.TryGetComponent(out SpriteRenderer renderer))
                {
                    renderer = visual.gameObject.AddComponent<SpriteRenderer>();
                    changed = true;
                }
                if (renderer.sprite != artwork)
                {
                    renderer.sprite = artwork;
                    changed = true;
                }
                if (renderer.color != Color.white)
                {
                    renderer.color = Color.white;
                    changed = true;
                }
                if (renderer.sortingOrder != 7)
                {
                    renderer.sortingOrder = 7;
                    changed = true;
                }

                const float targetWidth = 2.30f;
                float uniformScale = artwork.bounds.size.x > 0f
                    ? targetWidth / artwork.bounds.size.x
                    : 1f;
                Vector3 scale = new Vector3(
                    uniformScale,
                    uniformScale,
                    1f);
                if (visual.localPosition != Vector3.zero)
                {
                    visual.localPosition = Vector3.zero;
                    changed = true;
                }
                if (visual.localScale != scale)
                {
                    visual.localScale = scale;
                    changed = true;
                }
                if (visual.localRotation != Quaternion.identity)
                {
                    visual.localRotation = Quaternion.identity;
                    changed = true;
                }
            }

            string[] fallbackPieces =
            {
                "Wooden Post",
                "Wooden Board",
                "Inset Panel",
                "Pink Guide Mark",
                "Blue Guide Mark"
            };
            for (int index = 0; index < fallbackPieces.Length; index++)
            {
                Transform piece = parent.Find(fallbackPieces[index]);
                if (piece != null && piece.gameObject.activeSelf == artworkAvailable)
                {
                    piece.gameObject.SetActive(!artworkAvailable);
                    changed = true;
                }
            }
            return changed;
        }

        private static bool EnsureSignPiece(
            Transform parent,
            string pieceName,
            Vector2 localPosition,
            Vector2 localScale,
            Color color,
            int sortingOrder)
        {
            bool changed = false;
            Transform piece = parent.Find(pieceName);
            if (piece == null)
            {
                GameObject pieceObject = new GameObject(pieceName);
                piece = pieceObject.transform;
                piece.SetParent(parent, false);
                changed = true;
            }

            Vector3 position = new Vector3(
                localPosition.x,
                localPosition.y,
                0f);
            Vector3 scale = new Vector3(
                localScale.x,
                localScale.y,
                1f);
            if (piece.localPosition != position)
            {
                piece.localPosition = position;
                changed = true;
            }
            if (piece.localScale != scale)
            {
                piece.localScale = scale;
                changed = true;
            }
            if (piece.localRotation != Quaternion.identity)
            {
                piece.localRotation = Quaternion.identity;
                changed = true;
            }
            if (!piece.TryGetComponent(out SpriteRenderer renderer))
            {
                renderer = piece.gameObject.AddComponent<SpriteRenderer>();
                changed = true;
            }
            if (renderer.sortingOrder != sortingOrder)
            {
                renderer.sortingOrder = sortingOrder;
                changed = true;
            }
            if (!piece.TryGetComponent(out SolidSprite solid))
            {
                solid = piece.gameObject.AddComponent<SolidSprite>();
                changed = true;
            }
            if (solid.Color != color)
            {
                solid.Color = color;
                changed = true;
            }
            return changed;
        }

        private bool IsNearSign(int section)
        {
            if (section < 1 || section > SignPositions.Length)
            {
                return false;
            }
            return Vector2.Distance(
                playerBody.position,
                SignPositions[section - 1]) <= InteractionRange;
        }

        private void OpenGuide(int section)
        {
            openSection = section;
            if (audioFeedback == null)
            {
                audioFeedback = GetComponent<PrototypeAudioFeedback>();
            }
            audioFeedback?.PlayUiPaperOpened();
            wasPlayerMoverEnabled = playerMover != null && playerMover.enabled;
            wasRopeControllerEnabled =
                ropeController != null && ropeController.enabled;
            wasPlatformBuilderEnabled =
                platformBuilder != null && platformBuilder.enabled;

            if (playerMover != null)
            {
                playerMover.enabled = false;
            }
            if (ropeController != null)
            {
                ropeController.enabled = false;
            }
            if (platformBuilder != null)
            {
                platformBuilder.enabled = false;
            }
            disabledGameplay = true;
            Time.timeScale = 0f;
        }

        private void CloseGuide()
        {
            openSection = 0;
            if (disabledGameplay)
            {
                if (playerMover != null)
                {
                    playerMover.enabled = wasPlayerMoverEnabled;
                }
                if (ropeController != null)
                {
                    ropeController.enabled = wasRopeControllerEnabled;
                }
                if (platformBuilder != null)
                {
                    platformBuilder.enabled = wasPlatformBuilderEnabled;
                }
                disabledGameplay = false;
            }
            Time.timeScale = 1f;
        }

        private void DrawReadPrompt(int section)
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
            if (mainCamera == null)
            {
                return;
            }

            Vector3 screenPoint = mainCamera.WorldToScreenPoint(
                SignPositions[section - 1] + Vector2.up * 1.25f);
            if (screenPoint.z <= 0f)
            {
                return;
            }

            const float width = 250f;
            const float height = 54f;
            Rect rect = new Rect(
                Mathf.Clamp(screenPoint.x - width * 0.5f, 12f, Screen.width - width - 12f),
                Mathf.Clamp(Screen.height - screenPoint.y - height, 12f, Screen.height - height - 12f),
                width,
                height);
            HimoHitoUiParts.DrawWoodButtonLabel(
                rect,
                $"Z　区間{section}の看板を読む",
                promptStyle);
        }

        private void DrawGuide()
        {
            Rect screen = new Rect(0f, 0f, Screen.width, Screen.height);
            Color previous = GUI.color;
            if (helpBackground != null)
            {
                GUI.DrawTexture(
                    screen,
                    helpBackground,
                    ScaleMode.ScaleAndCrop,
                    true);
            }
            else
            {
                DrawRect(screen, new Color(0.06f, 0.05f, 0.13f));
            }

            GUI.color = new Color(0.03f, 0.025f, 0.08f, 0.38f);
            GUI.DrawTexture(screen, Texture2D.whiteTexture);
            GUI.color = previous;

            float panelWidth = Mathf.Min(1120f, Screen.width - 64f);
            float panelHeight = Mathf.Min(760f, Screen.height - 64f);
            Rect panel = new Rect(
                (Screen.width - panelWidth) * 0.5f,
                (Screen.height - panelHeight) * 0.5f,
                panelWidth,
                panelHeight);
            DrawRect(panel, new Color(0.96f, 0.90f, 0.78f, 0.98f));
            DrawBorder(panel, 5f, new Color(0.56f, 0.28f, 0.12f));

            int index = openSection - 1;
            Rect content = new Rect(
                panel.x + 42f,
                panel.y + 26f,
                panel.width - 84f,
                panel.height - 52f);
            GUI.Label(
                new Rect(content.x, content.y, content.width, 34f),
                $"TUTORIAL / T{openSection}",
                sectionLabelStyle);
            GUI.Label(
                new Rect(content.x, content.y + 34f, content.width, 58f),
                SectionTitles[index],
                titleStyle);
            GUI.Label(
                new Rect(content.x, content.y + 92f, content.width, 54f),
                SectionSummaries[index],
                summaryStyle);

            Rect illustration = new Rect(
                content.x,
                content.y + 154f,
                content.width * 0.58f,
                content.height - 224f);
            DrawRect(illustration, new Color(0.075f, 0.055f, 0.15f, 0.98f));
            DrawBorder(illustration, 3f, new Color(0.24f, 0.16f, 0.36f));
            DrawSectionIllustration(illustration, openSection);

            Rect steps = new Rect(
                illustration.xMax + 32f,
                illustration.y + 16f,
                content.xMax - illustration.xMax - 32f,
                illustration.height - 32f);
            GUI.Label(steps, SectionSteps[index], stepStyle);

            Rect closeRect = new Rect(
                content.xMax - 260f,
                content.yMax - 56f,
                260f,
                50f);
            HimoHitoUiParts.DrawWoodButtonLabel(
                closeRect,
                "Z　看板を閉じる",
                closeStyle);
        }

        private void DrawSectionIllustration(Rect rect, int section)
        {
            Rect inner = new Rect(
                rect.x + 24f,
                rect.y + 24f,
                rect.width - 48f,
                rect.height - 48f);
            switch (section)
            {
                case 1:
                    DrawSectionOneIllustration(inner);
                    break;
                case 2:
                    DrawSectionTwoIllustration(inner);
                    break;
                case 3:
                    DrawSectionThreeIllustration(inner);
                    break;
                case 4:
                    DrawSectionFourIllustration(inner);
                    break;
            }
        }

        private void DrawSectionOneIllustration(Rect rect)
        {
            Rect leftFloor = NormalizedRect(rect, 0.02f, 0.70f, 0.30f, 0.22f);
            Rect rightFloor = NormalizedRect(rect, 0.70f, 0.70f, 0.28f, 0.22f);
            DrawPlatform(leftFloor);
            DrawPlatform(rightFloor);

            Rect hook = NormalizedRect(rect, 0.47f, 0.08f, 0.10f, 0.13f);
            Rect player = NormalizedRect(rect, 0.31f, 0.42f, 0.10f, 0.23f);
            Vector2 ropePoint = new Vector2(
                player.center.x,
                player.y + player.height * 0.22f);
            Vector2 landingPoint = new Vector2(
                rightFloor.x + rightFloor.width * 0.30f,
                rightFloor.y - rect.height * 0.04f);

            DrawDashedCurve(
                player.center,
                new Vector2(rect.center.x, rect.y + rect.height * 0.82f),
                landingPoint,
                4f,
                new Color(0.45f, 0.95f, 0.76f));
            DrawYarnLine(
                ropePoint,
                hook.center,
                6f,
                new Color(1f, 0.36f, 0.56f));
            DrawPlayer(player);
            DrawRing(hook, new Color(0.24f, 0.64f, 0.96f));

            GUI.Label(
                NormalizedRect(rect, 0.12f, 0.20f, 0.30f, 0.12f),
                "Eで掛ける",
                diagramLabelStyle);
            GUI.Label(
                NormalizedRect(rect, 0.43f, 0.80f, 0.38f, 0.10f),
                "A / Dで振る　→ 対岸へ",
                diagramLabelStyle);
        }

        private void DrawSectionTwoIllustration(Rect rect)
        {
            Rect leftFloor = NormalizedRect(rect, 0.01f, 0.68f, 0.29f, 0.24f);
            Rect rightFloor = NormalizedRect(rect, 0.70f, 0.68f, 0.29f, 0.24f);
            DrawPlatform(leftFloor);
            DrawPlatform(rightFloor);
            Rect hook = NormalizedRect(rect, 0.45f, 0.07f, 0.10f, 0.13f);
            GUIStyle spikeStyle = new GUIStyle(diagramLabelStyle)
            {
                fontSize = 28
            };
            spikeStyle.normal.textColor = new Color(1f, 0.28f, 0.36f);
            GUI.Label(
                NormalizedRect(rect, 0.38f, 0.76f, 0.24f, 0.14f),
                "▲ ▲ ▲",
                spikeStyle);
            DrawCurve(
                new Vector2(leftFloor.xMax, leftFloor.y),
                new Vector2(rect.center.x, rect.y + rect.height * 0.46f),
                new Vector2(rightFloor.x, rightFloor.y),
                6f,
                new Color(0.45f, 0.95f, 0.76f));
            DrawCurve(
                new Vector2(leftFloor.xMax, leftFloor.y + 10f),
                new Vector2(rect.center.x, rect.y + rect.height * 0.94f),
                new Vector2(rightFloor.x, rightFloor.y + 10f),
                4f,
                new Color(1f, 0.36f, 0.56f, 0.72f));
            Rect player = NormalizedRect(rect, 0.46f, 0.46f, 0.08f, 0.21f);
            DrawYarnLine(
                hook.center,
                new Vector2(player.center.x, player.y + player.height * 0.20f),
                6f,
                new Color(1f, 0.36f, 0.56f));
            DrawPlayer(player);
            DrawRing(hook, new Color(0.24f, 0.64f, 0.96f));
            GUI.Label(
                NormalizedRect(rect, 0.05f, 0.24f, 0.34f, 0.12f),
                "長さ6～7　通れる",
                diagramLabelStyle);
            GUI.Label(
                NormalizedRect(rect, 0.59f, 0.46f, 0.35f, 0.12f),
                "長さ8　トゲへ",
                diagramLabelStyle);
        }

        private void DrawSectionThreeIllustration(Rect rect)
        {
            Rect leftFloor = NormalizedRect(rect, 0.01f, 0.62f, 0.31f, 0.30f);
            Rect rightFloor = NormalizedRect(rect, 0.68f, 0.62f, 0.31f, 0.30f);
            DrawPlatform(leftFloor);
            DrawPlatform(rightFloor);
            Rect leftRing = new Rect(leftFloor.xMax - 18f, leftFloor.y - 18f, 36f, 36f);
            Rect rightRing = new Rect(rightFloor.x - 18f, rightFloor.y - 18f, 36f, 36f);
            DrawRing(leftRing, new Color(0.33f, 1f, 0.76f));
            DrawRing(rightRing, new Color(0.33f, 1f, 0.76f));
            DrawCurve(
                leftRing.center,
                new Vector2(rect.center.x, rect.y + rect.height * 0.72f),
                rightRing.center,
                9f,
                Color.white,
                true);
            DrawPlayer(NormalizedRect(rect, 0.47f, 0.45f, 0.08f, 0.22f));
            GUI.Label(
                NormalizedRect(rect, 0.34f, 0.16f, 0.32f, 0.14f),
                "E → Q　足場化",
                diagramLabelStyle);
            GUI.Label(
                NormalizedRect(rect, 0.38f, 0.80f, 0.24f, 0.10f),
                "消費6",
                diagramLabelStyle);
        }

        private void DrawSectionFourIllustration(Rect rect)
        {
            Rect leftFloor = NormalizedRect(rect, 0.01f, 0.68f, 0.22f, 0.24f);
            Rect rightFloor = NormalizedRect(rect, 0.77f, 0.68f, 0.22f, 0.24f);
            DrawPlatform(leftFloor);
            DrawPlatform(rightFloor);
            Rect leftRing = new Rect(leftFloor.xMax - 16f, leftFloor.y - 16f, 32f, 32f);
            Rect centerRing = NormalizedRect(rect, 0.47f, 0.24f, 0.07f, 0.10f);
            Rect rightRing = new Rect(rightFloor.x - 16f, rightFloor.y - 16f, 32f, 32f);
            DrawRing(leftRing, new Color(0.33f, 1f, 0.76f));
            DrawRing(centerRing, new Color(0.24f, 0.64f, 0.96f));
            DrawRing(rightRing, new Color(0.33f, 1f, 0.76f));
            DrawCurve(
                leftRing.center,
                new Vector2(rect.x + rect.width * 0.35f, rect.y + rect.height * 0.60f),
                centerRing.center,
                5f,
                new Color(1f, 1f, 1f, 0.45f),
                true);
            DrawCurve(
                centerRing.center,
                new Vector2(rect.x + rect.width * 0.65f, rect.y + rect.height * 0.60f),
                rightRing.center,
                5f,
                new Color(1f, 1f, 1f, 0.45f),
                true);
            DrawCurve(
                leftRing.center,
                new Vector2(rect.center.x, rect.y + rect.height * 0.88f),
                rightRing.center,
                9f,
                Color.white,
                true);
            DrawRect(
                NormalizedRect(rect, 0.61f, 0.30f, 0.10f, 0.30f),
                new Color(0.48f, 0.26f, 0.12f));
            DrawPlayer(NormalizedRect(rect, 0.45f, 0.57f, 0.08f, 0.21f));
            GUI.Label(
                NormalizedRect(rect, 0.42f, 0.06f, 0.16f, 0.12f),
                "Fで外す",
                diagramLabelStyle);
            GUI.Label(
                NormalizedRect(rect, 0.27f, 0.82f, 0.46f, 0.10f),
                "1本になって梁の下へ",
                diagramLabelStyle);
        }

        private void DrawPlayer(Rect rect)
        {
            if (playerSprite != null && playerSprite.texture != null)
            {
                Texture2D texture = playerSprite.texture;
                Rect source = playerSprite.textureRect;
                float sourceAspect = source.width /
                    Mathf.Max(1f, source.height);
                Rect fitted = FitAspect(rect, sourceAspect);
                Rect uv = new Rect(
                    source.x / texture.width,
                    source.y / texture.height,
                    source.width / texture.width,
                    source.height / texture.height);
                GUI.DrawTextureWithTexCoords(fitted, texture, uv, true);
                return;
            }
            DrawRect(rect, new Color(1f, 0.36f, 0.56f));
        }

        private static Rect FitAspect(Rect bounds, float aspect)
        {
            float boundsAspect = bounds.width / Mathf.Max(1f, bounds.height);
            if (boundsAspect > aspect)
            {
                float width = bounds.height * aspect;
                return new Rect(
                    bounds.center.x - width * 0.5f,
                    bounds.y,
                    width,
                    bounds.height);
            }

            float height = bounds.width / Mathf.Max(0.01f, aspect);
            return new Rect(
                bounds.x,
                bounds.center.y - height * 0.5f,
                bounds.width,
                height);
        }

        private static void DrawPlatform(Rect rect)
        {
            DrawRect(rect, new Color(0.66f, 0.31f, 0.10f));
            DrawRect(
                new Rect(rect.x, rect.y, rect.width, Mathf.Min(12f, rect.height)),
                new Color(0.95f, 0.52f, 0.14f));
        }

        private static void DrawRing(Rect rect, Color color)
        {
            if (ringTexture == null)
            {
                ringTexture = CreateRingTexture();
            }
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, ringTexture, ScaleMode.StretchToFill, true);
            GUI.color = previous;
        }

        private static Texture2D CreateRingTexture()
        {
            const int size = 64;
            Texture2D texture = new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                false)
            {
                name = "Tutorial Guide Ring",
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            Color32[] pixels = new Color32[size * size];
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    byte alpha = distance >= 18f && distance <= 29f
                        ? (byte)255
                        : (byte)0;
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        private static void DrawCurve(
            Vector2 start,
            Vector2 control,
            Vector2 end,
            float width,
            Color color,
            bool textured = false)
        {
            const int segments = 28;
            Vector2 previous = start;
            float distance = 0f;
            for (int index = 1; index <= segments; index++)
            {
                float t = index / (float)segments;
                float inverse = 1f - t;
                Vector2 current =
                    inverse * inverse * start +
                    2f * inverse * t * control +
                    t * t * end;
                if (textured)
                    DrawYarnLine(previous, current, width, color, distance);
                else
                    DrawLine(previous, current, width, color);
                distance += Vector2.Distance(previous, current);
                previous = current;
            }
        }

        private static void DrawYarnLine(
            Vector2 start, Vector2 end, float width, Color color, float distance = 0f)
        {
            Texture2D yarn = YarnRopeTexture.Load();
            if (yarn == null)
            {
                DrawLine(start, end, width, new Color(1f, 0.36f, 0.56f, color.a));
                return;
            }
            Vector2 delta = end - start;
            if (delta.sqrMagnitude < 0.01f) return;
            float tileLength = width * yarn.width / yarn.height;
            Matrix4x4 previousMatrix = GUI.matrix;
            Color previousColor = GUI.color;
            // Retain the source pink; alpha distinguishes the old pair of bridges in T4.
            GUI.color = new Color(1f, 1f, 1f, color.a);
            GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, start);
            GUI.DrawTextureWithTexCoords(
                new Rect(start.x, start.y - width * 0.5f, delta.magnitude, width),
                yarn,
                new Rect(distance / tileLength, 0f, delta.magnitude / tileLength, 1f),
                true);
            GUI.matrix = previousMatrix;
            GUI.color = previousColor;
        }

        private static void DrawDashedCurve(
            Vector2 start,
            Vector2 control,
            Vector2 end,
            float width,
            Color color)
        {
            const int segments = 28;
            Vector2 previous = start;
            for (int index = 1; index <= segments; index++)
            {
                float t = index / (float)segments;
                float inverse = 1f - t;
                Vector2 current =
                    inverse * inverse * start +
                    2f * inverse * t * control +
                    t * t * end;
                if ((index / 2) % 2 == 0)
                {
                    DrawLine(previous, current, width, color);
                }
                previous = current;
            }
        }

        private static void DrawLine(
            Vector2 start,
            Vector2 end,
            float width,
            Color color)
        {
            Vector2 delta = end - start;
            if (delta.sqrMagnitude < 0.01f)
            {
                return;
            }

            Matrix4x4 previousMatrix = GUI.matrix;
            Color previousColor = GUI.color;
            GUI.color = color;
            GUIUtility.RotateAroundPivot(
                Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg,
                start);
            GUI.DrawTexture(
                new Rect(start.x, start.y - width * 0.5f, delta.magnitude, width),
                Texture2D.whiteTexture);
            GUI.matrix = previousMatrix;
            GUI.color = previousColor;
        }

        private static Rect NormalizedRect(
            Rect parent,
            float x,
            float y,
            float width,
            float height)
        {
            return new Rect(
                parent.x + parent.width * x,
                parent.y + parent.height * y,
                parent.width * width,
                parent.height * height);
        }

        private static void DrawRect(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private static void DrawBorder(Rect rect, float width, Color color)
        {
            DrawRect(new Rect(rect.x, rect.y, rect.width, width), color);
            DrawRect(new Rect(rect.x, rect.yMax - width, rect.width, width), color);
            DrawRect(new Rect(rect.x, rect.y, width, rect.height), color);
            DrawRect(new Rect(rect.xMax - width, rect.y, width, rect.height), color);
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            Color ink = new Color(0.11f, 0.08f, 0.20f);
            sectionLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.78f, 0.20f, 0.38f) }
            };
            promptStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = ink }
            };
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 34,
                fontStyle = FontStyle.Bold,
                normal = { textColor = ink }
            };
            summaryStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 18,
                wordWrap = true,
                normal = { textColor = new Color(0.26f, 0.22f, 0.36f) }
            };
            stepStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = ink }
            };
            diagramLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = new Color(0.52f, 1f, 0.82f) }
            };
            closeStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = ink }
            };
            HimoHitoGuiTheme.ApplyToStyles(
                sectionLabelStyle,
                promptStyle,
                titleStyle,
                summaryStyle,
                stepStyle,
                diagramLabelStyle,
                closeStyle);
        }
    }
}
