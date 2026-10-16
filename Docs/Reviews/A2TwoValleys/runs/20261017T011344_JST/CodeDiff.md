# 隔離差分（表示用）

Productionへ適用する指示ではない。全空白を含む正確な差分は[JSON](CodeDiff.json)。

```diff
--- candidate1/Assets/PlayerPresentation/PlayerMotionPresentation.cs
+++ candidate2/Assets/PlayerPresentation/PlayerMotionPresentation.cs
@@ -9,7 +9,8 @@
 {
     public static class PlayerPresentationProof
     {
-        public static bool Candidate => A2Fixture.Arg("-presentation", "baseline") == "candidate";
+        public static bool Candidate => A2Fixture.Arg("-presentation", "baseline") != "baseline";
+        public static bool SecondCandidate => A2Fixture.Arg("-presentation", "baseline") == "candidate2";
         [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
         static void Bootstrap()
         {
@@ -33,6 +34,8 @@
         Vector2 previousPosition, delayedVelocity;
         float fallSpeed, landingAge = 9f, impact, trail;
         bool wasGrounded, wasAttached;
+        readonly SwingPresentationState swing2 = new SwingPresentationState();
+        public SwingPresentationState Swing2 => swing2;
         public float Trail => trail;
         public float Impact => landingAge < .3f ? impact : 0f;
         public float Brace { get; private set; }
@@ -65,6 +68,16 @@
             if(wasAttached&&!attached&&!grounded) ReleaseEvents++;
             delayedVelocity=Vector2.Lerp(delayedVelocity,velocity,1f-Mathf.Exp(-dt/.11f));
             float speed=velocity.magnitude;
+            if(PlayerPresentationProof.SecondCandidate)
+            {
+                Vector2 radial=(body.position-rope.AnchorPoint).normalized;
+                Vector2 tangent=new Vector2(-radial.y,radial.x);
+                float signed=Vector2.Dot(velocity,tangent);
+                var characterRenderers=new List<SpriteRenderer>();visual.CollectCharacterRenderers(characterRenderers);
+                bool facingLeft=characterRenderers.Count>0&&characterRenderers[0].flipX;
+                swing2.Advance(attached,visual.SwingPresentationReady,visual.SwingReleasePresentationWeight,
+                    facingLeft?-signed:signed,speed,Vector2.Dot(radial,Vector2.down),mover.MovementInput,dt);
+            }
             // Velocity/acceleration response, never a periodic limb clock.
             float targetTrail=attached ? Mathf.Clamp(-velocity.x/12f*.11f+(delayedVelocity.x-velocity.x)*.012f,-.14f,.14f)
                 : !grounded ? Mathf.Clamp(-velocity.x/12f*.065f,-.08f,.08f) : 0f;
@@ -100,6 +113,11 @@
                 renderer.GetPropertyBlock(properties);
                 properties.SetVector("_SpriteBounds",new Vector4(bounds.min.x,bounds.min.y,bounds.size.x,bounds.size.y));
                 properties.SetVector("_Motion",new Vector4(renderer.flipX?-trail:trail,Brace,compression,attached?Mathf.Clamp01(speed/13f):0));
+                Vector4 rig=Vector4.zero;
+                bool articulate=PlayerPresentationProof.SecondCandidate&&visual.TryGetSwingPresentationRig(sprite,out rig);
+                if(!articulate)rig=Vector4.zero;
+                properties.SetVector("_SwingRig",rig);
+                properties.SetVector("_SwingPose",articulate?new Vector4(swing2.Angles.x,swing2.Angles.y,swing2.Angles.z,swing2.Weight):Vector4.zero);
                 renderer.SetPropertyBlock(properties);
             }
             renderers.Clear();
@@ -109,7 +127,7 @@
         static void AddPresentationMesh(Sprite sprite)
         {
             // Dense render-only grid gives hands/feet a lag. Never OverridePhysicsShape.
-            const int columns=12, rows=24;
+            int columns=PlayerPresentationProof.SecondCandidate?20:12, rows=PlayerPresentationProof.SecondCandidate?48:24;
             // OverrideGeometry accepts Sprite Rect PIXELS, unlike Sprite.vertices.
             Vector2 min=Vector2.one*.05f;
             Vector2 max=sprite.rect.size-Vector2.one*.05f;
--- candidate1/Assets/PlayerPresentation/PresentationCapture.cs
+++ candidate2/Assets/PlayerPresentation/PresentationCapture.cs
@@ -35,6 +35,7 @@
             public Vector3 camera,visualPosition,visualScale;public float ortho;
             public bool grounded,attached,flip,shadowVisible,shadowEnabled;public Vector3 shadowPosition;public int particles,visibleParticleRenderers,bridges;
             public float lowestVisibleFootDelta;public string material;
+            public Vector3 swingAngles;public float swingWeight,drive,stream,gather;public string swingPhase;
         }
         void CheckResult(string name,bool ok,string detail="")
         {checks.Add(new Check(name,ok,detail));Debug.Log("PRESENTATION_QA "+name+" "+(ok?"PASS":"FAIL")+" "+detail);}
@@ -54,8 +55,11 @@
             bool smoke=A2Fixture.Arg("-presentationQA","flow")=="smoke";
             if(A2Fixture.Arg("-presentationQA")=="regression")
             {yield return MergeRegression();Save();yield return Delay(.1f);A2Fixture.Quit(checks.TrueForAll(c=>c.status=="PASS")?0:2);yield break;}
+            if(A2Fixture.Arg("-presentationQA")=="cycle")
+            {yield return FreeSwingCycle();Save();yield return Delay(.1f);A2Fixture.Quit(checks.TrueForAll(c=>c.status=="PASS")?0:2);yield break;}
             if(smoke)
             {
+                ValidateSwingState();
                 CheckResult("Shader supported",Resources.Load<Shader>("PlayerPresentationYarn")?.isSupported==true);
                 CheckResult("Initial grounded",f.Mover.IsGrounded);
                 CaptureStill("Smoke");
@@ -108,12 +112,33 @@
             Time.timeScale=0f;yield return new WaitForSecondsRealtime(.25f);
             CheckResult("Pause freezes displayed body",Vector3.Distance(pos,renderer.transform.position)<.00001f&&Vector3.Distance(scale,renderer.transform.localScale)<.00001f&&sprite==renderer.sprite);
             CaptureStill("Paused");Time.timeScale=1f;yield return Delay(.1f);
+            yield return SwingPauseRegression();
+            f.Retry();input.Move=0;yield return Delay(.95f);
             Mark("bridge_build");f.Rope.RestoreSelectedRopeLength(11);
             bool attached=f.Rope.TryAttach(GameObject.Find(A2Fixture.EndNames[0]).transform.position);
             bool built=attached&&f.Builder.TryBuildCurrentPlatform();yield return Delay(.15f);CaptureStill("BridgeWeave");
             CheckResult("Bridge build / weave visual retained",built&&f.Builder.GeneratedPlatformCount==1&&f.Resource.CurrentLength==13);
             CheckResult("No Gameplay error",f.Errors==0);
             Save();yield return Delay(.2f);A2Fixture.Quit(checks.TrueForAll(c=>c.status=="PASS")?0:2);
+        }
+        void ValidateSwingState()
+        {
+            var state=new SwingPresentationState();float dt=1f/30;
+            for(int i=0;i<15;i++)state.Advance(true,false,0,6,6,1,1,dt);
+            CheckResult("Original attach receives no extra pose",state.Weight==0&&state.Angles==Vector3.zero);
+            for(int i=0;i<20;i++)state.Advance(true,true,0,5,5,.7f,1,dt);
+            bool pump=state.Phase=="active_pump";
+            for(int i=0;i<20;i++)state.Advance(true,true,0,13,13,1,1,dt);
+            bool bottom=state.Phase=="fast_bottom_trail";Vector3 streaming=state.Angles;
+            for(int i=0;i<20;i++)state.Advance(true,true,0,.5f,.5f,.45f,1,dt);
+            bool apex=state.Phase=="apex_gather";
+            CheckResult("Three physical states select different render poses",pump&&bottom&&apex&&Vector3.Distance(streaming,state.Angles)>.2f);
+            Vector3 held=state.Angles;state.Advance(true,true,0,13,13,1,-1,0);
+            CheckResult("Paused state cannot advance",state.Angles==held);
+            state.Advance(false,false,1,0,0,0,0,dt);
+            CheckResult("First release holds articulated pose",state.Angles==held&&state.Weight==1);
+            state.Advance(false,false,0,0,0,0,0,dt);
+            CheckResult("Original air pose restored after release settle",state.Weight==0&&state.Angles==Vector3.zero);
         }
         IEnumerator RecordFrames()
         {
@@ -147,7 +172,7 @@
             SpriteRenderer shadow=shadowOwner?(SpriteRenderer)typeof(PlayerContactShadow).GetField("shadow",A2Fixture.Private).GetValue(shadowOwner):null;
             float err=0;var line=f.Body.GetComponent<LineRenderer>();
             if(f.Rope.IsAttached&&line.positionCount>0)err=Vector2.Distance(line.GetPosition(0),bodyVisual.RopeOrigin);
-            return new Sample{frame=frame,time=Time.time-startTime,fixedTime=Time.fixedTime,phase=phase,position=f.Body.position,velocity=f.Body.linearVelocity,grounded=f.Mover.IsGrounded,attached=f.Rope.IsAttached,resource=f.Resource.CurrentLength,selected=f.Rope.SelectedRopeLength,bridges=f.Builder.GeneratedPlatformCount,sprite=r.sprite.name,flip=r.flipX,camera=Camera.main.transform.position,ortho=Camera.main.orthographicSize,visualPosition=r.transform.position,visualScale=r.transform.lossyScale,visualFoot=sole,lowestVisibleFootDelta=sole.y-f.Body.GetComponent<Collider2D>().bounds.min.y,ropeOrigin=bodyVisual.RopeOrigin,ropeOriginError=err,shadowVisible=shadow&&shadow.enabled&&shadow.isVisible,shadowEnabled=shadow&&shadow.enabled,shadowPosition=shadow?shadow.transform.position:Vector3.zero,particles=particles,visibleParticleRenderers=visible,trail=presentation?presentation.Trail:0,brace=presentation?presentation.Brace:0,impact=presentation?presentation.Impact:0,material=r.sharedMaterial?r.sharedMaterial.name:""};
+            return new Sample{frame=frame,time=Time.time-startTime,fixedTime=Time.fixedTime,phase=phase,position=f.Body.position,velocity=f.Body.linearVelocity,grounded=f.Mover.IsGrounded,attached=f.Rope.IsAttached,resource=f.Resource.CurrentLength,selected=f.Rope.SelectedRopeLength,bridges=f.Builder.GeneratedPlatformCount,sprite=r.sprite.name,flip=r.flipX,camera=Camera.main.transform.position,ortho=Camera.main.orthographicSize,visualPosition=r.transform.position,visualScale=r.transform.lossyScale,visualFoot=sole,lowestVisibleFootDelta=sole.y-f.Body.GetComponent<Collider2D>().bounds.min.y,ropeOrigin=bodyVisual.RopeOrigin,ropeOriginError=err,shadowVisible=shadow&&shadow.enabled&&shadow.isVisible,shadowEnabled=shadow&&shadow.enabled,shadowPosition=shadow?shadow.transform.position:Vector3.zero,particles=particles,visibleParticleRenderers=visible,trail=presentation?presentation.Trail:0,brace=presentation?presentation.Brace:0,impact=presentation?presentation.Impact:0,material=r.sharedMaterial?r.sharedMaterial.name:"",swingAngles=presentation?presentation.Swing2.Angles:Vector3.zero,swingWeight=presentation?presentation.Swing2.Weight:0,drive=presentation?presentation.Swing2.Drive:0,stream=presentation?presentation.Swing2.Stream:0,gather=presentation?presentation.Swing2.Gather:0,swingPhase=presentation?presentation.Swing2.Phase:"candidate1"};
         }
         void CaptureStill(string name)
         {
@@ -156,7 +181,67 @@
         }
         void Save()
         {
-            File.WriteAllText(Path.Combine(output,"Result.json"),JsonUtility.ToJson(new Result{variant=PlayerPresentationProof.Candidate?"candidate":"baseline",checks=checks,events=events,samples=samples,finished=true,status=checks.TrueForAll(c=>c.status=="PASS")?"PASS":"FAIL"},true));
+            File.WriteAllText(Path.Combine(output,"Result.json"),JsonUtility.ToJson(new Result{variant=A2Fixture.Arg("-presentation"),checks=checks,events=events,samples=samples,finished=true,status=checks.TrueForAll(c=>c.status=="PASS")?"PASS":"FAIL"},true));
+        }
+        IEnumerator SwingPauseRegression()
+        {
+            f.Rope.RestoreSelectedRopeLength(6);
+            bool attached=f.Rope.TryAttach(GameObject.Find(A2Fixture.SwingNames[0]).transform.position);input.Move=1;
+            float deadline=Time.time+3;
+            while(Time.time<deadline&&(f.Mover.IsGrounded||!bodyVisual.SwingPresentationReady||f.Body.linearVelocity.magnitude<6))yield return new WaitForFixedUpdate();
+            if(Application.isEditor)yield return null;else yield return new WaitForEndOfFrame();
+            var renderers=new List<SpriteRenderer>();bodyVisual.CollectCharacterRenderers(renderers);var r=renderers[0];
+            var block=new MaterialPropertyBlock();r.GetPropertyBlock(block);Vector4 pose=block.GetVector("_SwingPose");
+            Vector3 pos=r.transform.position,scale=r.transform.localScale;Quaternion rotation=r.transform.rotation;
+            Sprite sprite=r.sprite;Vector2 root=f.Body.position,velocity=f.Body.linearVelocity;
+            CaptureStill("SwingPause_Before");
+            Time.timeScale=0;yield return new WaitForSecondsRealtime(.35f);
+            r.GetPropertyBlock(block);
+            CheckResult("Swing pause freezes pose / root / momentum",attached&&sprite==r.sprite&&Vector3.Distance(pos,r.transform.position)<.00001f&&Quaternion.Angle(rotation,r.transform.rotation)<.00001f&&Vector3.Distance(scale,r.transform.localScale)<.00001f&&Vector4.Distance(pose,block.GetVector("_SwingPose"))<.00001f&&Vector2.Distance(root,f.Body.position)<.00001f&&Vector2.Distance(velocity,f.Body.linearVelocity)<.00001f,"Scaled-time API pause, not Esc key/menu evidence");
+            CaptureStill("SwingPause_Held");Time.timeScale=1;
+            yield return Delay(.20f);
+            if(Application.isEditor)yield return null;else yield return new WaitForEndOfFrame();
+            CaptureStill("SwingPause_Resumed");
+            CheckResult("Swing resume advances with same attachment",f.Rope.IsAttached&&!f.Mover.IsGrounded&&Vector2.Distance(root,f.Body.position)>.05f&&f.Resource.CurrentLength==24);
+        }
+        IEnumerator FreeSwingCycle()
+        {
+            // Existing ground attachment, then A/D braking to a small free arc.
+            // No diagnostic teleport, force/velocity/joint edit, or new floor.
+            f.Rope.RestoreSelectedRopeLength(6);
+            bool attached=f.Rope.TryAttach(GameObject.Find(A2Fixture.SwingNames[0]).transform.position);
+            CheckResult("Supplement original ground attach length6",attached);input.Move=1;
+            float warmupEnd=Time.time+8;
+            while(Time.time<warmupEnd&&(f.Mover.IsGrounded||!bodyVisual.SwingPresentationReady))yield return new WaitForFixedUpdate();
+            float target=Mathf.Abs(Physics2D.gravity.y)*f.Body.gravityScale*6*(1-Mathf.Cos(20*Mathf.Deg2Rad));
+            bool settled=false;
+            while(Time.time<warmupEnd&&!f.Respawn.IsFailureVisible)
+            {
+                Vector2 radial=(f.Body.position-f.Rope.AnchorPoint).normalized;
+                float energy=.5f*f.Body.linearVelocity.sqrMagnitude+Mathf.Abs(Physics2D.gravity.y)*f.Body.gravityScale*6*(1+radial.y);
+                if(!f.Mover.IsGrounded&&energy<target*1.25f&&energy>target*.5f&&-radial.y>.9f){settled=true;break;}
+                input.Move=energy>target?-Mathf.Sign(f.Body.linearVelocity.x):Mathf.Sign(f.Body.linearVelocity.x);
+                yield return new WaitForFixedUpdate();
+            }
+            CheckResult("Supplement small free arc via original A/D",settled,"Warmup automatic braking only; no state teleport or physics edit");input.Move=0;
+            startTime=Time.time;recording=true;StartCoroutine(RecordFrames());Mark("cycle_attach");input.Move=1;
+            bool positive=false,right=false,left=false;float deadline=Time.time+8;
+            while(Time.time<deadline&&!f.Respawn.IsFailureVisible)
+            {
+                Vector2 radial=(f.Body.position-f.Rope.AnchorPoint).normalized;
+                Vector2 tangent=new Vector2(-radial.y,radial.x);
+                float signed=Vector2.Dot(f.Body.linearVelocity,tangent);
+                if(signed>2)positive=true;
+                if(positive&&!right&&signed<0){right=true;Mark("cycle_right_apex");}
+                if(right&&!left&&signed>0){left=true;Mark("cycle_left_apex");break;}
+                input.Move=0; // Coast with the existing physics after braking.
+                yield return new WaitForFixedUpdate();
+            }
+            yield return Delay(.20f);
+            if(Application.isEditor)yield return null;else yield return new WaitForEndOfFrame();recording=false;input.Move=0;
+            CheckResult("Supplement two airborne angular turnarounds",right&&left&&samples.TrueForAll(s=>!s.grounded),"Angular tangent sign, not horizontal velocity sign; diagnostic setup only");
+            CheckResult("Supplement resource / original layout",f.Resource.CurrentLength==24&&f.Builder.GeneratedPlatformCount==0&&JsonUtility.ToJson(new A2Fixture.Layout(f))==f.ExpectedLayout);
+            CheckResult("No Gameplay error",f.Errors==0);
         }
         IEnumerator MergeRegression()
         {
--- candidate1/Assets/Resources/PlayerPresentationYarn.shader
+++ candidate2/Assets/Resources/PlayerPresentationYarn.shader
@@ -25,7 +25,12 @@
             #pragma multi_compile _ PIXELSNAP_ON
             #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
             #include "UnitySprites.cginc"
-            float4 _SpriteBounds, _Motion;
+            float4 _SpriteBounds, _Motion, _SwingRig, _SwingPose;
+            float2 RotateJoint(float2 p, float2 pivot, float angle)
+            {
+                float s=sin(angle),c=cos(angle);float2 d=p-pivot;
+                return pivot+float2(c*d.x-s*d.y,s*d.x+c*d.y);
+            }
             struct ProofOutput {float4 vertex:SV_POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0;float2 local:TEXCOORD1;};
             ProofOutput ProofVertex(appdata_t input)
             {
@@ -41,6 +46,22 @@
                 input.vertex.x += side*_Motion.z*_SpriteBounds.z*sin(y*3.14159);
                 // Airborne bracing lifts loose ends a little, not the physics root.
                 input.vertex.y += _Motion.y*.035*_SpriteBounds.w*lower;
+                // Candidate 2 only: three skinned render joints below the head.
+                // Every vertex at/above the neck is exactly unchanged. Crown,
+                // eye/head shape, physical root, joint and drawn rope stay put.
+                if(_SwingPose.w>0)
+                {
+                    float2 p=input.vertex.xy;
+                    float neck=_SwingRig.y, hip=_SwingRig.z, knee=_SwingRig.w;
+                    float band=max(.006,_SpriteBounds.w*.025);
+                    float kneeMask=1-smoothstep(knee-band,knee+band,input.vertex.y);
+                    float hipMask=1-smoothstep(hip-band,hip+band,input.vertex.y);
+                    float neckMask=1-smoothstep(neck-band,neck,input.vertex.y);
+                    p=lerp(p,RotateJoint(p,float2(_SwingRig.x,knee),_SwingPose.z),kneeMask);
+                    p=lerp(p,RotateJoint(p,float2(_SwingRig.x,hip),_SwingPose.y),hipMask);
+                    p=lerp(p,RotateJoint(p,float2(_SwingRig.x,neck),_SwingPose.x),neckMask);
+                    input.vertex.xy=lerp(input.vertex.xy,p,_SwingPose.w);
+                }
                 output.vertex=UnityObjectToClipPos(UnityFlipSprite(input.vertex,_Flip));
                 #ifdef PIXELSNAP_ON
                 output.vertex=UnityPixelSnap(output.vertex);
--- candidate1/Assets/Scripts/RopeBodyVisual.cs
+++ candidate2/Assets/Scripts/RopeBodyVisual.cs
@@ -284,6 +284,22 @@
             if (balanceBodyRenderer != null) output.Add(balanceBodyRenderer);
         }

+        // Read-only landmarks for candidate 2's isolated render mesh. No pose or
+        // gameplay state is written here; candidate 1 takes its original path.
+        public bool SwingPresentationReady => isSwingPoseDisplayed && !swingAttachPose.IsActive;
+        public float SwingReleasePresentationWeight => isSwingReleasePoseDisplayed
+            ? 1f - swingReleasePose.SettleBlend : 0f;
+        public bool TryGetSwingPresentationRig(Sprite sprite, out Vector4 rig)
+        {
+            rig = Vector4.zero;
+            if (sprite == null || !SwingFrameScale.ContainsKey(sprite) ||
+                !PoseHeadCenters.TryGetValue(sprite, out Vector3 head) ||
+                !FrameHeadWidths.TryGetValue(sprite, out float width)) return false;
+            float neck = head.y - width * .45f;
+            rig = new Vector4(head.x, neck, neck - width * .62f, neck - width * .85f);
+            return true;
+        }
+
         private void LateUpdate()
         {
             RemoveBridgeOffset();
--- /dev/null
+++ candidate2/Assets/PlayerPresentation/SwingPresentationState.cs
@@ -0,0 +1,55 @@
+// Isolated candidate 2. Only render-joint angles; no engine/physics writes.
+using UnityEngine;
+
+namespace HimoHito
+{
+    public sealed class SwingPresentationState
+    {
+        public Vector3 Angles { get; private set; }
+        public float Weight { get; private set; }
+        public float Drive { get; private set; }
+        public float Stream { get; private set; }
+        public float Gather { get; private set; }
+        public string Phase { get; private set; } = "unchanged";
+        float lastSpeed, direction = 1f, releaseStartWeight;
+        bool wasAttached;
+
+        public void Advance(bool attached, bool ready, float releaseWeight,
+            float signedSpeed, float speed, float downwardAlignment, float input, float dt)
+        {
+            if (dt <= 0f) return;
+            if (!attached)
+            {
+                // Preserve the final articulated swing for the first release
+                // drawing, then meet the ORIGINAL .24 s release settle. The
+                // original airborne/landing poses and their clocks are intact.
+                if(wasAttached) releaseStartWeight=Weight;
+                Weight = releaseStartWeight * releaseWeight;
+                if (releaseWeight <= 0f) { Angles = Vector3.zero; Weight = 0; }
+                Phase = releaseWeight > 0f ? "release_handoff" : "unchanged";
+                wasAttached = false;
+                return;
+            }
+            if (!wasAttached) { Angles = Vector3.zero; Weight = 0; lastSpeed = speed; }
+            wasAttached = true;
+            if (!ready) { Weight = 0; lastSpeed = speed; Phase = "original_attach"; return; }
+            if (Mathf.Abs(signedSpeed) > 1.5f) direction = Mathf.Sign(signedSpeed);
+            float acceleration = Mathf.Max(0f, (speed - lastSpeed) / dt);
+            lastSpeed = speed;
+            Drive = Mathf.Clamp01(Mathf.Abs(input) * .75f + acceleration / 24f);
+            Stream = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(4.5f, 12f, speed)) *
+                Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.45f, .95f, downwardAlignment));
+            Gather = 1f - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(1.2f, 4.7f, speed));
+            // Rigid rotations at neck / hips / knees, not full-body stretch.
+            // Fast bottom: trail. Pumping: lead with bent legs. Apex: gather.
+            Vector3 target = new Vector3(
+                -direction * (7f * Drive + 24f * Stream),
+                direction * (26f * Drive * (1f - Stream) + 30f * Gather - 12f * Stream),
+                -direction * (34f * Drive + 50f * Gather) * (1f - .65f * Stream));
+            float response = 1f - Mathf.Exp(-dt / .085f);
+            Angles = Vector3.Lerp(Angles, target * Mathf.Deg2Rad, response);
+            Weight = Mathf.MoveTowards(Weight, 1f, dt / .18f);
+            Phase = Gather > .5f ? "apex_gather" : Stream > .5f ? "fast_bottom_trail" : "active_pump";
+        }
+    }
+}
--- /dev/null
+++ candidate2/Assets/PlayerPresentation/SwingPresentationState.cs.meta
@@ -0,0 +1,11 @@
+fileFormatVersion: 2
+guid: 4de4839d90234dcfa7a4478b27bd9d91
+MonoImporter:
+  externalObjects: {}
+  serializedVersion: 2
+  defaultReferences: []
+  executionOrder: 0
+  icon: {instanceID: 0}
+  userData:
+  assetBundleName:
+  assetBundleVariant:
```
