# 固定Buildソースの照合と抜粋

対象はFriendPlay_20261006_214500/.build/Project。Productionの編集用Assetsは評価基準にしていない。
行番号はこの固定コピーの番号。抜粋は静的事実の根拠であり、実行・人間試遊の証拠ではない。

## PrototypeHud.cs

SHA-256 `2e6d442c4933dda55aa3680ed1a835372e53274fe0c7404498684dc23abb6780`。BuildInputs一致：True。

### 68–134
```text
68:         private void OnGUI()
69:         {
70:             if (MainStagePreview.IsActive) return;
71: 
72:             HimoHitoGuiTheme.ApplyToSkin(GUI.skin);
73: 
74:             if (platformBuilder == null)
75:             {
76:                 platformBuilder = FindFirstObjectByType<RopePlatformBuilder>();
77:             }
78: 
79:             EnsureStyles();
80:             if (runController == null || runController.Outcome != PrototypeRunController.RunOutcome.Clear)
81:                 clearJourney = null;
82: 
83:             if (runController != null &&
84:                 runController.Outcome == PrototypeRunController.RunOutcome.WaitingToStart)
85:             {
86:                 (stageSelection ??= new StageSelectionView()).Draw(runController);
87:                 return;
88:             }
89: 
90:             if (runController != null &&
91:                 runController.Outcome == PrototypeRunController.RunOutcome.Clear)
92:             {
93:                 DrawClearScreen();
94:                 return;
95:             }
96: 
97:             if (runController != null &&
98:                 runController.Outcome == PrototypeRunController.RunOutcome.Failed)
99:             {
100:                 if (!runController.IsFallUnravelling) DrawFailureScreen();
101:                 return;
102:             }
103: 
104:             HimoHitoUiParts.BeginHudArea(new Rect(22f, 18f, Mathf.Min(360f, Screen.width - 44f), HimoHitoUiParts.CompactHudHeight));
105:             GUILayout.Label("チュートリアル　" + $"第{(runController != null ? runController.CurrentTutorialSection : 1)}区間 / {PrototypeRunController.TutorialSectionCount}", titleStyle);
106:             if (ropeResource != null)
107:             {
108:                 GUILayout.Label(
109:                     $"ヒモ残量  {HimoHitoGuiTheme.FormatRopeValue(ropeResource.CurrentLength)} / " +
110:                     HimoHitoGuiTheme.FormatRopeValue(ropeResource.MaximumLength), lengthStyle);
111:                 RopeResourceGauge.Draw(ropeResource);
112:             }
113:             if (ropeController != null)
114:             {
115:                 GUILayout.Label($"次に使う長さ  {ropeController.SelectedRopeLength} / " +
116:                     $"{ropeController.MaximumSelectableRopeLength}" +
117:                     (ropeController.IsAttached ? "　接続中" : ""), bodyStyle);
118:             }
119:             GUILayout.Label("Tab　操作説明", bodyStyle);
120:             GUILayout.EndArea();
121: 
122:             DrawWeavePrompt();
123:         }
124: 
125:         private void DrawFailureScreen()
126:         {
127:             bool fell = runController.FailureReason ==
128:                 PrototypeRunController.RunFailureReason.Fell;
129:             HimoHitoFailureScreen.Draw(
130:                 fell ? "足を踏み外しました" : "ヒモが尽きました",
131:                 fell
132:                     ? "少しだけ休んで、同じ区間からもう一度。"
133:                     : "使う長さを選び直せば、まだ先へ進めます。",
134:                 "R　この区間から再挑戦");
```

### 178–186
```text
178:         private void DrawWeavePrompt()
179:         {
180:             if (platformBuilder == null ||
181:                 ropeController == null ||
182:                 !ropeController.IsAttached ||
183:                 platformBuilder.CurrentPlatformCost <= 0f)
184:             {
185:                 return;
186:             }
```

### 232–268
```text
232:             GUILayout.BeginArea(contentArea);
233:             GUILayout.Label("この照準方向へヒモ足場を作る", weavePromptTitleStyle);
234:             GUILayout.Label(
235:                 $"永久に使う長さ  {platformBuilder.CurrentPlatformCost:0.0}",
236:                 weavePromptBodyStyle);
237: 
238:             bool lacksReserve = ropeResource != null &&
239:                 ropeResource.CurrentLength - platformBuilder.CurrentPlatformCost <
240:                 platformBuilder.MinimumRopeReserve - 0.001f;
241:             string actionMessage;
242:             if (platformBuilder.CanBuildCurrentPlatform)
243:             {
244:                 actionMessage = "Q：この方向へヒモ足場を作る";
245:             }
246:             else if (lacksReserve)
247:             {
248:                 actionMessage =
249:                     $"足場にできません\nヒモを{platformBuilder.MinimumRopeReserve:0.0}以上残してください";
250:             }
251:             else if (runController != null &&
252:                 runController.CurrentTutorialSection == 4)
253:             {
254:                 actionMessage =
255:                     "この接続先では足場を作れません\nEで外し、右の緑フックを狙ってください";
256:             }
257:             else if (ropeController.ActiveHookPoint != null &&
258:                 ropeController.ActiveHookPoint.TryGetComponent(
259:                     out RopePlatformAnchor _))
260:             {
261:                 actionMessage =
262:                     "ヒモが対岸フックまで届いていません\nEで外し、Wで長くしてください";
263:             }
264:             else
265:             {
266:                 actionMessage = "この接続先では足場を作れません";
267:             }
268:             GUILayout.Label(actionMessage, weavePromptActionStyle);
```

## MainStageHud.cs

SHA-256 `30d2177386b912c1c8fc86961460208b53fd2f45df0597552055c19e41ecae50`。BuildInputs一致：True。

### 50–109
```text
50:         {
51:             goal ??= FindFirstObjectByType<MainStageGoalZone>();
52:         }
53: 
54:         private void OnGUI()
55:         {
56:             if (MainStagePreview.IsActive)
57:             {
58:                 wasPreviewing = true;
59:                 return;
60:             }
61:             if (wasPreviewing)
62:             {
63:                 sectionTitleUntil = Time.unscaledTime + 1.2f;
64:                 stageStartedAt = Time.time;
65:                 wasPreviewing = false;
66:             }
67: 
68:             HimoHitoGuiTheme.ApplyToSkin(GUI.skin);
69:             goal ??= FindFirstObjectByType<MainStageGoalZone>();
70:             EnsureStyles();
71:             if (goal != null && goal.IsClear)
72:             {
73:                 DrawClearScreen();
74:                 return;
75:             }
76:             clearJourney = null;
77: 
78:             if (respawn != null && respawn.IsFailureVisible)
79:             {
80:                 if (respawn.IsFallUnravelling) return;
81:                 bool fell = respawn.IsFallFailure;
82:                 HimoHitoFailureScreen.Draw(
83:                     fell ? "足を踏み外しました" : "ヒモが尽きました",
84:                     fell
85:                         ? "同じ区間から、落ち着いてもう一度。"
86:                         : "使った長さを見直せば、まだ先へ進めます。",
87:                     "R　この区間から再挑戦");
88:                 return;
89:             }
90: 
91:             DrawSectionTitle();
92: 
93:             HimoHitoUiParts.BeginHudArea(new Rect(22f, 18f, Mathf.Min(360f, Screen.width - 44f), HimoHitoUiParts.CompactHudHeight));
94:             GUILayout.Label("本編　" + $"第{(respawn != null ? respawn.CurrentSection : 1)}区間", titleStyle);
95:             if (ropeResource != null)
96:             {
97:                 GUILayout.Label(
98:                     $"ヒモ残量  {HimoHitoGuiTheme.FormatRopeValue(ropeResource.CurrentLength)} / " +
99:                     HimoHitoGuiTheme.FormatRopeValue(ropeResource.MaximumLength), ropeStyle);
100:                 RopeResourceGauge.Draw(ropeResource);
101:             }
102:             if (ropeController != null)
103:             {
104:                 GUILayout.Label($"次に使う長さ  {ropeController.SelectedRopeLength} / " +
105:                     $"{ropeController.MaximumSelectableRopeLength}" +
106:                     (ropeController.IsAttached ? "　接続中" : ""), bodyStyle);
107:             }
108:             GUILayout.Label("Tab　操作説明", bodyStyle);
109:             GUILayout.EndArea();
```

## StageOverlayControls.cs

SHA-256 `7d63dcbaa1dbce7fb04a8e387b0013050d343084f3fc6d267f4064a69a28755a`。BuildInputs一致：True。

### 20–24
```text
20:         private GUIStyle bodyStyle;
21:         private GUIStyle cornerHintStyle;
22:         private static readonly GUIContent CornerHint = new GUIContent("Tab：操作確認　Esc：ポーズ");
23:         private static readonly GUIContent NarrowCornerHint = new GUIContent("Tab：操作確認\nEsc：ポーズ");
24:         private GUIStyle helpTitleStyle;
```

### 62–81
```text
62:                 tutorialSectionGuide =
63:                     FindFirstObjectByType<TutorialSectionGuide>();
64:             }
65:             if (tutorialSectionGuide != null &&
66:                 tutorialSectionGuide.IsVisible)
67:             {
68:                 return;
69:             }
70: 
71:             if (Input.GetKeyDown(KeyCode.Tab))
72:             {
73:                 if (showHelp) CloseHelp();
74:                 else OpenHelp();
75:                 return;
76:             }
77: 
78:             if (showHelp && Input.GetKeyDown(KeyCode.Escape))
79:             {
80:                 CloseHelp();
81:                 return;
```

### 175–200
```text
175:             }
176:             // Lower IMGUI depth values are drawn in front. Keep the help sheet
177:             // above the title HUD regardless of component creation order.
178:             GUI.depth = -1000;
179:             HimoHitoGuiTheme.ApplyToSkin(GUI.skin);
180:             EnsureStyles();
181:             bool isOnTitleScreen = runController != null &&
182:                 runController.Outcome == PrototypeRunController.RunOutcome.WaitingToStart;
183:             if (!isOnTitleScreen && !IsOverlayVisible)
184:             {
185:                 DrawCornerHint();
186:             }
187: 
188:             if (!showHelp && !isPaused)
189:             {
190:                 return;
191:             }
192: 
193:             if (showHelp)
194:             {
195:                 DrawHelpScreen();
196:                 return;
197:             }
198: 
199:             DrawPauseScreen();
200:         }
```

