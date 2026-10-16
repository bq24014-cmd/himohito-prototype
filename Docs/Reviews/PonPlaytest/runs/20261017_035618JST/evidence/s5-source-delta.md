# S5 追加読み取りの短い抜粋

既知の起点/Spot/rail説明はOct3監査と既存07/10を参照。ここは長さ判定・保存対象・更新順の照合用。固定BuildInputsでhash確認済み。実行結果ではない。

## Assets/Scripts/PlatformOccludedLightHazard.cs:42–64

```text
42:         private void FixedUpdate()
43:         {
44:             if (playerRope == null || playerCollider == null || lightSource == null)
45:             {
46:                 return;
47:             }
48: 
49:             IsBlocked = IsBlockedByGeneratedPlatform();
50:             float targetAlpha = IsBlocked ? blockedAlpha : activeAlpha;
51:             currentAlpha = Mathf.MoveTowards(
52:                 currentAlpha,
53:                 targetAlpha,
54:                 Time.fixedDeltaTime / Mathf.Max(0.01f, fadeDuration));
55:             Color color = spotRenderer.color;
56:             color.a = currentAlpha;
57:             spotRenderer.color = color;
58: 
59:             if (!IsBlocked && playerRope.IsAttached &&
60:                 Physics2D.Distance(triggerArea, playerCollider).isOverlapped)
61:             {
62:                 playerRope.DetachAndRefund();
63:             }
64:         }
```

## Assets/Scripts/PlatformOccludedLightHazard.cs:66–83

```text
66:         private bool IsBlockedByGeneratedPlatform()
67:         {
68:             Vector2 source = lightSource.position;
69:             Vector2 target = transform.position;
70:             RaycastHit2D[] hits = Physics2D.LinecastAll(source, target);
71:             foreach (RaycastHit2D hit in hits)
72:             {
73:                 if (hit.collider == null || hit.collider == playerCollider)
74:                 {
75:                     continue;
76:                 }
77:                 if (hit.collider.TryGetComponent(out GeneratedRopePlatform _))
78:                 {
79:                     return true;
80:                 }
81:             }
82:             return false;
83:         }
```

## Assets/Scripts/RopePlatformBuilder.cs:397–405

```text
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

## Assets/Scripts/RopePlatformBuilder.cs:468–473

```text
468:             GeneratedRopePlatform generated =
469:                 platformObject.AddComponent<GeneratedRopePlatform>();
470:             generated.Configure(
471:                 start,
472:                 end,
473:                 ropeLength,
```

## Assets/Scripts/RopePlatformBuilder.cs:484–495

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

## Assets/Scripts/RopePlatformBuilder.cs:699–709

```text
699:             bool shouldIgnore =
700:                 isPlayerAttached &&
701:                 !isBuildingPlatform &&
702:                 !preserveCollisionUntilSeparated;
703:             if (shouldIgnore == isIgnoringPlayerCollision)
704:             {
705:                 return;
706:             }
707: 
708:             Physics2D.IgnoreCollision(platformCollider, playerCollider, shouldIgnore);
709:             isIgnoringPlayerCollision = shouldIgnore;
```

## Assets/Scripts/MainStageRespawnOnFall.cs:201–207

```text
201:         private void CaptureCheckpointState()
202:         {
203:             checkpointRopeLength = ropeResource.CurrentLength;
204:             checkpointSelectedRopeLength = ropeController.SelectedRopeLength;
205:             checkpointPlatformStates = platformBuilder.CapturePlatformStates();
206:             checkpointRemovedHooks = platformBuilder.CaptureRemovedHooks();
207:         }
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

## Assets/Scripts/MainStageSectionFiveSetup.cs:130–147

```text
130:         /// <summary>
131:         /// Reasserts both authored rail shelves after R or a fall. Generated
132:         /// rope platforms are restored separately by RopePlatformBuilder.
133:         /// </summary>
134:         public static void RestoreRailShelvesAfterRestart()
135:         {
136:             EnsureRailShelf(
137:                 LeftShelfName,
138:                 LeftShelfPosition,
139:                 ShelfSize);
140:             EnsureRailShelf(
141:                 RightShelfName,
142:                 RightShelfPosition,
143:                 ShelfSize);
144: 
145:             RestoreRailCollision(FindSceneObject(LeftShelfName));
146:             RestoreRailCollision(FindSceneObject(RightShelfName));
147:             MainStageVisuals.RestoreSectionFiveRailVisuals();
```

## Assets/Scripts/SwingPassThroughRailPlatform.cs:98–109

```text
98:         private void RestorePlayerCollision()
99:         {
100:             if (railCollider == null || playerCollider == null)
101:             {
102:                 return;
103:             }
104: 
105:             Physics2D.IgnoreCollision(
106:                 railCollider,
107:                 playerCollider,
108:                 false);
109:             isIgnoringPlayer = false;
```

## 旧診断 checks.txt:68–77（結果文字列の保存）

非通常入力の過去報告。遮光の面白さ・普通の操作での再現性をPASSにしない。
```text
68: PASS S5 hazard unblocked without bridge
69: CONTROLLED_SETUP position=(94.45, 0.08)
70: PASS S5_SHADOW E attach
71: PASS S5_SHADOW E no permanent spend
72: PASS S5_SHADOW Q build
73: PASS S5_SHADOW Q exact resource cost
74: PASS S5 authored bridge occludes light
75: PASS retry playable S5_BRIDGE_R_1
76: PASS S5 retry removes unsaved shadow bridge 1
77: PASS S5 rail shelves active 1
```
