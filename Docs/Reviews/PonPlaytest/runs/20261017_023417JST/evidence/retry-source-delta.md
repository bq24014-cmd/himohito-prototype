# 復帰後の再実行：追加根拠だけ

既存07のsnapshot/補充解析と08の入力解析は再収録しない。新しい区間配置・帰路の有効化・更新経路を抜粋する。

## MainStageFloorCollisionSetup.cs:15–42

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
```

## MainStageSectionThreeSetup.cs:35–71

```text
35:         public static readonly Vector2 LowerHookPosition =
36:             new Vector2(38.2f, -1.65f);
37:         public static readonly Vector2 UpperHookPosition =
38:             new Vector2(38.4f, 1.75f);
39:         public static readonly Vector2 HookSize =
40:             new Vector2(1.6f, 0.45f);
41:         public static readonly Vector2 LowDeadEndPosition =
42:             new Vector2(46.65f, -11.05f);
43:         public static readonly Vector2 LowDeadEndSize =
44:             // Keep the left edge at 43.7; close the gap to the high bank at 49.6.
45:             new Vector2(5.9f, 10f);
46:         public static readonly Vector2 ReturnStepAPosition =
47:             new Vector2(42.8f, -11.05f);
48:         public static readonly Vector2 ReturnStepASize =
49:             new Vector2(1.8f, 10f);
50:         public static readonly Vector2 ReturnStepBPosition =
51:             new Vector2(41f, -10.775f);
52:         public static readonly Vector2 ReturnStepBSize =
53:             new Vector2(1.8f, 10.55f);
54:         public static readonly Vector2 ReturnStepCPosition =
55:             new Vector2(39.2f, -10.5f);
56:         public static readonly Vector2 ReturnStepCSize =
57:             new Vector2(1.8f, 11.1f);
58:         public static readonly Vector2 ReturnStepDPosition =
59:             new Vector2(37.4f, -10.225f);
60:         public static readonly Vector2 ReturnStepDSize =
61:             new Vector2(1.8f, 11.65f);
62:         public static readonly Vector2 RecoverySwitchPosition =
63:             new Vector2(45.6f, -5.75f);
64:         public static readonly Vector2 RecoverySwitchSize =
65:             new Vector2(0.9f, 0.35f);
66:         public static readonly Vector2 HighShelfPosition =
67:             new Vector2(57.1f, -7.15f);
68:         public static readonly Vector2 HighShelfSize =
69:             new Vector2(15f, 10f);
70:         public static readonly Vector2 HighShelfRespawnPosition =
71:             new Vector2(53.6f, -1.45f);
```

## MainStageSectionThreeSetup.cs:122–134

```text
122:             GameObject highShelf = FindSceneObject(HighShelfName);
123:             if (highShelf != null)
124:             {
125:                 if (!highShelf.TryGetComponent(
126:                         out MainStageCheckpoint checkpoint))
127:                 {
128:                     checkpoint = highShelf.AddComponent<MainStageCheckpoint>();
129:                     changed = true;
130:                 }
131:                 checkpoint.Configure(
132:                     4,
133:                     HighShelfRespawnPosition,
134:                     45f);
```

## MainStageSectionThreeRecoveryStairs.cs:37–76

```text
37:         public void Configure()
38:         {
39:             if (Application.isPlaying && !revealed)
40:             {
41:                 SetStairsActive(false);
42:             }
43:         }
44: 
45:         private void Awake()
46:         {
47:             Configure();
48:         }
49: 
50:         public void Reveal()
51:         {
52:             if (revealed)
53:             {
54:                 return;
55:             }
56: 
57:             revealed = true;
58:             SetStairsActive(true);
59:             // Refresh once after activation, before capturing the final visual poses.
60:             RopeResource player = Object.FindFirstObjectByType<RopeResource>();
61:             if (player != null) MainStageVisuals.Apply(player.gameObject);
62:             if (Application.isPlaying) BeginReveal();
63:         }
64: 
65:         public void Hide()
66:         {
67:             FinishReveal();
68:             if (!revealed)
69:             {
70:                 SetStairsActive(false);
71:                 return;
72:             }
73: 
74:             revealed = false;
75:             SetStairsActive(false);
76:         }
```

## MainStageSectionThreeRecoveryStairs.cs:175–198

```text
175:         private static void SetStairsActive(bool active)
176:         {
177:             foreach (string name in StepNames) SetActive(name, active);
178:         }
179: 
180:         private static void SetActive(string objectName, bool active)
181:         {
182:             GameObject step = FindStep(objectName);
183:             if (step != null) step.SetActive(active);
184:         }
185: 
186:         private static GameObject FindStep(string objectName)
187:         {
188:             foreach (GameObject candidate in
189:                      Resources.FindObjectsOfTypeAll<GameObject>())
190:             {
191:                 if (candidate.scene.IsValid() &&
192:                     candidate.scene.name == "MainStage" &&
193:                     candidate.name == objectName)
194:                 {
195:                     return candidate;
196:                 }
197:             }
198:             return null;
```

## MainStageSectionThreeRecoverySwitch.cs:62–94

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

## MainStageSectionFourSetup.cs:28–54

```text
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
```

## MainStageSectionFourSetup.cs:88–96

```text
88:                         out MainStageCheckpoint checkpoint))
89:                 {
90:                     checkpoint = landing.AddComponent<MainStageCheckpoint>();
91:                     changed = true;
92:                 }
93: 
94:                 checkpoint.Configure(5, LandingRespawnPosition, 45f);
95:             }
96: 
```

## MainStageSectionSixSetup.cs:69–74

```text
69:         public static readonly Vector2 MergePosition =
70:             new Vector2(151.1f, -7.15f);
71:         public static readonly Vector2 MergeSize =
72:             new Vector2(8f, 10f);
73:         public static readonly Vector2 MergeRespawnPosition =
74:             new Vector2(147.9f, -1.45f);
```

## MainStageSectionSixSetup.cs:125–127

```text
125:                 // 25 is only a safety lower bound. The lower route arrives
126:                 // with 39, so its fourteen-rope advantage remains after merging.
127:                 checkpoint.Configure(7, MergeRespawnPosition, 25f);
```

## MainStageSectionSevenSetup.cs:30–56

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

## MainStageSectionSevenSetup.cs:100–109

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
