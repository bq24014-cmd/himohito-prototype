# UI状態遷移：10/6固定ソースの実本文抜粋

A=0。行番号は原ソースの1-based。仮想操作や内部APIの実行結果ではない。

## Assets/Scripts/StageOverlayControls.cs:9–39

```text
9:         private bool showHelp;
10:         private bool isPaused;
11:         private bool confirmReturnToTitle;
12:         private int lastHelpInputFrame = -1;
13:         private PrototypeRunController runController;
14:         private MainStageRespawnOnFall mainStageRespawn;
15:         private MainStageGoalZone mainStageGoal;
16:         private TutorialSectionGuide tutorialSectionGuide;
17:         private PrototypeAudioFeedback audioFeedback;
18:         private Texture2D helpBackground;
19:         private GUIStyle titleStyle;
20:         private GUIStyle bodyStyle;
21:         private GUIStyle cornerHintStyle;
22:         private static readonly GUIContent CornerHint = new GUIContent("Tab：操作確認　Esc：ポーズ");
23:         private static readonly GUIContent NarrowCornerHint = new GUIContent("Tab：操作確認\nEsc：ポーズ");
24:         private GUIStyle helpTitleStyle;
25:         private GUIStyle helpBodyStyle;
26:         private GUIStyle helpKeyStyle;
27:         private Vector2 pauseScroll;
28: 
29:         public bool IsHelpVisible => showHelp;
30:         public bool IsOverlayVisible => showHelp || isPaused;
31:         public bool HelpInputConsumedThisFrame => lastHelpInputFrame == Time.frameCount;
32: 
33:         private void Awake()
34:         {
35:             runController = FindFirstObjectByType<PrototypeRunController>();
36:             mainStageRespawn = FindFirstObjectByType<MainStageRespawnOnFall>();
37:             mainStageGoal = FindFirstObjectByType<MainStageGoalZone>();
38:             tutorialSectionGuide =
39:                 FindFirstObjectByType<TutorialSectionGuide>();
```

## Assets/Scripts/StageOverlayControls.cs:45–137

```text
45:         private void Update()
46:         {
47:             if (StageStartTransition.IsActive) return;
48:             if (MainStagePreview.IsActive) return;
49: 
50:             if (IsResultScreenActive())
51:             {
52:                 if (IsOverlayVisible)
53:                 {
54:                     showHelp = false;
55:                     isPaused = false;
56:                     ApplyPauseState();
57:                 }
58:                 return;
59:             }
60:             if (tutorialSectionGuide == null)
61:             {
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
82:             }
83: 
84:             bool isOnTitleScreen = runController != null &&
85:                 runController.Outcome == PrototypeRunController.RunOutcome.WaitingToStart;
86:             if (isOnTitleScreen)
87:             {
88:                 return;
89:             }
90: 
91:             if (isPaused && !showHelp && confirmReturnToTitle)
92:             {
93:                 if (Input.GetKeyDown(KeyCode.Escape)) confirmReturnToTitle = false;
94:                 else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
95:                     ReturnToTitle();
96:                 return;
97:             }
98: 
99:             if (isPaused && !showHelp && Input.GetKeyDown(KeyCode.T))
100:             {
101:                 confirmReturnToTitle = true;
102:                 return;
103:             }
104: 
105:             if (Input.GetKeyDown(KeyCode.Escape))
106:             {
107:                 isPaused = !isPaused;
108:                 if (isPaused)
109:                 {
110:                     PlayOpenSound();
111:                 }
112:                 ApplyPauseState();
113:             }
114:         }
115: 
116:         public void OpenMenuHelp()
117:         {
118:             if (StageStartTransition.IsActive) return;
119:             if (runController == null || !runController.IsStageSelectionOpen || IsOverlayVisible) return;
120:             OpenHelp();
121:         }
122: 
123:         private void OpenHelp()
124:         {
125:             confirmReturnToTitle = false;
126:             showHelp = true;
127:             lastHelpInputFrame = Time.frameCount;
128:             PlayOpenSound();
129:             ApplyPauseState();
130:         }
131: 
132:         public void CloseHelp()
133:         {
134:             if (!showHelp) return;
135:             showHelp = false;
136:             lastHelpInputFrame = Time.frameCount;
137:             ApplyPauseState();
```

## Assets/Scripts/StageOverlayControls.cs:149–199

```text
149:         private void OnDisable()
150:         {
151:             HimoHitoAudioSettings.Save();
152:             if (isPaused || showHelp)
153:             {
154:                 Time.timeScale = 1f;
155:                 isPaused = false;
156:                 showHelp = false;
157:             }
158:         }
159: 
160:         private void OnApplicationQuit() => HimoHitoAudioSettings.Save();
161:         private void OnApplicationPause(bool paused)
162:         {
163:             if (paused) HimoHitoAudioSettings.Save();
164:         }
165: 
166:         private void OnGUI()
167:         {
168:             if (StageStartTransition.IsActive) return;
169:             if (MainStagePreview.IsActive) return;
170: 
171:             if (IsResultScreenActive() ||
172:                 (tutorialSectionGuide != null && tutorialSectionGuide.IsVisible))
173:             {
174:                 return;
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
```

## Assets/Scripts/StageOverlayControls.cs:264–283

```text
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
283:             if (close) { CloseHelp(); GUIUtility.ExitGUI(); }
```

## Assets/Scripts/StageOverlayControls.cs:314–387

```text
314:                 (availableHeight - height) * 0.5f,
315:                 width,
316:                 height), GUI.skin.box);
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
329:                 GUI.matrix = previousMatrix;
330:                 if (cancel) confirmReturnToTitle = false;
331:                 if (accept) { ReturnToTitle(); GUIUtility.ExitGUI(); }
332:                 return;
333:             }
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
352:             GUILayout.FlexibleSpace();
353:             Rect resumeButtonRow = GUILayoutUtility.GetRect(
354:                 1f,
355:                 58f,
356:                 GUILayout.ExpandWidth(true),
357:                 GUILayout.Height(58f));
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
373:             if (titleClicked) confirmReturnToTitle = true;
374:         }
375: 
376:         private void ReturnToTitle()
377:         {
378:             if (!isPaused || showHelp || !confirmReturnToTitle) return;
379:             if (!Application.CanStreamedLevelBeLoaded("Tutorial"))
380:             {
381:                 Debug.LogError("ステージ選択へ戻れません。TutorialをBuild Settingsに登録してください。");
382:                 return;
383:             }
384:             HimoHitoAudioSettings.Save();
385:             isPaused = showHelp = confirmReturnToTitle = false;
386:             Time.timeScale = 1f;
387:             SceneManager.LoadScene("Tutorial");
```

## Assets/Scripts/StageOverlayControls.cs:409–425

```text
409:         private void ApplyPauseState()
410:         {
411:             Time.timeScale = isPaused || showHelp ? 0f : 1f;
412:             if (!isPaused && !showHelp) HimoHitoAudioSettings.Save();
413:         }
414: 
415:         private bool IsResultScreenActive()
416:         {
417:             if (runController != null)
418:             {
419:                 return runController.Outcome != PrototypeRunController.RunOutcome.Playing &&
420:                     runController.Outcome != PrototypeRunController.RunOutcome.WaitingToStart;
421:             }
422: 
423:             return (mainStageRespawn != null && mainStageRespawn.IsFailureVisible) ||
424:                 (mainStageGoal != null && mainStageGoal.IsCompleting);
425:         }
```

## Assets/Scripts/TutorialSectionGuide.cs:9–27

```text
9:     public sealed class TutorialSectionGuide : MonoBehaviour
10:     {
11:         private const float InteractionRange = 3.25f;
12:         private const string SignPrefix = "Tutorial Guide Sign T";
13:         private const string SignArtworkPath = "Art/TutorialGuideSign-v1";
14:         private const string CraftSignArtworkPath = "Art/HimoHitoCraftGuideSign-v1";
15:         private static Sprite craftSignSprite;
16:         private const string SignArtworkName = "Picture-book Guide Sign Visual";
17:         private const string HelpBackgroundPath =
18:             "Art/HimoHitoControlsBackground-v1";
19:         private const string PlayerTexturePath = "Art/HimoHitoPlayer-v1";
20: 
21:         private static readonly Vector2[] SignPositions =
22:         {
23:             new Vector2(-6.1f, 1.35f),
24:             new Vector2(10.1f, 1.35f),
25:             new Vector2(25.1f, 1.35f),
26:             new Vector2(37.1f, 1.35f)
27:         };
```

## Assets/Scripts/TutorialSectionGuide.cs:124–172

