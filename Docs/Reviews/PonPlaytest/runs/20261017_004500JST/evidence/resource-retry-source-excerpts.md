# 資源・復帰・S6空中接続の固定Buildソース抜粋

10/6 FriendPlayの固定コピー。Bの根拠。実行・通過・待ち時間の測定ではない。行番号は原ファイルの1-based。

## Assets/Scripts/RopeResource.cs:11–20

```text
11:         [SerializeField, Min(1f)] private float maximumLength = 12f;
12:         [SerializeField, Min(0f)] private float currentLength = 12f;
13: 
14:         public float MaximumLength => maximumLength;
15:         public float CurrentLength => currentLength;
16:         public float NormalizedLength => maximumLength <= 0f ? 0f : currentLength / maximumLength;
17: 
18:         private void Awake()
19:         {
20:             currentLength = Mathf.Clamp(currentLength, 0f, maximumLength);
```

## Assets/Scripts/RopeResource.cs:35–69

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
44:         }
45: 
46:         public void Refund(float amount)
47:         {
48:             currentLength = Mathf.Clamp(currentLength + Mathf.Max(0f, amount), 0f, maximumLength);
49:         }
50: 
51:         public void ResetToMaximum()
52:         {
53:             RopeResourceGauge.ClearFlash(this);
54:             currentLength = maximumLength;
55:         }
56: 
57:         public void RestoreCapacityAndCurrent(
58:             float capacity,
59:             float amount)
60:         {
61:             RopeResourceGauge.ClearFlash(this);
62:             maximumLength = Mathf.Max(1f, capacity);
63:             currentLength = Mathf.Clamp(amount, 0f, maximumLength);
64:         }
65: 
66:         public void RestoreCurrentLength(float amount)
67:         {
68:             RopeResourceGauge.ClearFlash(this);
69:             currentLength = Mathf.Clamp(amount, 0f, maximumLength);
```

## Assets/Scripts/RopeController.cs:33–42

```text
33:         [SerializeField, Min(1f)] private float maximumShotDistance = 14f;
34:         [SerializeField, Min(0.01f)] private float ropeWidth = 0.16f;
35:         [SerializeField, Min(0.01f)] private float minimumRopeWidth = 0.06f;
36:         [SerializeField, Range(3, 32)] private int ropeVisualSegments = 14;
37:         [SerializeField] private Color ropeColor = new Color(0.95f, 0.82f, 0.35f);
38:         [SerializeField, Min(10f)] private float aimRotationSpeed = 120f;
39:         [SerializeField, Min(1)] private int minimumSelectableRopeLength = 1;
40:         [SerializeField, Min(0f)] private float lengthSelectionRepeatDelay = 0.35f;
41:         [SerializeField, Min(0.01f)] private float lengthSelectionRepeatInterval = 0.1f;
42:         [SerializeField, Min(0.1f)] private float airChainReconnectWindow = 1.5f;
```

## Assets/Scripts/RopeController.cs:74–82

```text
74:         public int SelectedRopeLength => selectedRopeLength;
75:         public int MaximumSelectableRopeLength => GetMaximumSelectableRopeLength();
76:         public float ActiveRopeLength => activeRopeLength;
77:         public int AttachmentSequence { get; private set; }
78:         public Color VisibleRopeColor => GetVisibleRopeColor();
79:         public bool IsAirChainReconnectOpen =>
80:             !IsAttached &&
81:             !string.IsNullOrEmpty(pendingAirChainGroup) &&
82:             Time.unscaledTime <= airChainReconnectExpiresAt;
```

## Assets/Scripts/RopeController.cs:222–282

```text
222: 
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

## Assets/Scripts/RopeController.cs:285–333

```text
285:         public void DetachAndRefund(bool playReleaseSound = false)
286:         {
287:             if (missVisual != null) missVisual.Cancel();
288:             hookArrivalPending = false;
289:             if (!playReleaseSound)
290:             {
291:                 ClearHookKnot();
292:                 // Retry can follow E in the same frame, before the released knot's update.
293:                 foreach (HookYarnKnotVisual knot in GetComponentsInChildren<HookYarnKnotVisual>(true))
294:                     knot.Cancel();
295:             }
296:             if (!playReleaseSound) ropeBodyVisual?.CancelActionPoses();
297:             if (!IsAttached)
298:             {
299:                 return;
300:             }
301: 
302:             // Disabling the joint must not erase the velocity built up by the pendulum.
303:             Vector2 preservedVelocity = isPlatformBuildHoldActive
304:                 ? Vector2.zero
305:                 : body.linearVelocity;
306:             float preservedAngularVelocity = isPlatformBuildHoldActive
307:                 ? 0f
308:                 : body.angularVelocity;
309:             HookPoint releasedHook = activeHookPoint;
310:             // Manual release only: copy the artwork before hiding the live rope.
311:             if (playReleaseSound)
312:             {
313:                 RopeRetractVisual returning = RopeRetractVisual.Play(lineRenderer, body);
314:                 if (hookKnot != null) hookKnot.Release(returning);
315:                 hookKnot = null;
316:             }
317:             ropeJoint.enabled = false;
318:             EndPlatformBuildHold();
319:             activeHookPoint = null;
320:             body.linearVelocity = preservedVelocity;
321:             body.angularVelocity = preservedAngularVelocity;
322:             lineRenderer.enabled = false;
323:             activeRopeLength = 0f;
324:             OpenAirChainReconnectWindow(releasedHook);
325:             ClampSelectedRopeLength();
326:             if (playReleaseSound)
327:             {
328:                 // The joint and momentum have already been handled. Only the
329:                 // visible body follows through; reattachment remains immediate.
330:                 ropeBodyVisual?.PlaySwingReleasePose();
331:                 audioFeedback.PlayRopeReleased(preservedVelocity.magnitude);
332:             }
333:         }
```

## Assets/Scripts/RopeController.cs:335–421

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
356:         }
357: 
358:         private bool TryResolveNextAirChainHook(
359:             out Vector2 resolvedAnchor,
360:             out HookPoint hookPoint)
361:         {
362:             resolvedAnchor = default;
363:             hookPoint = null;
364:             if (!IsAirChainReconnectOpen)
365:             {
366:                 return false;
367:             }
368: 
369:             float shotDistance = Mathf.Min(
370:                 SelectedRopeLength,
371:                 maximumShotDistance);
372:             foreach (HookPoint candidate in
373:                      Object.FindObjectsByType<HookPoint>(
374:                          FindObjectsSortMode.None))
375:             {
376:                 if (!CanReconnectAirChainTo(candidate))
377:                 {
378:                     continue;
379:                 }
380: 
381:                 Vector2 candidateAnchor = candidate.GetAttachmentPoint(
382:                     candidate.transform.position);
383:                 if (Vector2.Distance(body.position, candidateAnchor) >
384:                     shotDistance + 0.01f)
385:                 {
386:                     continue;
387:                 }
388: 
389:                 resolvedAnchor = candidateAnchor;
390:                 hookPoint = candidate;
391:                 return true;
392:             }
393: 
394:             return false;
395:         }
396: 
397:         private void ClearAirChainReconnectWindow()
398:         {
399:             pendingAirChainGroup = null;
400:             pendingAirChainOrder = 0;
401:             airChainReconnectExpiresAt = float.NegativeInfinity;
402:         }
403: 
404:         public bool CommitAttachedRopeAsPlatform(float permanentCost)
405:         {
406:             if (!IsAttached || permanentCost <= 0f ||
407:                 !ropeResource.TrySpend(permanentCost))
408:             {
409:                 return false;
410:             }
411: 
412:             hookArrivalPending = false;
413:             ClearHookKnot();
414:             ropeBodyVisual?.CancelActionPoses();
415:             ropeJoint.enabled = false;
416:             EndPlatformBuildHold();
417:             activeHookPoint = null;
418:             lineRenderer.enabled = false;
419:             activeRopeLength = 0f;
420:             ClampSelectedRopeLength();
421:             return true;
```

## Assets/Scripts/RopeController.cs:455–459

```text
455:         public void RestoreSelectedRopeLength(int length)
456:         {
457:             selectedRopeLength = length;
458:             ClampSelectedRopeLength();
459:         }
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

## Assets/Scripts/RopeController.cs:730–737

```text
730:         private int GetMaximumSelectableRopeLength()
731:         {
732:             float availableLength = ropeResource != null
733:                 ? ropeResource.CurrentLength
734:                 : maximumShotDistance;
735:             int availableWholeUnits = Mathf.FloorToInt(
736:                 Mathf.Min(maximumShotDistance, availableLength));
737:             return Mathf.Max(minimumSelectableRopeLength, availableWholeUnits);
```

## Assets/Scripts/RopePlatformBuilder.cs:100–126

```text
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

## Assets/Scripts/RopePlatformBuilder.cs:143–202

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
186:         }
187: 
188:         public PlatformState[] CapturePlatformStates()
189:         {
190:             List<PlatformState> states = new(generatedPlatforms.Count);
191:             foreach (GameObject platformObject in generatedPlatforms)
192:             {
193:                 if (platformObject != null &&
194:                     platformObject.TryGetComponent(out GeneratedRopePlatform platform))
195:                 {
196:                     states.Add(new PlatformState(
197:                         platform.Start,
198:                         platform.End,
199:                         platform.RopeLength));
200:                 }
201:             }
202:             return states.ToArray();
```

## Assets/Scripts/RopePlatformBuilder.cs:222–255

```text
222:         public GameObject[] CaptureRemovedHooks()
223:         {
224:             List<GameObject> hooks = new(removedHooks.Count);
225:             foreach (GameObject hook in removedHooks)
226:             {
227:                 if (hook != null && !hook.activeSelf)
228:                 {
229:                     hooks.Add(hook);
230:                 }
231:             }
232:             return hooks.ToArray();
233:         }
234: 
235:         public void RestorePlatformStates(
236:             IReadOnlyList<PlatformState> states,
237:             IReadOnlyList<GameObject> removedHookStates = null)
238:         {
239:             ClearPlatforms();
240:             if (states != null)
241:             {
242:                 for (int i = 0; i < states.Count; i++)
243:                 {
244:                     CreatePlatform(states[i].Start, states[i].End, states[i].RopeLength);
245:                 }
246:             }
247: 
248:             if (removedHookStates == null) return;
249:             foreach (GameObject hook in removedHookStates)
250:             {
251:                 if (hook == null || hook.scene != gameObject.scene) continue;
252:                 hook.SetActive(false);
253:                 if (!removedHooks.Contains(hook))
254:                 {
255:                     removedHooks.Add(hook);
```

## Assets/Scripts/RopePlatformBuilder.cs:351–401

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
```

## Assets/Scripts/MainStageRespawnOnFall.cs:25–49

```text
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
48:         public bool IsFailureVisible => IsRopeExhausted || IsFallFailure;
49:         public bool IsFallUnravelling => FallUnravelVisual.IsPlayingFor(gameObject);
```

## Assets/Scripts/MainStageRespawnOnFall.cs:65–76

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

## Assets/Scripts/MainStageRespawnOnFall.cs:118–153

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
```

## Assets/Scripts/MainStageRespawnOnFall.cs:178–206

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

## Assets/Scripts/MainStageCheckpoint.cs:18–49

```text
18:         public void Configure(int section, Vector2 position, float lowerBound)
19:         {
20:             sectionNumber = Mathf.Clamp(section, 2, 10);
21:             respawnPosition = position;
22:             minimumRopeAfterCheckpoint = Mathf.Max(0f, lowerBound);
23:         }
24: 
25:         private void OnCollisionStay2D(Collision2D collision)
26:         {
27:             if (reached || !IsPlayerLandingOnTop(collision))
28:             {
29:                 return;
30:             }
31: 
32:             MainStageRespawnOnFall respawn =
33:                 collision.rigidbody.GetComponent<MainStageRespawnOnFall>();
34:             if (respawn != null && respawn.TryReachSection(
35:                     sectionNumber,
36:                     respawnPosition,
37:                     minimumRopeAfterCheckpoint))
38:             {
39:                 reached = true;
40:                 SectionArrivalFeedback.Notify(collision.rigidbody.gameObject,
41:                     sectionNumber, respawnPosition, checkpointCollider);
42:             }
43:         }
44: 
45:         private bool IsPlayerLandingOnTop(Collision2D collision)
46:         {
47:             checkpointCollider ??= GetComponent<Collider2D>();
48:             return landingGate.Evaluate(collision, checkpointCollider, respawnPosition, Time.fixedTime);
49:         }
```

## Assets/Scripts/CheckpointLandingGate.cs:8–64

```text
8:         private const float SettleTime = .12f;
9:         private const float SurfaceTolerance = .06f;
10:         private Rigidbody2D supportedBody;
11:         private float supportedSince, lastSupportedAt;
12: 
13:         public void Reset() { supportedBody = null; }
14: 
15:         public void Exit(Rigidbody2D body)
16:         {
17:             if (supportedBody == body) Reset();
18:         }
19: 
20:         public bool Evaluate(Collision2D collision, Collider2D floor, Vector2 respawn, float now)
21:         {
22:             if (!HasTopSupport(collision, floor, respawn) || MainStagePreview.IsActive)
23:             {
24:                 Reset();
25:                 return false;
26:             }
27:             Rigidbody2D body = collision.rigidbody;
28:             if (supportedBody != body || now < lastSupportedAt ||
29:                 now - lastSupportedAt > Time.fixedDeltaTime * 1.5f)
30:             {
31:                 supportedBody = body;
32:                 supportedSince = now;
33:             }
34:             lastSupportedAt = now;
35:             return now - supportedSince >= SettleTime - .0001f;
36:         }
37: 
38:         private static bool HasTopSupport(Collision2D collision, Collider2D floor, Vector2 respawn)
39:         {
40:             Rigidbody2D body = collision.rigidbody;
41:             if (body == null || !body.simulated || floor == null || !floor.enabled || floor.isTrigger)
42:                 return false;
43:             Bounds bounds = floor.bounds;
44:             // A relay with a stale copied checkpoint cannot save a different, distant landing.
45:             if (respawn.x < bounds.min.x || respawn.x > bounds.max.x) return false;
46:             Collider2D player = collision.collider;
47:             if (player == null || player.attachedRigidbody != body) return false;
48:             Bounds feet = player.bounds;
49:             float inset = Mathf.Min(.04f, bounds.size.x * .1f);
50:             if (feet.center.x < bounds.min.x + inset || feet.center.x > bounds.max.x - inset ||
51:                 feet.min.y < bounds.max.y - SurfaceTolerance ||
52:                 feet.min.y > bounds.max.y + SurfaceTolerance ||
53:                 body.worldCenterOfMass.y <= bounds.max.y)
54:                 return false;
55:             float floorSpeed = floor.attachedRigidbody != null ? floor.attachedRigidbody.linearVelocity.y : 0f;
56:             if (Mathf.Abs(body.linearVelocity.y - floorSpeed) > .5f) return false;
57: 
58:             for (int i = 0; i < collision.contactCount; i++)
59:             {
60:                 ContactPoint2D contact = collision.GetContact(i);
61:                 if (contact.point.x >= bounds.min.x && contact.point.x <= bounds.max.x &&
62:                     Mathf.Abs(contact.point.y - bounds.max.y) <= SurfaceTolerance &&
63:                     Mathf.Abs(contact.normal.y) >= .8f)
64:                     return true;
```

## Assets/Scripts/MainStageSectionFourSetup.cs:40–45

```text
40:         public static readonly Vector2 BridgeAnchorPosition =
41:             new Vector2(68.3f, -0.65f);
42:         public static readonly Vector2 BridgeAnchorSize =
43:             new Vector2(0.62f, 0.62f);
44:         public const int BridgeRopeLength = 5;
45:         public static readonly Vector2 FarHookPosition =
```

## Assets/Scripts/MainStageSectionFourSetup.cs:88–96

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

## Assets/Scripts/MainStageSectionFiveSetup.cs:23–27

```text
23:         public const string LandingName = "Main S05 Landing";
24: 
25:         public const int BridgeRopeLength = 6;
26:         public const int SwingRopeLength = 7;
27: 
```

## Assets/Scripts/MainStageSectionFiveSetup.cs:117–127

```text
117:             if (landing != null)
118:             {
119:                 if (!landing.TryGetComponent(out MainStageCheckpoint checkpoint))
120:                 {
121:                     checkpoint = landing.AddComponent<MainStageCheckpoint>();
122:                     changed = true;
123:                 }
124:                 checkpoint.Configure(6, LandingRespawnPosition, 34f);
125:             }
126: 
127:             return changed;
```

## Assets/Scripts/MainStageSectionSixSetup.cs:21–74

```text
21:         public const string UpperBridgeStartHookName =
22:             "Main S06 Green Upper Bridge Start Hook";
23:         public const string UpperBridgeEndHookName =
24:             "Main S06 Green Upper Bridge End Hook";
25:         public const string UpperShelfAName = "Main S06 Upper Left";
26:         public const string UpperShelfBName = "Main S06 Upper Right";
27:         public const string UpperShelfCName = "Main S06 Upper Final Step";
28:         public const string LowerHookAName = "Main S06 Lower Hook A";
29:         public const string LowerHookBName = "Main S06 Lower Hook B";
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
```

## Assets/Scripts/MainStageSectionSixSetup.cs:111–127

```text
111:             changed |= EnsureHook(LowerHookAName, LowerHookAPosition, 1);
112:             changed |= EnsureHook(LowerHookBName, LowerHookBPosition, 2);
113:             changed |= EnsureHook(LowerHookCName, LowerHookCPosition, 3);
114: 
115:             changed |= EnsureTerrain(MergeName, MergePosition, MergeSize);
116:             GameObject merge = FindSceneObject(MergeName);
117:             if (merge != null)
118:             {
119:                 if (!merge.TryGetComponent(out MainStageCheckpoint checkpoint))
120:                 {
121:                     checkpoint = merge.AddComponent<MainStageCheckpoint>();
122:                     changed = true;
123:                 }
124: 
125:                 // 25 is only a safety lower bound. The lower route arrives
126:                 // with 39, so its fourteen-rope advantage remains after merging.
127:                 checkpoint.Configure(7, MergeRespawnPosition, 25f);
```

## Assets/Scripts/MainStageSectionSixSetup.cs:206–228

```text
206:         private static bool EnsureHook(
207:             string objectName,
208:             Vector2 position,
209:             int airChainOrder)
210:         {
211:             GameObject hook = EnsureObject(
212:                 objectName,
213:                 position,
214:                 NormalHookSize,
215:                 HookColor,
216:                 out bool changed);
217:             if (!hook.TryGetComponent(out HookPoint hookPoint))
218:             {
219:                 hookPoint = hook.AddComponent<HookPoint>();
220:                 changed = true;
221:             }
222:             changed |= hookPoint.ConfigureFixedAttachmentPoint(Vector2.zero);
223:             changed |= hookPoint.ConfigureAirChainStep(
224:                 LowerAirChainGroup,
225:                 airChainOrder);
226: 
227:             if (hook.TryGetComponent(out RopePlatformAnchor platformAnchor))
228:             {
```

## Assets/Scripts/MainStageSectionSevenSetup.cs:21–33

```text
21:         public const string BridgeStartHookName =
22:             "Main S07 Green Lower Bridge Start Hook";
23:         public const string BridgeEndHookName =
24:             "Main S07 Green Middle Bridge End Hook";
25:         public const string MiddleShelfName = "Main S07 Middle Shelf";
26:         public const string UpperShelfName = "Main S07 Upper Step";
27:         public const string UpperHookName = "Main S07 Upper Hook";
28:         public const string GoalFloorName = "Main S07 Landing";
29: 
30:         public const int BridgeRopeLength = 4;
31:         public const int FinalSwingRopeLength = 5;
32: 
33:         // The merge floor from section six is the lower floor in the slide.
```

## Assets/Scripts/MainStageSectionSevenSetup.cs:100–109

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

## Assets/Scripts/MainStageSectionNineSetup.cs:14–28

```text
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
```

## Assets/Scripts/MainStageSectionNineSetup.cs:86–95

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

## Assets/Scripts/MainStageSectionTenSetup.cs:12–28

```text
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
```

## Assets/Scripts/SectionArrivalFeedback.cs:33–60

```text
33:         public static void Notify(GameObject player, int section, Vector2 respawn, Collider2D floor)
34:         {
35:             if (player == null || floor == null || section < 2 || section > 10) return;
36:             if (!player.TryGetComponent(out SectionArrivalFeedback feedback))
37:                 feedback = player.AddComponent<SectionArrivalFeedback>();
38:             feedback.ShowArrival(section, respawn, floor);
39:         }
40: 
41:         private void ShowArrival(int section, Vector2 respawn, Collider2D floor)
42:         {
43:             if (flags.ContainsKey(section)) return;
44:             Bounds bounds = floor.bounds;
45:             float size = Mathf.Min(1f, bounds.size.x / 1.5f);
46:             float x = Mathf.Clamp(respawn.x + 2f,
47:                 bounds.min.x + .25f * size, bounds.max.x - 1.1f * size);
48:             var flagObject = new GameObject($"Reached Section {section} Yarn Flag")
49:             {
50:                 hideFlags = HideFlags.DontSave
51:             };
52:             // Keep it on the actual accepted floor, not a raycast-selected hook.
53:             flagObject.transform.position = new Vector3(x, bounds.max.y - .04f, floor.transform.position.z);
54:             flagObject.transform.localScale = Vector3.one * size;
55:             var flag = flagObject.AddComponent<CheckpointYarnFlag>();
56:             flag.Initialize(section);
57:             flags.Add(section, flag);
58:             noticeText = $"第{section}区間";
59:             noticeElapsed = 0f;
60:         }
```

## Assets/Scripts/SectionArrivalFeedback.cs:89–120

```text
89:         private void OnGUI()
90:         {
91:             if (!IsPlaying || IsCovered || noticeElapsed >= NoticeDuration) return;
92:             if (titleStyle == null)
93:             {
94:                 titleStyle = new GUIStyle(GUI.skin.label)
95:                 {
96:                     fontSize = 28, fontStyle = FontStyle.Bold,
97:                     alignment = TextAnchor.MiddleCenter, wordWrap = false
98:                 };
99:                 subtitleStyle = new GUIStyle(titleStyle) { fontSize = 13, fontStyle = FontStyle.Normal };
100:                 HimoHitoGuiTheme.ApplyToStyles(titleStyle, subtitleStyle);
101:                 titleStyle.normal.textColor = subtitleStyle.normal.textColor = new Color(.12f, .08f, .19f);
102:             }
103:             float alpha = Mathf.SmoothStep(0f, 1f, noticeElapsed / .22f) *
104:                 (1f - Mathf.SmoothStep(0f, 1f, (noticeElapsed - 1.55f) / .55f));
105:             Rect safe = Screen.safeArea;
106:             float width = Mathf.Min(286f, Mathf.Max(0f, safe.width - 32f));
107:             if (width < 100f || safe.height < 90f) return;
108:             // Below the compact HUD on narrow windows; never outside the safe area.
109:             float y = Screen.height - safe.yMax + (safe.width < 1020f ? 198f : 22f);
110:             y = Mathf.Min(y, Screen.height - safe.yMin - 86f);
111:             var panel = new Rect(safe.xMin + (safe.width - width) * .5f, y, width, 70f);
112:             Color previousColor = GUI.color;
113:             int previousDepth = GUI.depth;
114:             GUI.depth = -5;
115:             GUI.color = new Color(previousColor.r, previousColor.g, previousColor.b, previousColor.a * alpha);
116:             if (HimoHitoUiParts.IsAvailable) HimoHitoUiParts.DrawWoodButton(panel);
117:             else GUI.Box(panel, GUIContent.none);
118:             GUI.Label(new Rect(panel.x, panel.y + 5f, panel.width, 38f), noticeText, titleStyle);
119:             GUI.Label(new Rect(panel.x, panel.y + 40f, panel.width, 22f), "復帰地点を更新", subtitleStyle);
120:             GUI.color = previousColor;
```

## Assets/Scripts/PlayerMover.cs:30–34

```text
30:         [SerializeField] private float jumpImpulse = 10f;
31:         [SerializeField, Range(0f, 1f)] private float jumpHorizontalSpeedRetention = 0.6923077f;
32:         [SerializeField] private float coyoteTime = 0.1f;
33:         [SerializeField] private float jumpBufferTime = 0.15f;
34:         [SerializeField] private float groundProbeDistance = 0.08f;
```

## Assets/Scripts/PlayerMover.cs:149–153

```text
149:             }
150:             landingFluffAirTime = IsGrounded ? 0f : landingFluffAirTime + Time.fixedDeltaTime;
151:             coyoteTimer = IsGrounded ? coyoteTime : coyoteTimer - Time.fixedDeltaTime;
152: 
153:             bool isSwinging = ropeController != null && ropeController.IsAttached;
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

## MainStage保存値（選択上限・空中例外・開始残量・橋の最低残量）

```text
9752:   maximumShotDistance: 14
9761:   airChainReconnectWindow: 1.5
9809:   maximumLength: 50
9810:   currentLength: 50
10101:   minimumRopeReserve: 1
```

## 復元の補足：一旦除去フックを戻し、保存された除去状態を再適用

```text
330:         public void ClearPlatforms()
331:         {
332:             RestoreRemovedHooks();
333:             for (int i = generatedPlatforms.Count - 1; i >= 0; i--)
334:             {
335:                 RemovePlatform(generatedPlatforms[i]);
336:             }
337:         }
338: 
339:         private void RestoreRemovedHooks()
340:         {
341:             foreach (GameObject hook in removedHooks)
342:             {
343:                 if (hook != null)
344:                 {
345:                     hook.SetActive(true);
346:                 }
347:             }
348:             removedHooks.Clear();
349:         }
```

## MainStage保存値：枯渇の判定下限

```text
9739:  minimumUsableRopeLength: 1
```
