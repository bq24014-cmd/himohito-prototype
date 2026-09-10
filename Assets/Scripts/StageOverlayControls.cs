using UnityEngine;
using UnityEngine.SceneManagement;

namespace HimoHito
{
    /// <summary>Tab opens the compact control sheet; Esc pauses the run.</summary>
    public sealed class StageOverlayControls : MonoBehaviour
    {
        private bool showHelp;
        private bool isPaused;
        private bool confirmReturnToTitle;
        private int lastHelpInputFrame = -1;
        private PrototypeRunController runController;
        private MainStageRespawnOnFall mainStageRespawn;
        private MainStageGoalZone mainStageGoal;
        private TutorialSectionGuide tutorialSectionGuide;
        private PrototypeAudioFeedback audioFeedback;
        private Texture2D helpBackground;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle cornerHintStyle;
        private static readonly GUIContent CornerHint = new GUIContent("Tab：操作確認　Esc：ポーズ");
        private static readonly GUIContent NarrowCornerHint = new GUIContent("Tab：操作確認\nEsc：ポーズ");
        private GUIStyle helpTitleStyle;
        private GUIStyle helpBodyStyle;
        private GUIStyle helpKeyStyle;
        private Vector2 pauseScroll;

        public bool IsHelpVisible => showHelp;
        public bool IsOverlayVisible => showHelp || isPaused;
        public bool HelpInputConsumedThisFrame => lastHelpInputFrame == Time.frameCount;

        private void Awake()
        {
            runController = FindFirstObjectByType<PrototypeRunController>();
            mainStageRespawn = FindFirstObjectByType<MainStageRespawnOnFall>();
            mainStageGoal = FindFirstObjectByType<MainStageGoalZone>();
            tutorialSectionGuide =
                FindFirstObjectByType<TutorialSectionGuide>();
            audioFeedback = FindFirstObjectByType<PrototypeAudioFeedback>();
            helpBackground = Resources.Load<Texture2D>(
                "Art/HimoHitoControlsBackground-v1");
        }

        private void Update()
        {
            if (StageStartTransition.IsActive) return;
            if (MainStagePreview.IsActive) return;

            if (IsResultScreenActive())
            {
                if (IsOverlayVisible)
                {
                    showHelp = false;
                    isPaused = false;
                    ApplyPauseState();
                }
                return;
            }
            if (tutorialSectionGuide == null)
            {
                tutorialSectionGuide =
                    FindFirstObjectByType<TutorialSectionGuide>();
            }
            if (tutorialSectionGuide != null &&
                tutorialSectionGuide.IsVisible)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (showHelp) CloseHelp();
                else OpenHelp();
                return;
            }

            if (showHelp && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseHelp();
                return;
            }

            bool isOnTitleScreen = runController != null &&
                runController.Outcome == PrototypeRunController.RunOutcome.WaitingToStart;
            if (isOnTitleScreen)
            {
                return;
            }

            if (isPaused && !showHelp && confirmReturnToTitle)
            {
                if (Input.GetKeyDown(KeyCode.Escape)) confirmReturnToTitle = false;
                else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    ReturnToTitle();
                return;
            }

            if (isPaused && !showHelp && Input.GetKeyDown(KeyCode.T))
            {
                confirmReturnToTitle = true;
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                isPaused = !isPaused;
                if (isPaused)
                {
                    PlayOpenSound();
                }
                ApplyPauseState();
            }
        }

        public void OpenMenuHelp()
        {
            if (StageStartTransition.IsActive) return;
            if (runController == null || !runController.IsStageSelectionOpen || IsOverlayVisible) return;
            OpenHelp();
        }

        private void OpenHelp()
        {
            confirmReturnToTitle = false;
            showHelp = true;
            lastHelpInputFrame = Time.frameCount;
            PlayOpenSound();
            ApplyPauseState();
        }

        public void CloseHelp()
        {
            if (!showHelp) return;
            showHelp = false;
            lastHelpInputFrame = Time.frameCount;
            ApplyPauseState();
        }

