# 削除/統合反証：依存関係を結ぶ短い追加根拠

GoalChestPresentationのmarker-null分岐とMainGoalの判定、CP保存/復元、S7地形は既存06/07/10を再利用。Sceneの実値はdependencies JSONに元行番号付きで収録。

## PrototypeRunController.cs:356–374

```text
356:         public void TryReachTutorialSection(
357:             int sectionNumber,
358:             Vector2 respawnPosition,
359:             int startingRopeLength)
360:         {
361:             bool canReachCheckpointWhileAttached =
362:                 CurrentTutorialSection == 1 && sectionNumber == 2;
363:             if (Outcome != RunOutcome.Playing ||
364:                 (ropeController.IsAttached && !canReachCheckpointWhileAttached) ||
365:                 sectionNumber <= CurrentTutorialSection ||
366:                 sectionNumber > TutorialSectionCount)
367:             {
368:                 return;
369:             }
370: 
371:             CurrentTutorialSection = sectionNumber;
372:             checkpointPosition = respawnPosition;
373:             // Keep the player's selection across sections instead of applying the lesson preset.
374:             CaptureCheckpointState();
```

## MainStageRespawnOnFall.cs:16–16

```text
16:         private const int SectionOneStartingRopeLength = 7;
```

## MainStageRespawnOnFall.cs:65–76

```text
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
```

## MainStageRespawnOnFall.cs:178–195

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
```

## MainStageSectionSevenSetup.cs:97–109

```text
97:             GameObject goalFloor = FindSceneObject(GoalFloorName);
98:             if (goalFloor != null)
99:             {
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

## RopePlatformBuilder.cs:408–417

```text
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
```

## ClearJourneySnapshot.cs:21–31

```text
21:         public readonly float Remaining, Capacity;
22: 
23:         public ClearJourneySnapshot(RopeResource owner, RopePlatformBuilder builder, bool tutorial)
24:         {
25:             var floors = new List<Floor>();
26:             Bridges = builder != null && owner != null && builder.gameObject.scene == owner.gameObject.scene
27:                 ? builder.CaptureJourneyCurves() : new Vector2[0][];
28:             Remaining = owner != null ? owner.CurrentLength : 0f;
29:             Capacity = owner != null ? owner.MaximumLength : 0f;
30:             Vector2? start = null, goal = null;
31:             string startName = tutorial ? TutorialSectionOneSetup.StartFloorName : "Main Start Ground";
```
