# 追補の固定ソース実本文抜粋

すべて10/6 FriendPlay固定コピー。実行結果ではない。引用は1-based行番号。

## TutorialGuideAnimation.cs

BuildInputs一致。SHA-256 `bb2cbbc6c7afa6ea22478d500932befb5cddab983c6dff75ea9f2c374e59a8e5`

### 18–39
```text
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
```

### 67–94
```text
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
```

## TutorialSectionGuide.cs

BuildInputs一致。SHA-256 `380f0e643fe2e76bc3d561c3157982b268aee650b3648357af8d3a1bf336f385`

### 33–62
```text
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

### 617–673
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
664:         {
665:             // Advance once per Update, never per IMGUI layout/repaint event.
666:             if (IsVisible)
667:                 illustrationTime = Mathf.Repeat(illustrationTime + Mathf.Clamp(unscaledDelta, 0f, .1f),
668:                     TutorialGuideAnimation.Duration(openSection));
669:         }
670: 
671:         private void DrawSectionIllustration(Rect rect, int section)
672:         {
673:             Rect inner = new Rect(
```

### 767–795
```text
767:         private void DrawSectionFourIllustration(Rect rect)
768:         {
769:             var f=TutorialGuideAnimation.Sample(4,illustrationTime);
770:             DrawPlatform(NormalizedRect(rect,.02f,.67f,.22f,.25f));
771:             DrawPlatform(NormalizedRect(rect,.76f,.67f,.22f,.25f));
772:             Vector2 previous=DiagramPoint(rect,TutorialGuideAnimation.MergedPoint(0f,f.Merge));
773:             float distance=0f;
774:             for(int i=1;i<=48;i++)
775:             {
776:                 float along=i/48f;
777:                 Vector2 point=DiagramPoint(rect,TutorialGuideAnimation.MergedPoint(along,f.Merge));
778:                 float built=along<=.5f ? f.LeftBridge : f.RightBridge;
779:                 float localProgress=along<=.5f ? along*2f : (along-.5f)*2f;
780:                 float alpha=built<=0f ? 0f : localProgress<=built ? 1f : .12f;
781:                 DrawYarnLine(previous,point,8f,new Color(1f,1f,1f,f.Opacity*alpha),distance);
782:                 distance+=Vector2.Distance(previous,point);
783:                 previous=point;
784:             }
785:             DrawPlatform(NormalizedRect(rect,.64f,.23f,.075f,.27f));
786:             GUI.Label(NormalizedRect(rect,.62f,.15f,.12f,.08f),"梁",diagramLabelStyle);
787:             DrawDemoRing(DiagramPoint(rect,new Vector2(.24f,.67f)),new Color(.33f,1f,.76f));
788:             DrawDemoRing(DiagramPoint(rect,new Vector2(.76f,.67f)),new Color(.33f,1f,.76f));
789:             DrawDemoRing(DiagramPoint(rect,new Vector2(.5f,.35f)),
790:                 new Color(.24f,.64f,.96f,(1f-f.Merge)*f.Opacity));
791:             if(f.Connection>0f)
792:                 DrawDemoConnection(rect,f,DiagramPoint(rect,f.ConnectionTarget));
793:             DrawDemoPlayer(rect,f);
794:             DrawDemoCaption(rect,f.Caption);
795:         }
```

## TutorialSignInscription.cs

BuildInputs一致。SHA-256 `5b0194605ac744e2aecf3106c1911dad2cfe6c34bb61f9acb859e9cd22c1ab1e`

### 83–109
```text
83:                 case 4:
84:                     firstRope=Curve(new Vector2(.20f, .76f), new Vector2(.27f, .65f), new Vector2(.375f, .81f), false);
85:                     secondRope=Curve(new Vector2(.375f, .81f), new Vector2(.48f, .65f), new Vector2(.55f, .76f), false);
86:                     mergedRope=Curve(new Vector2(.20f, .76f), new Vector2(.375f, .40f), new Vector2(.55f, .76f), true);
87:                     middleRing=Ring(new Vector2(.375f, .81f), Blue);
88:                     Ring(new Vector2(.20f, .76f), Green);
89:                     Ring(new Vector2(.55f, .76f), Green);
90:                     Arrow(new Vector2(.375f, .72f), new Vector2(.375f, .62f));
91:                     PlayerPicture(new Vector2(.46f, .625f), .115f);
92:                     Key("F", new Vector2(.72f, .69f), .15f, .23f);
93:                     break;
94:             }
95:             if(section>=3)
96:             {
97:                 connectionRope=Curve(Vector2.zero,Vector2.zero,Vector2.zero,true);
98:                 SetRopeAppearance(connectionRope,0f);
99:             }
100:             if(Application.isPlaying) AdvanceAnimation(0f);
101:         }
102: 
103:         private void AdvanceAnimation(float delta)
104:         {
105:             if(drawing==null || picturePlayer==null || delta<0f) return;
106:             // The world signs use scaled time; unlike the open explanation they pause with gameplay.
107:             if(delta>0f) animationTime=Mathf.Repeat(animationTime+Mathf.Min(delta,.1f),
108:                 TutorialGuideAnimation.Duration(section));
109:             var frame=TutorialGuideAnimation.Sample(section,animationTime);
```