        private void PlayOpenSound()
        {
            if (audioFeedback == null)
            {
                audioFeedback = FindFirstObjectByType<PrototypeAudioFeedback>();
            }
            audioFeedback?.PlayUiPaperOpened();
        }

        private void OnDisable()
        {
            HimoHitoAudioSettings.Save();
            if (isPaused || showHelp)
            {
                Time.timeScale = 1f;
                isPaused = false;
                showHelp = false;
            }
        }

        private void OnApplicationQuit() => HimoHitoAudioSettings.Save();
        private void OnApplicationPause(bool paused)
        {
            if (paused) HimoHitoAudioSettings.Save();
        }

        private void OnGUI()
        {
            if (StageStartTransition.IsActive) return;
            if (MainStagePreview.IsActive) return;

            if (IsResultScreenActive() ||
                (tutorialSectionGuide != null && tutorialSectionGuide.IsVisible))
            {
                return;
            }
            // Lower IMGUI depth values are drawn in front. Keep the help sheet
            // above the title HUD regardless of component creation order.
            GUI.depth = -1000;
            HimoHitoGuiTheme.ApplyToSkin(GUI.skin);
            EnsureStyles();
            bool isOnTitleScreen = runController != null &&
                runController.Outcome == PrototypeRunController.RunOutcome.WaitingToStart;
            if (!isOnTitleScreen && !IsOverlayVisible)
            {
                DrawCornerHint();
            }

            if (!showHelp && !isPaused)
            {
                return;
            }

            if (showHelp)
            {
                DrawHelpScreen();
                return;
            }

            DrawPauseScreen();
        }

        private void DrawCornerHint()
        {
            // The pause body's wrapping/centering must not be used in a
            // fixed-height corner label: two lines were clipped top/bottom.
            Rect safe = Screen.safeArea;
            if (safe.width <= 0f || safe.height <= 0f)
                safe = new Rect(0f, 0f, Screen.width, Screen.height);
            const float margin = 18f;
            float availableWidth = Mathf.Max(1f, safe.width - margin * 2f);
            cornerHintStyle.fontSize = 15;
            GUIContent content = CornerHint;
            Vector2 size = cornerHintStyle.CalcSize(content);
            if (size.x > availableWidth)
            {
                content = NarrowCornerHint;
                size = cornerHintStyle.CalcSize(content);
            }
            while (size.x > availableWidth && cornerHintStyle.fontSize > 8)
            {
                cornerHintStyle.fontSize--;
                size = cornerHintStyle.CalcSize(content);
            }
            size.x = Mathf.Min(size.x, availableWidth);
            // safeArea is bottom-origin; IMGUI coordinates are top-origin.
            GUI.Label(new Rect(safe.xMax - margin - size.x,
                Screen.height - safe.yMax + margin, size.x, size.y),
                content, cornerHintStyle);
        }