### 255–282
```text
255:                 width,
256:                 height));
257:             GUILayout.Label("操作方法", helpTitleStyle);
258:             GUILayout.Space(18f);
259:             GUILayout.Label(
260:                 "ヒモを掛けて振り、必要な場所では足場として残します",
261:                 helpBodyStyle);
262:             GUILayout.Space(28f);
263: 
264:             GUILayout.BeginHorizontal();
265:             GUILayout.BeginVertical(GUILayout.Width(width * 0.48f));
266:             DrawHelpRow("A / D", "移動・振り子を加速");
267:             DrawHelpRow("Space", "ジャンプ");
268:             DrawHelpRow("W / S", "次に使う長さを変更");
269:             DrawHelpRow("矢印キー", "照準方向を変更");
270:             GUILayout.EndVertical();
271:             GUILayout.Space(width * 0.04f);
272:             GUILayout.BeginVertical(GUILayout.Width(width * 0.48f));
273:             DrawHelpRow("E", "ヒモを接続・解除");
274:             DrawHelpRow("Q", "接続中のヒモを足場化");
275:             DrawHelpRow("F", "中央Hookを外してまとめる");
276:             DrawHelpRow("R", "現在の区間から再挑戦");
277:             GUILayout.EndVertical();
278:             GUILayout.EndHorizontal();
279: 
280:             GUILayout.FlexibleSpace();
281:             bool close = GUILayout.Button("Tab / Esc　閉じる", helpKeyStyle);
282:             GUILayout.EndArea();
```

### 317–328
```text
317:             GUILayout.Label("PAUSE", titleStyle);
318:             if (confirmReturnToTitle)
319:             {
320:                 GUILayout.FlexibleSpace();
321:                 GUILayout.Label("ステージ選択へ戻りますか？", titleStyle);
322:                 GUILayout.Space(16f);
323:                 GUILayout.Label("現在の区間・ヒモ橋の進行はリセットされます。\n音量設定は保持されます。", bodyStyle);
324:                 GUILayout.FlexibleSpace();
325:                 Rect row = GUILayoutUtility.GetRect(1f, 52f, GUILayout.ExpandWidth(true), GUILayout.Height(52f));
326:                 bool cancel = HimoHitoUiParts.WoodButton(new Rect(row.x, row.y, (row.width - 12f) * .5f, row.height), "Esc　キャンセル", bodyStyle);
327:                 bool accept = HimoHitoUiParts.WoodButton(new Rect(row.x + (row.width + 12f) * .5f, row.y, (row.width - 12f) * .5f, row.height), "Enter　ステージ選択へ", bodyStyle);
328:                 GUILayout.EndArea();
```

### 334–351
```text
334:             pauseScroll = GUILayout.BeginScrollView(pauseScroll);
335:             GUILayout.Space(12f);
336:             GUILayout.Label("音量設定", bodyStyle);
337:             DrawVolumeRow("BGM", true);
338:             DrawVolumeRow("効果音", false);
339:             GUILayout.Space(12f);
340:             GUILayout.BeginHorizontal();
341:             if (GUILayout.Button("効果音を試聴", GUILayout.Height(36f)))
342:                 audioFeedback?.PlayUiPaperOpened();
343:             if (GUILayout.Button("初期音量に戻す", GUILayout.Height(36f)))
344:             {
345:                 HimoHitoAudioSettings.SetMusic(1f);
346:                 HimoHitoAudioSettings.SetEffects(1f);
347:             }
348:             GUILayout.EndHorizontal();
349:             GUILayout.Space(12f);
350:             GUILayout.Label("100％＝これまでの音量 ／ 0％＝無音\n設定は自動保存されます。操作方法はTabで確認", bodyStyle);
351:             GUILayout.EndScrollView();
```

### 358–372
```text
358:             Rect resumeButton = new Rect(
359:                 resumeButtonRow.x,
360:                 resumeButtonRow.y,
361:                 (resumeButtonRow.width - 12f) * .5f,
362:                 resumeButtonRow.height);
363:             bool resumeClicked = HimoHitoUiParts.WoodButton(
364:                 resumeButton,
365:                 "Esc　ゲームに戻る",
366:                 bodyStyle);
367:             Rect titleButton = new Rect(resumeButton.xMax + 12f, resumeButton.y,
368:                 resumeButton.width, resumeButton.height);
369:             bool titleClicked = HimoHitoUiParts.WoodButton(titleButton, "T　ステージ選択へ", bodyStyle);
370:             GUILayout.EndArea();
371:             GUI.matrix = previousMatrix;
372:             if (resumeClicked) { isPaused = false; ApplyPauseState(); }
```

## StageSelectionView.cs

SHA-256 `de0e76b56065337bb46cf256ad5778a9c9bb9cb1fa74aee6e8c8c724ffab3cb5`。BuildInputs一致：True。

### 61–64
```text
61:                 GUI.Label(new Rect(240, 51, 800, 23), "H I M O H I T O", eyebrow);
62:                 GUI.Label(new Rect(180, 74, 920, 66), "ヒモヒト", heading);
63:                 GUI.Label(new Rect(240, 140, 800, 29), "あそぶ場所を、えらぼう", description);
64: 
```

### 95–114
```text
95:                     {
96:                         DrawSelectionWeave(new Vector2(node.x, node.y - 82));
97:                         GUI.Label(new Rect(node.x - 100, node.y - 214, 200, 24), "いま選んでいる場所", selectedLabel);
98:                     }
99:                     GUI.color = stage.available ? Color.white : new Color(.86f, .88f, .94f, .85f);
100:                     // Only the miniature actor/lid moves. Bases, input rectangles and camera stay fixed.
101:                     Sprite stageArt = GetStageArt(stage);
102:                     StageSelectionDioramaState.Motion motion = dioramaState.GetMotion(stage.id);
103:                     if (!stage.available || stage.mapArt != "toybox" ||
104:                         !StageSelectionDioramaArt.DrawToybox(iconRect, motion))
105:                         DrawSprite(iconRect, stageArt, false);
106:                     if (stage.available && stage.mapArt == "practice")
107:                         StageSelectionDioramaArt.DrawPractice(iconRect, stageArt, motion);
108:                     GUI.color = Color.white;
109:                     if (StageProgress.IsCleared(stage)) DrawClearFlag(node);
110:                     if (ClothButton(nameRect, stage.title, label, active)) run.SelectStage(index);
111:                     if (GUI.Button(iconRect, GUIContent.none, GUIStyle.none)) run.SelectStage(index);
112:                     GUI.Label(new Rect(node.x - 162, node.y + 91, 324, 28),
113:                         stage.available ? stage.subtitle : "COMING SOON  /  追加予定",
114:                         stage.available ? small : selectedLabel);
```

### 120–147
```text
120:                     hasError ? selectedLabel : description, hasError ? 15 : 19, 14);
121:                 bool enabled = GUI.enabled;
122:                 quit = ClothButton(new Rect(151, 602, 255, 56), "Esc　ゲーム終了", button, false);
123:                 help = ClothButton(new Rect(512, 602, 255, 56), "Tab　操作説明", button, false);
124:                 GUI.enabled = enabled && chosen.available;
125:                 start = ClothButton(new Rect(873, 602, 255, 56),
126:                     chosen.available ? "Enter　ここであそぶ" : "追加をお楽しみに", button, chosen.available);
127:                 GUI.enabled = enabled;
128: 
129:                 int pages = PageCount(stages.Count);
130:                 if (pages > 1)
131:                 {
132:                     GUI.enabled = enabled && page > 0;
133:                     if (GUI.Button(new Rect(463, 574, 45, 27), "◀", selectedLabel)) run.SelectStage(first - StagesPerPage);
134:                     GUI.enabled = enabled && page + 1 < pages;
135:                     if (GUI.Button(new Rect(773, 574, 45, 27), "▶", selectedLabel)) run.SelectStage(first + StagesPerPage);
136:                     GUI.enabled = enabled;
137:                     GUI.Label(new Rect(511, 574, 258, 27), $"{page + 1} / {pages}", small);
138:                 }
139:                 GUI.Label(new Rect(330, 662, 620, 25), "← → / A D：選ぶ　　クリックでも選択", eyebrow);
140:                 GUI.Label(new Rect(220, 688, 840, 20),
141:                     "制作：bq24014-cmd　／　Unity 6.3 LTS　／　使用素材はREADME参照", credits);
142:             }
143:             finally
144:             {
145:                 GUI.matrix = oldMatrix;
146:                 GUI.color = oldColor;
147:                 GUI.enabled = oldEnabled;
```

## TutorialSectionGuide.cs

SHA-256 `380f0e643fe2e76bc3d561c3157982b268aee650b3648357af8d3a1bf336f385`。BuildInputs一致：True。

### 30–62
```text
30:         {
31:             "掛けて、振って、渡る",
32:             "長さを、選ぶ",
33:             "編む――初めて、体が減る",
34:             "外して、1本にする"
35:         };
36: 
37:         private static readonly string[] SectionSummaries =
38:         {
39:             "青いHookへヒモを掛け、歩き出して振り子になり、対岸へ渡ります。",
40:             "長さで振り子の底が変わります。トゲを避ける長さを選びます。",
41:             "頭上にHookがない谷です。対岸の緑フックへ掛けたヒモを足場にします。",
42:             "2本の足場を中央で外し、梁の下を通れる1本の橋へまとめます。"
43:         };
44: 
45:         private static readonly string[] SectionSteps =
46:         {
47:             "1　W / Sで長さ、矢印キーで向きを調整\n2　青いHookへ照準を合わせて E\n3　右へ歩いて振り子になり、対岸へ着地",
48:             "1　長さ8では底が下がり、トゲに当たる\n2　Eで外し、W / Sで長さ6～7を選ぶ\n3　もう一度 Eで掛け、トゲの上を渡る",
49:             "1　W / Sで長さ7にする（推奨）\n2　対岸の緑フックへ E → Qで足場にする\n3　残量を7使ってできた橋を歩いて渡る",
50:             "1　左岸→中央→右岸へ長さ6の足場を2本作る\n2　中央の青いHookへ照準を合わせる\n3　Fで中央を外し、深くたわんだ1本の橋を渡る"
51:         };
52: 
53:         private static Texture2D ringTexture;
54: 
55:         private PrototypeRunController runController;
56:         private StageOverlayControls overlayControls;
57:         private PlayerMover playerMover;
58:         private RopeController ropeController;
59:         private RopePlatformBuilder platformBuilder;
60:         private PrototypeAudioFeedback audioFeedback;
61:         private Rigidbody2D playerBody;
62:         private Camera mainCamera;
```

### 136–175
```text
136:             {
137:                 AdvanceIllustration(Time.unscaledDeltaTime);
138:                 if (runController.Outcome !=
139:                     PrototypeRunController.RunOutcome.Playing ||
140:                     runController.CurrentTutorialSection != openSection)
141:                 {
142:                     CloseGuide();
143:                     return;
144:                 }
145: 
146:                 if (Input.GetKeyDown(KeyCode.Z))
147:                 {
148:                     CloseGuide();
149:                 }
150:                 return;
151:             }
152: 
153:             if (runController.Outcome !=
154:                 PrototypeRunController.RunOutcome.Playing)
155:             {
156:                 return;
157:             }
158: 
159:             if (overlayControls == null)
160:             {
161:                 overlayControls = FindFirstObjectByType<StageOverlayControls>();
162:             }
163:             if (overlayControls != null && overlayControls.IsOverlayVisible)
164:             {
165:                 return;
166:             }
167: 
168:             int section = runController.CurrentTutorialSection;
169:             if (IsNearSign(section) && Input.GetKeyDown(KeyCode.Z))
170:             {
171:                 OpenGuide(section);
172:             }
173:         }
174: 
175:         private void UpdateSignGreetings()
```

