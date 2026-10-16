# 見せ場比較：結果の手掛かりに限った追加抜粋

A=0。T4形状式・S6消費/再接続条件・通常HUD経路は既存06/07/09を参照し、再複製しない。

## TutorialSignInscription.cs:204–230

```text
204:         private void LengthChoicePicture()
205:         {
206:             Color danger = new Color(1f, .29f, .34f);
207:             // Feet trajectories, not two pre-built bridges. The longer swing reaches the spikes.
208:             Stroke(new[] { new Vector3(.15f, .69f), new Vector3(.235f, .69f),
209:                 new Vector3(.235f, .515f) }, .013f, DiagramInk, false, "Left Bank");
210:             Stroke(new[] { new Vector3(.56f, .515f), new Vector3(.56f, .69f),
211:                 new Vector3(.615f, .69f) }, .013f, DiagramInk, false, "Right Bank");
212:             DashedPath(.58f, Green, "Safe Swing Path");
213:             DashedPath(.37f, danger, "Too Long Swing Path");
214:             for (int i = 0; i < 3; i++)
215:             {
216:                 float x = .345f + i * .045f;
217:                 Stroke(new[] { new Vector3(x - .018f, .49f), new Vector3(x, .55f),
218:                     new Vector3(x + .018f, .49f), new Vector3(x - .018f, .49f) },
219:                     .011f, danger, false, "Valley Spike");
220:             }
221:             Stroke(new[] { new Vector3(.555f, .748f), new Vector3(.570f, .731f),
222:                 new Vector3(.600f, .769f) }, .010f, Green, false, "Safe Check");
223:             Stroke(new[] { new Vector3(.50f, .545f), new Vector3(.526f, .515f) },
224:                 .010f, danger, false, "Too Long Cross");
225:             Stroke(new[] { new Vector3(.50f, .515f), new Vector3(.526f, .545f) },
226:                 .010f, danger, false, "Too Long Cross");
227:             Vector2 head = PlayerPicture(new Vector2(.395f, .635f), .115f);
228:             firstRope=Curve(head, Vector2.Lerp(head, new Vector2(.395f, .838f), .5f),
229:                 new Vector2(.395f, .838f), true);
230:             Ring(new Vector2(.395f, .838f), Blue);
```

## TutorialSectionGuide.cs:709–736

```text
709:         private void DrawSectionTwoIllustration(Rect rect)
710:         {
711:             var f = TutorialGuideAnimation.Sample(2, illustrationTime);
712:             DrawSwingBanks(rect);
713:             Vector2 hook = DiagramPoint(rect, new Vector2(.5f,.15f));
714:             for (int path=0;path<2;path++)
715:             {
716:                 bool danger=path==1;
717:                 DrawDashedCurve(DiagramPoint(rect,TutorialGuideAnimation.Swing(0f,danger)),
718:                     DiagramPoint(rect,new Vector2(.5f,danger ? 1.08f : .88f)),
719:                     DiagramPoint(rect,TutorialGuideAnimation.Swing(1f,danger)), 2f,
720:                     danger ? new Color(1f,.36f,.56f,.32f) : new Color(.45f,.95f,.76f,.45f));
721:             }
722:             for (int i=0;i<3;i++)
723:             {
724:                 Vector2 peak=DiagramPoint(rect,new Vector2(.44f+i*.06f,.83f));
725:                 float width=rect.width*.05f, height=rect.height*.065f;
726:                 for(int row=0;row<12;row++)
727:                 {
728:                     float fraction=(row+1)/12f;
729:                     DrawRect(new Rect(peak.x-width*fraction*.5f,peak.y+height*row/12f,
730:                         width*fraction,height/12f+1f),new Color(1f,.28f,.36f));
731:                 }
732:             }
733:             DrawDemoConnection(rect,f,hook);
734:             DrawDemoPlayer(rect,f);
735:             DrawDemoRing(hook,new Color(.24f,.64f,.96f));
736:             DrawDemoCaption(rect,f.Caption);
```

## RopeController.cs:248–250

```text
248:             // when Q converts the active rope into a platform.
249:             float selectedLength = SelectedRopeLength;
250:             activeRopeLength = selectedLength;
```

## RopeController.cs:650–667

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

## RopeController.cs:786–811

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
801:             if (!available && !TryResolveAttachmentPoint(target, out anchor, out hook,
802:                     previewBeyondSelectedLength: true))
803:             {
804:                 highlightedHook = null; highlightElapsed = 0f;
805:                 return;
806:             }
807:             if (!available || highlightedHook != hook) highlightElapsed = 0f;
808:             highlightedHook = available ? hook : null;
809:             if (available) highlightElapsed += Time.deltaTime;
810:             hasAvailableAimAnchor = available;
811:             availableAimAnchor = anchor;
```

## RopeSpikeHazard.cs:42–67

```text
42:         private void Cut(Collider2D other)
43:         {
44:             RopeController rope = other.GetComponentInParent<RopeController>();
45:             if (rope == null && other.attachedRigidbody != null)
46:             {
47:                 rope = other.attachedRigidbody.GetComponent<RopeController>();
48:             }
49:             if (rope != null && rope.IsAttached)
50:             {
51:                 CacheReferences();
52:                 Vector2 point = other.bounds.center;
53:                 if (detectionArea != null)
54:                 {
55:                     ColliderDistance2D contact = Physics2D.Distance(detectionArea, other);
56:                     point = contact.isValid ? contact.pointA : detectionArea.ClosestPoint(point);
57:                 }
58:                 Rigidbody2D body = rope.GetComponent<Rigidbody2D>();
59:                 Vector2 velocity = body != null ? body.linearVelocity : Vector2.zero;
60:                 // Preserve the existing detach/refund first. Presentation adds no force.
61:                 rope.DetachAndRefund();
62:                 if (body != null && body.simulated && Time.timeScale > 0f)
63:                 {
64:                     GetComponent<ToySpikeVisual>()?.PlayContact(point, velocity);
65:                     RopeLandingFluff.PlaySpikeContact(other, point, velocity);
66:                 }
67:             }
```

## PrototypeHud.cs:124–134

```text
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
