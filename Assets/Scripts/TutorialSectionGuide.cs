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
        private const string CraftSignArtworkPath = "Art/HimoHitoCraftGuideSign-v1";
        private static Sprite craftSignSprite;
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
            "1　W / Sで長さ7にする（推奨）\n2　対岸の緑フックへ E → Qで足場にする\n3　残量を7使ってできた橋を歩いて渡る",
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
        private float illustrationTime;
        private GUIStyle sectionLabelStyle;
        private GUIStyle promptStyle;
        private GUIStyle titleStyle;
        private GUIStyle summaryStyle;
        private GUIStyle stepStyle;
        private GUIStyle diagramLabelStyle;
        private GUIStyle closeStyle;

        public bool IsVisible => openSection > 0;
        private Transform glanceSign;
        private int glanceSection;
        private readonly TutorialSignGreeting[] signGreetings = new TutorialSignGreeting[4];

        // A read-only presentation target. Opening/closing the guide remains user-controlled.
        public bool TryGetGlanceTarget(out Vector2 position, out int section)
        {
            position = default; section = 0;
            if (IsVisible || runController == null || playerBody == null ||
                runController.Outcome != PrototypeRunController.RunOutcome.Playing ||
                (overlayControls != null && overlayControls.IsOverlayVisible)) return false;
            int current = runController.CurrentTutorialSection;
            if (!IsNearSign(current)) return false;
            if (glanceSign == null || glanceSection != current)
            {
                GameObject sign = SceneObjectLookup.Find(SignPrefix + current, "Tutorial");
                glanceSign = sign != null ? sign.transform : null;
                glanceSection = current;
            }
            if (glanceSign == null || !glanceSign.gameObject.activeInHierarchy) return false;
            position = (Vector2)glanceSign.position + Vector2.up * .5f;
            section = current;
            return true;
        }

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
            playerSprite = CraftPlayerArt.LoadStandingSprite(PlayerTexturePath);
        }

        private void Update()
        {
            if (StageStartTransition.IsActive) return;
            UpdateSignGreetings();
            if (MainStagePreview.IsActive) return;

            if (runController == null || playerBody == null)
            {
                return;
            }

            if (IsVisible)
            {
                AdvanceIllustration(Time.unscaledDeltaTime);
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

        private void UpdateSignGreetings()
        {
            if (playerBody == null || runController == null) return;
            bool permitted = !MainStagePreview.IsActive && !IsVisible && playerBody.simulated &&
                runController.Outcome == PrototypeRunController.RunOutcome.Playing &&
                (overlayControls == null || !overlayControls.IsOverlayVisible);
            for (int index = 0; index < signGreetings.Length; index++)
            {
                if (signGreetings[index] == null)
                {
                    GameObject sign = SceneObjectLookup.Find(SignPrefix + (index + 1), "Tutorial");
                    Transform visual = sign != null ? sign.transform.Find(SignArtworkName) : null;
                    if (visual == null || !visual.gameObject.activeInHierarchy) continue;
                    if (!visual.TryGetComponent(out signGreetings[index]))
                        signGreetings[index] = visual.gameObject.AddComponent<TutorialSignGreeting>();
                }
                signGreetings[index].Tick(playerBody.position, SignPositions[index], InteractionRange,
                    permitted && runController.CurrentTutorialSection == index + 1, Time.deltaTime);
            }
        }

        private void OnDisable()
        {
            foreach (TutorialSignGreeting greeting in signGreetings)
                if (greeting != null) greeting.Cancel();
            if (IsVisible)
            {
                CloseGuide();
            }
        }

        private void OnGUI()
        {
            if (StageStartTransition.IsActive) return;
            if (MainStagePreview.IsActive) return;

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
                Transform board = sign.transform.Find(SignArtworkName);
                if (board != null && board.gameObject.activeSelf)
                {
                    if (!board.TryGetComponent(out TutorialSignInscription inscription))
                    {
                        inscription = board.gameObject.AddComponent<TutorialSignInscription>();
                        changed = true;
                    }
                    changed |= inscription.Configure(section);
                }
            }
            return changed;
        }

        private static bool EnsureSignArtwork(Transform parent)
        {
            bool changed = false;
            Sprite artwork = GetSignArtwork();
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

        private static Sprite GetSignArtwork()
        {
            if (craftSignSprite != null && craftSignSprite.texture != null) return craftSignSprite;
            if (Resources.Load<Texture2D>(CraftSignArtworkPath) != null)
            {
                Sprite cutout = TutorialFirstSectionVisuals.LoadProcessedToySprite(CraftSignArtworkPath, true);
                if (cutout != null)
                {
                    // Keep the square canvas: inscription and greeting use its normalized coordinates.
                    Texture2D texture = cutout.texture;
                    craftSignSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                        new Vector2(.5f, .5f), 100f, 0, SpriteMeshType.FullRect);
                    craftSignSprite.name = "Craft Guide Sign Full Canvas";
                    craftSignSprite.hideFlags = HideFlags.HideAndDontSave;
                    return craftSignSprite;
                }
            }
            return Resources.Load<Sprite>(SignArtworkPath);
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
            GameObject sign = SceneObjectLookup.Find(SignPrefix + section, "Tutorial");
            Vector2 target = sign != null ? (Vector2)sign.transform.position : SignPositions[section - 1];
            openSection = section;
            illustrationTime = 0f;
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
            // Disabling rope controls cancels action poses, so set the reading gaze afterwards.
            GetComponent<RopeBodyVisual>()?.BeginReadingSign(target + Vector2.up * .5f);
            Time.timeScale = 0f;
        }

        private void CloseGuide()
        {
            GetComponent<RopeBodyVisual>()?.EndReadingSign();
            openSection = 0;
            illustrationTime = 0f;
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

        private void AdvanceIllustration(float unscaledDelta)
        {
            // Advance once per Update, never per IMGUI layout/repaint event.
            if (IsVisible)
                illustrationTime = Mathf.Repeat(illustrationTime + Mathf.Clamp(unscaledDelta, 0f, .1f),
                    TutorialGuideAnimation.Duration(openSection));
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
            var f = TutorialGuideAnimation.Sample(1, illustrationTime);
            DrawSwingBanks(rect);
            Vector2 hook = DiagramPoint(rect, new Vector2(.5f,.15f));
            DrawDashedCurve(DiagramPoint(rect,TutorialGuideAnimation.Swing(0f)),
                DiagramPoint(rect,new Vector2(.5f,.88f)),
                DiagramPoint(rect,TutorialGuideAnimation.Swing(1f)), 2f, new Color(.45f,.95f,.76f,.3f));
            DrawDemoConnection(rect, f, hook);
            DrawDemoPlayer(rect, f);
            DrawDemoRing(hook, new Color(.24f,.64f,.96f));
            DrawDemoCaption(rect, f.Caption);
        }

        private void DrawSectionTwoIllustration(Rect rect)
        {
            var f = TutorialGuideAnimation.Sample(2, illustrationTime);
            DrawSwingBanks(rect);
            Vector2 hook = DiagramPoint(rect, new Vector2(.5f,.15f));
            for (int path=0;path<2;path++)
            {
                bool danger=path==1;
                DrawDashedCurve(DiagramPoint(rect,TutorialGuideAnimation.Swing(0f,danger)),
                    DiagramPoint(rect,new Vector2(.5f,danger ? 1.08f : .88f)),
                    DiagramPoint(rect,TutorialGuideAnimation.Swing(1f,danger)), 2f,
                    danger ? new Color(1f,.36f,.56f,.32f) : new Color(.45f,.95f,.76f,.45f));
            }
            for (int i=0;i<3;i++)
            {
                Vector2 peak=DiagramPoint(rect,new Vector2(.44f+i*.06f,.83f));
                float width=rect.width*.05f, height=rect.height*.065f;
                for(int row=0;row<12;row++)
                {
                    float fraction=(row+1)/12f;
                    DrawRect(new Rect(peak.x-width*fraction*.5f,peak.y+height*row/12f,
                        width*fraction,height/12f+1f),new Color(1f,.28f,.36f));
                }
            }
            DrawDemoConnection(rect,f,hook);
            DrawDemoPlayer(rect,f);
            DrawDemoRing(hook,new Color(.24f,.64f,.96f));
            DrawDemoCaption(rect,f.Caption);
        }

        private void DrawSectionThreeIllustration(Rect rect)
        {
            var f=TutorialGuideAnimation.Sample(3,illustrationTime);
            DrawPlatform(NormalizedRect(rect,.02f,.66f,.28f,.25f));
            DrawPlatform(NormalizedRect(rect,.72f,.66f,.26f,.25f));
            Vector2 left=DiagramPoint(rect,new Vector2(.30f,.66f));
            Vector2 right=DiagramPoint(rect,new Vector2(.72f,.66f));
            DrawDemoConnection(rect,f,right);
            Vector2 previous=left;
            float distance=0f;
            for(int i=1;i<=36;i++)
            {
                float t=i/36f;
                Vector2 point=DiagramPoint(rect,TutorialGuideAnimation.BridgePoint(t));
                DrawYarnLine(previous,point,8f,new Color(1f,1f,1f,
                    f.Opacity*(t<=f.Bridge ? 1f : .12f)),distance);
                distance+=Vector2.Distance(previous,point);
                previous=point;
            }
            DrawDemoRing(left,new Color(.33f,1f,.76f));
            DrawDemoRing(right,new Color(.33f,1f,.76f));
            DrawDemoPlayer(rect,f);
            DrawDemoCaption(rect,f.Caption);
            GUI.Label(NormalizedRect(rect,.2f,.88f,.6f,.1f),
                f.Bridge>0f ? "残量の例　99 → 93（戻らない）" : "掛けるだけなら消費なし",
                diagramLabelStyle);
        }

        private void DrawSectionFourIllustration(Rect rect)
        {
            var f=TutorialGuideAnimation.Sample(4,illustrationTime);
            DrawPlatform(NormalizedRect(rect,.02f,.67f,.22f,.25f));
            DrawPlatform(NormalizedRect(rect,.76f,.67f,.22f,.25f));
            Vector2 previous=DiagramPoint(rect,TutorialGuideAnimation.MergedPoint(0f,f.Merge));
            float distance=0f;
            for(int i=1;i<=48;i++)
            {
                float along=i/48f;
                Vector2 point=DiagramPoint(rect,TutorialGuideAnimation.MergedPoint(along,f.Merge));
                float built=along<=.5f ? f.LeftBridge : f.RightBridge;
                float localProgress=along<=.5f ? along*2f : (along-.5f)*2f;
                float alpha=built<=0f ? 0f : localProgress<=built ? 1f : .12f;
                DrawYarnLine(previous,point,8f,new Color(1f,1f,1f,f.Opacity*alpha),distance);
                distance+=Vector2.Distance(previous,point);
                previous=point;
            }
            DrawPlatform(NormalizedRect(rect,.64f,.23f,.075f,.27f));
            GUI.Label(NormalizedRect(rect,.62f,.15f,.12f,.08f),"梁",diagramLabelStyle);
            DrawDemoRing(DiagramPoint(rect,new Vector2(.24f,.67f)),new Color(.33f,1f,.76f));
            DrawDemoRing(DiagramPoint(rect,new Vector2(.76f,.67f)),new Color(.33f,1f,.76f));
            DrawDemoRing(DiagramPoint(rect,new Vector2(.5f,.35f)),
                new Color(.24f,.64f,.96f,(1f-f.Merge)*f.Opacity));
            if(f.Connection>0f)
                DrawDemoConnection(rect,f,DiagramPoint(rect,f.ConnectionTarget));
            DrawDemoPlayer(rect,f);
            DrawDemoCaption(rect,f.Caption);
        }

        private static Vector2 DiagramPoint(Rect rect, Vector2 normalized) =>
            new Vector2(rect.x+rect.width*normalized.x,rect.y+rect.height*normalized.y);

        private static void DrawSwingBanks(Rect rect)
        {
            DrawPlatform(NormalizedRect(rect,.02f,.64f,.27f,.28f));
            DrawPlatform(NormalizedRect(rect,.71f,.64f,.27f,.28f));
        }

        private static void DrawDemoRing(Vector2 center, Color color)
        {
            DrawRing(new Rect(center.x-17f,center.y-17f,34f,34f),color);
        }

        private Rect DemoPlayerRect(Rect rect, Vector2 feet)
        {
            float height=rect.height*.17f;
            float aspect=playerSprite!=null ? playerSprite.rect.width/Mathf.Max(1f,playerSprite.rect.height) : .55f;
            Vector2 position=DiagramPoint(rect,feet);
            return new Rect(position.x-height*aspect*.5f,position.y-height,height*aspect,height);
        }

        private void DrawDemoPlayer(Rect rect, TutorialGuideAnimation.Frame frame)
        {
            Color previous=GUI.color;
            GUI.color=new Color(previous.r,previous.g,previous.b,previous.a*frame.Opacity);
            DrawPlayer(DemoPlayerRect(rect,frame.Feet));
            GUI.color=previous;
        }

        private void DrawDemoConnection(Rect rect, TutorialGuideAnimation.Frame frame, Vector2 hook)
        {
            Vector2 head=DemoPlayerCrown(rect,frame.Feet);
            DrawYarnLine(head,Vector2.Lerp(head,hook,frame.Connection),5f,
                new Color(1f,1f,1f,frame.Opacity*frame.Connection));
        }

        private Vector2 DemoPlayerCrown(Rect rect, Vector2 feet)
        {
            Rect player = DemoPlayerRect(rect, feet);
            if (!CraftPlayerArt.TryGetStandingCrownPoint(playerSprite, out Vector2 crown))
                return new Vector2(player.center.x, player.y + player.height * .06f);
            Bounds bounds = playerSprite.bounds;
            float x = (crown.x - bounds.min.x) / Mathf.Max(.001f, bounds.size.x);
            float y = (crown.y - bounds.min.y) / Mathf.Max(.001f, bounds.size.y);
            // IMGUI points downward; the sprite's local Y points upward.
            return new Vector2(player.x + player.width * x, player.yMax - player.height * y);
        }

        private void DrawDemoCaption(Rect rect, string caption)
        {
            GUI.Label(NormalizedRect(rect,0f,-.015f,1f,.12f),caption,diagramLabelStyle);
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