### 496–528
```text
496:         private void OpenGuide(int section)
497:         {
498:             GameObject sign = SceneObjectLookup.Find(SignPrefix + section, "Tutorial");
499:             Vector2 target = sign != null ? (Vector2)sign.transform.position : SignPositions[section - 1];
500:             openSection = section;
501:             illustrationTime = 0f;
502:             if (audioFeedback == null)
503:             {
504:                 audioFeedback = GetComponent<PrototypeAudioFeedback>();
505:             }
506:             audioFeedback?.PlayUiPaperOpened();
507:             wasPlayerMoverEnabled = playerMover != null && playerMover.enabled;
508:             wasRopeControllerEnabled =
509:                 ropeController != null && ropeController.enabled;
510:             wasPlatformBuilderEnabled =
511:                 platformBuilder != null && platformBuilder.enabled;
512: 
513:             if (playerMover != null)
514:             {
515:                 playerMover.enabled = false;
516:             }
517:             if (ropeController != null)
518:             {
519:                 ropeController.enabled = false;
520:             }
521:             if (platformBuilder != null)
522:             {
523:                 platformBuilder.enabled = false;
524:             }
525:             disabledGameplay = true;
526:             // Disabling rope controls cancels action poses, so set the reading gaze afterwards.
527:             GetComponent<RopeBodyVisual>()?.BeginReadingSign(target + Vector2.up * .5f);
528:             Time.timeScale = 0f;
```

### 617–663
```text
617:             int index = openSection - 1;
618:             Rect content = new Rect(
619:                 panel.x + 42f,
620:                 panel.y + 26f,
621:                 panel.width - 84f,
622:                 panel.height - 52f);
623:             GUI.Label(
624:                 new Rect(content.x, content.y, content.width, 34f),
625:                 $"TUTORIAL / T{openSection}",
626:                 sectionLabelStyle);
627:             GUI.Label(
628:                 new Rect(content.x, content.y + 34f, content.width, 58f),
629:                 SectionTitles[index],
630:                 titleStyle);
631:             GUI.Label(
632:                 new Rect(content.x, content.y + 92f, content.width, 54f),
633:                 SectionSummaries[index],
634:                 summaryStyle);
635: 
636:             Rect illustration = new Rect(
637:                 content.x,
638:                 content.y + 154f,
639:                 content.width * 0.58f,
640:                 content.height - 224f);
641:             DrawRect(illustration, new Color(0.075f, 0.055f, 0.15f, 0.98f));
642:             DrawBorder(illustration, 3f, new Color(0.24f, 0.16f, 0.36f));
643:             DrawSectionIllustration(illustration, openSection);
644: 
645:             Rect steps = new Rect(
646:                 illustration.xMax + 32f,
647:                 illustration.y + 16f,
648:                 content.xMax - illustration.xMax - 32f,
649:                 illustration.height - 32f);
650:             GUI.Label(steps, SectionSteps[index], stepStyle);
651: 
652:             Rect closeRect = new Rect(
653:                 content.xMax - 260f,
654:                 content.yMax - 56f,
655:                 260f,
656:                 50f);
657:             HimoHitoUiParts.DrawWoodButtonLabel(
658:                 closeRect,
659:                 "Z　看板を閉じる",
660:                 closeStyle);
661:         }
662: 
663:         private void AdvanceIllustration(float unscaledDelta)
```

### 738–760
```text
738: 
739:         private void DrawSectionThreeIllustration(Rect rect)
740:         {
741:             var f=TutorialGuideAnimation.Sample(3,illustrationTime);
742:             DrawPlatform(NormalizedRect(rect,.02f,.66f,.28f,.25f));
743:             DrawPlatform(NormalizedRect(rect,.72f,.66f,.26f,.25f));
744:             Vector2 left=DiagramPoint(rect,new Vector2(.30f,.66f));
745:             Vector2 right=DiagramPoint(rect,new Vector2(.72f,.66f));
746:             DrawDemoConnection(rect,f,right);
747:             Vector2 previous=left;
748:             float distance=0f;
749:             for(int i=1;i<=36;i++)
750:             {
751:                 float t=i/36f;
752:                 Vector2 point=DiagramPoint(rect,TutorialGuideAnimation.BridgePoint(t));
753:                 DrawYarnLine(previous,point,8f,new Color(1f,1f,1f,
754:                     f.Opacity*(t<=f.Bridge ? 1f : .12f)),distance);
755:                 distance+=Vector2.Distance(previous,point);
756:                 previous=point;
757:             }
758:             DrawDemoRing(left,new Color(.33f,1f,.76f));
759:             DrawDemoRing(right,new Color(.33f,1f,.76f));
760:             DrawDemoPlayer(rect,f);
```

## RopePlatformBuilder.cs

SHA-256 `11ecb4c1870cb8ef0449343155559211a5e1d9e1321132bce0dcf8e14e279fd2`。BuildInputs一致：True。

### 47–68
```text
47:         public float CurrentPlatformCost =>
48:             IsPlatformBuildingUnlocked &&
49:             ropeController != null && ropeController.IsAttached
50:                 ? GetCurrentPlatformCost()
51:                 : 0f;
52:         public float MinimumRopeReserve => minimumRopeReserve;
53:         public int GeneratedPlatformCount => generatedPlatforms.Count;
54:         public bool IsPlatformBuildingUnlocked
55:         {
56:             get
57:             {
58:                 PrototypeRunController tutorial = GetComponent<PrototypeRunController>();
59:                 if (tutorial != null && gameObject.scene.name == "Tutorial")
60:                 {
61:                     return tutorial.CurrentTutorialSection >= 3;
62:                 }
63: 
64:                 MainStageRespawnOnFall main = GetComponent<MainStageRespawnOnFall>();
65:                 return main == null || main.CurrentSection >= 4;
66:             }
67:         }
68:         public bool CanBuildCurrentPlatform => CanBuild(out _, out _, out _);
```

### 143–184
```text
143:         public bool TryRemoveAimedHook()
144:         {
145:             if (ropeController == null || ropeController.IsAttached ||
146:                 !ropeController.TryResolveCurrentAimHook(out HookPoint hook, out Vector2 anchor))
147:             {
148:                 return false;
149:             }
150: 
151:             List<GeneratedRopePlatform> joined = new(2);
152:             foreach (GameObject platformObject in generatedPlatforms)
153:             {
154:                 if (platformObject != null &&
155:                     platformObject.TryGetComponent(out GeneratedRopePlatform platform) &&
156:                     platform.Touches(anchor, SharedEndpointTolerance))
157:                 {
158:                     joined.Add(platform);
159:                 }
160:             }
161: 
162:             if (joined.Count != 2)
163:             {
164:                 return false;
165:             }
166: 
167:             Vector2 firstOuter = joined[0].GetOtherEndpoint(anchor);
168:             Vector2 secondOuter = joined[1].GetOtherEndpoint(anchor);
169:             float combinedLength = joined[0].RopeLength + joined[1].RopeLength;
170:             Vector2[] firstCurve = BuildSaggingCurve(firstOuter, anchor, joined[0].RopeLength);
171:             Vector2[] secondCurve = BuildSaggingCurve(anchor, secondOuter, joined[1].RopeLength);
172:             RopeBridgeBindings firstBindings = joined[0].GetComponent<RopeBridgeBindings>();
173:             RopeBridgeBindings secondBindings = joined[1].GetComponent<RopeBridgeBindings>();
174:             RemovePlatform(joined[0].gameObject);
175:             RemovePlatform(joined[1].gameObject);
176:             GeneratedRopePlatform merged = CreatePlatform(firstOuter, secondOuter, combinedLength);
177:             RopeBridgeMergeVisual.Play(merged.GetComponent<LineRenderer>(), firstCurve, secondCurve);
178:             merged.GetComponent<RopeBridgeBindings>()?.ReleaseCenter(firstBindings, secondBindings, anchor);
179:             if (!removedHooks.Contains(hook.gameObject))
180:             {
181:                 removedHooks.Add(hook.gameObject);
182:             }
183:             hook.gameObject.SetActive(false);
184:             audioFeedback?.PlayRopePlatformsMerged();
```

### 351–405
```text
351:         private bool CanBuild(
352:             out float ropeLength,
353:             out Vector2 start,
354:             out Vector2 end)
355:         {
356:             ropeLength = 0f;
357:             start = default;
358:             end = default;
359:             if (body == null || ropeResource == null || ropeController == null ||
360:                 !IsPlatformBuildingUnlocked || !ropeController.IsAttached)
361:             {
362:                 return false;
363:             }
364: 
365:             start = ropeController.AnchorPoint;
366:             end = body.position;
367:             ropeLength = ropeController.ActiveRopeLength;
368: 
369:             HookPoint activeHook = ropeController.ActiveHookPoint;
370:             RopePlatformAnchor platformAnchor = null;
371:             if (activeHook != null)
372:             {
373:                 activeHook.TryGetComponent(out platformAnchor);
374:             }
375: 
376:             // Q is reserved for authored bridge Hooks. The chosen length does
377:             // not have to match the suggested solution; it only has to span
378:             // the actual distance between this Hook and its paired endpoint.
379:             if (platformAnchor == null ||
380:                 !platformAnchor.TryGetPairedAnchor(
381:                     out Vector2 pairedAnchor))
382:             {
383:                 return false;
384:             }
385: 
386:             Vector2 activeAnchor = ropeController.AnchorPoint;
387:             bool pairedAnchorIsCloserToPlayer =
388:                 Vector2.Distance(body.position, pairedAnchor) <=
389:                 Vector2.Distance(body.position, activeAnchor);
390:             start = pairedAnchorIsCloserToPlayer
391:                 ? activeAnchor
392:                 : pairedAnchor;
393:             end = pairedAnchorIsCloserToPlayer
394:                 ? pairedAnchor
395:                 : activeAnchor;
396: 
397:             float directDistance = Vector2.Distance(start, end);
398:             return ropeLength >= minimumPlatformLength &&
399:                    directDistance <= ropeLength + 0.05f &&
400:                    ropeResource.CurrentLength - ropeLength >= minimumRopeReserve;
401:         }
402: 
403:         private float GetCurrentPlatformCost()
404:         {
405:             return ropeController.ActiveRopeLength;
```

### 484–507
```text
484:         private Vector2[] BuildSaggingCurve(Vector2 start, Vector2 end, float ropeLength)
485:         {
486:             int count = Mathf.Max(MinimumSmoothCurveSegments, curveSegments);
487:             Vector2[] points = new Vector2[count];
488:             float slack = Mathf.Max(0f, ropeLength - Vector2.Distance(start, end));
489:             for (int i = 0; i < count; i++)
490:             {
491:                 float t = i / (float)(count - 1);
492:                 points[i] = Vector2.Lerp(start, end, t) +
493:                     Vector2.down * (slack * 4f * t * (1f - t));
494:             }
495:             return points;
496:         }
497: 
498:         private void PlacePlayerOnPlatform(GeneratedRopePlatform platform)
499:         {
500:             Vector2 tangent = platform.GetEndTangent();
501:             Vector2 normal = new Vector2(-tangent.y, tangent.x).normalized;
502:             if (normal.y < 0f)
503:             {
504:                 normal = -normal;
505:             }
506: 
507:             float support = 0.5f;
```