```text
124:         private void Update()
125:         {
126:             if (StageStartTransition.IsActive) return;
127:             UpdateSignGreetings();
128:             if (MainStagePreview.IsActive) return;
129: 
130:             if (runController == null || playerBody == null)
131:             {
132:                 return;
133:             }
134: 
135:             if (IsVisible)
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
```

## Assets/Scripts/TutorialSectionGuide.cs:196–231

```text
196:         private void OnDisable()
197:         {
198:             foreach (TutorialSignGreeting greeting in signGreetings)
199:                 if (greeting != null) greeting.Cancel();
200:             if (IsVisible)
201:             {
202:                 CloseGuide();
203:             }
204:         }
205: 
206:         private void OnGUI()
207:         {
208:             if (StageStartTransition.IsActive) return;
209:             if (MainStagePreview.IsActive) return;
210: 
211:             GUI.depth = -1100;
212:             HimoHitoGuiTheme.ApplyToSkin(GUI.skin);
213:             EnsureStyles();
214: 
215:             if (IsVisible)
216:             {
217:                 DrawGuide();
218:                 return;
219:             }
220: 
221:             if (runController == null ||
222:                 runController.Outcome !=
223:                     PrototypeRunController.RunOutcome.Playing ||
224:                 (overlayControls != null &&
225:                     overlayControls.IsOverlayVisible) ||
226:                 !IsNearSign(runController.CurrentTutorialSection))
227:             {
228:                 return;
229:             }
230: 
231:             DrawReadPrompt(runController.CurrentTutorialSection);
```

## Assets/Scripts/TutorialSectionGuide.cs:485–553

```text
485:         private bool IsNearSign(int section)
486:         {
487:             if (section < 1 || section > SignPositions.Length)
488:             {
489:                 return false;
490:             }
491:             return Vector2.Distance(
492:                 playerBody.position,
493:                 SignPositions[section - 1]) <= InteractionRange;
494:         }
495: 
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
529:         }
530: 
531:         private void CloseGuide()
532:         {
533:             GetComponent<RopeBodyVisual>()?.EndReadingSign();
534:             openSection = 0;
535:             illustrationTime = 0f;
536:             if (disabledGameplay)
537:             {
538:                 if (playerMover != null)
539:                 {
540:                     playerMover.enabled = wasPlayerMoverEnabled;
541:                 }
542:                 if (ropeController != null)
543:                 {
544:                     ropeController.enabled = wasRopeControllerEnabled;
545:                 }
546:                 if (platformBuilder != null)
547:                 {
548:                     platformBuilder.enabled = wasPlatformBuilderEnabled;
549:                 }
550:                 disabledGameplay = false;
551:             }
552:             Time.timeScale = 1f;
553:         }
```

## Assets/Scripts/TutorialSectionGuide.cs:652–660

```text
652:             Rect closeRect = new Rect(
653:                 content.xMax - 260f,
654:                 content.yMax - 56f,
655:                 260f,
656:                 50f);
657:             HimoHitoUiParts.DrawWoodButtonLabel(
658:                 closeRect,
659:                 "Z　看板を閉じる",
660:                 closeStyle);
```

## Assets/Scripts/PrototypeRunController.cs:137–211

```text
137:         private void Update()
138:         {
139:             if (StageStartTransition.IsActive) return;
140:             if (clearNavigationStarted) return;
141:             if (MainStagePreview.IsActive) return;
142:             if (IsFallUnravelling) return;
143: 
144:             if (Outcome == RunOutcome.WaitingToStart)
145:             {
146:                 UpdateStartScreen();
147:                 return;
148:             }
149: 
150:             if (Outcome == RunOutcome.Clear && Input.GetKeyDown(KeyCode.Escape))
151:             {
152:                 ReturnToStageSelectionFromClear();
153:                 return;
154:             }
155: 
156:             if (Outcome == RunOutcome.Clear &&
157:                 (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
158:             {
159:                 ContinueAfterClear();
160:                 return;
161:             }
162: 
163:             if (Outcome == RunOutcome.Clearing)
164:             {
165:                 return;
166:             }
167: 
168:             if (Outcome == RunOutcome.Failed &&
169:                 Input.GetKeyDown(KeyCode.Escape))
170:             {
171:                 SceneManager.LoadScene("Tutorial");
172:                 return;
173:             }
174: 
175:             if (tutorialSectionGuide != null &&
176:                 tutorialSectionGuide.IsVisible)
177:             {
178:                 return;
179:             }
180: 
181:             if (Input.GetKeyDown(KeyCode.R))
182:             {
183:                 if (Outcome == RunOutcome.Clear)
184:                 {
185:                     RestartTutorial();
186:                 }
187:                 else
188:                 {
189:                     RestartFromCheckpoint();
190:                 }
191:                 return;
192:             }
193: 
194:             if (Outcome != RunOutcome.Playing)
195:             {
196:                 return;
197:             }
198: 
199:             bool fell = body.position.y < fallThreshold;
200:             bool cannotUseRope = !ropeController.IsAttached &&
201:                                  ropeResource.CurrentLength < minimumUsableRopeLength;
202:             if (fell)
203:             {
204:                 Finish(RunOutcome.Failed, RunFailureReason.Fell);
205:                 return;
206:             }
207: 
208:             if (cannotUseRope)
209:             {
210:                 Finish(RunOutcome.Failed, RunFailureReason.RopeExhausted);
211:             }
```

## Assets/Scripts/PrototypeRunController.cs:214–284

```text
214:         private void EnterStartScreen()
215:         {
216:             titleEnteredFrame = Time.frameCount;
217:             ropeController.DetachAndRefund();
218:             body.linearVelocity = Vector2.zero;
219:             body.angularVelocity = 0f;
220:             body.simulated = false;
221:             playerMover.enabled = false;
222:             ropeController.enabled = false;
223:             FailureReason = RunFailureReason.None;
224:             Outcome = RunOutcome.WaitingToStart;
225:             SelectedStageIndex = 0;
226:             StageSelectionError = string.Empty;
227:         }
228: 
229:         private void UpdateStartScreen()
230:         {
231:             // The stage map is the only home screen. Returning from a scene or
232:             // closing help must not reuse that frame's key as Start or Quit.
233:             if (!CanUseTitleMenu())
234:             {
235:                 return;
236:             }
237: 
238:             if (Input.GetKeyDown(KeyCode.Escape)) QuitFromTitle();
239:             else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
240:                 SelectStage(SelectedStageIndex - 1);
241:             else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
242:                 SelectStage(SelectedStageIndex + 1);
243:             else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
244:                 StartSelectedStage();
245:         }
246: 
247:         public bool CanUseTitleMenu() => Outcome == RunOutcome.WaitingToStart && !StageStartTransition.IsActive &&
248:             (!Application.isPlaying || Time.frameCount > titleEnteredFrame) &&
249:             !Input.GetKeyDown(KeyCode.Tab) &&
250:             (overlayControls == null || (!overlayControls.IsOverlayVisible &&
251:                 !overlayControls.HelpInputConsumedThisFrame));
252: 
253:         public void OpenStageSelection()
254:         {
255:             if (!CanUseTitleMenu()) return;
256:             SelectStage(SelectedStageIndex);
257:         }
258: 
259:         public void OpenMenuHelp()
260:         {
261:             if (!CanUseTitleMenu()) return;
262:             overlayControls?.OpenMenuHelp();
263:         }
264: 
265:         public void SelectStage(int index)
266:         {
267:             if (!CanUseTitleMenu() || !IsStageSelectionOpen) return;
268:             SelectedStageIndex = Mathf.Clamp(index, 0, StageCatalog.Entries.Count - 1);
269:             StageSelectionError = string.Empty;
270:         }
271: 
272:         public bool StartSelectedStage()
273:         {
274:             if (!CanUseTitleMenu() || !IsStageSelectionOpen) return false;
275:             StageCatalog.Entry stage = StageCatalog.Entries[SelectedStageIndex];
276:             if (!stage.available) { StageSelectionError = "このステージは追加予定です。今はまだあそべません。"; return false; }
277:             if (stage.scenePath != StageCatalog.TitleScenePath && !Application.CanStreamedLevelBeLoaded(stage.scenePath))
278:             {
279:                 StageSelectionError = "ステージが見つかりません。Build Settingsを確認してください。";
280:                 return false;
281:             }
282:             return StageStartTransition.Begin(this, stage,
283:                 StageSelectionView.SelectedIslandOrigin(SelectedStageIndex, StageCatalog.Entries.Count,
284:                     Screen.width, Screen.height));
```

## Assets/Scripts/PrototypeRunController.cs:295–320