### 135–171
```text
135:             else
136:             {
137:                 Vector2 a=new Vector2(.20f,.76f), center=new Vector2(.375f,.81f), c=new Vector2(.55f,.76f);
138:                 AnimateCurve(firstRope,a,new Vector2(.27f,.65f),center,frame.LeftBridge,
139:                     frame.Merge>0f ? 0f : opacity*frame.LeftBridge);
140:                 AnimateCurve(secondRope,center,new Vector2(.48f,.65f),c,frame.RightBridge,
141:                     frame.Merge>0f ? 0f : opacity*frame.RightBridge);
142:                 for(int i=0;i<25;i++)
143:                 {
144:                     Vector2 point=SmallMergedPoint(i/24f,frame.Merge);
145:                     foreach(var line in mergedRope) line.SetPosition(i,point);
146:                 }
147:                 SetRopeAppearance(mergedRope,frame.Merge>0f ? opacity : 0f);
148:                 float along=(frame.Feet.x-.24f)/.52f;
149:                 feet=along<0f || along>1f ? new Vector2(Mathf.Clamp(.20f+along*.35f,.155f,.59f),.76f) :
150:                     SmallMergedPoint(along,frame.Merge);
151:                 Vector2 head=PictureCrown(feet);
152:                 Vector2 target=frame.ConnectionTarget.x<.6f ? center : c;
153:                 AnimateCurve(connectionRope,head,Vector2.Lerp(head,target,.5f),target,
154:                     frame.Connection,opacity*frame.Connection);
155:                 for(int i=0;i<middleRing.Length;i++)
156:                 {
157:                     Color color=i==0 ? Ink : Blue;
158:                     color.a*=1f-frame.Merge;
159:                     middleRing[i].startColor=middleRing[i].endColor=color;
160:                 }
161:             }
162:             SetPictureFeet(feet);
163:             picturePlayer.color=new Color(1f,1f,1f,opacity);
164:         }
165: 
166:         private static Vector2 SmallMergedPoint(float along,float merge)
167:         {
168:             Vector2 a=new Vector2(.20f,.76f), center=new Vector2(.375f,.81f), c=new Vector2(.55f,.76f);
169:             Vector2 pair=along<=.5f ? TutorialGuideAnimation.Curve(a,new Vector2(.27f,.65f),center,along*2f) :
170:                 TutorialGuideAnimation.Curve(center,new Vector2(.48f,.65f),c,(along-.5f)*2f);
171:             return Vector2.Lerp(pair,TutorialGuideAnimation.Curve(a,new Vector2(.375f,.40f),c,along),merge);
```

## TutorialSectionFourSetup.cs

BuildInputs一致。SHA-256 `ca097254c033e04e2c486bdd900d800cb3226b2aa9a4cc62838be7aeee89b37c`

### 11–38
```text
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
```

### 56–98
```text
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
81:                 true);
82:             changed |= EnsureTerrain(
83:                 BeamName,
84:                 BeamPosition,
85:                 BeamSize,
86:                 out _);
87:             changed |= EnsureTerrain(
88:                 GoalFloorName,
89:                 GoalFloorPosition,
90:                 GoalFloorSize,
91:                 out GameObject goalFloor);
92:             if (!goalFloor.TryGetComponent(out GoalZone _))
93:             {
94:                 goalFloor.AddComponent<GoalZone>();
95:                 changed = true;
96:             }
97:             changed |= EnsureGoalMarker();
98:             return changed;
```