## RopeController.cs

SHA-256 `3ef4a50076994279839f398652180256f5ec37c01b954eeefc67c79b831e25d6`。BuildInputs一致：True。

### 226–278
```text
226:             {
227:                 return false;
228:             }
229: 
230:             audioFeedback?.PlayRopeShot();
231: 
232:             bool foundTarget = TryResolveAvailableAttachment(worldTarget,
233:                 out Vector2 resolvedAnchor, out HookPoint hookPoint);
234:             if (!foundTarget)
235:             {
236:                 audioFeedback?.PlayRopeAttachMiss();
237:                 if (isActiveAndEnabled && body.simulated && Time.timeScale > 0f)
238:                 {
239:                     if (missVisual == null) missVisual = RopeMissVisual.Create(this, lineRenderer, body);
240:                     missVisual?.Play(worldTarget - body.position, SelectedRopeLength,
241:                         GetVisibleRopeWidth(), GetVisibleRopeColor());
242:                 }
243:                 return false;
244:             }
245: 
246:             if (missVisual != null) missVisual.Cancel();
247:             // Attaching and swinging are free. Rope is permanently consumed only
248:             // when Q converts the active rope into a platform.
249:             float selectedLength = SelectedRopeLength;
250:             activeRopeLength = selectedLength;
251:             anchorPoint = resolvedAnchor;
252:             activeHookPoint = hookPoint;
253:             ropeJoint.connectedBody = null;
254:             ropeJoint.connectedAnchor = anchorPoint;
255:             float jointDistance = selectedLength;
256:             bool isPlatformBuildAttachment =
257:                 hookPoint != null &&
258:                 hookPoint.TryGetComponent(out RopePlatformAnchor _);
259:             if (isPlatformBuildAttachment)
260:             {
261:                 // A platform Hook is followed by Q, not by a swing. The player
262:                 // stands about half a collider outside the paired bank Hook, so
263:                 // do not let that targeting margin pull them off the edge.
264:                 jointDistance = Mathf.Max(
265:                     jointDistance,
266:                     Vector2.Distance(body.position, anchorPoint));
267:             }
268:             ropeJoint.distance = jointDistance;
269:             ropeJoint.enabled = true;
270:             HideAimHookFeedback();
271:             if (isPlatformBuildAttachment)
272:             {
273:                 BeginPlatformBuildHold();
274:             }
275:             AttachmentSequence++;
276:             lineRenderer.enabled = true;
277:             ClearAirChainReconnectWindow();
278:             // The joint is already active. Only presentation travels to the Hook.
```

### 510–516
```text
510:             out Vector2 resolvedAnchor, out HookPoint hookPoint)
511:         {
512:             bool found = IsAirChainReconnectOpen
513:                 ? TryResolveNextAirChainHook(out resolvedAnchor, out hookPoint)
514:                 : TryResolveAttachmentPoint(worldTarget, out resolvedAnchor, out hookPoint);
515:             return found && (playerMover == null || playerMover.IsGrounded ||
516:                 CanReconnectAirChainTo(hookPoint));
```

### 650–711
```text
650:         private void UpdateSelectedRopeLength()
651:         {
652:             if (IsAttached)
653:             {
654:                 ResetLengthSelectionRepeat();
655:                 return;
656:             }
657: 
658:             ClampSelectedRopeLength();
659:             UpdateLengthSelectionKey(
660:                 KeyCode.W,
661:                 1,
662:                 ref nextLengthIncreaseTime);
663:             UpdateLengthSelectionKey(
664:                 KeyCode.S,
665:                 -1,
666:                 ref nextLengthDecreaseTime);
667:         }
668: 
669:         private void UpdateLengthSelectionKey(
670:             KeyCode key,
671:             int amount,
672:             ref float nextRepeatTime)
673:         {
674:             if (Input.GetKeyDown(key))
675:             {
676:                 ChangeSelectedRopeLength(amount, true);
677:                 nextRepeatTime = Time.unscaledTime + lengthSelectionRepeatDelay;
678:                 return;
679:             }
680: 
681:             if (!Input.GetKey(key))
682:             {
683:                 nextRepeatTime = 0f;
684:                 return;
685:             }
686: 
687:             if (nextRepeatTime <= 0f || Time.unscaledTime < nextRepeatTime)
688:             {
689:                 return;
690:             }
691: 
692:             ChangeSelectedRopeLength(amount, false);
693:             nextRepeatTime = Time.unscaledTime + lengthSelectionRepeatInterval;
694:         }
695: 
696:         private void ChangeSelectedRopeLength(
697:             int amount,
698:             bool playLimitSound)
699:         {
700:             int previousLength = selectedRopeLength;
701:             selectedRopeLength = Mathf.Clamp(
702:                 selectedRopeLength + amount,
703:                 minimumSelectableRopeLength,
704:                 GetMaximumSelectableRopeLength());
705: 
706:             if (selectedRopeLength != previousLength)
707:             {
708:                 audioFeedback?.PlayRopeLengthChanged(amount);
709:             }
710:             else if (playLimitSound)
711:             {
```

## PlayerMover.cs

SHA-256 `02f6bab1132599184cc6450c5053990b612998ff657372f56cd9877cb5201f8d`。BuildInputs一致：True。

### 151–195
```text
151:             coyoteTimer = IsGrounded ? coyoteTime : coyoteTimer - Time.fixedDeltaTime;
152: 
153:             bool isSwinging = ropeController != null && ropeController.IsAttached;
154:             if (isSwinging)
155:             {
156:                 if (ShouldUseAttachmentLaunchControl())
157:                 {
158:                     ApplyAttachmentLaunchControl();
159:                 }
160:                 else
161:                 {
162:                     ApplySwingControl();
163:                 }
164:             }
165:             else if (IsGrounded)
166:             {
167:                 TryStepOntoRopeEndpoint();
168:                 ApplyGroundControl();
169:             }
170:             else
171:             {
172:                 ApplyAirControl();
173:             }
174: 
175:             bool jumpedThisStep = false;
176:             if (jumpBufferTimer > 0f && coyoteTimer > 0f && !isSwinging)
177:             {
178:                 float horizontalBrakeImpulse = -body.linearVelocity.x
179:                     * (1f - jumpHorizontalSpeedRetention)
180:                     * body.mass;
181:                 Vector2 takeoffImpulse = new Vector2(horizontalBrakeImpulse, jumpImpulse);
182:                 body.AddForce(takeoffImpulse, ForceMode2D.Impulse);
183:                 audioFeedback?.PlayPlayerJumped();
184:                 GetComponent<RopeBodyVisual>()?.PlayTakeoffElasticity();
185:                 // Coyote-time jumps happen in the air, so do not invent a floor puff.
186:                 if (IsGrounded)
187:                 {
188:                     RopeLandingFluff.PlayTakeoff(bodyCollider);
189:                     RopeBridgeStepVisual.PlayTakeoff(groundedRopePlatform,
190:                         new Vector2(bodyCollider.bounds.center.x, bodyCollider.bounds.min.y));
191:                 }
192:                 ClearRecentRopePlatform();
193:                 jumpBufferTimer = 0f;
194:                 coyoteTimer = 0f;
195:                 jumpedThisStep = true;
```

## MainStageSectionThreeRecoverySwitch.cs

SHA-256 `740694ff88f82362da4c42170d5c415206d4d1808a83b07eb2d5ca75f32f0f1a`。BuildInputs一致：True。

### 64–79
```text
64:             if (MainStagePreview.IsActive) return;
65: 
66:             if (stairs == null || visual == null) RestoreReferences();
67:             if (trackedPlayer == null)
68:             {
69:                 trackedPlayer = Object.FindFirstObjectByType<PlayerMover>();
70:             }
71: 
72:             if (Input.GetKeyDown(KeyCode.F)) TryActivate();
73: 
74:             if (trackedPlayer != null &&
75:                 stairs != null &&
76:                 stairs.IsRevealed &&
77:                 trackedPlayer.transform.position.x <=
78:                     MainStageSectionTwoSetup.LandingRightEdge + 0.1f &&
79:                 trackedPlayer.transform.position.y >= -5.2f)
```

### 134–147
```text
134:                 fontSize = 20,
135:                 fontStyle = FontStyle.Bold
136:             };
137:             HimoHitoGuiTheme.ApplyToStyles(style);
138:             GUI.Box(
139:                 new Rect(
140:                     (Screen.width - 360f) * 0.5f,
141:                     Screen.height - 92f,
142:                     360f,
143:                     48f),
144:                 "F：帰り道のスイッチを押す",
145:                 style);
146:         }
147: 
```

## MainStageSectionTwoSetup.cs

SHA-256 `4169465af91057f291d7a077ba26d17e6dfb9cd3f6716c904379fa39cd75dcfc`。BuildInputs一致：True。

### 5–44
```text
5:     /// <summary>
6:     /// Builds the section-two "length window" from the 0829 stage manual.
7:     /// Length 5 cannot reach the raised bank, length 6 clears the spikes, and
8:     /// length 7 or more drops low enough to touch the tall near spike.
9:     /// </summary>
10:     public static class MainStageSectionTwoSetup
11:     {
12:         public const string BoardName =
13:             "Main Section 2 Length Window Board";
14:         public const string FrontSpikeName =
15:             "Main Section 2 Front Spike";
16:         public const string FarSpikeName =
17:             "Main Section 2 Far Spike";
18: 
19:         private const string HookName = "Main Hook 2";
20:         private const string LandingName = "Main Landing 2";
21: 
22:         // Main Landing 1 is centered at x = 9.5, ends at x = 13.5,
23:         // and its top is y = -4.85.
24:         // The invisible valley baseline is therefore y = -8.35.
25:         private static readonly Vector2 HookPosition =
26:             new Vector2(17.25f, -0.85f);
27:         private static readonly Vector2 LandingPosition =
28:             new Vector2(29.75f, -9.35f);
29:         private static readonly Vector2 LandingSize =
30:             new Vector2(9.5f, 10f);
31:         // Keep the section-two arrival edge at x = 25. Extend only the
32:         // section-three departure edge for the jump back from return step D.
33:         internal static float LandingRightEdge =>
34:             LandingPosition.x + LandingSize.x * 0.5f;
35:         private static readonly Vector2 FrontSpikePosition =
36:             new Vector2(15.35f, -7.9f);
37:         private static readonly Vector2 FrontSpikeSize =
38:             new Vector2(0.9f, 0.9f);
39:         private static readonly Vector2 FarSpikePosition =
40:             new Vector2(19.55f, -8.2f);
41:         private static readonly Vector2 FarSpikeSize =
42:             new Vector2(0.7f, 0.3f);
43:         private static readonly Vector2 RespawnPosition =
44:             new Vector2(29f, -3.65f);
```

## MainStageSectionSixSetup.cs

SHA-256 `71b394fdf99a0edcbb4085774aa1f4fc263054a799eec641179fae527a74e482`。BuildInputs一致：True。