```text
295:         internal void CommitTutorialStart()
296:         {
297:             if (Outcome != RunOutcome.WaitingToStart) return;
298:             Time.timeScale = 1f;
299:             body.simulated = true;
300:             playerMover.enabled = true;
301:             ropeController.enabled = true;
302:             Outcome = RunOutcome.Playing;
303:             MainStagePreview.PlayFor(gameObject);
304:         }
305: 
306:         public void QuitFromTitle()
307:         {
308:             if (!CanUseTitleMenu()) return;
309:             HimoHitoAudioSettings.Save();
310:             Time.timeScale = 1f;
311:             QuitApplication();
312:         }
313: 
314:         private static void QuitApplication()
315:         {
316: #if UNITY_EDITOR
317:             UnityEditor.EditorApplication.isPlaying = false;
318: #else
319:             Application.Quit();
320: #endif
```

## Assets/Scripts/PrototypeRunController.cs:333–354

```text
333:         private void BeginClearSequence()
334:         {
335:             ropeController.DetachAndRefund();
336:             body.linearVelocity = Vector2.zero;
337:             body.angularVelocity = 0f;
338:             body.simulated = false;
339:             FailureReason = RunFailureReason.None;
340:             Outcome = RunOutcome.Clearing;
341: 
342:             GetComponent<RopeBodyVisual>()?.PlayGoalPose(ClearRevealDelay,
343:                 GameObject.Find(TutorialSectionFourSetup.GoalMarkerName)?.transform);
344: 
345:             StartCoroutine(CompleteClearSequence());
346:         }
347: 
348:         private IEnumerator CompleteClearSequence()
349:         {
350:             yield return new WaitForSecondsRealtime(ClearRevealDelay);
351:             GetComponent<PrototypeAudioFeedback>()?.PlayClearRevealed();
352:             Outcome = RunOutcome.Clear;
353:             StageProgress.MarkSceneCleared(gameObject.scene.path);
354:         }
```

## Assets/Scripts/PrototypeRunController.cs:377–409

```text
377:         private void Finish(
378:             RunOutcome outcome,
379:             RunFailureReason failureReason = RunFailureReason.None)
380:         {
381:             ropeController.DetachAndRefund();
382:             body.linearVelocity = Vector2.zero;
383:             body.angularVelocity = 0f;
384:             body.simulated = false;
385:             FailureReason = failureReason;
386:             Outcome = outcome;
387:             if (failureReason == RunFailureReason.Fell)
388:             {
389:                 playerMover.enabled = false;
390:                 ropeController.enabled = false;
391:                 FallUnravelVisual.Play(gameObject);
392:             }
393:             if (failureReason == RunFailureReason.RopeExhausted)
394:             {
395:                 GetComponent<PrototypeAudioFeedback>()?.PlayRopeExhausted();
396:             }
397:         }
398: 
399:         private void RestartFromCheckpoint()
400:         {
401:             body.simulated = true;
402:             ropeController.DetachAndRefund();
403:             RestoreCheckpointState();
404:             body.position = checkpointPosition;
405:             // Also reset the interpolated presentation before camera/reveal code runs.
406:             transform.position = new Vector3(checkpointPosition.x, checkpointPosition.y, transform.position.z);
407:             ResetMotionAndResume();
408:             RespawnWeaveVisual.Play(gameObject);
409:         }
```

## Assets/Scripts/PrototypeRunController.cs:447–488

```text
447:         private void ResetMotionAndResume()
448:         {
449:             GetComponent<PrototypeAudioFeedback>()?.StopRopeExhaustedAudio();
450:             body.linearVelocity = Vector2.zero;
451:             body.angularVelocity = 0f;
452:             playerMover.enabled = true;
453:             ropeController.enabled = true;
454:             FailureReason = RunFailureReason.None;
455:             Outcome = RunOutcome.Playing;
456:             Camera.main?.GetComponent<HorizontalCameraFollow>()?.ResetFraming();
457:         }
458: 
459:         public void ReturnToStageSelectionFromClear()
460:         {
461:             if (Outcome != RunOutcome.Clear || clearNavigationStarted) return;
462:             if (!Application.CanStreamedLevelBeLoaded(StageCatalog.TitleScenePath))
463:             {
464:                 Debug.LogError("ステージ選択へ戻れません。TutorialをBuild Settingsに登録してください。");
465:                 return;
466:             }
467:             clearNavigationStarted = true;
468:             HimoHitoAudioSettings.Save();
469:             Time.timeScale = 1f;
470:             SceneManager.LoadScene(StageCatalog.TitleScenePath);
471:         }
472: 
473:         public void ContinueAfterClear()
474:         {
475:             if (Outcome != RunOutcome.Clear || clearNavigationStarted) return;
476:             LoadNextStage();
477:         }
478: 
479:         private void LoadNextStage()
480:         {
481:             const string MainStageSceneName = "MainStage";
482:             if (Application.CanStreamedLevelBeLoaded(MainStageSceneName))
483:             {
484:                 clearNavigationStarted = true;
485:                 HimoHitoAudioSettings.Save();
486:                 Time.timeScale = 1f;
487:                 SceneManager.LoadScene(MainStageSceneName);
488:                 return;
```

## Assets/Scripts/MainStageRespawnOnFall.cs:9–12

```text
9:     /// lower bound for the sections ahead.
10:     /// </summary>
11:     [DefaultExecutionOrder(-200)]
12:     [RequireComponent(typeof(Rigidbody2D), typeof(RopeResource), typeof(RopeController))]
```

## Assets/Scripts/MainStageRespawnOnFall.cs:108–175

```text
108:         private void Update()
109:         {
110:             if (StageStartTransition.IsActive) return;
111:             if (MainStagePreview.IsActive) return;
112: 
113:             goalZone ??= FindFirstObjectByType<MainStageGoalZone>();
114:             if (goalZone != null && goalZone.IsCompleting)
115:             {
116:                 return;
117:             }
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
140:             // between a retry teleport and the next physics step.
141:             if (body.position.y < fallThreshold)
142:             {
143:                 EnterFallFailureState();
144:                 return;
145:             }
146: 
147:             if (!ropeController.IsAttached &&
148:                 ropeResource.CurrentLength < minimumUsableRopeLength)
149:             {
150:                 EnterRopeExhaustedState();
151:             }
152:         }
153: 
154:         private void EnterRopeExhaustedState()
155:         {
156:             body.linearVelocity = Vector2.zero;
157:             body.angularVelocity = 0f;
158:             body.simulated = false;
159:             playerMover.enabled = false;
160:             ropeController.enabled = false;
161:             IsRopeExhausted = true;
162:             IsFallFailure = false;
163:             audioFeedback?.PlayRopeExhausted();
164:         }
165: 
166:         private void EnterFallFailureState()
167:         {
168:             body.linearVelocity = Vector2.zero;
169:             body.angularVelocity = 0f;
170:             body.simulated = false;
171:             playerMover.enabled = false;
172:             ropeController.enabled = false;
173:             IsRopeExhausted = false;
174:             IsFallFailure = true;
175:             FallUnravelVisual.Play(gameObject);
```

## Assets/Scripts/MainStageRespawnOnFall.cs:222–240

```text
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
```

## Assets/Scripts/MainStageGoalZone.cs:13–88

```text
13:         private const float ClearRevealDelay = 1.35f;
14: 
15:         public bool IsClear { get; private set; }
16:         public bool IsCompleting => isClearPending || IsClear;
17:         private bool isClearPending;
18:         private bool clearNavigationStarted;
19: 
20:         private void Update()
21:         {
22:             if (!IsClear || clearNavigationStarted) return;
23:             if (Input.GetKeyDown(KeyCode.Escape)) ReturnToStageSelectionFromClear();
24:             else if (Input.GetKeyDown(KeyCode.R)) RetryAfterClear();
25:         }
26: 
27:         public void RetryAfterClear()
28:         {
29:             if (!IsClear || clearNavigationStarted) return;
30:             clearNavigationStarted = true;
31:             HimoHitoAudioSettings.Save();
32:             Time.timeScale = 1f;
33:             SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
34:         }
35: 
36:         public void ReturnToStageSelectionFromClear()
37:         {
38:             if (!IsClear || clearNavigationStarted) return;
39:             if (!Application.CanStreamedLevelBeLoaded(StageCatalog.TitleScenePath))
40:             {
41:                 Debug.LogError("ステージ選択へ戻れません。TutorialをBuild Settingsに登録してください。");
42:                 return;
43:             }
44:             clearNavigationStarted = true;
45:             HimoHitoAudioSettings.Save();
46:             Time.timeScale = 1f;
47:             SceneManager.LoadScene(StageCatalog.TitleScenePath);
48:         }
49: 
50:         private void OnCollisionStay2D(Collision2D collision)
51:         {
52:             if (IsClear ||
53:                 isClearPending ||
54:                 collision.rigidbody == null ||
55:                 collision.rigidbody.position.y < transform.position.y ||
56:                 !collision.rigidbody.TryGetComponent(out RopeResource _))
57:             {
58:                 return;
59:             }
60: 
61:             if (!GoalChestPresentation.ReadyToClear(collision, GetComponent<Collider2D>(),
62:                 MainStageSectionTenSetup.GoalMarkerName)) return;
63: 
64:             if (collision.rigidbody.TryGetComponent(out RopeController ropeController))
65:             {
66:                 ropeController.DetachAndRefund();
67:             }
68: 
69:             collision.rigidbody.linearVelocity = Vector2.zero;
70:             collision.rigidbody.angularVelocity = 0f;
71:             collision.rigidbody.simulated = false;
72: 
73:             isClearPending = true;
74:             collision.rigidbody.GetComponent<RopeBodyVisual>()?.PlayGoalPose(ClearRevealDelay,
75:                 GameObject.Find(MainStageSectionTenSetup.GoalMarkerName)?.transform);
76:             PrototypeAudioFeedback audioFeedback =
77:                 collision.rigidbody.GetComponent<PrototypeAudioFeedback>();
78:             StartCoroutine(CompleteClearSequence(audioFeedback));
79:         }
80: 
81:         private IEnumerator CompleteClearSequence(
82:             PrototypeAudioFeedback audioFeedback)
83:         {
84:             yield return new WaitForSecondsRealtime(ClearRevealDelay);
85:             audioFeedback?.PlayClearRevealed();
86:             IsClear = true;
87:             isClearPending = false;
88:             StageProgress.MarkSceneCleared(gameObject.scene.path);
```