### 216–250
```text
216:         private static bool EnsureTerrain(
217:             string objectName,
218:             Vector2 position,
219:             Vector2 size,
220:             out GameObject terrain)
221:         {
222:             terrain = EnsureObject(
223:                 objectName,
224:                 position,
225:                 size,
226:                 TerrainColor,
227:                 out bool changed);
228:             BoxCollider2D collider = terrain.GetComponent<BoxCollider2D>();
229:             if (!collider.enabled || collider.isTrigger)
230:             {
231:                 collider.enabled = true;
232:                 collider.isTrigger = false;
233:                 changed = true;
234:             }
235:             if (!terrain.TryGetComponent(out Rigidbody2D body))
236:             {
237:                 body = terrain.AddComponent<Rigidbody2D>();
238:                 changed = true;
239:             }
240:             if (body.bodyType != RigidbodyType2D.Static)
241:             {
242:                 body.bodyType = RigidbodyType2D.Static;
243:                 changed = true;
244:             }
245:             if (!terrain.TryGetComponent(out SolidSwingSurface _))
246:             {
247:                 terrain.AddComponent<SolidSwingSurface>();
248:                 changed = true;
249:             }
250:             return changed;
```

## RopePlatformBuilder.cs

BuildInputs一致。SHA-256 `11ecb4c1870cb8ef0449343155559211a5e1d9e1321132bce0dcf8e14e279fd2`

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