### 30–76
```text
30:         public const string LowerHookCName = "Main S06 Lower Hook C";
31:         public const string MergeName = "Main S06 Merge";
32:         private const string LowerAirChainGroup = "MainS06Lower";
33: 
34:         public const int UpperBridgeRopeLength = 14;
35:         public const int LowerRouteRopeLength = 8;
36: 
37:         // Section five's right bank ends at X=119.1 with a top of Y=-2.15.
38:         // Only this start anchor remains on the bank; the remaining route is
39:         // raised three units to create the intended vertical separation.
40:         public static readonly Vector2 UpperBridgeStartHookPosition =
41:             new Vector2(119.1f, -2.15f);
42:         public static readonly Vector2 UpperBridgeEndHookPosition =
43:             new Vector2(131.2f, 3.45f);
44:         public static readonly Vector2 BridgeHookSize =
45:             new Vector2(0.62f, 0.62f);
46: 
47:         public static readonly Vector2 UpperShelfAPosition =
48:             new Vector2(132.7f, 3.1f);
49:         public static readonly Vector2 UpperShelfASize =
50:             new Vector2(3f, 0.7f);
51:         public static readonly Vector2 UpperShelfBPosition =
52:             new Vector2(136.4f, 4.4f);
53:         public static readonly Vector2 UpperShelfBSize =
54:             new Vector2(2.6f, 0.7f);
55:         public static readonly Vector2 UpperShelfCPosition =
56:             new Vector2(141f, 3.75f);
57:         public static readonly Vector2 UpperShelfCSize =
58:             new Vector2(4.6f, 0.7f);
59: 
60:         public static readonly Vector2 LowerHookAPosition =
61:             new Vector2(124.4f, 1.6f);
62:         public static readonly Vector2 LowerHookBPosition =
63:             new Vector2(131.8f, 1.8f);
64:         public static readonly Vector2 LowerHookCPosition =
65:             new Vector2(138.6f, 1.8f);
66:         public static readonly Vector2 NormalHookSize =
67:             new Vector2(1.6f, 0.45f);
68: 
69:         public static readonly Vector2 MergePosition =
70:             new Vector2(151.1f, -7.15f);
71:         public static readonly Vector2 MergeSize =
72:             new Vector2(8f, 10f);
73:         public static readonly Vector2 MergeRespawnPosition =
74:             new Vector2(147.9f, -1.45f);
75: 
76:         private static readonly Color TerrainColor =
```

## MainStageSectionSevenSetup.cs

SHA-256 `c5d3845135fa2a83de7b1c10200f5630fffbf25f96d144b977187e80c4d95513`。BuildInputs一致：True。

### 30–56
```text
30:         public const int BridgeRopeLength = 4;
31:         public const int FinalSwingRopeLength = 5;
32: 
33:         // The merge floor from section six is the lower floor in the slide.
34:         public static readonly Vector2 BridgeStartHookPosition =
35:             new Vector2(155.1f, -2.15f);
36:         public static readonly Vector2 BridgeEndHookPosition =
37:             new Vector2(157.9f, -0.35f);
38:         public static readonly Vector2 MiddleShelfPosition =
39:             new Vector2(159.4f, -0.7f);
40:         public static readonly Vector2 MiddleShelfSize =
41:             new Vector2(3f, 0.7f);
42:         // Jump impulse 10 against gravity 9.81 * 2.8 rises 1.82 at the apex.
43:         // The player reaches the 1.1-wide horizontal gap after about 0.20 s,
44:         // where the rise is only about 1.46. A 1.2 step keeps collision margin.
45:         public static readonly Vector2 UpperShelfPosition =
46:             new Vector2(163.5f, 0.5f);
47:         public static readonly Vector2 UpperShelfSize =
48:             new Vector2(3f, 0.7f);
49:         public static readonly Vector2 UpperHookPosition =
50:             new Vector2(167.35f, 3.7f);
51:         public static readonly Vector2 GoalFloorPosition =
52:             new Vector2(176f, -7.15f);
53:         public static readonly Vector2 GoalFloorSize =
54:             new Vector2(8f, 10f);
55:         public static readonly Vector2 GoalRespawnPosition =
56:             new Vector2(172.8f, -1.45f);
```

### 100–109
```text
100:                 if (!goalFloor.TryGetComponent(out MainStageCheckpoint checkpoint))
101:                 {
102:                     checkpoint = goalFloor.AddComponent<MainStageCheckpoint>();
103:                     changed = true;
104:                 }
105: 
106:                 // Preserve the section-six route difference while guaranteeing
107:                 // the slide's section-nine entry lower bound. Section eight
108:                 // was removed, so this checkpoint advances directly to nine.
109:                 checkpoint.Configure(9, GoalRespawnPosition, 23f);
```

## MainStageSectionNineSetup.cs

SHA-256 `794454c44069d0430c9136af0cade40b72c513e3708c7a594b5e1cd62a5566da`。BuildInputs一致：True。

### 5–38
```text
5: namespace HimoHito
6: {
7:     /// <summary>
8:     /// Builds section nine from slide 28 of the 0829 stage manual. Two
9:     /// length-six platforms meet at the center Hook. Removing that Hook with
10:     /// F joins them into one deeper length-twelve path beneath the beam.
11:     /// </summary>
12:     public static class MainStageSectionNineSetup
13:     {
14:         public const string LeftAnchorName = "Main S09 Green Left Bank";
15:         public const string CenterHookName = "Main S09 Center Hook";
16:         public const string RightAnchorName = "Main S09 Green Right Bank";
17:         public const string BeamName = "Main S09 Hanging Beam";
18:         public const string GoalFloorName = "Main S09 Goal Floor";
19:         public const int PlatformRopeLength = 6;
20: 
21:         private const string SectionEightPrefix = "Main S08 ";
22: 
23:         public static readonly Vector2 LeftAnchorPosition =
24:             new Vector2(180f, -2.15f);
25:         public static readonly Vector2 CenterHookPosition =
26:             new Vector2(185.5f, -1.05f);
27:         public static readonly Vector2 RightAnchorPosition =
28:             new Vector2(191f, -2.15f);
29:         public static readonly Vector2 BeamPosition =
30:             new Vector2(187f, 1.55f);
31:         public static readonly Vector2 BeamSize =
32:             new Vector2(2f, 5f);
33:         public static readonly Vector2 GoalFloorPosition =
34:             new Vector2(195f, -7.15f);
35:         public static readonly Vector2 GoalFloorSize =
36:             new Vector2(8f, 10f);
37:         public static readonly Vector2 GoalRespawnPosition =
38:             new Vector2(192f, -1.45f);
```

### 86–95
```text
86:             if (!goalFloor.TryGetComponent(out MainStageCheckpoint checkpoint))
87:             {
88:                 checkpoint = goalFloor.AddComponent<MainStageCheckpoint>();
89:                 changed = true;
90:             }
91:             checkpoint.Configure(
92:                 10,
93:                 GoalRespawnPosition,
94:                 MainStageSectionTenSetup.MinimumRopeAtEntry);
95:             return changed;
```

### 249–278
```text
249:         private static bool RemoveSectionEightObjects()
250:         {
251:             List<GameObject> obsolete = new();
252:             foreach (GameObject candidate in
253:                      Resources.FindObjectsOfTypeAll<GameObject>())
254:             {
255:                 if (candidate.scene.IsValid() &&
256:                     candidate.name.StartsWith(
257:                         SectionEightPrefix,
258:                         StringComparison.Ordinal))
259:                 {
260:                     obsolete.Add(candidate);
261:                 }
262:             }
263: 
264:             foreach (GameObject target in obsolete)
265:             {
266:                 target.SetActive(false);
267:                 if (Application.isPlaying)
268:                 {
269:                     UnityEngine.Object.Destroy(target);
270:                 }
271:                 else
272:                 {
273:                     UnityEngine.Object.DestroyImmediate(target);
274:                 }
275:             }
276:             return obsolete.Count > 0;
277:         }
278: 
```

## MainStageSectionTenSetup.cs

SHA-256 `80095b57b5ea63b2768ffaeac76dbe15270efb3b9d0669e84f65b2ccde134e11`。BuildInputs一致：True。

### 5–29
```text
5:     /// <summary>
6:     /// Builds the finale from slide 29 of the 0829 stage manual. The player
7:     /// spends length ten between two bank anchors and walks to the toy box.
8:     /// No swing Hook is placed above the valley.
9:     /// </summary>
10:     public static class MainStageSectionTenSetup
11:     {
12:         public const string LeftAnchorName = "Main S10 Green Left Bank";
13:         public const string RightAnchorName = "Main S10 Green Right Bank";
14:         public const string GoalFloorName = "Main S10 Goal Floor";
15:         public const string GoalMarkerName = "Main S10 Toy Box Goal";
16:         public const int BridgeRopeLength = 10;
17:         public const float MinimumRopeAtEntry = 11f;
18: 
19:         public static readonly Vector2 LeftAnchorPosition =
20:             new Vector2(199f, -2.15f);
21:         public static readonly Vector2 RightAnchorPosition =
22:             new Vector2(208f, -2.15f);
23:         public static readonly Vector2 GoalFloorPosition =
24:             new Vector2(215.5f, -7.15f);
25:         public static readonly Vector2 GoalFloorSize =
26:             new Vector2(15f, 10f);
27:         public static readonly Vector2 GoalMarkerPosition =
28:             new Vector2(219f, -0.95f);
29:         public static readonly Vector2 GoalMarkerSize =
```

## MainStageRespawnOnFall.cs

SHA-256 `314060136dfb7846ae7ebbc43cfaf0028625b839b4b9a05e9171d4f5830c4d8c`。BuildInputs一致：True。

### 118–139
```text
118: 
119:             if (IsFailureVisible)
120:             {
121:                 if (IsFallUnravelling) return;
122:                 if (Input.GetKeyDown(KeyCode.R))
123:                 {
124:                     RestartFromCheckpoint();
125:                 }
126:                 else if (Input.GetKeyDown(KeyCode.Escape))
127:                 {
128:                     SceneManager.LoadScene("Tutorial");
129:                 }
130:                 return;
131:             }
132: 
133:             if (Input.GetKeyDown(KeyCode.R))
134:             {
135:                 RestartFromCheckpoint();
136:                 return;
137:             }
138: 
139:             // Interpolated Transform can still show the previous fall position
```

### 178–206
```text
178:         public bool TryReachSection(
179:             int sectionNumber,
180:             Vector2 respawnPosition,
181:             float minimumRopeAfterCheckpoint)
182:         {
183:             if (ropeController.IsAttached || sectionNumber <= CurrentSection)
184:             {
185:                 return false;
186:             }
187: 
188:             CurrentSection = Mathf.Clamp(sectionNumber, 1, 10);
189:             checkpointPosition = respawnPosition;
190:             if (ropeResource.CurrentLength < minimumRopeAfterCheckpoint)
191:             {
192:                 ropeResource.RestoreCurrentLength(minimumRopeAfterCheckpoint);
193:                 RefillCount++;
194:             }
195:             CaptureCheckpointState();
196:             IsRopeExhausted = false;
197:             IsFallFailure = false;
198:             return true;
199:         }
200: 
201:         private void CaptureCheckpointState()
202:         {
203:             checkpointRopeLength = ropeResource.CurrentLength;
204:             checkpointSelectedRopeLength = ropeController.SelectedRopeLength;
205:             checkpointPlatformStates = platformBuilder.CapturePlatformStates();
206:             checkpointRemovedHooks = platformBuilder.CaptureRemovedHooks();
```