## Assets/Scripts/PlayerMover.cs:110–125

```text
110:         private void Update()
111:         {
112:             if (ClearInputWhileSimulationStopped()) return;
113: 
114:             // Arrow keys are reserved for rope aiming, so movement uses A/D only.
115:             float left = Input.GetKey(KeyCode.A) ? -1f : 0f;
116:             float right = Input.GetKey(KeyCode.D) ? 1f : 0f;
117:             moveInput = left + right;
118: 
119:             if (Input.GetButtonDown("Jump"))
120:             {
121:                 jumpBufferTimer = jumpBufferTime;
122:             }
123: 
124:             jumpBufferTimer -= Time.deltaTime;
125:         }
```

## Assets/Scripts/PlayerMover.cs:171–195

```text
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

## Assets/Scripts/PlayerMover.cs:218–226

```text
218:         private bool ClearInputWhileSimulationStopped()
219:         {
220:             if (body != null && body.simulated) return false;
221: 
222:             // Clear/failure presentation owns the pose while physics is stopped.
223:             // Do not read keys, move the body, or carry buffered steps/jumps into retry.
224:             moveInput = 0f;
225:             jumpBufferTimer = coyoteTimer = footstepTimer = 0f;
226:             return true;
```

## Assets/Scripts/RopeController.cs:141–195

```text
141:         private void Update()
142:         {
143:             UpdateSelectedRopeLength();
144:             UpdateKeyboardAim();
145: 
146:             if (Input.GetKeyDown(KeyCode.E))
147:             {
148:                 if (IsAttached)
149:                 {
150:                     DetachAndRefund(playReleaseSound: true);
151:                 }
152:                 else
153:                 {
154:                     Vector2 keyboardTarget =
155:                         body.position + keyboardAimDirection * maximumShotDistance;
156:                     TryAttach(keyboardTarget);
157:                 }
158:             }
159: 
160:             if (Input.GetMouseButtonDown(0))
161:             {
162:                 if (IsAttached)
163:                 {
164:                     DetachAndRefund(playReleaseSound: true);
165:                 }
166:                 else
167:                 {
168:                     TryAttachTowardCursor();
169:                 }
170:             }
171:         }
172: 
173:         private void LateUpdate()
174:         {
175:             UpdateAimHookFeedback();
176:             if (!IsAttached)
177:             {
178:                 DrawAimGuide();
179:                 return;
180:             }
181: 
182:             lineRenderer.enabled = true;
183:             float visibleRopeWidth = GetVisibleRopeWidth();
184:             lineRenderer.startWidth = visibleRopeWidth;
185:             lineRenderer.endWidth = visibleRopeWidth;
186:             Color visibleRopeColor = GetVisibleRopeColor();
187:             lineRenderer.startColor = visibleRopeColor;
188:             lineRenderer.endColor = visibleRopeColor;
189:             DrawAttachedRope();
190:         }
191: 
192:         private void OnDisable()
193:         {
194:             HideAimHookFeedback();
195:             DetachAndRefund();
```

## Assets/Scripts/RopeController.cs:223–282

```text
223:         public bool TryAttach(Vector2 worldTarget)
224:         {
225:             if (IsAttached)
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
279:             ropeRevealStartedAt = Time.time;
280:             hookArrivalPending = true;
281:             DrawAttachedRope();
282:             return true;
```

## Assets/Scripts/RopeController.cs:335–355

```text
335:         private void OpenAirChainReconnectWindow(HookPoint releasedHook)
336:         {
337:             if (releasedHook == null || !releasedHook.IsAirChainStep)
338:             {
339:                 ClearAirChainReconnectWindow();
340:                 return;
341:             }
342: 
343:             pendingAirChainGroup = releasedHook.AirChainGroup;
344:             pendingAirChainOrder = releasedHook.AirChainOrder;
345:             airChainReconnectExpiresAt =
346:                 Time.unscaledTime + airChainReconnectWindow;
347:         }
348: 
349:         private bool CanReconnectAirChainTo(HookPoint targetHook)
350:         {
351:             return targetHook != null &&
352:                    targetHook.IsAirChainStep &&
353:                    Time.unscaledTime <= airChainReconnectExpiresAt &&
354:                    targetHook.AirChainGroup == pendingAirChainGroup &&
355:                    targetHook.AirChainOrder == pendingAirChainOrder + 1;
```

## Assets/Scripts/RopeController.cs:509–517

```text
509:         private bool TryResolveAvailableAttachment(Vector2 worldTarget,
510:             out Vector2 resolvedAnchor, out HookPoint hookPoint)
511:         {
512:             bool found = IsAirChainReconnectOpen
513:                 ? TryResolveNextAirChainHook(out resolvedAnchor, out hookPoint)
514:                 : TryResolveAttachmentPoint(worldTarget, out resolvedAnchor, out hookPoint);
515:             return found && (playerMover == null || playerMover.IsGrounded ||
516:                 CanReconnectAirChainTo(hookPoint));
517:         }
```

## Assets/Scripts/RopeController.cs:650–667

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
```

## Assets/Scripts/RopeController.cs:730–783

```text
730:         private int GetMaximumSelectableRopeLength()
731:         {
732:             float availableLength = ropeResource != null
733:                 ? ropeResource.CurrentLength
734:                 : maximumShotDistance;
735:             int availableWholeUnits = Mathf.FloorToInt(
736:                 Mathf.Min(maximumShotDistance, availableLength));
737:             return Mathf.Max(minimumSelectableRopeLength, availableWholeUnits);
738:         }
739: 
740:         private void TryAttachTowardCursor()
741:         {
742:             if (mainCamera == null)
743:             {
744:                 mainCamera = Camera.main;
745:             }
746: 
747:             if (mainCamera == null)
748:             {
749:                 return;
750:             }
751: 
752:             Vector3 cursor = mainCamera.ScreenToWorldPoint(Input.mousePosition);
753:             TryAttach(new Vector2(cursor.x, cursor.y));
754:         }
755: 
756:         private bool UpdateKeyboardAim()
757:         {
758:             bool changed = false;
759: 
760:             if (Input.GetKeyDown(KeyCode.UpArrow))
761:             {
762:                 keyboardAimDirection = Vector2.up;
763:                 changed = true;
764:             }
765: 
766:             if (Input.GetKeyDown(KeyCode.DownArrow))
767:             {
768:                 keyboardAimDirection = Vector2.down;
769:                 changed = true;
770:             }
771: 
772:             float rotation = 0f;
773:             if (Input.GetKey(KeyCode.LeftArrow)) rotation += aimRotationSpeed * Time.deltaTime;
774:             if (Input.GetKey(KeyCode.RightArrow)) rotation -= aimRotationSpeed * Time.deltaTime;
775: 
776:             if (!Mathf.Approximately(rotation, 0f))
777:             {
778:                 keyboardAimDirection = Quaternion.Euler(0f, 0f, rotation) * keyboardAimDirection;
779:                 keyboardAimDirection.Normalize();
780:                 changed = true;
781:             }
782: 
783:             return changed;
```

## Assets/Scripts/RopePlatformBuilder.cs:9–12

```text
9:     /// the aimed hook when two generated bridges meet there and joins both arcs.
10:     /// </summary>
11:     [DefaultExecutionOrder(-100)]
12:     [RequireComponent(typeof(Rigidbody2D), typeof(RopeResource), typeof(RopeController))]
```

## Assets/Scripts/RopePlatformBuilder.cs:53–126

```text
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
69: 
70:         private void Awake()
71:         {
72:             body = GetComponent<Rigidbody2D>();
73:             bodyCollider = GetComponent<Collider2D>();
74:             ropeResource = GetComponent<RopeResource>();
75:             ropeController = GetComponent<RopeController>();
76:             audioFeedback = GetComponent<PrototypeAudioFeedback>();
77:             if (audioFeedback == null)
78:             {
79:                 audioFeedback = gameObject.AddComponent<PrototypeAudioFeedback>();
80:             }
81:         }
82: 
83:         private void Update()
84:         {
85:             if (MainStagePreview.IsActive) return;
86:             // Failure presentation must not accept Q/F while simulation is stopped.
87:             if (body != null && !body.simulated) return;
88: 
89:             if (Input.GetKeyDown(KeyCode.Q) && IsPlatformBuildingUnlocked)
90:             {
91:                 TryBuildCurrentPlatform();
92:             }
93: 
94:             if (Input.GetKeyDown(KeyCode.F) && IsHookRemovalUnlocked())
95:             {
96:                 TryRemoveAimedHook();
97:             }
98:         }
99: 
100:         public bool TryBuildCurrentPlatform()
101:         {
102:             if (!CanBuild(out float ropeLength, out Vector2 start, out Vector2 end) ||
103:                 !ropeController.CommitAttachedRopeAsPlatform(ropeLength))
104:             {
105:                 return false;
106:             }
107: 
108:             // Keep standing on an existing bridge while extending the route.
109:             // Its sag depends on its own length; the new endpoint is not the
110:             // player's position, even when both bridges share the same Hook.
111:             GeneratedRopePlatform existingSupport = FindSupportingPlatform();
112:             GeneratedRopePlatform platform = CreatePlatform(start, end, ropeLength);
113:             RopeResourceGauge.NotifyPlatformBuilt(ropeResource, ropeLength);
114:             if (existingSupport == null)
115:             {
116:                 PlacePlayerOnPlatform(platform);
117:             }
118:             if (TryGetComponent(out PlayerMover mover))
119:             {
120:                 mover.RegisterGeneratedRopePlatformContact(existingSupport != null ? existingSupport : platform);
121:             }
122:             audioFeedback?.PlayRopePlatformBuilt();
123:             RopeBuildFluff.Play(platform.GetComponent<LineRenderer>());
124:             RopeBridgeReveal.Play(platform.GetComponent<LineRenderer>(), transform.position);
125:             GetComponent<RopeBodyVisual>()?.PlayWeavePose();
126:             return true;
```

## Assets/Scripts/RopePlatformBuilder.cs:143–185

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
185:             return true;
```

## Assets/Scripts/RopePlatformBuilder.cs:351–418

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
406:         }
407: 
408:         private bool IsHookRemovalUnlocked()
409:         {
410:             PrototypeRunController tutorial = GetComponent<PrototypeRunController>();
411:             if (tutorial != null && gameObject.scene.name == "Tutorial")
412:             {
413:                 return tutorial.CurrentTutorialSection >= 4;
414:             }
415: 
416:             MainStageRespawnOnFall main = GetComponent<MainStageRespawnOnFall>();
417:             return main == null || main.CurrentSection >= 9;
418:         }
```

## Assets/Scripts/MainStageSectionThreeRecoverySwitch.cs:62–94

```text
62:         private void Update()
63:         {
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
80:             {
81:                 stairs.Hide();
82:                 SetVisualState(false);
83:             }
84:         }
85: 
86:         private void TryActivate()
87:         {
88:             if (MainStagePreview.IsActive || nearbyPlayer == null || !nearbyPlayer.enabled ||
89:                 Time.timeScale <= 0f || stairs == null || stairs.IsRevealed) return;
90:             // Activation is immediate. The character gesture is presentation only.
91:             stairs.Reveal();
92:             SetVisualState(true);
93:             nearbyPlayer.GetComponent<RopeBodyVisual>()?.PlaySwitchPressPose(transform);
94:         }
```

## Assets/Scripts/StageSelectionView.cs:47–69

```text
47:             bool oldEnabled = GUI.enabled;
48:             bool quit = false, help = false, start = false;
49:             try
50:             {
51:                 GUI.color = new Color(.055f, .045f, .085f);
52:                 GUI.DrawTexture(new Rect(0, 0, viewportWidth, viewportHeight), Texture2D.whiteTexture);
53:                 float scale = Mathf.Min(viewportWidth / 1280f, viewportHeight / 720f);
54:                 GUI.matrix = oldMatrix * Matrix4x4.TRS(
55:                     new Vector3((viewportWidth - 1280 * scale) / 2, (viewportHeight - 720 * scale) / 2, 0),
56:                     Quaternion.identity, new Vector3(scale, scale, 1));
57:                 GUI.color = Color.white;
58:                 if (background != null) GUI.DrawTexture(new Rect(0, 0, 1280, 720), background);
59:                 GUI.enabled = oldEnabled && run.CanUseTitleMenu();
60: 
61:                 GUI.Label(new Rect(240, 51, 800, 23), "H I M O H I T O", eyebrow);
62:                 GUI.Label(new Rect(180, 74, 920, 66), "ヒモヒト", heading);
63:                 GUI.Label(new Rect(240, 140, 800, 29), "あそぶ場所を、えらぼう", description);
64: 
65:                 var stages = StageCatalog.Entries;
66:                 int selected = run.SelectedStageIndex;
67:                 selectionWeave.Observe(stages[selected].id, now, GUI.enabled);
68:                 dioramaState.Configure(stages);
69:                 dioramaState.Observe(stages[selected].id, now, GUI.enabled);
```

## Assets/Scripts/StageSelectionView.cs:111–164

```text
111:                     if (GUI.Button(iconRect, GUIContent.none, GUIStyle.none)) run.SelectStage(index);
112:                     GUI.Label(new Rect(node.x - 162, node.y + 91, 324, 28),
113:                         stage.available ? stage.subtitle : "COMING SOON  /  追加予定",
114:                         stage.available ? small : selectedLabel);
115:                 }
116: 
117:                 StageCatalog.Entry chosen = stages[run.SelectedStageIndex];
118:                 bool hasError = !string.IsNullOrEmpty(run.StageSelectionError);
119:                 FitLabel(new Rect(255, 522, 770, 52), hasError ? run.StageSelectionError : chosen.description,
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
148:             }
149:             if (quit) { run.QuitFromTitle(); GUIUtility.ExitGUI(); }
150:             if (help) { run.OpenMenuHelp(); GUIUtility.ExitGUI(); }
151:             if (start && run.StartSelectedStage()) GUIUtility.ExitGUI();
152:         }
153: 
154:         private bool ClothButton(Rect rect, string text, GUIStyle style, bool selected)
155:         {
156:             bool clicked = GUI.Button(rect, GUIContent.none, GUIStyle.none);
157:             Color old = GUI.color;
158:             bool hover = GUI.enabled && rect.Contains(Event.current.mousePosition);
159:             GUI.color = selected || hover ? Color.white : new Color(.83f, .85f, .88f);
160:             if (pieces != null && pieces[2] != null) DrawSprite(rect, pieces[2], true);
161:             else { GUI.color = Cream; GUI.DrawTexture(rect, Texture2D.whiteTexture); }
162:             GUI.color = old;
163:             FitLabel(new Rect(rect.x + 32, rect.y + 8, rect.width - 64, rect.height - 16), text, style, style.fontSize, 14);
164:             return clicked;
```

## Assets/Scripts/HimoHitoUiParts.cs:219–242

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
```

## Assets/Scripts/PrototypeHud.cs:68–134

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

## Assets/Scripts/MainStageHud.cs:54–110

```text
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
110:         }
```

## Assets/Scripts/MainStagePreview.cs:108–125

```text
108:             StageStartTransition.ReleasePlayerForPreview(player.gameObject);
109:             bodyWasSimulated = playerBody.simulated;
110:             moverWasEnabled = playerMover != null && playerMover.enabled;
111:             ropeWasEnabled = ropeController != null && ropeController.enabled;
112:             followWasEnabled = cameraFollow != null && cameraFollow.enabled;
113:             if (cameraFollow != null) cameraFollow.enabled = false;
114:             playerBody.simulated = false;
115:             if (playerMover != null) playerMover.enabled = false;
116:             if (ropeController != null) ropeController.enabled = false;
117: 
118:             elapsed = 0f;
119:             beganFrame = Time.frameCount;
120:             finishPending = false;
121:             hasStarted = true;
122:             IsPreviewing = true;
123:             activePreview = this;
124:             transform.position = goalView;
125:             return true;
```

## Assets/Scripts/MainStagePreview.cs:128–165

```text
128:         private void Update()
129:         {
130:             if (!IsPreviewing) return;
131:             if (StageStartTransition.IsActive)
132:             {
133:                 // Tour is already parked at the goal; don't spend its hold/travel time under cloth.
134:                 beganFrame = Time.frameCount;
135:                 return;
136:             }
137:             if (playerBody == null || previewCamera == null)
138:             {
139:                 RestoreState();
140:                 return;
141:             }
142:             // The Enter that starts Tutorial must not also skip its tour.
143:             bool skip = Time.frameCount > beganFrame + 1
144:                 && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)
145:                     || Input.GetKeyDown(KeyCode.KeypadEnter));
146:             Advance(Time.unscaledDeltaTime, skip);
147:         }
148: 
149:         private void Advance(float deltaTime, bool skip)
150:         {
151:             if (!IsPreviewing || finishPending) return;
152:             elapsed += Mathf.Max(0f, deltaTime);
153:             float travelTime = Mathf.Max(0f, elapsed - holdDuration);
154:             float progress = PanProgress(travelTime, travelDuration, rampDuration);
155:             transform.position = Vector3.Lerp(goalView, startView, progress);
156:             // No zoom pumping or vertical swaying during the tour.
157:             previewCamera.orthographicSize = gameplayOrthographicSize;
158:             if (skip || travelTime >= travelDuration) finishPending = true;
159:         }
160: 
161:         private void LateUpdate()
162:         {
163:             // Restore after every input Update, so skip-Space cannot also trigger a jump.
164:             if (finishPending) RestoreState();
165:         }
```

## Assets/Scripts/MainStagePreview.cs:184–195

```text
184:         private void RestoreState()
185:         {
186:             if (!IsPreviewing) return;
187:             transform.position = startView;
188:             if (previewCamera != null) previewCamera.orthographicSize = gameplayOrthographicSize;
189:             if (playerBody != null) playerBody.simulated = bodyWasSimulated;
190:             if (playerMover != null) playerMover.enabled = moverWasEnabled;
191:             if (ropeController != null) ropeController.enabled = ropeWasEnabled;
192:             if (cameraFollow != null)
193:             {
194:                 cameraFollow.enabled = followWasEnabled;
195:                 if (followWasEnabled) cameraFollow.ResetFraming();
```

## Assets/Scripts/StageStartTransition.cs:7–15

```text
7:     [DefaultExecutionOrder(9000)]
8:     public sealed class StageStartTransition : MonoBehaviour
9:     {
10:         internal const float ThreadDuration = 0.22f;
11:         internal const float CloseDuration = 0.58f;
12:         internal const float OpenDuration = 0.62f;
13:         private enum Phase { Thread, Closing, Covered, Loading, Opening }
14:         private static StageStartTransition active;
15:         public static bool IsActive => active != null && active.isActiveAndEnabled;
```

## Assets/Scripts/StageStartTransition.cs:35–49

```text
35:         internal static bool Begin(PrototypeRunController owner, StageCatalog.Entry stage, Vector2 normalizedOrigin)
36:         {
37:             if (!Application.isPlaying || IsActive || owner == null || stage == null || !stage.available)
38:                 return false;
39:             GameObject root = new GameObject("Stage start cloth curtain");
40:             DontDestroyOnLoad(root);
41:             StageStartTransition transition = root.AddComponent<StageStartTransition>();
42:             transition.source = owner;
43:             transition.targetPath = stage.scenePath;
44:             transition.stageTitle = stage.title;
45:             transition.origin = normalizedOrigin;
46:             active = transition;
47:             SceneManager.sceneLoaded += transition.OnSceneLoaded;
48:             return true;
49:         }
```

## Assets/Scripts/StageStartTransition.cs:98–149

```text
98:         private void CommitStart()
99:         {
100:             if (source == null) { Finish(); return; }
101:             if (targetPath == StageCatalog.TitleScenePath)
102:             {
103:                 destinationPlayer = source.gameObject;
104:                 source.CommitTutorialStart();
105:                 if (!MainStagePreview.IsActive) HoldPlayer(destinationPlayer);
106:                 Enter(Phase.Opening);
107:                 return;
108:             }
109:             // Revalidate after animation in case the destination became unavailable.
110:             if (!Application.CanStreamedLevelBeLoaded(targetPath))
111:             {
112:                 Fail("ステージが見つかりません。Build Settingsを確認してください。");
113:                 return;
114:             }
115:             Time.timeScale = 1f;
116:             HimoHitoAudioSettings.Save();
117:             Enter(Phase.Loading);
118:             loadedFrame = Time.frameCount;
119:             try
120:             {
121:                 load = SceneManager.LoadSceneAsync(targetPath, LoadSceneMode.Single);
122:                 if (load == null) Fail("ステージを読み込めませんでした。");
123:             }
124:             catch (System.Exception exception)
125:             {
126:                 Debug.LogException(exception);
127:                 Fail("ステージを読み込めませんでした。");
128:             }
129:         }
130: 
131:         private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
132:         {
133:             if (mode == LoadSceneMode.Additive) return;
134:             if (phase != Phase.Loading || scene.path != targetPath)
135:             {
136:                 Finish();
137:                 return;
138:             }
139:             destinationSeen = true;
140:             loadedFrame = Time.frameCount;
141:             foreach (GameObject root in scene.GetRootGameObjects())
142:             {
143:                 PlayerMover mover = root.GetComponentInChildren<PlayerMover>();
144:                 if (mover == null) continue;
145:                 destinationPlayer = mover.gameObject;
146:                 HoldPlayer(destinationPlayer);
147:                 break;
148:             }
149:         }
```

## Assets/Scripts/StageStartTransition.cs:151–182

```text
151:         private void HoldPlayer(GameObject player)
152:         {
153:             heldBody = player.GetComponent<Rigidbody2D>();
154:             heldMover = player.GetComponent<PlayerMover>();
155:             heldRope = player.GetComponent<RopeController>();
156:             bodyWasSimulated = heldBody != null && heldBody.simulated;
157:             moverWasEnabled = heldMover != null && heldMover.enabled;
158:             ropeWasEnabled = heldRope != null && heldRope.enabled;
159:             if (heldBody != null) heldBody.simulated = false;
160:             if (heldMover != null) heldMover.enabled = false;
161:             if (heldRope != null) heldRope.enabled = false;
162:         }
163: 
164:         // Called immediately before the tour snapshots control state, never a frame earlier.
165:         internal static void ReleasePlayerForPreview(GameObject player)
166:         {
167:             if (IsActive && active.destinationPlayer == player) active.ReleaseHold();
168:         }
169: 
170:         private void ReleaseHold()
171:         {
172:             if (heldBody != null) heldBody.simulated = bodyWasSimulated;
173:             if (heldMover != null) heldMover.enabled = moverWasEnabled;
174:             if (heldRope != null) heldRope.enabled = ropeWasEnabled;
175:             heldBody = null;
176:             heldMover = null;
177:             heldRope = null;
178:         }
179: 
180:         private void Enter(Phase next) { phase = next; elapsed = 0; }
181: 
182:         private void Fail(string message)
```

## Assets/Scripts/RespawnWeaveVisual.cs:24–75

```text
24:         public static void Play(GameObject player)
25:         {
26:             if (player == null) return;
27:             if (!player.TryGetComponent(out RespawnWeaveVisual effect))
28:                 effect = player.AddComponent<RespawnWeaveVisual>();
29:             effect.Begin();
30:         }
31: 
32:         private void Begin()
33:         {
34:             GetComponent<FallUnravelVisual>()?.Cancel();
35:             Restore();
36:             if (!isActiveAndEnabled) return;
37:             body = GetComponent<Rigidbody2D>();
38:             mover = GetComponent<PlayerMover>();
39:             if (body == null || !body.simulated || MainStagePreview.IsActive) return;
40:             Shader shader = Resources.Load<Shader>("RespawnWeave");
41:             if (shader == null || !shader.isSupported) return;
42:             if (TryGetComponent(out RopeBodyVisual art)) art.CollectCharacterRenderers(renderers);
43:             else if (TryGetComponent(out SpriteRenderer fallback)) renderers.Add(fallback);
44:             if (renderers.Count == 0) return;
45:             if (revealMaterial == null)
46:                 revealMaterial = new Material(shader) { name = "Player Respawn Stitches", hideFlags = HideFlags.HideAndDontSave };
47:             foreach (SpriteRenderer renderer in renderers)
48:             {
49:                 originalMaterials.Add(renderer.sharedMaterial);
50:                 renderer.sharedMaterial = revealMaterial;
51:             }
52:             elapsed = 0f;
53:             playing = true;
54:             UpdateArt(0f);
55:         }
56: 
57:         private void LateUpdate()
58:         {
59:             if (!playing) return;
60:             if (body == null || !body.simulated || (mover != null && !mover.enabled) || MainStagePreview.IsActive)
61:             {
62:                 Restore();
63:                 return;
64:             }
65:             Advance(Time.deltaTime);
66:         }
67: 
68:         private void Advance(float deltaTime)
69:         {
70:             if (!playing) return;
71:             elapsed += Mathf.Max(0f, deltaTime);
72:             if (elapsed >= Duration) { Restore(); return; }
73:             // Bounds follow the current pose after RopeBodyVisual.LateUpdate, including movement.
74:             UpdateArt(Mathf.Clamp01(elapsed / Duration));
75:         }
```

## メタデータの照合

これらに入力所有者の順序を指定する独自値は見当たらない。属性と実行時の順序全体を実測したという意味ではない。

### Assets/Scripts/StageOverlayControls.cs.meta

```text
fileFormatVersion: 2
guid: 74d5c2b07e774ff6a393114466535355
```

### Assets/Scripts/TutorialSectionGuide.cs.meta

```text
fileFormatVersion: 2
guid: b35af0c8805c4c7d87229cbb4bd48e44
```

### Assets/Scripts/PrototypeRunController.cs.meta

```text
fileFormatVersion: 2
guid: 7e21f0a9d34f4bb0a2c58817f851b2d4
```

### Assets/Scripts/RopePlatformBuilder.cs.meta

```text
fileFormatVersion: 2
guid: 74a582b16d6849cda6bd71fa1b4ea16f
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData: 
  assetBundleName: 
  assetBundleVariant:
```

### Assets/Scripts/MainStageRespawnOnFall.cs.meta

```text
fileFormatVersion: 2
guid: 085691a767a788d4cb3948e3318d8cbb
```

### Assets/Scripts/MainStageGoalZone.cs.meta

```text
fileFormatVersion: 2
guid: 8f91dd2dfa544d96a0d4754e2ab2961c
```

# r1：依存関数・Scene状態の追加根拠

## Assets/Scripts/PrototypeRunController.cs:84–103

```text
84:             body = GetComponent<Rigidbody2D>();
85:             ropeResource = GetComponent<RopeResource>();
86:             ropeController = GetComponent<RopeController>();
87:             platformBuilder = GetComponent<RopePlatformBuilder>();
88:             if (platformBuilder == null)
89:             {
90:                 platformBuilder = gameObject.AddComponent<RopePlatformBuilder>();
91:             }
92:             playerMover = GetComponent<PlayerMover>();
93:             overlayControls = FindFirstObjectByType<StageOverlayControls>();
94:             if (overlayControls == null)
95:             {
96:                 overlayControls = gameObject.AddComponent<StageOverlayControls>();
97:             }
98:             tutorialSectionGuide = GetComponent<TutorialSectionGuide>();
99:             if (tutorialSectionGuide == null)
100:             {
101:                 tutorialSectionGuide =
102:                     gameObject.AddComponent<TutorialSectionGuide>();
103:             }
```

## Assets/Scripts/MainStageHud.cs:28–44

```text
28:         private void Awake()
29:         {
30:             // Saved scenes may predate the overlay component added by the scene builder.
31:             if (FindFirstObjectByType<StageOverlayControls>() == null)
32:                 gameObject.AddComponent<StageOverlayControls>();
33:             ropeResource = FindFirstObjectByType<RopeResource>();
34:             ropeController = FindFirstObjectByType<RopeController>();
35:             platformBuilder = FindFirstObjectByType<RopePlatformBuilder>();
36:             preview = FindFirstObjectByType<MainStagePreview>();
37:             respawn = FindFirstObjectByType<MainStageRespawnOnFall>();
38:             goal = FindFirstObjectByType<MainStageGoalZone>();
39:             endingBackground = Resources.Load<Texture2D>(
40:                 "Art/HimoHitoEndingBackground-v1");
41:             if (ropeController != null)
42:             {
43:                 playerBody = ropeController.GetComponent<Rigidbody2D>();
44:             }
```

## Assets/Scripts/RopeController.cs:84–102

```text
84:         private void Awake()
85:         {
86:             RopeController[] controllers = GetComponents<RopeController>();
87:             if (controllers.Length > 0 && controllers[0] != this)
88:             {
89:                 Debug.LogWarning(
90:                     "Duplicate RopeController was disabled to prevent one E press " +
91:                     "from attaching and detaching in the same frame.",
92:                     this);
93:                 enabled = false;
94:                 return;
95:             }
96: 
97:             body = GetComponent<Rigidbody2D>();
98:             bodyCollider = GetComponent<Collider2D>();
99:             ropeJoint = GetComponent<DistanceJoint2D>();
100:             lineRenderer = GetComponent<LineRenderer>();
101:             ropeResource = GetComponent<RopeResource>();
102:             playerMover = GetComponent<PlayerMover>();
```

## Assets/Scripts/RopeController.cs:424–453

```text
424:         private void BeginPlatformBuildHold()
425:         {
426:             if (isPlatformBuildHoldActive || body == null)
427:             {
428:                 return;
429:             }
430: 
431:             // A RopePlatformAnchor is a construction target. It must not switch
432:             // the player into a pendulum while they are preparing Q at a bridge
433:             // edge, so hold the successful E position until Q or detach.
434:             constraintsBeforePlatformBuildHold = body.constraints;
435:             body.linearVelocity = Vector2.zero;
436:             body.angularVelocity = 0f;
437:             body.constraints =
438:                 constraintsBeforePlatformBuildHold |
439:                 RigidbodyConstraints2D.FreezePositionX |
440:                 RigidbodyConstraints2D.FreezePositionY;
441:             isPlatformBuildHoldActive = true;
442:         }
443: 
444:         private void EndPlatformBuildHold()
445:         {
446:             if (!isPlatformBuildHoldActive || body == null)
447:             {
448:                 return;
449:             }
450: 
451:             body.constraints = constraintsBeforePlatformBuildHold;
452:             isPlatformBuildHoldActive = false;
453:         }
```

## Assets/Scripts/RopeController.cs:461–507

```text
461:         public bool TryResolveCurrentAimHook(
462:             out HookPoint hookPoint,
463:             out Vector2 resolvedAnchor)
464:         {
465:             Vector2 origin = body.position;
466:             float baseSearchDistance = Mathf.Min(
467:                 SelectedRopeLength,
468:                 maximumShotDistance);
469:             float hookSearchDistance = GetHookSearchDistance(
470:                 baseSearchDistance);
471:             RaycastHit2D[] hits = Physics2D.RaycastAll(
472:                 origin,
473:                 keyboardAimDirection,
474:                 hookSearchDistance);
475:             foreach (RaycastHit2D hit in hits)
476:             {
477:                 if (hit.collider == null || hit.collider == bodyCollider)
478:                 {
479:                     continue;
480:                 }
481: 
482:                 HookPoint candidate =
483:                     hit.collider.GetComponentInParent<HookPoint>();
484:                 if (candidate == null)
485:                 {
486:                     continue;
487:                 }
488: 
489:                 Vector2 candidateAnchor = candidate.GetAttachmentPoint(hit.point);
490:                 float allowedDistance = CanUsePlatformHookTargetingGrace(
491:                     candidate,
492:                     baseSearchDistance)
493:                     ? hookSearchDistance
494:                     : baseSearchDistance;
495:                 if (Vector2.Distance(origin, candidateAnchor) <=
496:                     allowedDistance + 0.01f)
497:                 {
498:                     hookPoint = candidate;
499:                     resolvedAnchor = candidateAnchor;
500:                     return true;
501:                 }
502:             }
503: 
504:             resolvedAnchor = default;
505:             hookPoint = null;
506:             return false;
507:         }
```

## Assets/Scripts/RopeController.cs:626–648

```text
626:         private float GetHookSearchDistance(float baseDistance)
627:         {
628:             return Mathf.Min(
629:                 maximumShotDistance,
630:                 baseDistance + PlatformHookTargetingGrace);
631:         }
632: 
633:         private static bool CanUsePlatformHookTargetingGrace(
634:             HookPoint hook,
635:             float ropeLength)
636:         {
637:             if (hook == null ||
638:                 !hook.TryGetComponent(out RopePlatformAnchor platformAnchor) ||
639:                 !platformAnchor.TryGetPairedAnchor(out Vector2 pairedAnchor))
640:             {
641:                 return false;
642:             }
643: 
644:             Vector2 hookAnchor = hook.GetAttachmentPoint(
645:                 hook.transform.position);
646:             return Vector2.Distance(hookAnchor, pairedAnchor) <=
647:                 ropeLength + 0.05f;
648:         }
```

## Assets/Scripts/RopeController.cs:786–800

```text
786:         private void UpdateAimHookFeedback()
787:         {
788:             hasAvailableAimAnchor = false;
789:             if (aimHookOutline != null) aimHookOutline.enabled = false;
790:             if (aimHookHalo != null) aimHookHalo.enabled = false;
791:             if (aimHookTip != null) aimHookTip.enabled = false;
792:             if (IsAttached || Time.timeScale <= 0f || body == null || !body.simulated)
793:             {
794:                 highlightedHook = null; highlightElapsed = 0f;
795:                 return;
796:             }
797: 
798:             Vector2 target = body.position + keyboardAimDirection * maximumShotDistance;
799:             bool available = TryResolveAvailableAttachment(target, out Vector2 anchor,
800:                 out HookPoint hook);
```

## Assets/Scripts/RopePlatformBuilder.cs:420–439

```text
420:         private GeneratedRopePlatform CreatePlatform(
421:             Vector2 start,
422:             Vector2 end,
423:             float ropeLength)
424:         {
425:             GameObject platformObject = new GameObject(
426:                 $"Generated Rope Platform {generatedPlatforms.Count + 1}");
427:             Vector2[] points = BuildSaggingCurve(start, end, ropeLength);
428: 
429:             LineRenderer line = platformObject.AddComponent<LineRenderer>();
430:             line.useWorldSpace = true;
431:             line.positionCount = points.Length;
432:             for (int i = 0; i < points.Length; i++)
433:             {
434:                 line.SetPosition(i, points[i]);
435:             }
436:             line.startWidth = platformWidth;
437:             line.endWidth = platformWidth;
438:             line.startColor = ropeController.VisibleRopeColor;
439:             line.endColor = ropeController.VisibleRopeColor;
```

## Assets/Scripts/RopePlatformBuilder.cs:464–481

```text
464:             EdgeCollider2D edge = platformObject.AddComponent<EdgeCollider2D>();
465:             edge.edgeRadius = platformWidth * 0.5f;
466:             edge.points = points;
467: 
468:             GeneratedRopePlatform generated =
469:                 platformObject.AddComponent<GeneratedRopePlatform>();
470:             generated.Configure(
471:                 start,
472:                 end,
473:                 ropeLength,
474:                 points,
475:                 material,
476:                 edge,
477:                 bodyCollider,
478:                 ropeController);
479:             generatedPlatforms.Add(platformObject);
480:             RopeBridgeBindings.Ensure(line);
481:             return generated;
```

## Assets/Scripts/RopePlatformBuilder.cs:498–531

```text
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
508:             if (bodyCollider != null)
509:             {
510:                 Vector2 extents = bodyCollider.bounds.extents;
511:                 support = Mathf.Abs(normal.x) * extents.x +
512:                           Mathf.Abs(normal.y) * extents.y;
513:             }
514: 
515:             body.position = platform.End + normal *
516:                 (support + platformWidth * 0.5f + StandingClearance);
517:             body.linearVelocity = Vector2.zero;
518:             body.angularVelocity = 0f;
519:         }
520: 
521:         private void RemovePlatform(GameObject platformObject)
522:         {
523:             generatedPlatforms.Remove(platformObject);
524:             if (platformObject == null)
525:             {
526:                 return;
527:             }
528: 
529:             platformObject.SetActive(false);
530:             Destroy(platformObject);
531:         }
```

## Assets/Scripts/RopePlatformBuilder.cs:575–585

```text
575:         public bool Touches(Vector2 point, float tolerance)
576:         {
577:             return Vector2.Distance(Start, point) <= tolerance ||
578:                    Vector2.Distance(End, point) <= tolerance;
579:         }
580: 
581:         public Vector2 GetOtherEndpoint(Vector2 sharedPoint)
582:         {
583:             return Vector2.Distance(Start, sharedPoint) <=
584:                    Vector2.Distance(End, sharedPoint)
585:                 ? End
```

## Assets/Scripts/RopeResource.cs:35–43

```text
35:         public bool TrySpend(float amount)
36:         {
37:             if (amount <= 0f || amount > currentLength)
38:             {
39:                 return false;
40:             }
41: 
42:             currentLength -= amount;
43:             return true;
```

## Assets/Scripts/RopePlatformAnchor.cs:30–54

```text
30:         public bool TryGetPairedAnchor(out Vector2 anchorPosition)
31:         {
32:             anchorPosition = default;
33:             if (string.IsNullOrWhiteSpace(pairedAnchorName))
34:             {
35:                 return false;
36:             }
37: 
38:             foreach (GameObject candidate in
39:                      Resources.FindObjectsOfTypeAll<GameObject>())
40:             {
41:                 if (candidate != gameObject &&
42:                     candidate.scene == gameObject.scene &&
43:                     candidate.activeInHierarchy &&
44:                     candidate.name == pairedAnchorName &&
45:                     candidate.TryGetComponent(out HookPoint pairedHook))
46:                 {
47:                     anchorPosition = pairedHook.GetAttachmentPoint(
48:                         candidate.transform.position);
49:                     return true;
50:                 }
51:             }
52: 
53:             return false;
54:         }
```

## Assets/Scripts/HookPoint.cs:45–50

```text
45:         public Vector2 GetAttachmentPoint(Vector2 raycastHitPoint)
46:         {
47:             return useFixedAttachmentPoint
48:                 ? (Vector2)transform.TransformPoint(localAttachmentPoint)
49:                 : raycastHitPoint;
50:         }
```

## Assets/Scripts/RopeBridgeReveal.cs:16–20

```text
16:         public static void Play(LineRenderer line, Vector3 playerPosition)
17:         {
18:             if (line == null || line.positionCount < 2) return;
19:             RopeBridgeBindings.Ensure(line)?.BeginWeave(Duration);
20:             RopeBridgeReveal effect = line.gameObject.AddComponent<RopeBridgeReveal>();
```

## Assets/Scripts/RopeBridgeReveal.cs:45–63

```text
45:             Color faintStart = line.startColor;
46:             Color faintEnd = line.endColor;
47:             faintStart.a *= 0.25f;
48:             faintEnd.a *= 0.25f;
49:             line.startColor = faintStart;
50:             line.endColor = faintEnd;
51:             effect.Draw(0f);
52:         }
53: 
54:         private void Update()
55:         {
56:             elapsed += Time.deltaTime;
57:             if (bridge == null || elapsed >= Duration)
58:             {
59:                 Restore();
60:                 Destroy(this);
61:                 return;
62:             }
63:             Draw(Mathf.SmoothStep(0f, 1f, elapsed / Duration));
```

## Assets/Scenes/Tutorial.unity：保存されたPlayer/Body/Component状態

```text
--- !u!1 &631669796
  m_Name: Player
  m_IsActive: 1
--- !u!114 &631669797 PrototypeRunController owner=631669796 m_Enabled=1
--- !u!114 &631669799 PlayerMover owner=631669796 m_Enabled=1
--- !u!114 &631669800 RopePlatformBuilder owner=631669796 m_Enabled=1
--- !u!114 &631669801 RopeController owner=631669796 m_Enabled=1
--- !u!50 &631669805
  m_GameObject: {fileID: 631669796}
  m_BodyType: 0
  m_Simulated: 1
  m_Constraints: 4
--- !u!4 &631669809
  m_GameObject: {fileID: 631669796}
  m_Father: {fileID: 0}
```

## Assets/Scenes/MainStage.unity：保存されたPlayer/Body/Component状態

```text
--- !u!114 &1119008754 MainStageHud owner=1119008753 m_Enabled=1
--- !u!1 &1380120218
  m_Name: Main Player
  m_IsActive: 1
--- !u!4 &1380120219
  m_GameObject: {fileID: 1380120218}
  m_Father: {fileID: 0}
--- !u!114 &1380120220 MainStageRespawnOnFall owner=1380120218 m_Enabled=1
--- !u!114 &1380120221 RopeController owner=1380120218 m_Enabled=1
--- !u!114 &1380120222 PlayerMover owner=1380120218 m_Enabled=1
--- !u!50 &1380120227
  m_GameObject: {fileID: 1380120218}
  m_BodyType: 0
  m_Simulated: 1
  m_Constraints: 4
--- !u!114 &1380120231 RopePlatformBuilder owner=1380120218 m_Enabled=1
```

## ProjectSettings/ProjectSettings.asset：関連保存値

```text
91:   runInBackground: 0
730:   activeInputHandler: 0
```

## ProjectSettings/Physics2DSettings.asset：関連保存値

```text
50:   m_QueriesHitTriggers: 1
51:   m_QueriesStartInColliders: 1
54:   m_AutoSyncTransforms: 0
```