        private void DrawHelpScreen()
        {
            Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
            Color previous = GUI.color;
            if (helpBackground != null)
            {
                GUI.DrawTexture(
                    screenRect,
                    helpBackground,
                    ScaleMode.ScaleAndCrop,
                    true);
            }
            else
            {
                GUI.color = new Color(0.94f, 0.85f, 0.70f, 1f);
                GUI.Box(screenRect, GUIContent.none);
            }
            GUI.color = previous;

            float width = Mathf.Min(Screen.width * 0.56f, 980f);
            float height = Mathf.Min(Screen.height * 0.65f, 700f);
            GUILayout.BeginArea(new Rect(
                (Screen.width - width) * 0.5f,
                Screen.height * 0.145f,
                width,
                height));
            GUILayout.Label("操作方法", helpTitleStyle);
            GUILayout.Space(18f);
            GUILayout.Label(
                "ヒモを掛けて振り、必要な場所では足場として残します",
                helpBodyStyle);
            GUILayout.Space(28f);

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(width * 0.48f));
            DrawHelpRow("A / D", "移動・振り子を加速");
            DrawHelpRow("Space", "ジャンプ");
            DrawHelpRow("W / S", "次に使う長さを変更");
            DrawHelpRow("矢印キー", "照準方向を変更");
            GUILayout.EndVertical();
            GUILayout.Space(width * 0.04f);
            GUILayout.BeginVertical(GUILayout.Width(width * 0.48f));
            DrawHelpRow("E", "ヒモを接続・解除");
            DrawHelpRow("Q", "接続中のヒモを足場化");
            DrawHelpRow("F", "中央Hookを外してまとめる");
            DrawHelpRow("R", "現在の区間から再挑戦");
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            bool close = GUILayout.Button("Tab / Esc　閉じる", helpKeyStyle);
            GUILayout.EndArea();
            if (close) { CloseHelp(); GUIUtility.ExitGUI(); }
        }

        private void DrawHelpRow(string key, string description)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(key, helpKeyStyle, GUILayout.Width(112f));
            GUILayout.Label(description, helpBodyStyle);
            GUILayout.EndHorizontal();
            GUILayout.Space(12f);
        }

        private void DrawPauseScreen()
        {
            Color previous = GUI.color;
            GUI.color = new Color(0.03f, 0.035f, 0.075f, 0.94f);
            GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none);
            GUI.color = previous;

            // Scale this panel as a whole: labels, slider handles and hit areas grow together.
            // Keep the help sheet and gameplay HUD at their existing sizes.
            Matrix4x4 previousMatrix = GUI.matrix;
            float panelScale = Mathf.Min(1.35f, Screen.width / 760f, Screen.height / 470f);
            panelScale = Mathf.Max(0.1f, panelScale);
            GUI.matrix = previousMatrix * Matrix4x4.Scale(new Vector3(panelScale, panelScale, 1f));
            float availableWidth = Screen.width / panelScale;
            float availableHeight = Screen.height / panelScale;
            float width = Mathf.Min(720f, availableWidth - 40f);
            float height = Mathf.Min(430f, availableHeight - 24f);
            GUILayout.BeginArea(new Rect(
                (availableWidth - width) * 0.5f,
                (availableHeight - height) * 0.5f,
                width,
                height), GUI.skin.box);
            GUILayout.Label("PAUSE", titleStyle);
            if (confirmReturnToTitle)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("ステージ選択へ戻りますか？", titleStyle);
                GUILayout.Space(16f);
                GUILayout.Label("現在の区間・ヒモ橋の進行はリセットされます。\n音量設定は保持されます。", bodyStyle);
                GUILayout.FlexibleSpace();
                Rect row = GUILayoutUtility.GetRect(1f, 52f, GUILayout.ExpandWidth(true), GUILayout.Height(52f));
                bool cancel = HimoHitoUiParts.WoodButton(new Rect(row.x, row.y, (row.width - 12f) * .5f, row.height), "Esc　キャンセル", bodyStyle);
                bool accept = HimoHitoUiParts.WoodButton(new Rect(row.x + (row.width + 12f) * .5f, row.y, (row.width - 12f) * .5f, row.height), "Enter　ステージ選択へ", bodyStyle);
                GUILayout.EndArea();
                GUI.matrix = previousMatrix;
                if (cancel) confirmReturnToTitle = false;
                if (accept) { ReturnToTitle(); GUIUtility.ExitGUI(); }
                return;
            }
            pauseScroll = GUILayout.BeginScrollView(pauseScroll);
            GUILayout.Space(12f);
            GUILayout.Label("音量設定", bodyStyle);
            DrawVolumeRow("BGM", true);
            DrawVolumeRow("効果音", false);
            GUILayout.Space(12f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("効果音を試聴", GUILayout.Height(36f)))
                audioFeedback?.PlayUiPaperOpened();
            if (GUILayout.Button("初期音量に戻す", GUILayout.Height(36f)))
            {
                HimoHitoAudioSettings.SetMusic(1f);
                HimoHitoAudioSettings.SetEffects(1f);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(12f);
            GUILayout.Label("100％＝これまでの音量 ／ 0％＝無音\n設定は自動保存されます。操作方法はTabで確認", bodyStyle);
            GUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            Rect resumeButtonRow = GUILayoutUtility.GetRect(
                1f,
                58f,
                GUILayout.ExpandWidth(true),
                GUILayout.Height(58f));
            Rect resumeButton = new Rect(
                resumeButtonRow.x,
                resumeButtonRow.y,
                (resumeButtonRow.width - 12f) * .5f,
                resumeButtonRow.height);
            bool resumeClicked = HimoHitoUiParts.WoodButton(
                resumeButton,
                "Esc　ゲームに戻る",
                bodyStyle);
            Rect titleButton = new Rect(resumeButton.xMax + 12f, resumeButton.y,
                resumeButton.width, resumeButton.height);
            bool titleClicked = HimoHitoUiParts.WoodButton(titleButton, "T　ステージ選択へ", bodyStyle);
            GUILayout.EndArea();
            GUI.matrix = previousMatrix;
            if (resumeClicked) { isPaused = false; ApplyPauseState(); }
            if (titleClicked) confirmReturnToTitle = true;
        }

        private void ReturnToTitle()
        {
            if (!isPaused || showHelp || !confirmReturnToTitle) return;
            if (!Application.CanStreamedLevelBeLoaded("Tutorial"))
            {
                Debug.LogError("ステージ選択へ戻れません。TutorialをBuild Settingsに登録してください。");
                return;
            }
            HimoHitoAudioSettings.Save();
            isPaused = showHelp = confirmReturnToTitle = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene("Tutorial");
        }

        private void DrawVolumeRow(string label, bool music)
        {
            float current = music ? HimoHitoAudioSettings.Music : HimoHitoAudioSettings.Effects;
            GUILayout.Space(10f);
            GUILayout.Label($"{label}　{Mathf.RoundToInt(current * 100f)}％", bodyStyle);
            GUILayout.BeginHorizontal();
            float next = current;
            if (GUILayout.Button("−", GUILayout.Width(42f), GUILayout.Height(30f))) next -= 0.05f;
            next = GUILayout.HorizontalSlider(next, 0f, 1f, GUILayout.Height(30f));
            if (GUILayout.Button("＋", GUILayout.Width(42f), GUILayout.Height(30f))) next += 0.05f;
            GUILayout.EndHorizontal();
            next = Mathf.Clamp01(next);
            if (!Mathf.Approximately(next, current))
            {
                if (music) HimoHitoAudioSettings.SetMusic(next);
                else HimoHitoAudioSettings.SetEffects(next);
            }
        }

        private void ApplyPauseState()
        {
            Time.timeScale = isPaused || showHelp ? 0f : 1f;
            if (!isPaused && !showHelp) HimoHitoAudioSettings.Save();
        }

        private bool IsResultScreenActive()
        {
            if (runController != null)
            {
                return runController.Outcome != PrototypeRunController.RunOutcome.Playing &&
                    runController.Outcome != PrototypeRunController.RunOutcome.WaitingToStart;
            }

            return (mainStageRespawn != null && mainStageRespawn.IsFailureVisible) ||
                (mainStageGoal != null && mainStageGoal.IsCompleting);
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
            cornerHintStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperRight,
                fontSize = 15,
                wordWrap = false,
                fixedWidth = 0f,
                fixedHeight = 0f,
                contentOffset = Vector2.zero,
                padding = new RectOffset(6, 6, 4, 4),
                normal = { textColor = Color.white }
            };

            Color ink = new Color(0.11f, 0.08f, 0.20f);
            helpTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 34,
                fontStyle = FontStyle.Bold,
                normal = { textColor = ink }
            };
            helpBodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 18,
                wordWrap = true,
                normal = { textColor = ink }
            };
            helpKeyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.78f, 0.20f, 0.38f) }
            };

            HimoHitoGuiTheme.ApplyToStyles(
                titleStyle,
                bodyStyle,
                cornerHintStyle,
                helpTitleStyle,
                helpBodyStyle,
                helpKeyStyle);
        }
    }
}