### 220–243
```text
220:         }
221: 
222:         private void RestartFromCheckpoint()
223:         {
224:             audioFeedback?.StopRopeExhaustedAudio();
225:             body.simulated = true;
226:             ropeController.DetachAndRefund();
227:             ropeResource.RestoreCurrentLength(checkpointRopeLength);
228:             ropeController.RestoreSelectedRopeLength(checkpointSelectedRopeLength);
229:             platformBuilder.RestorePlatformStates(checkpointPlatformStates, checkpointRemovedHooks);
230:             MainStageSectionFiveSetup.RestoreRailShelvesAfterRestart();
231:             body.position = checkpointPosition;
232:             transform.position = new Vector3(checkpointPosition.x, checkpointPosition.y, transform.position.z);
233:             body.linearVelocity = Vector2.zero;
234:             body.angularVelocity = 0f;
235:             playerMover.enabled = true;
236:             ropeController.enabled = true;
237:             IsRopeExhausted = false;
238:             IsFallFailure = false;
239:             Camera.main?.GetComponent<HorizontalCameraFollow>()?.ResetFraming();
240:             RespawnWeaveVisual.Play(gameObject);
241:         }
242:     }
243: }
```

## HimoHitoFailureScreen.cs

SHA-256 `60b4e7bf917b46ab16c9f234eeacdb1d846d502b110ca85e36550c06739fb363`。BuildInputs一致：True。

### 48–76
```text
48:             float panelWidth = Mathf.Min(780f, shadeWidth - 72f);
49:             float panelHeight = Mathf.Min(520f, Screen.height * 0.62f);
50:             GUILayout.BeginArea(new Rect(
51:                 Mathf.Max(42f, shadeWidth * 0.08f),
52:                 Screen.height * 0.14f,
53:                 panelWidth,
54:                 panelHeight));
55:             GUILayout.Label("もう一度、編み直そう", eyebrowStyle);
56:             GUILayout.Space(12f);
57:             GUILayout.Label(title, titleStyle);
58:             GUILayout.Space(22f);
59:             GUILayout.Label(message, bodyStyle);
60:             GUILayout.FlexibleSpace();
61:             Rect retryButtonRow = GUILayoutUtility.GetRect(
62:                 1f,
63:                 58f,
64:                 GUILayout.ExpandWidth(true),
65:                 GUILayout.Height(58f));
66:             Rect retryButton = new Rect(
67:                 retryButtonRow.x,
68:                 retryButtonRow.y,
69:                 Mathf.Min(470f, retryButtonRow.width),
70:                 retryButtonRow.height);
71:             HimoHitoUiParts.DrawWoodButtonLabel(
72:                 retryButton,
73:                 retryPrompt,
74:                 promptStyle);
75:             GUILayout.Label("Esc　ステージ選択へ", bodyStyle);
76:             GUILayout.EndArea();
```

## HimoHitoUiParts.cs

SHA-256 `0696cd16c13616c677f40c05349018f42108822c5578a8852f5f7671cfd9c764`。BuildInputs一致：True。

### 219–243
```text
219:         public static void DrawWoodButtonLabel(
220:             Rect rect,
221:             string text,
222:             GUIStyle style)
223:         {
224:             DrawWoodButton(rect);
225: 
226:             GUIStyle buttonLabelStyle = new GUIStyle(style)
227:             {
228:                 alignment = TextAnchor.MiddleCenter
229:             };
230:             buttonLabelStyle.normal.textColor = new Color(0.11f, 0.08f, 0.20f);
231:             GUI.Label(rect, text, buttonLabelStyle);
232:         }
233: 
234:         public static bool WoodButton(Rect rect, string text, GUIStyle style)
235:         {
236:             bool clicked = GUI.Button(rect, GUIContent.none, GUIStyle.none);
237:             Color previous = GUI.color;
238:             if (GUI.enabled && rect.Contains(Event.current.mousePosition))
239:                 GUI.color = previous * new Color(1f, .9f, .8f, 1f);
240:             DrawWoodButtonLabel(rect, text, style);
241:             GUI.color = previous;
242:             return clicked;
243:         }
```

## ClearJourneyView.cs

SHA-256 `d06a4cb0a7a0b8e8a1f7d1aa8d0e4e6e790321a3d2d94313aeaa4b253f19369e`。BuildInputs一致：True。

### 34–44
```text
34:                 GUI.Label(new Rect(80, 48, 1120, 65), tutorial ? "チュートリアルクリア" : "CLEAR", title);
35:                 GUI.Label(new Rect(80, 127, 1120, 38),
36:                     $"残ったヒモ  {snapshot.Remaining:0.0} / {snapshot.Capacity:0.0}     残したヒモ橋  {snapshot.Bridges.Length} 本", text);
37:                 GUI.Label(new Rect(80, 174, 1120, 32), tutorial
38:                     ? "ヒモを掛ける・長さを選ぶ・足場にする・まとめるを習得しました"
39:                     : $"補充  {refills} 回      かかった時間  {elapsed}", small);
40:                 DrawMap(new Rect(96, 232, 1088, 310));
41:                 bool primary = HimoHitoUiParts.WoodButton(new Rect(108, 588, 540, 58), tutorial
42:                     ? "Enter　本編ステージへ" : "R　本編を最初から再挑戦", action);
43:                 bool stageSelection = HimoHitoUiParts.WoodButton(new Rect(672, 588, 500, 58),
44:                     "Esc　ステージ選択へ戻る", action);
```

### 59–62
```text
59:             GUI.Label(new Rect(panel.x + 22, panel.y + 12, panel.width - 44, 32), "あなたが残した道", heading);
60:             GUI.Label(new Rect(panel.x + 22, panel.yMax - 37, panel.width - 44, 27),
61:                 "ピンク：クリア時に残ったヒモ橋　／　床と高さを簡略化した見取り図", small);
62:             Rect plot = new Rect(panel.x + 48, panel.y + 94, panel.width - 96, panel.height - 174);
```

## TutorialGuideAnimation.cs

SHA-256 `bb2cbbc6c7afa6ea22478d500932befb5cddab983c6dff75ea9f2c374e59a8e5`。BuildInputs一致：True。

### 1–99
```text
1: using UnityEngine;
2: 
3: namespace HimoHito
4: {
5:     /// <summary>Read-only, normalized diagram poses. Never drives the real player or rope.</summary>
6:     internal static class TutorialGuideAnimation
7:     {
8:         internal struct Frame
9:         {
10:             public Vector2 Feet;
11:             public Vector2 ConnectionTarget;
12:             public float Opacity, Connection, Bridge, Merge;
13:             public float LeftBridge, RightBridge;
14:             public bool Danger;
15:             public string Caption;
16:         }
17: 
18:         internal static float Duration(int section) => section == 2 ? 13f : section == 4 ? 21f : 10f;
19:         internal static float Ease(float time, float start, float end) =>
20:             Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(start, end, time));
21:         internal static Vector2 Curve(Vector2 a, Vector2 b, Vector2 c, float t) =>
22:             (1f-t)*(1f-t)*a + 2f*(1f-t)*t*b + t*t*c;
23:         internal static Vector2 Swing(float t, bool dangerous = false) =>
24:             Curve(new Vector2(.29f,.64f), new Vector2(.5f,dangerous ? 1.08f : .88f),
25:                 new Vector2(.71f,.64f),t);
26:         internal static Vector2 BridgePoint(float t) =>
27:             Curve(new Vector2(.30f,.66f),new Vector2(.51f,.72f),new Vector2(.72f,.66f),t);
28:         internal static Vector2 MergedPoint(float t, float merge)
29:         {
30:             Vector2 a=new Vector2(.24f,.67f), b=new Vector2(.5f,.35f), c=new Vector2(.76f,.67f);
31:             Vector2 pair=t<=.5f ? Curve(a,new Vector2(.37f,.72f),b,t*2f) :
32:                 Curve(b,new Vector2(.63f,.72f),c,(t-.5f)*2f);
33:             return Vector2.Lerp(pair,Curve(a,new Vector2(.5f,.99f),c,t),merge);
34:         }
35: 
36:         internal static Frame Sample(int section, float elapsed)
37:         {
38:             float duration=Duration(section), t=Mathf.Repeat(Mathf.Max(0f,elapsed),duration);
39:             Frame f=new Frame { Opacity=Ease(t,0f,.35f)*(1f-Ease(t,duration-.35f,duration)) };
40:             switch(section)
41:             {
42:                 case 1:
43:                     f.Feet=Swing(Ease(t,2.3f,6f));
44:                     f.Feet.x+=.12f*Ease(t,6.6f,7.8f);
45:                     f.Connection=Ease(t,1.2f,1.7f)*(1f-Ease(t,6f,6.5f));
46:                     f.Caption=t<1.2f ? "矢印キーで青いHookを狙う" : t<2.3f ? "E　ヒモを掛ける" :
47:                         t<6f ? "D　歩き出して、振り子で渡る" : "対岸へ着地！";
48:                     break;
49:                 case 2:
50:                     f.Danger=t<5f;
51:                     f.Feet=Swing(f.Danger ? .5f*Ease(t,1.5f,3.6f) : Ease(t,7.2f,10.8f),f.Danger);
52:                     f.Connection=f.Danger ? Ease(t,.9f,1.4f) : Ease(t,6.6f,7.1f)*(1f-Ease(t,10.8f,11.3f));
53:                     f.Opacity*=t<5f ? 1f-Ease(t,4.6f,5f) : Ease(t,5f,5.4f);
54:                     f.Caption=t<3.6f ? "長さ8　底が下がってしまう" : t<5f ? "×　トゲに当たる" :
55:                         t<6.6f ? "Eで外して選び直す　S：8 → 6" : t<7.2f ? "E　もう一度掛ける" :
56:                         t<10.8f ? "長さ6～7　トゲの上を渡れる" : "長さを選んで、対岸へ！";
57:                     break;
58:                 case 3:
59:                     f.Connection=Ease(t,1.1f,1.6f)*(1f-Ease(t,2.5f,3.3f));
60:                     f.Bridge=Ease(t,2.5f,3.5f);
61:                     float walk=Ease(t,4.4f,7.8f);
62:                     float x=Mathf.Lerp(.23f,.84f,walk);
63:                     f.Feet=x<.30f || x>.72f ? new Vector2(x,.66f) : BridgePoint((x-.30f)/.42f);
64:                     f.Caption=t<1.1f ? "推奨：長さ7で対岸の緑Hookを狙う" : t<2.5f ? "E　対岸へ接続" :
65:                         t<4.4f ? "Q　編んで足場にする（消費7）" : "D　できたヒモ橋を歩いて渡る";
66:                     break;
67:                 case 4:
68:                     f.LeftBridge=Ease(t,1.8f,2.8f);
69:                     f.RightBridge=Ease(t,6.5f,7.5f);
70:                     if(t<9f)
71:                     {
72:                         float approach=Mathf.Lerp(.18f,.5f,Ease(t,3.3f,5.3f));
73:                         f.Feet=approach<.24f ? new Vector2(approach,.67f) :
74:                             MergedPoint((approach-.24f)/.52f,0f);
75:                         bool first=t<5.5f;
76:                         f.ConnectionTarget=first ? new Vector2(.5f,.35f) : new Vector2(.76f,.67f);
77:                         f.Connection=first ? Ease(t,.8f,1.3f)*(1f-Ease(t,1.8f,2.8f)) :
78:                             Ease(t,5.5f,6f)*(1f-Ease(t,6.5f,7.5f));
79:                         f.Caption=t<.8f ? "長さ6で中央の青いHookを狙う" : t<1.8f ? "E　左岸から中央へ接続" :
80:                             t<3.3f ? "Q　1本目を編む（消費6）" : t<5.5f ? "D　できた橋を歩いて中央へ" :
81:                             t<6.5f ? "長さ6で右岸へ E" : t<8.5f ? "Q　2本目を編む（消費6）" :
82:                             "2本の橋ができた！";
83:                         break;
84:                     }
85:                     // Continue directly from the completed pair, with no position reset.
86:                     t-=9f;
87:                     f.Merge=Ease(t,4.8f,5.7f);
88:                     float along=.5f+.24f*Ease(t,1.4f,2.6f)-.24f*Ease(t,3.6f,4.8f);
89:                     along+=.74f*Ease(t,6.6f,10f);
90:                     f.Feet=along<=1f ? MergedPoint(along,f.Merge) : new Vector2(.76f+(along-1f)*.52f,.67f);
91:                     f.Caption=t<2.6f ? "2本の橋では通り道が高い" : t<3.6f ? "梁があって進めない" :
92:                         t<4.8f ? "中央の青いHookへ戻って狙う" : t<6.6f ? "F　中央を外すと1本に下がる" :
93:                         "D　低くなった橋で梁の下へ";
94:                     break;
95:             }
96:             return f;
97:         }
98:     }
99: }
```