### 484–495
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
```

## MainStageSectionTenSetup.cs

BuildInputs一致。SHA-256 `80095b57b5ea63b2768ffaeac76dbe15270efb3b9d0669e84f65b2ccde134e11`

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

### 41–77
```text
41:         public static bool ApplyCurrentScene()
42:         {
43:             bool changed = EnsurePairedAnchor(
44:                 LeftAnchorName,
45:                 LeftAnchorPosition,
46:                 RightAnchorName,
47:                 false);
48:             changed |= EnsurePairedAnchor(
49:                 RightAnchorName,
50:                 RightAnchorPosition,
51:                 LeftAnchorName,
52:                 true);
53:             changed |= EnsureTerrain(
54:                 GoalFloorName,
55:                 GoalFloorPosition,
56:                 GoalFloorSize,
57:                 out GameObject goalFloor);
58:             if (!goalFloor.TryGetComponent(out MainStageGoalZone _))
59:             {
60:                 goalFloor.AddComponent<MainStageGoalZone>();
61:                 changed = true;
62:             }
63:             changed |= EnsureGoalMarker();
64:             return changed;
65:         }
66: 
67:         public static GameObject EnsureCreated()
68:         {
69:             ApplyCurrentScene();
70:             return FindSceneObject(GoalMarkerName);
71:         }
72: 
73:         public static bool HasFinalPlatform(RopePlatformBuilder builder)
74:         {
75:             return builder != null && builder.HasPlatformBetween(
76:                 LeftAnchorPosition,
77:                 RightAnchorPosition);
```

### 82–109
```text
82:             Vector2 position,
83:             string pairedAnchorName,
84:             bool canAttach)
85:         {
86:             GameObject hook = EnsureSolidObject(
87:                 objectName,
88:                 position,
89:                 AnchorSize,
90:                 BridgeHookColor,
91:                 out bool changed);
92:             if (!hook.TryGetComponent(out HookPoint hookPoint))
93:             {
94:                 hookPoint = hook.AddComponent<HookPoint>();
95:                 changed = true;
96:             }
97:             changed |= hookPoint.ConfigureFixedAttachmentPoint(Vector2.zero);
98: 
99:             if (!hook.TryGetComponent(out RopePlatformAnchor anchor))
100:             {
101:                 anchor = hook.AddComponent<RopePlatformAnchor>();
102:                 changed = true;
103:             }
104:             changed |= anchor.Configure(BridgeRopeLength, pairedAnchorName);
105: 
106:             if (hook.TryGetComponent(out BoxCollider2D collider) &&
107:                 collider.enabled != canAttach)
108:             {
109:                 collider.enabled = canAttach;
```

## MainStageSectionNineSetup.cs

BuildInputs一致。SHA-256 `794454c44069d0430c9136af0cade40b72c513e3708c7a594b5e1cd62a5566da`

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

## MainStageGoalZone.cs

BuildInputs一致。SHA-256 `54bb45cc0dcdf0cfafc08fad10e865059e058b15e635601d81e66305d162e9d9`

### 50–88
```text
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

## GoalChestPresentation.cs

BuildInputs一致。SHA-256 `d2c59ac777a4d2ad0e9732f14a81cf745c7d2b00ebd188f046838cf3d35db6c4`

### 9–11
```text
9:         private const float OpeningDuration = 0.45f;
10:         private const float OpenDistance = 2.6f;
11:         private const float CloseDistance = 3.6f;
```

### 56–94
```text
56:         public bool Approach(Rigidbody2D player)
57:         {
58:             float distance = Mathf.Abs(player.position.x - transform.position.x);
59:             if (!opening && distance <= OpenDistance)
60:             {
61:                 visitor = player;
62:                 opening = true;
63:                 // Reapproaching during closing reverses from the current frame, without a snap.
64:                 player.GetComponent<PrototypeAudioFeedback>()?.PlayGoalChestOpened();
65:             }
66:             return opening && elapsed >= OpeningDuration && distance <= 1.05f;
67:         }
68: 
69:         public static bool ReadyToClear(Collision2D collision, Collider2D surface,
70:             string markerName)
71:         {
72:             Rigidbody2D player = collision.rigidbody;
73:             if (player == null || !player.TryGetComponent(out RopeResource _)) return false;
74:             float top = surface.bounds.max.y;
75:             Collider2D body = player.GetComponent<Collider2D>();
76:             if (player.worldCenterOfMass.y <= top ||
77:                 (body != null && body.bounds.min.y < top - 0.12f)) return false;
78:             bool onTop = false;
79:             for (int i = 0; i < collision.contactCount; i++)
80:             {
81:                 ContactPoint2D point = collision.GetContact(i);
82:                 onTop |= point.point.y >= top - 0.12f && Mathf.Abs(point.normal.y) >= 0.6f;
83:             }
84:             if (!onTop) return false;
85:             GameObject marker = GameObject.Find(markerName);
86:             if (marker == null)
87:             {
88:                 // Legacy/graybox scenes retain the original opening-to-clear sound sequence.
89:                 player.GetComponent<PrototypeAudioFeedback>()?.PlayGoalChestOpened();
90:                 return true;
91:             }
92:             Ensure(marker, surface.gameObject);
93:             return marker.GetComponent<GoalChestPresentation>().Approach(player);
94:         }
```

## MainStageFloorCollisionSetup.cs

BuildInputs一致。SHA-256 `e5c1af6e2339d7ebfef9485e4b2c79e1c39b1d355b33ceae2d5d0045d480af3f`

### 15–47
```text
15:         [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
16:         private static void RegisterSceneSetup()
17:         {
18:             // Also run after Tutorial -> MainStage and scene reloads. Unsubscribe
19:             // first so Enter Play Mode without domain reload cannot double-register.
20:             SceneManager.sceneLoaded -= OnSceneLoaded;
21:             SceneManager.sceneLoaded += OnSceneLoaded;
22:         }
23: 
24:         private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
25:         {
26:             if (scene.name == MainStageSceneName && scene == SceneManager.GetActiveScene())
27:             {
28:                 ApplyCurrentScene();
29:             }
30:         }
31: 
32:         public static bool ApplyCurrentScene()
33:         {
34:             bool changed = EnsureSectionTwoCheckpoint();
35:             changed |= MainStageSectionTwoSetup.ApplyCurrentScene();
36:             changed |= MainStageSectionThreeSetup.ApplyCurrentScene();
37:             changed |= MainStageSectionFourSetup.ApplyCurrentScene();
38:             changed |= MainStageSectionFiveSetup.ApplyCurrentScene();
39:             changed |= MainStageSectionSixSetup.ApplyCurrentScene();
40:             changed |= MainStageSectionSevenSetup.ApplyCurrentScene();
41:             changed |= MainStageSectionNineSetup.ApplyCurrentScene();
42:             changed |= MainStageSectionTenSetup.ApplyCurrentScene();
43:             foreach (GameObject candidate in
44:                      Resources.FindObjectsOfTypeAll<GameObject>())
45:             {
46:                 if (!candidate.scene.IsValid() ||
47:                     candidate.scene.name != MainStageSceneName ||
```

## RopePlatformAnchor.cs

BuildInputs一致。SHA-256 `bb3309fe1e78fce3261683e25bd71dfbded4c4bf210a062c9e60e3f1409709c0`

### 5–27
```text
5:     /// <summary>
6:     /// Marks a special Hook that is intended for making a permanent rope
7:     /// platform. The stored length is the authored recommendation; building
8:     /// succeeds with any selected rope long enough to reach the paired Hook.
9:     /// </summary>
10:     [DisallowMultipleComponent]
11:     [RequireComponent(typeof(HookPoint))]
12:     public sealed class RopePlatformAnchor : MonoBehaviour
13:     {
14:         [SerializeField, Min(1)] private int requiredRopeLength = 5;
15:         [SerializeField] private string pairedAnchorName;
16: 
17:         public int RecommendedRopeLength => requiredRopeLength;
18: 
19:         public bool Configure(int ropeLength, string pairedObjectName = "")
20:         {
21:             int safeLength = Mathf.Max(1, ropeLength);
22:             string safePairedName = pairedObjectName ?? string.Empty;
23:             bool changed = requiredRopeLength != safeLength ||
24:                            pairedAnchorName != safePairedName;
25:             requiredRopeLength = safeLength;
26:             pairedAnchorName = safePairedName;
27:             return changed;
```

## PrototypeRunController.cs

BuildInputs一致。SHA-256 `ec6fd5170e7c00a97593b28e109b7fc3fbc5d6f922b5aa1b68823467e3623977`

### 76–81
```text
76:         private void Awake()
77:         {
78:             TutorialSectionOneSetup.ApplyCurrentScene();
79:             TutorialSectionTwoSetup.ApplyCurrentScene();
80:             TutorialSectionThreeSetup.ApplyCurrentScene();
81:             TutorialSectionFourSetup.ApplyCurrentScene();
```

### 104–138
```text
104:             ropeResource.RestoreCapacityAndCurrent(
105:                 TutorialRopeCapacity,
106:                 TutorialRopeCapacity);
107:             if (startFromSectionFourForDevelopment)
108:             {
109:                 startTutorialSection = 4;
110:                 CurrentTutorialSection = 4;
111:                 body.position =
112:                     TutorialSectionThreeSetup.LandingRespawnPosition;
113:                 ropeController.RestoreSelectedRopeLength(
114:                     TutorialSectionFourSetup.PlatformRopeLength);
115:             }
116:             else
117:             {
118:                 body.position = TutorialSectionOneSetup.StartRespawnPosition;
119:                 ropeController.RestoreSelectedRopeLength(
120:                     TutorialSectionOneSetup.StartingRopeLength);
121:             }
122:             Physics2D.SyncTransforms();
123:             startPosition = body.position;
124:             startRopeCapacity = ropeResource.MaximumLength;
125:             startRopeLength = ropeResource.CurrentLength;
126:             startSelectedRopeLength = ropeController.SelectedRopeLength;
127: 
128:         }
129: 
130:         private void Start()
131:         {
132:             EnterStartScreen();
133:             checkpointPosition = startPosition;
134:             CaptureCheckpointState();
135:         }
136: 
137:         private void Update()
138:         {
```

### 439–492
```text
439: 
440:         private void RestoreCheckpointState()
441:         {
442:             ropeResource.RestoreCurrentLength(checkpointRopeLength);
443:             ropeController.RestoreSelectedRopeLength(checkpointSelectedRopeLength);
444:             platformBuilder.RestorePlatformStates(checkpointPlatformStates, checkpointRemovedHooks);
445:         }
446: 
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
489:             }
490: 
491:             Debug.LogError(
492:                 $"本編シーンを読み込めません。Build Settingsに" +
```

## MainStageRespawnOnFall.cs

BuildInputs一致。SHA-256 `314060136dfb7846ae7ebbc43cfaf0028625b839b4b9a05e9171d4f5830c4d8c`

### 12–47
```text
12:     [RequireComponent(typeof(Rigidbody2D), typeof(RopeResource), typeof(RopeController))]
13:     [RequireComponent(typeof(RopePlatformBuilder), typeof(PlayerMover))]
14:     public sealed class MainStageRespawnOnFall : MonoBehaviour
15:     {
16:         private const int SectionOneStartingRopeLength = 7;
17:         private const float EndingPreviewRopeLength = 5f;
18: 
19:         [SerializeField]
20:         private bool startNearGoalForEndingPreview;
21: 
22:         [SerializeField]
23:         private bool startWithPartDPreview;
24: 
25:         [SerializeField] private float fallThreshold = -9f;
26:         [SerializeField, Min(0.01f)] private float minimumUsableRopeLength = 1f;
27: 
28:         private Rigidbody2D body;
29:         private RopeResource ropeResource;
30:         private RopeController ropeController;
31:         private RopePlatformBuilder platformBuilder;
32:         private PlayerMover playerMover;
33:         private PrototypeAudioFeedback audioFeedback;
34:         private MainStageGoalZone goalZone;
35:         private Vector2 checkpointPosition;
36:         private float checkpointRopeLength;
37:         private int checkpointSelectedRopeLength;
38:         private RopePlatformBuilder.PlatformState[] checkpointPlatformStates;
39:         private GameObject[] checkpointRemovedHooks;
40: 
41:         public int CurrentSection { get; private set; } = 1;
42:         public int RefillCount { get; private set; }
43:         public bool HasReachedMidpoint => CurrentSection >= 6;
44:         public bool HasReachedSectionNine => CurrentSection >= 9;
45:         public bool HasReachedSectionTen => CurrentSection >= 10;
46:         public bool IsRopeExhausted { get; private set; }
47:         public bool IsFallFailure { get; private set; }
```

### 51–96
```text
51:         private void Awake()
52:         {
53:             body = GetComponent<Rigidbody2D>();
54:             ropeResource = GetComponent<RopeResource>();
55:             ropeController = GetComponent<RopeController>();
56:             platformBuilder = GetComponent<RopePlatformBuilder>();
57:             playerMover = GetComponent<PlayerMover>();
58:             audioFeedback = GetComponent<PrototypeAudioFeedback>();
59:             if (audioFeedback == null)
60:             {
61:                 audioFeedback = gameObject.AddComponent<PrototypeAudioFeedback>();
62:             }
63:             goalZone = FindFirstObjectByType<MainStageGoalZone>();
64: 
65:             CurrentSection = 1;
66:             ropeResource.ResetToMaximum();
67:             ropeController.RestoreSelectedRopeLength(
68:                 SectionOneStartingRopeLength);
69: 
70:             if (!startWithPartDPreview && startNearGoalForEndingPreview)
71:             {
72:                 ApplyEndingPreviewStart();
73:             }
74: 
75:             checkpointPosition = body.position;
76:             CaptureCheckpointState();
77:         }
78: 
79:         private void Start()
80:         {
81:             if (startWithPartDPreview)
82:             {
83:                 ApplyPartDPreviewStart();
84:                 return;
85:             }
86: 
87:             if (!startNearGoalForEndingPreview)
88:             {
89:                 return;
90:             }
91: 
92:             // Reapply after every Awake so authored section setup cannot leave
93:             // the player at an older checkpoint during the ending preview.
94:             ApplyEndingPreviewStart();
95:             checkpointPosition = body.position;
96:             CaptureCheckpointState();
```

## StageStartTransition.cs

BuildInputs一致。SHA-256 `294e890fac62f18212ab7628e65b799d1e299ccb362f7ee579602ae8fbdebf3f`

### 1–175
```text
1: using UnityEngine;
2: using UnityEngine.SceneManagement;
3: 
4: namespace HimoHito
5: {
6:     /// <summary>Owns only the menu-to-stage curtain. The existing tour owns its player freeze.</summary>
7:     [DefaultExecutionOrder(9000)]
8:     public sealed class StageStartTransition : MonoBehaviour
9:     {
10:         internal const float ThreadDuration = 0.22f;
11:         internal const float CloseDuration = 0.58f;
12:         internal const float OpenDuration = 0.62f;
13:         private enum Phase { Thread, Closing, Covered, Loading, Opening }
14:         private static StageStartTransition active;
15:         public static bool IsActive => active != null && active.isActiveAndEnabled;
16: 
17:         private readonly StageStartCurtainView view = new StageStartCurtainView();
18:         private PrototypeRunController source;
19:         private string targetPath, stageTitle;
20:         private Vector2 origin;
21:         private Phase phase;
22:         private float elapsed;
23:         private int coveredFrame, loadedFrame;
24:         private bool finished, destinationSeen;
25:         private AsyncOperation load;
26:         private GameObject destinationPlayer;
27:         private Rigidbody2D heldBody;
28:         private PlayerMover heldMover;
29:         private RopeController heldRope;
30:         private bool bodyWasSimulated, moverWasEnabled, ropeWasEnabled;
31: 
32:         [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
33:         private static void ResetStatics() => active = null;
34: 
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
50: 
51:         private void Update()
52:         {
53:             if (finished) return;
54:             // A scene replaced externally must never retain the old menu's curtain.
55:             if (source == null && phase != Phase.Loading && phase != Phase.Opening)
56:             {
57:                 Finish();
58:                 return;
59:             }
60:             elapsed += Mathf.Max(0, Time.unscaledDeltaTime);
61:             switch (phase)
62:             {
63:                 case Phase.Thread:
64:                     if (elapsed >= ThreadDuration) Enter(Phase.Closing);
65:                     break;
66:                 case Phase.Closing:
67:                     if (elapsed >= CloseDuration)
68:                     {
69:                         Enter(Phase.Covered);
70:                         coveredFrame = Time.frameCount;
71:                     }
72:                     break;
73:                 case Phase.Covered:
74:                     // Keep an entire opaque rendered frame before any scene activation.
75:                     if (Time.frameCount > coveredFrame + 1) CommitStart();
76:                     break;
77:                 case Phase.Loading:
78:                     if (load != null && !load.isDone) break;
79:                     if (Time.frameCount <= loadedFrame + 2) break;
80:                     if (!destinationSeen)
81:                     {
82:                         Fail("ステージを開けませんでした。もう一度お試しください。");
83:                         break;
84:                     }
85:                     if (!MainStagePreview.IsActive && destinationPlayer != null)
86:                         MainStagePreview.PlayFor(destinationPlayer);
87:                     // Missing tour markers must not leave controls or an opaque curtain stuck.
88:                     if (!MainStagePreview.IsActive)
89:                         Debug.LogWarning("ステージ紹介の対象がないため、布を開いて通常プレイへ戻します。");
90:                     Enter(Phase.Opening);
91:                     break;
92:                 case Phase.Opening:
93:                     if (elapsed >= OpenDuration) Finish();
94:                     break;
95:             }
96:         }
97: 
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
150: 
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
```

## StageProgress.cs

BuildInputs一致。SHA-256 `4119bab062a2c0f0ddea9101fe71ffb9fa53bb508ab73f1a98fe6b65c215cfa6`

### 1–40
```text
1: using System;
2: using UnityEngine;
3: 
4: namespace HimoHito
5: {
6:     /// <summary>Persistent clear flags only; stage availability stays in StageCatalog.</summary>
7:     public static class StageProgress
8:     {
9:         // Stable IDs keep progress when the catalog is reordered or scene paths change.
10:         private const string ClearedKeyPrefix = "HimoHito.StageProgress.v1.Cleared.";
11: 
12:         public static bool IsCleared(StageCatalog.Entry stage)
13:         {
14:             return stage != null && stage.available &&
15:                 !string.IsNullOrWhiteSpace(stage.id) &&
16:                 !string.IsNullOrWhiteSpace(stage.scenePath) &&
17:                 PlayerPrefs.GetInt(ClearedKeyPrefix + stage.id, 0) == 1;
18:         }
19: 
20:         public static void MarkSceneCleared(string scenePath)
21:         {
22:             if (string.IsNullOrWhiteSpace(scenePath)) return;
23: 
24:             bool changed = false;
25:             foreach (StageCatalog.Entry stage in StageCatalog.Entries)
26:             {
27:                 if (stage == null || !stage.available ||
28:                     string.IsNullOrWhiteSpace(stage.id) ||
29:                     string.IsNullOrWhiteSpace(stage.scenePath) ||
30:                     !string.Equals(stage.scenePath, scenePath, StringComparison.Ordinal) ||
31:                     IsCleared(stage)) continue;
32: 
33:                 PlayerPrefs.SetInt(ClearedKeyPrefix + stage.id, 1);
34:                 changed = true;
35:             }
36: 
37:             if (changed) PlayerPrefs.Save();
38:         }
39:     }
40: }
```

## TutorialSectionThreeSetup.cs

BuildInputs一致。SHA-256 `b5a4788208fd57acee9735eaff24780f2c55fe7d56afe3f14a97099adee0e076`

### 10–48
```text
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
```

## 追加：完了定数と選択画面への再読込

### MainStageGoalZone.cs:12–17

```text
12:     {
13:         private const float ClearRevealDelay = 1.35f;
14: 
15:         public bool IsClear { get; private set; }
16:         public bool IsCompleting => isClearPending || IsClear;
17:         private bool isClearPending;
```

### StageOverlayControls.cs:374–388

```text
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
388:         }
```