## TutorialSectionOneSetup.cs

SHA-256 `64398ae8e596131799dbaae7ec914911a08ccff24217006dbff2fcd97d6178dc`。BuildInputs一致：True。

### 1–70
```text
1: using UnityEngine;
2: 
3: namespace HimoHito
4: {
5:     /// <summary>
6:     /// Keeps tutorial T1 aligned with the 0829 manual: one six-unit valley,
7:     /// one centered swing Hook, and no additional mechanic in the play space.
8:     /// </summary>
9:     public static class TutorialSectionOneSetup
10:     {
11:         public const string StartFloorName = "Start Ground";
12:         public const string HookName = "Tutorial Hook";
13:         public const string LandingFloorName = "Tutorial Landing";
14:         public const string LegacyFlashlightSpotName =
15:             "Tutorial Flashlight Spot";
16:         public const int StartingRopeLength = 6;
17:         public const int NextSectionStartingRopeLength = 8;
18: 
19:         private const float PlayerColliderWorldHeight = 1.2f;
20:         private const float StartGroundingInset = 0.03f;
21: 
22:         public static readonly Vector2 StartFloorPosition =
23:             new Vector2(-4f, -4.65f);
24:         public static readonly Vector2 StartFloorSize =
25:             new Vector2(6f, 10f);
26:         public static readonly Vector2 HookPosition =
27:             new Vector2(2f, 5.9f);
28:         public static readonly Vector2 LandingFloorPosition =
29:             new Vector2(12f, -4.65f);
30:         public static readonly Vector2 LandingFloorSize =
31:             new Vector2(6f, 10f);
32:         public static readonly Vector2 StartRespawnPosition =
33:             new Vector2(
34:                 StartFloorPosition.x,
35:                 StartFloorPosition.y + StartFloorSize.y * 0.5f +
36:                 PlayerColliderWorldHeight * 0.5f - StartGroundingInset);
37:         public static readonly Vector2 LandingRespawnPosition =
38:             new Vector2(12f, 0.95f);
39: 
40:         private static readonly Vector2 HookSize =
41:             new Vector2(1.6f, 0.45f);
42:         private static readonly Color TerrainColor =
43:             new Color(0.96f, 0.55f, 0.18f);
44:         private static readonly Color HookColor =
45:             new Color(0.30f, 0.76f, 1f);
46: 
47:         public static bool ApplyCurrentScene()
48:         {
49:             bool changed = RemoveLegacyFlashlightSpot();
50:             changed |= EnsureTerrain(
51:                 StartFloorName,
52:                 StartFloorPosition,
53:                 StartFloorSize,
54:                 out _);
55:             changed |= EnsureHook();
56:             changed |= EnsureTerrain(
57:                 LandingFloorName,
58:                 LandingFloorPosition,
59:                 LandingFloorSize,
60:                 out GameObject landing);
61: 
62:             if (!landing.TryGetComponent(out TutorialCheckpoint checkpoint))
63:             {
64:                 checkpoint = landing.AddComponent<TutorialCheckpoint>();
65:                 changed = true;
66:             }
67:             checkpoint.Configure(
68:                 2,
69:                 LandingRespawnPosition,
70:                 NextSectionStartingRopeLength);
```

## TutorialSectionTwoSetup.cs

SHA-256 `ccd88a949811ada96d1751ebdcb8273d94e6094d8a06975c8522a4ed30ae99e1`。BuildInputs一致：True。

### 1–65
```text
1: using UnityEngine;
2: 
3: namespace HimoHito
4: {
5:     /// <summary>
6:     /// Builds tutorial T2 so rope lengths six and seven clear the spikes,
7:     /// while the initial length eight drops the player into them.
8:     /// </summary>
9:     public static class TutorialSectionTwoSetup
10:     {
11:         public const string HookName = "Tutorial T2 Hook";
12:         public const string LandingFloorName = "Tutorial T2 Landing";
13:         public const string LeftSpikeName = "Tutorial T2 Spike Left";
14:         public const string CenterSpikeName = "Tutorial T2 Spike Center";
15:         public const string RightSpikeName = "Tutorial T2 Spike Right";
16:         public const int NextSectionStartingRopeLength = 6;
17: 
18:         public static readonly Vector2 HookPosition =
19:             new Vector2(18f, 5.5f);
20:         public static readonly Vector2 LandingFloorPosition =
21:             new Vector2(27f, -4.65f);
22:         public static readonly Vector2 LandingFloorSize =
23:             new Vector2(6f, 10f);
24:         public static readonly Vector2 LandingRespawnPosition =
25:             new Vector2(27f, 0.95f);
26: 
27:         private static readonly Vector2 HookSize =
28:             new Vector2(1.6f, 0.45f);
29:         private static readonly Vector2 SpikeSize =
30:             new Vector2(0.55f, 0.6f);
31:         private static readonly Color TerrainColor =
32:             new Color(0.96f, 0.55f, 0.18f);
33:         private static readonly Color HookColor =
34:             new Color(0.30f, 0.76f, 1f);
35:         private static readonly Color SpikeColor =
36:             new Color(1f, 0.18f, 0.25f);
37: 
38:         public static bool ApplyCurrentScene()
39:         {
40:             bool changed = EnsureHook();
41:             changed |= EnsureTerrain(out GameObject landing);
42:             changed |= EnsureSpike(
43:                 LeftSpikeName,
44:                 new Vector2(17.35f, -2.65f));
45:             changed |= EnsureSpike(
46:                 CenterSpikeName,
47:                 new Vector2(18f, -2.65f));
48:             changed |= EnsureSpike(
49:                 RightSpikeName,
50:                 new Vector2(18.65f, -2.65f));
51: 
52:             if (!landing.TryGetComponent(out TutorialCheckpoint checkpoint))
53:             {
54:                 checkpoint = landing.AddComponent<TutorialCheckpoint>();
55:                 changed = true;
56:             }
57:             checkpoint.Configure(
58:                 3,
59:                 LandingRespawnPosition,
60:                 NextSectionStartingRopeLength);
61:             return changed;
62:         }
63: 
64:         public static GameObject EnsureCreated()
65:         {
```

## TutorialSectionThreeSetup.cs

SHA-256 `b5a4788208fd57acee9735eaff24780f2c55fe7d56afe3f14a97099adee0e076`。BuildInputs一致：True。

### 1–75
```text
1: using UnityEngine;
2: 
3: namespace HimoHito
4: {
5:     /// <summary>
6:     /// Builds tutorial T3 and exposes its authored terrain-to-terrain bridge.
7:     /// </summary>
8:     public static class TutorialSectionThreeSetup
9:     {
10:         public const string LandingFloorName = "Tutorial T3 Landing";
11:         public const string BridgeStartHookName =
12:             "Tutorial T3 Green Bridge Start Hook";
13:         public const string BridgeEndHookName =
14:             "Tutorial T3 Green Bridge End Hook";
15:         public const int RequiredRopeLength = 6;
16:         public const int NextSectionStartingRopeLength = 6;
17: 
18:         public static readonly Vector2 LandingFloorPosition =
19:             new Vector2(39f, -4.65f);
20:         public static readonly Vector2 LandingFloorSize =
21:             new Vector2(6f, 10f);
22:         public static readonly Vector2 LandingRespawnPosition =
23:             new Vector2(39f, 0.95f);
24:         public static readonly Vector2 LeftBridgeEndpoint =
25:             new Vector2(30f, 0.35f);
26:         public static readonly Vector2 RightBridgeEndpoint =
27:             new Vector2(36f, 0.35f);
28: 
29:         private const string BridgeAnchorVisualName =
30:             "Green Rope Anchor Ring Visual";
31:         private static readonly Vector2 BridgeHookSize =
32:             new Vector2(0.62f, 0.62f);
33:         private static readonly Color TerrainColor =
34:             new Color(0.96f, 0.55f, 0.18f);
35:         private static readonly Color BridgeHookColor =
36:             new Color(0.33f, 1f, 0.76f);
37: 
38:         public static bool ApplyCurrentScene()
39:         {
40:             bool changed = EnsureTerrain(out GameObject landing);
41:             changed |= EnsureBridgeHook(
42:                 BridgeStartHookName,
43:                 LeftBridgeEndpoint,
44:                 BridgeEndHookName,
45:                 false);
46:             changed |= EnsureBridgeHook(
47:                 BridgeEndHookName,
48:                 RightBridgeEndpoint,
49:                 BridgeStartHookName,
50:                 true);
51:             if (!landing.TryGetComponent(out TutorialCheckpoint checkpoint))
52:             {
53:                 checkpoint = landing.AddComponent<TutorialCheckpoint>();
54:                 changed = true;
55:             }
56:             checkpoint.Configure(
57:                 4,
58:                 LandingRespawnPosition,
59:                 NextSectionStartingRopeLength);
60:             return changed;
61:         }
62: 
63:         public static GameObject EnsureCreated()
64:         {
65:             ApplyCurrentScene();
66:             return FindSceneObject(LandingFloorName);
67:         }
68: 
69:         private static bool EnsureBridgeHook(
70:             string objectName,
71:             Vector2 position,
72:             string pairedHookName,
73:             bool canAttach)
74:         {
75:             GameObject hook = EnsureObject(
```

## TutorialSectionFourSetup.cs

SHA-256 `ca097254c033e04e2c486bdd900d800cb3226b2aa9a4cc62838be7aeee89b37c`。BuildInputs一致：True。

### 1–80
```text
1: using UnityEngine;
2: 
3: namespace HimoHito
4: {
5:     /// <summary>
6:     /// Builds the tutorial finale. Two length-six platforms meet at the
7:     /// center Hook; removing it with F joins them into one deeper bridge.
8:     /// </summary>
9:     public static class TutorialSectionFourSetup
10:     {
11:         public const string LeftAnchorName =
12:             "Tutorial T4 Green Left Bank";
13:         public const string CenterHookName =
14:             "Tutorial T4 Center Hook";
15:         public const string RightAnchorName =
16:             "Tutorial T4 Green Right Bank";
17:         public const string BeamName = "Tutorial T4 Hanging Beam";
18:         public const string GoalFloorName = "Tutorial T4 Goal Floor";
19:         public const string GoalMarkerName = "Tutorial T4 Toy Box Goal";
20:         public const int PlatformRopeLength = 6;
21: 
22:         public static readonly Vector2 LeftAnchorPosition =
23:             new Vector2(42f, 0.35f);
24:         public static readonly Vector2 CenterHookPosition =
25:             new Vector2(47f, 1.55f);
26:         public static readonly Vector2 RightAnchorPosition =
27:             new Vector2(52f, 0.35f);
28:         public static readonly Vector2 BeamPosition =
29:             new Vector2(48.5f, 3.30f);
30:         public static readonly Vector2 BeamSize =
31:             new Vector2(2f, 5f);
32:         public static readonly Vector2 GoalFloorPosition =
33:             new Vector2(59.5f, -4.65f);
34:         public static readonly Vector2 GoalFloorSize =
35:             new Vector2(15f, 10f);
36:         public static readonly Vector2 GoalMarkerPosition =
37:             new Vector2(63f, 1.55f);
38:         public static readonly Vector2 GoalMarkerSize =
39:             new Vector2(2.4f, 2.2f);
40: 
41:         private const string BridgeAnchorVisualName =
42:             "Green Rope Anchor Ring Visual";
43:         private static readonly Vector2 BridgeHookSize =
44:             new Vector2(0.62f, 0.62f);
45:         private static readonly Vector2 CenterHookSize =
46:             new Vector2(1.6f, 0.45f);
47:         private static readonly Color TerrainColor =
48:             new Color(0.96f, 0.55f, 0.18f);
49:         private static readonly Color BridgeHookColor =
50:             new Color(0.33f, 1f, 0.76f);
51:         private static readonly Color SwingHookColor =
52:             new Color(0.30f, 0.76f, 1f);
53:         private static readonly Color GoalMarkerColor =
54:             new Color(0.80f, 0.48f, 0.18f);
55: 
56:         public static bool ApplyCurrentScene()
57:         {
58:             bool changed = EnsurePairedHook(
59:                 LeftAnchorName,
60:                 LeftAnchorPosition,
61:                 BridgeHookSize,
62:                 BridgeHookColor,
63:                 CenterHookName,
64:                 false,
65:                 true);
66:             changed |= EnsurePairedHook(
67:                 CenterHookName,
68:                 CenterHookPosition,
69:                 CenterHookSize,
70:                 SwingHookColor,
71:                 LeftAnchorName,
72:                 true,
73:                 false);
74:             changed |= EnsurePairedHook(
75:                 RightAnchorName,
76:                 RightAnchorPosition,
77:                 BridgeHookSize,
78:                 BridgeHookColor,
79:                 CenterHookName,
80:                 true,
```

## MainStageSectionFourSetup.cs

SHA-256 `c1173ea1c61b95a6ddd3994456ed34b0d5e1167c04a4edddd43fd6e13012a6dc`。BuildInputs一致：True。

### 1–70
```text
1: using UnityEngine;
2: 
3: namespace HimoHito
4: {
5:     /// <summary>
6:     /// Builds main-stage section four from slide 23 of the 0829 manual.
7:     /// A length-5 permanent rope reaches the relay column, then a length-9
8:     /// swing from that column reaches the far Hook and the opposite bank.
9:     /// </summary>
10:     public static class MainStageSectionFourSetup
11:     {
12:         public const string IntermediateColumnName =
13:             "Main Section 4 Intermediate Column";
14:         public const string FarHookName =
15:             "Main Section 4 Far Hook";
16:         public const string BridgeStartMarkerName =
17:             "Main Section 4 Green Bridge Start Hook";
18:         public const string BridgeAnchorName =
19:             "Main Section 4 Green Bridge End Hook";
20:         private const string LegacyBridgeStartMarkerName =
21:             "Main Section 4 Bridge Start Marker";
22:         private const string LegacyBridgeAnchorName =
23:             "Main Section 4 Yellow Bridge Hook";
24:         public const string LandingName =
25:             "Main Section 4 Landing";
26: 
27:         // Section three's goal bank and this whole section share the +3 translation.
28:         public static readonly Vector2 StartEdgePosition =
29:             new Vector2(64.6f, -2.15f);
30:         public static readonly Vector2 IntermediateTopPosition =
31:             new Vector2(69.2f, -0.65f);
32:         public static readonly Vector2 IntermediateColumnPosition =
33:             new Vector2(69.2f, -5.65f);
34:         public static readonly Vector2 IntermediateColumnSize =
35:             new Vector2(1.8f, 10f);
36:         public static readonly Vector2 BridgeStartMarkerPosition =
37:             new Vector2(64.6f, -2.15f);
38:         public static readonly Vector2 BridgeStartMarkerSize =
39:             new Vector2(0.62f, 0.62f);
40:         public static readonly Vector2 BridgeAnchorPosition =
41:             new Vector2(68.3f, -0.65f);
42:         public static readonly Vector2 BridgeAnchorSize =
43:             new Vector2(0.62f, 0.62f);
44:         public const int BridgeRopeLength = 5;
45:         public static readonly Vector2 FarHookPosition =
46:             new Vector2(75.6f, 2.55f);
47:         public static readonly Vector2 HookSize =
48:             new Vector2(1.6f, 0.45f);
49:         public static readonly Vector2 LandingPosition =
50:             new Vector2(89.1f, -7.15f);
51:         public static readonly Vector2 LandingSize =
52:             new Vector2(6f, 10f);
53:         public static readonly Vector2 LandingRespawnPosition =
54:             new Vector2(87.1f, -1.45f);
55: 
56:         private static readonly Color TerrainColor =
57:             new Color(0.56f, 0.29f, 0.09f);
58:         private static readonly Color HookColor =
59:             new Color(0.298f, 0.765f, 1f);
60:         public static readonly Color BridgeAnchorColor =
61:             new Color(0.33f, 1f, 0.76f);
62: 
63:         public static bool ApplyCurrentScene()
64:         {
65:             bool changed = false;
66:             changed |= RenameLegacyObject(
67:                 LegacyBridgeStartMarkerName,
68:                 BridgeStartMarkerName);
69:             changed |= RenameLegacyObject(
70:                 LegacyBridgeAnchorName,
```

## MainStageSectionFiveSetup.cs

SHA-256 `b7419423f4814c47a3b031c1c5221fbbd26b9272695050e76ffa299879d6def7`。BuildInputs一致：True。

### 1–75
```text
1: using UnityEngine;
2: 
3: namespace HimoHito
4: {
5:     /// <summary>
6:     /// Builds section five from slide 24 of the 0829 stage manual.
7:     /// A length-six bridge between the green shelf Hooks blocks the light,
8:     /// then the central blue Hook provides the final swing to the right bank.
9:     /// </summary>
10:     public static class MainStageSectionFiveSetup
11:     {
12:         public const string LeftShelfName = "Main S05 Left Shelf";
13:         public const string RightShelfName = "Main S05 Right Shelf";
14:         public const string BridgeStartHookName =
15:             "Main Section 5 Green Shadow Start Hook";
16:         public const string BridgeEndHookName =
17:             "Main Section 5 Green Shadow End Hook";
18:         public const string LightMountName = "Main S05 Flashlight Source";
19:         public const string LightOcclusionOriginName =
20:             "Main S05 Light Occlusion Origin";
21:         public const string LightSpotName = "Main S05 Flashlight Spot";
22:         public const string CentralHookName = "Main S05 Central Hook";
23:         public const string LandingName = "Main S05 Landing";
24: 
25:         public const int BridgeRopeLength = 6;
26:         public const int SwingRopeLength = 7;
27: 
28:         // Section four's landing top is Y=-2.15. The shelf tops are 1.5 higher.
29:         public static readonly Vector2 LeftShelfPosition =
30:             new Vector2(93.6f, -0.95f);
31:         public static readonly Vector2 RightShelfPosition =
32:             new Vector2(101f, -0.95f);
33:         public static readonly Vector2 ShelfSize = new Vector2(3f, 0.6f);
34:         public static readonly Vector2 BridgeStartHookPosition =
35:             new Vector2(95.1f, -0.65f);
36:         public static readonly Vector2 BridgeEndHookPosition =
37:             new Vector2(99.5f, -0.65f);
38:         public static readonly Vector2 BridgeHookSize =
39:             new Vector2(0.62f, 0.62f);
40:         public static readonly Vector2 CentralHookPosition =
41:             new Vector2(102f, 2.95f);
42:         public static readonly Vector2 LightMountPosition =
43:             new Vector2(102f, 3.35f);
44:         public static readonly Vector2 LightOcclusionOriginPosition =
45:             new Vector2(97.3f, 2.85f);
46:         public static readonly Vector2 LightSpotPosition =
47:             new Vector2(97.3f, -3.15f);
48:         public const float LightSpotDiameter = 4f;
49:         public static readonly Vector2 LandingPosition =
50:             new Vector2(115.1f, -7.15f);
51:         public static readonly Vector2 LandingSize = new Vector2(8f, 10f);
52:         public static readonly Vector2 LandingRespawnPosition =
53:             new Vector2(112.4f, -1.45f);
54: 
55:         private static readonly Color ShelfColor =
56:             new Color(0.56f, 0.29f, 0.09f);
57:         private static readonly Color HookColor =
58:             new Color(0.298f, 0.765f, 1f);
59:         private static readonly Color BridgeHookColor =
60:             new Color(0.33f, 1f, 0.76f);
61:         private static readonly Color MountColor =
62:             new Color(0.42f, 0.24f, 0.12f);
63:         private static readonly Color LightColor =
64:             new Color(1f, 0.82f, 0.56f, 0.62f);
65: 
66:         public static bool ApplyCurrentScene()
67:         {
68:             bool changed = false;
69:             changed |= DisableLegacyOverlappingObjects();
70:             changed |= EnsureRailShelf(
71:                 LeftShelfName,
72:                 LeftShelfPosition,
73:                 ShelfSize);
74:             changed |= EnsureRailShelf(
75:                 RightShelfName,
```
