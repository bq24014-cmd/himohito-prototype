# 隔離Proofの差分

表示用として末尾空白だけ除去したプレビュー。正確な差分は[CodeDiff.json](CodeDiff.json)のpatchフィールドに保持。Productionへ適用しない。

```diff
--- before/Assets/Scripts/PlayerContactShadow.cs
+++ isolated/Assets/Scripts/PlayerContactShadow.cs
@@ -54,7 +54,10 @@
                 if (floor==null || floor==player || floor.attachedRigidbody==player.attachedRigidbody ||
                     floor.isTrigger || hit.normal.y < .8f || hit.fraction<=0f || Physics2D.GetIgnoreCollision(player,floor)) continue;
                 // The first real support occludes anything below, including rope bridges.
-                if (!floor.TryGetComponent(out WoodenPlatformDepthVisual _) ||
+                bool wooden = floor.TryGetComponent(out WoodenPlatformDepthVisual _);
+                bool proofFloor = PlayerPresentationProof.Candidate &&
+                    (floor.name == "A2 Proof Entry Box" || floor.name == "A2 Proof Middle Box" || floor.name == "A2 Proof Exit Box");
+                if ((!wooden && !proofFloor) ||
                     Mathf.Abs(Mathf.DeltaAngle(floor.transform.eulerAngles.z,0f)) > .1f) return;
                 float height=Mathf.Max(0f,bounds.min.y-hit.point.y);
                 float fade=1f-Mathf.SmoothStep(0f,1f,height/1.9f);
@@ -63,7 +66,7 @@
                 float left=Mathf.Max(hit.point.x-width*.5f,floor.bounds.min.x+.02f);
                 float right=Mathf.Min(hit.point.x+width*.5f,floor.bounds.max.x-.02f);
                 if(right<=left) return;
-                shadow.transform.position=new Vector3((left+right)*.5f,hit.point.y-WoodenPlatformDepthVisual.SurfaceInset-.10f,0f);
+                shadow.transform.position=new Vector3((left+right)*.5f,proofFloor ? hit.point.y-.025f : hit.point.y-WoodenPlatformDepthVisual.SurfaceInset-.10f,0f);
                 shadow.transform.localScale=new Vector3(right-left,.28f,1f);
                 if(floor.TryGetComponent(out SpriteRenderer source))
                 { shadow.sortingLayerID=source.sortingLayerID; shadow.sortingOrder=source.sortingOrder+3; }
--- before/Assets/Scripts/PrototypeAudioFeedback.cs
+++ isolated/Assets/Scripts/PrototypeAudioFeedback.cs
@@ -177,6 +177,7 @@
             if (audioSource != null && playerLandingClip != null)
             {
                 audioSource.PlayOneShot(playerLandingClip, playerLandingVolume);
+                PresentationCapture.AudioMarker("landing");
             }
         }

@@ -351,6 +352,7 @@
                 releaseFullVolume,
                 speedRatio);
             audioSource.PlayOneShot(ropeReleaseWindClip, volume);
+            PresentationCapture.AudioMarker("release");
         }
     }
 }
--- before/Assets/Scripts/RopeBodyVisual.cs
+++ isolated/Assets/Scripts/RopeBodyVisual.cs
@@ -615,7 +615,9 @@
                 landingFrames.Length == LandingFrameCount &&
                 landingElapsed < landingDuration &&
                 isGrounded &&
-                !isAttached;
+                !isAttached &&
+                (!PlayerPresentationProof.Candidate ||
+                    (Mathf.Abs(playerMover.MovementInput) < .05f && !Input.GetButton("Jump")));

             weavePose.Advance(Time.deltaTime, CanShowWeavePose() && !shouldShowLanding);
             switchPressPose.Advance(Time.timeScale > 0f ? Time.deltaTime : 0f,
@@ -1315,12 +1317,23 @@
                     ? 1
                     : LandingFrameCount - 1;

+            if (PlayerPresentationProof.Candidate)
+            {
+                // Use the already drawn deep knee bend and recovery; no control lock.
+                int[] recovery = { 0, 2, 2, 4, 1, 5 };
+                frameIndex = recovery[Mathf.Min(5, Mathf.FloorToInt(normalizedTime * 6f))];
+            }
+
             visualRenderer.sprite = landingFrames[frameIndex];
             visualTransform.localPosition = new Vector3(
                 0f,
                 -CharacterVisualHeight * 0.5f,
                 0f);
             visualBaseScale = landingVisualBaseScale;
+            if (PlayerPresentationProof.Candidate &&
+                FrameHeadWidths.TryGetValue(landingFrames[frameIndex], out float landingHeadWidth) &&
+                craftHeadWorldWidth > 0f && landingHeadWidth > 0f)
+                visualBaseScale = Vector2.one * (craftHeadWorldWidth / landingHeadWidth);
             isWalking = false;
             walkFrameProgress = 0f;
             airborneElapsed = 0f;
@@ -1899,6 +1912,8 @@
                     softLandingStartedAt = Time.time;
                     softLandingStrength = Mathf.Lerp(0.4f, 1f,
                         Mathf.InverseLerp(minimumLandingSpeed, 10f, fastestFallSpeed));
+                    if (PlayerPresentationProof.Candidate)
+                        landingDuration = Mathf.Lerp(.20f, .28f, softLandingStrength);
                     takeoffStartedAt = float.NegativeInfinity;
                 }

@@ -1957,7 +1972,7 @@
             if (playerMover.IsGrounded && landTime >= 0f && landTime < 1f)
             {
                 float wave = Mathf.Sin(landTime * Mathf.PI);
-                stretch = -0.10f * softLandingStrength * wave * wave;
+                stretch = -(PlayerPresentationProof.Candidate ? .055f : .10f) * softLandingStrength * wave * wave;
             }
             Vector3 baseScale = visualTransform.localScale;
             visualTransform.localScale = new Vector3(baseScale.x / (1f + stretch),
--- before/Assets/Scripts/RopeLandingFluff.cs
+++ isolated/Assets/Scripts/RopeLandingFluff.cs
@@ -63,6 +63,8 @@

             bool spikeContact = contactPoint.HasValue;
             int count = spikeContact ? 4 : walking ? 2 : strongLanding ? 5 : 3;
+            if (PlayerPresentationProof.Candidate && !takeoff && !walking && !spikeContact)
+                count = strongLanding ? 6 : 4;
             GameObject effect = new GameObject(spikeContact ? "Spike Contact Yarn Fibres" : walking ? "Footstep Yarn Fibres" :
                 takeoff ? "Takeoff Yarn Fluff" : "Landing Yarn Fluff");
             effect.transform.SetParent(feet.transform, false);
@@ -83,6 +85,11 @@
             main.startSizeY = walking ? new ParticleSystem.MinMaxCurve(.018f, .026f) :
                 new ParticleSystem.MinMaxCurve(0.022f, 0.035f);
             main.startSizeZ = 1f;
+            if (PlayerPresentationProof.Candidate && !takeoff && !walking && !spikeContact)
+            {
+                main.startSizeX = new ParticleSystem.MinMaxCurve(.12f,.18f);
+                main.startSizeY = new ParticleSystem.MinMaxCurve(.028f,.04f);
+            }
             main.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
             main.startColor = Color.white;
             main.gravityModifier = walking ? .035f : 0.055f;
--- before/Assets/Scripts/RopeSwingPoseState.cs
+++ isolated/Assets/Scripts/RopeSwingPoseState.cs
@@ -35,7 +35,7 @@
             // reverses local left/right, so mirror the speed, not the frame clock.
             float localSpeed = facingLeft ? -tangentialSpeed : tangentialSpeed;
             float target = NeutralPose - Math.Max(-1f, Math.Min(1f,
-                localSpeed / FullTrailSpeed)) * NeutralPose;
+                localSpeed / (PlayerPresentationProof.Candidate ? 12f : FullTrailSpeed))) * NeutralPose;
             float blend = (float)(1.0 - Math.Exp(-deltaTime / ResponseSeconds));
             pose += (target - pose) * blend;
             frameElapsed += deltaTime;
--- /dev/null
+++ isolated/Assets/PlayerPresentation/PlayerMotionPresentation.cs
@@ -0,0 +1,126 @@
+// Isolated presentation candidate only. No player/root/collider/joint/velocity writes.
+using System;
+using System.Collections.Generic;
+using HimoHitoA2Proof;
+using UnityEngine;
+using UnityEngine.SceneManagement;
+
+namespace HimoHito
+{
+    public static class PlayerPresentationProof
+    {
+        public static bool Candidate => A2Fixture.Arg("-presentation", "baseline") == "candidate";
+        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
+        static void Bootstrap()
+        {
+            Time.captureFramerate=30; // Identical offline capture clock for A/B. fixedDeltaTime unchanged.
+            SceneManager.sceneLoaded+=(scene,mode)=>
+            {
+                if(scene.name=="MainStage"&&!UnityEngine.Object.FindFirstObjectByType<PresentationCapture>())
+                    new GameObject("ISOLATED Player Presentation Comparison").AddComponent<PresentationCapture>();
+            };
+        }
+    }
+
+    [DefaultExecutionOrder(-40)] // After body pose (-50), before rope draw (0).
+    public sealed class PlayerMotionPresentation : MonoBehaviour
+    {
+        readonly List<SpriteRenderer> renderers = new List<SpriteRenderer>();
+        readonly HashSet<Sprite> meshed = new HashSet<Sprite>();
+        MaterialPropertyBlock properties;
+        RopeBodyVisual visual; RopeController rope; PlayerMover mover; Rigidbody2D body;
+        Material yarnMaterial;
+        Vector2 previousPosition, delayedVelocity;
+        float fallSpeed, landingAge = 9f, impact, trail;
+        bool wasGrounded, wasAttached;
+        public float Trail => trail;
+        public float Impact => landingAge < .3f ? impact : 0f;
+        public float Brace { get; private set; }
+        public int LandingEvents { get; private set; }
+        public int ReleaseEvents { get; private set; }
+
+        void Start()
+        {
+            visual=GetComponent<RopeBodyVisual>(); rope=GetComponent<RopeController>();
+            mover=GetComponent<PlayerMover>(); body=GetComponent<Rigidbody2D>();
+            properties=new MaterialPropertyBlock();
+            yarnMaterial=new Material(Resources.Load<Shader>("PlayerPresentationYarn"));
+            yarnMaterial.name="Proof wool fibres and room light";
+            previousPosition=body.position; wasGrounded=mover.IsGrounded;
+        }
+
+        void LateUpdate()
+        {
+            if(!visual || !body || !body.simulated || !mover.enabled) return;
+            float dt=Time.deltaTime;
+            if(dt<=0f) return; // No unscaled pose clock while paused.
+            bool grounded=mover.IsGrounded, attached=rope.IsAttached;
+            if(Vector2.Distance(previousPosition,body.position)>3f)
+            {landingAge=9; fallSpeed=0; impact=0; trail=0; delayedVelocity=body.linearVelocity; wasGrounded=grounded; wasAttached=attached;}
+            previousPosition=body.position;
+            Vector2 velocity=body.linearVelocity;
+            if(!grounded) fallSpeed=Mathf.Max(fallSpeed,-velocity.y);
+            if(grounded&&!wasGrounded)
+            {impact=Mathf.InverseLerp(1.5f,10f,fallSpeed); landingAge=0; LandingEvents++; fallSpeed=0;}
+            if(wasAttached&&!attached&&!grounded) ReleaseEvents++;
+            delayedVelocity=Vector2.Lerp(delayedVelocity,velocity,1f-Mathf.Exp(-dt/.11f));
+            float speed=velocity.magnitude;
+            // Velocity/acceleration response, never a periodic limb clock.
+            float targetTrail=attached ? Mathf.Clamp(-velocity.x/12f*.11f+(delayedVelocity.x-velocity.x)*.012f,-.14f,.14f)
+                : !grounded ? Mathf.Clamp(-velocity.x/12f*.065f,-.08f,.08f) : 0f;
+            trail=Mathf.Lerp(trail,targetTrail,1f-Mathf.Exp(-dt/.09f));
+            Brace=!attached&&!grounded&&velocity.y<-.4f ? Mathf.Clamp01(-velocity.y/9f) : 0f;
+            float compression=grounded&&landingAge<.28f ? Mathf.Sin(Mathf.Clamp01(landingAge/.28f)*Mathf.PI)*impact*.055f : 0f;
+            if(grounded&&Mathf.Abs(mover.MovementInput)>.05f) compression*=.35f;
+            visual.CollectCharacterRenderers(renderers);
+            // Resource-scaled drawings share the actual sole line. Display parts only.
+            // Preserve the separate approved rope-bridge bend offset.
+            if(grounded&&!attached&&mover.SupportedRopePlatform==null)
+            {
+                bool found=false;float soleY=float.PositiveInfinity;
+                foreach(var part in renderers)if(part&&part.enabled&&part.sprite)
+                {soleY=Mathf.Min(soleY,PlayerSoleRegistration.WorldPoint(part).y);found=true;}
+                var collider=GetComponent<Collider2D>();
+                if(found&&collider)
+                {
+                    float correction=collider.bounds.min.y-soleY;
+                    if(Mathf.Abs(correction)<.45f)
+                        foreach(var part in renderers)if(part&&part.enabled)part.transform.position+=Vector3.up*correction;
+                }
+            }
+            foreach(var renderer in renderers)
+            {
+                if(!renderer || !renderer.sprite) continue;
+                if(renderer.sharedMaterial && renderer.sharedMaterial.HasProperty("_WeaveProgress"))
+                    continue; // Preserve existing respawn/unravel reveal material.
+                Sprite sprite=renderer.sprite;
+                if(meshed.Add(sprite)) AddPresentationMesh(sprite);
+                renderer.sharedMaterial=yarnMaterial;
+                Bounds bounds=sprite.bounds;
+                renderer.GetPropertyBlock(properties);
+                properties.SetVector("_SpriteBounds",new Vector4(bounds.min.x,bounds.min.y,bounds.size.x,bounds.size.y));
+                properties.SetVector("_Motion",new Vector4(renderer.flipX?-trail:trail,Brace,compression,attached?Mathf.Clamp01(speed/13f):0));
+                renderer.SetPropertyBlock(properties);
+            }
+            renderers.Clear();
+            landingAge+=dt;wasGrounded=grounded;wasAttached=attached;
+        }
+
+        static void AddPresentationMesh(Sprite sprite)
+        {
+            // Dense render-only grid gives hands/feet a lag. Never OverridePhysicsShape.
+            const int columns=12, rows=24;
+            // OverrideGeometry accepts Sprite Rect PIXELS, unlike Sprite.vertices.
+            Vector2 min=Vector2.one*.05f;
+            Vector2 max=sprite.rect.size-Vector2.one*.05f;
+            var vertices=new Vector2[(columns+1)*(rows+1)];
+            var triangles=new ushort[columns*rows*6];int index=0;
+            for(int y=0;y<=rows;y++)for(int x=0;x<=columns;x++)
+                vertices[y*(columns+1)+x]=new Vector2(Mathf.Lerp(min.x,max.x,x/(float)columns),Mathf.Lerp(min.y,max.y,y/(float)rows));
+            for(int y=0;y<rows;y++)for(int x=0;x<columns;x++)
+            {int v=y*(columns+1)+x; triangles[index++]=(ushort)v;triangles[index++]=(ushort)(v+columns+1);triangles[index++]=(ushort)(v+1);triangles[index++]=(ushort)(v+1);triangles[index++]=(ushort)(v+columns+1);triangles[index++]=(ushort)(v+columns+2);}
+            sprite.OverrideGeometry(vertices,triangles);
+        }
+        void OnDestroy(){if(yarnMaterial)Destroy(yarnMaterial);}
+    }
+}
--- /dev/null
+++ isolated/Assets/PlayerPresentation/PlayerMotionPresentation.cs.meta
@@ -0,0 +1,11 @@
+fileFormatVersion: 2
+guid: f96d98ccd16a435c9c3145b7c6ccc529
+MonoImporter:
+  externalObjects: {}
+  serializedVersion: 2
+  defaultReferences: []
+  executionOrder: 0
+  icon: {instanceID: 0}
+  userData:
+  assetBundleName:
+  assetBundleVariant:
--- /dev/null
+++ isolated/Assets/PlayerPresentation/PlayerSoleRegistration.cs
@@ -0,0 +1,48 @@
+using System.Collections.Generic;
+using UnityEngine;
+
+namespace HimoHito
+{
+    // Read-only artwork landmarks: transparent sprite padding is not a foot.
+    public static class PlayerSoleRegistration
+    {
+        static readonly Dictionary<Sprite,Vector2> soles=new Dictionary<Sprite,Vector2>();
+        static readonly Dictionary<Texture2D,Color32[]> textures=new Dictionary<Texture2D,Color32[]>();
+        public static Vector3 WorldPoint(SpriteRenderer renderer)
+        {
+            Sprite sprite=renderer.sprite;
+            if(!soles.TryGetValue(sprite,out Vector2 local))
+            {
+                Texture2D texture=sprite.texture;
+                if(!textures.TryGetValue(texture,out Color32[] pixels))
+                {
+                    if(texture.isReadable)pixels=texture.GetPixels32();
+                    else
+                    {
+                        var old=RenderTexture.active;var copy=RenderTexture.GetTemporary(texture.width,texture.height,0,RenderTextureFormat.ARGB32);
+                        Graphics.Blit(texture,copy);RenderTexture.active=copy;
+                        var readable=new Texture2D(texture.width,texture.height,TextureFormat.RGBA32,false);
+                        readable.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);readable.Apply();pixels=readable.GetPixels32();
+                        RenderTexture.active=old;RenderTexture.ReleaseTemporary(copy);Object.Destroy(readable);
+                    }
+                    textures[texture]=pixels;
+                }
+                Rect rect=sprite.rect;float xsum=0;int count=0;int bottom=-1;
+                for(int y=Mathf.RoundToInt(rect.yMin);y<Mathf.RoundToInt(rect.yMax)&&count==0;y++)
+                {
+                    for(int x=Mathf.RoundToInt(rect.xMin);x<Mathf.RoundToInt(rect.xMax);x++)
+                    {
+                        Color32 pixel=pixels[y*texture.width+x];
+                        int low=Mathf.Min(pixel.r,Mathf.Min(pixel.g,pixel.b)),high=Mathf.Max(pixel.r,Mathf.Max(pixel.g,pixel.b));
+                        if(pixel.a<128||(low>=205&&high-low<=28))continue;
+                        xsum+=x+.5f;count++;bottom=y;
+                    }
+                }
+                local=count>0?(new Vector2(xsum/count-rect.x,bottom+.5f-rect.y)-sprite.pivot)/sprite.pixelsPerUnit:new Vector2(sprite.bounds.center.x,sprite.bounds.min.y);
+                soles[sprite]=local;
+            }
+            if(renderer.flipX)local.x=-local.x;if(renderer.flipY)local.y=-local.y;
+            return renderer.transform.TransformPoint(local);
+        }
+    }
+}
--- /dev/null
+++ isolated/Assets/PlayerPresentation/PlayerSoleRegistration.cs.meta
@@ -0,0 +1,11 @@
+fileFormatVersion: 2
+guid: f604e451584c433fba6e283ae02a7e52
+MonoImporter:
+  externalObjects: {}
+  serializedVersion: 2
+  defaultReferences: []
+  executionOrder: 0
+  icon: {instanceID: 0}
+  userData:
+  assetBundleName:
+  assetBundleVariant:
--- /dev/null
+++ isolated/Assets/PlayerPresentation/PresentationCapture.cs
@@ -0,0 +1,187 @@
+// Diagnostic capture uses existing input fields/APIs; NOT Human keyboard evidence.
+using System;
+using System.Collections;
+using System.Collections.Generic;
+using System.IO;
+using System.Reflection;
+using HimoHitoA2Proof;
+using UnityEngine;
+
+namespace HimoHito
+{
+    [DefaultExecutionOrder(20000)]
+    public sealed class PresentationCapture : MonoBehaviour
+    {
+        A2Fixture f; A2InputDriver input; RopeBodyVisual bodyVisual;
+        PlayerMotionPresentation presentation;
+        string output, phase="initial";
+        bool recording;
+        float startTime;
+        int frame;
+        readonly List<Sample> samples=new List<Sample>();
+        readonly List<Check> checks=new List<Check>();
+        readonly List<Event> events=new List<Event>();
+        static PresentationCapture active;
+        void Awake(){active=this;}
+        public static void AudioMarker(string name)
+        {if(active&&active.recording)active.events.Add(new Event("audio_"+name,Time.time-active.startTime,active.frame));}
+        [Serializable] public class Check{public string name,status,detail;public Check(string n,bool ok,string d){name=n;status=ok?"PASS":"FAIL";detail=d;}}
+        [Serializable] public class Event{public string name;public float time;public int frame;public Event(string n,float t,int i){name=n;time=t;frame=i;}}
+        [Serializable] public class Result{public string kind="Player texture/swing/landing presentation Proof",variant,method="Scripted original input fields/API and actual Unity Physics2D. No Human keys. Offline 30fps capture clock.",status;public List<Check> checks;public List<Event> events;public List<Sample> samples;public bool finished;}
+        [Serializable] public class Sample
+        {
+            public int frame;public float time,fixedTime,resource,selected,trail,brace,impact,ropeOriginError;
+            public string phase,sprite;public Vector2 position,velocity,visualFoot,ropeOrigin;
+            public Vector3 camera,visualPosition,visualScale;public float ortho;
+            public bool grounded,attached,flip,shadowVisible,shadowEnabled;public Vector3 shadowPosition;public int particles,visibleParticleRenderers,bridges;
+            public float lowestVisibleFootDelta;public string material;
+        }
+        void CheckResult(string name,bool ok,string detail="")
+        {checks.Add(new Check(name,ok,detail));Debug.Log("PRESENTATION_QA "+name+" "+(ok?"PASS":"FAIL")+" "+detail);}
+        void Mark(string name){events.Add(new Event(name,Time.time-startTime,frame));phase=name;}
+        IEnumerator Delay(float seconds){yield return new WaitForSeconds(seconds);}
+        IEnumerator Start()
+        {
+            while(!(f=A2Fixture.Instance)||!f.Ready)yield return null;
+            output=A2Fixture.Arg("-presentationOutput",f.Output+"/Presentation");
+            Directory.CreateDirectory(output);
+            bodyVisual=f.Body.GetComponent<RopeBodyVisual>();
+            if(PlayerPresentationProof.Candidate) presentation=f.Body.gameObject.AddComponent<PlayerMotionPresentation>();
+            yield return Delay(.5f);
+            input=f.gameObject.AddComponent<A2InputDriver>();
+            f.Write("PresentationConfig.json",new A2Fixture.Config(f));
+            startTime=Time.time;
+            bool smoke=A2Fixture.Arg("-presentationQA","flow")=="smoke";
+            if(A2Fixture.Arg("-presentationQA")=="regression")
+            {yield return MergeRegression();Save();yield return Delay(.1f);A2Fixture.Quit(checks.TrueForAll(c=>c.status=="PASS")?0:2);yield break;}
+            if(smoke)
+            {
+                CheckResult("Shader supported",Resources.Load<Shader>("PlayerPresentationYarn")?.isSupported==true);
+                CheckResult("Initial grounded",f.Mover.IsGrounded);
+                CaptureStill("Smoke");
+                CheckResult("No Gameplay error",f.Errors==0);
+                Save();yield return Delay(.1f);A2Fixture.Quit(checks.TrueForAll(c=>c.status=="PASS")?0:2);yield break;
+            }
+            recording=true;StartCoroutine(RecordFrames());
+            Mark("standing");yield return Delay(.65f);
+            Mark("attach");f.Rope.RestoreSelectedRopeLength(6);
+            CheckResult("Attach original API",f.Rope.TryAttach(GameObject.Find(A2Fixture.SwingNames[0]).transform.position));
+            input.Move=1;
+            float deadline=Time.time+12f;
+            bool seenPositive=false, seenRightApex=false, seenLeftApex=false;
+            while(Time.time<deadline&&!f.Respawn.IsFailureVisible)
+            {
+                Vector2 v=f.Body.linearVelocity;
+                if(v.x>2)seenPositive=true;
+                if(seenPositive&&!seenRightApex&&v.x<0){seenRightApex=true;Mark("attached_right_apex");}
+                if(seenRightApex&&!seenLeftApex&&v.x>0){seenLeftApex=true;Mark("attached_left_apex");}
+                input.Move=v.x<-.05f?-1:1;
+                Vector2 delta=f.Body.position-f.Rope.AnchorPoint;
+                float angle=Mathf.Atan2(delta.x,-delta.y)*Mathf.Rad2Deg;
+                if(seenLeftApex&&angle>=36&&v.x>0&&v.y>0)break;
+                if(!seenRightApex&&angle>-10)phase="accelerating_bottom";
+                yield return new WaitForFixedUpdate();
+            }
+            CheckResult("Attached apex reached",seenRightApex&&seenLeftApex);
+            Vector2 releaseBefore=f.Body.linearVelocity;
+            Mark("release");f.Rope.DetachAndRefund(true);
+            CheckResult("Detach momentum preserved",Vector2.Distance(releaseBefore,f.Body.linearVelocity)<.00001f);
+            input.Move=1;phase="free_flight";
+            deadline=Time.time+5;
+            while(Time.time<deadline&&!f.Respawn.IsFailureVisible&&!f.OnBank(f.Middle))yield return new WaitForFixedUpdate();
+            bool landed=f.OnBank(f.Middle);Mark("land");input.Move=0;
+            CheckResult("Same terrain landing",landed,"API trajectory; not fun/forgiveness claim");
+            yield return Delay(.38f);
+            Mark("resume_walk_left");input.Move=-1;yield return Delay(.20f);
+            Mark("walk_right");input.Move=1;yield return Delay(.10f);input.Move=0;
+            yield return Delay(.25f);Mark("sequence_end");
+            if(Application.isEditor)yield return null;else yield return new WaitForEndOfFrame();recording=false;
+            CheckResult("Resource unchanged by swing",Mathf.Approximately(f.Resource.CurrentLength,24)&&f.Builder.GeneratedPlatformCount==0);
+            CheckResult("Input/physics fixture invariant",f.IsolationValid&&JsonUtility.ToJson(new A2Fixture.Layout(f))==f.ExpectedLayout);
+            // Existing R restores the exact entry, rather than changing the level.
+            f.Retry();yield return Delay(.95f);Mark("after_R");CaptureStill("AfterR");
+            CheckResult("R entry/budget/bridge state",Vector2.Distance(f.Body.position,f.Spawn)<.2f&&f.Resource.CurrentLength==24&&f.Rope.SelectedRopeLength==6&&f.Builder.GeneratedPlatformCount==0&&!f.Rope.IsAttached);
+            var playerRenderers=new List<SpriteRenderer>();bodyVisual.CollectCharacterRenderers(playerRenderers);
+            var renderer=playerRenderers[0];
+            Vector3 pos=renderer.transform.position,scale=renderer.transform.localScale;Sprite sprite=renderer.sprite;
+            // Exercise the same scaled-time pause contract, no input-system edits.
+            Time.timeScale=0f;yield return new WaitForSecondsRealtime(.25f);
+            CheckResult("Pause freezes displayed body",Vector3.Distance(pos,renderer.transform.position)<.00001f&&Vector3.Distance(scale,renderer.transform.localScale)<.00001f&&sprite==renderer.sprite);
+            CaptureStill("Paused");Time.timeScale=1f;yield return Delay(.1f);
+            Mark("bridge_build");f.Rope.RestoreSelectedRopeLength(11);
+            bool attached=f.Rope.TryAttach(GameObject.Find(A2Fixture.EndNames[0]).transform.position);
+            bool built=attached&&f.Builder.TryBuildCurrentPlatform();yield return Delay(.15f);CaptureStill("BridgeWeave");
+            CheckResult("Bridge build / weave visual retained",built&&f.Builder.GeneratedPlatformCount==1&&f.Resource.CurrentLength==13);
+            CheckResult("No Gameplay error",f.Errors==0);
+            Save();yield return Delay(.2f);A2Fixture.Quit(checks.TrueForAll(c=>c.status=="PASS")?0:2);
+        }
+        IEnumerator RecordFrames()
+        {
+            while(recording)
+            {
+                if(Application.isEditor)yield return null;else yield return new WaitForEndOfFrame();
+                if(!recording)break;
+                samples.Add(Observe());
+                if(!Application.isEditor)
+                {
+                    var image=ScreenCapture.CaptureScreenshotAsTexture();
+                    File.WriteAllBytes(Path.Combine(output,"Frame_"+frame.ToString("D4")+".png"),image.EncodeToPNG());
+                    Destroy(image);
+                }
+                frame++;
+            }
+        }
+        Sample Observe()
+        {
+            var renderers=new List<SpriteRenderer>();bodyVisual.CollectCharacterRenderers(renderers);
+            SpriteRenderer r=renderers[0];
+            Bounds character=r.bounds;
+            Vector3 sole=PlayerSoleRegistration.WorldPoint(r);
+            foreach(var part in renderers)if(part&&part.enabled&&part.sprite)character.Encapsulate(part.bounds);
+            foreach(var part in renderers)if(part&&part.enabled&&part.sprite)
+            {Vector3 point=PlayerSoleRegistration.WorldPoint(part);if(point.y<sole.y)sole=point;}
+            var ps=f.Body.GetComponentsInChildren<ParticleSystem>();int particles=0,visible=0;
+            foreach(var p in ps){particles+=p.particleCount;if(p.particleCount>0&&p.GetComponent<ParticleSystemRenderer>().isVisible)visible++;}
+            var shadowOwner=f.Body.GetComponent<PlayerContactShadow>();
+            // DontSave renderers are intentionally excluded from FindObjectsByType.
+            SpriteRenderer shadow=shadowOwner?(SpriteRenderer)typeof(PlayerContactShadow).GetField("shadow",A2Fixture.Private).GetValue(shadowOwner):null;
+            float err=0;var line=f.Body.GetComponent<LineRenderer>();
+            if(f.Rope.IsAttached&&line.positionCount>0)err=Vector2.Distance(line.GetPosition(0),bodyVisual.RopeOrigin);
+            return new Sample{frame=frame,time=Time.time-startTime,fixedTime=Time.fixedTime,phase=phase,position=f.Body.position,velocity=f.Body.linearVelocity,grounded=f.Mover.IsGrounded,attached=f.Rope.IsAttached,resource=f.Resource.CurrentLength,selected=f.Rope.SelectedRopeLength,bridges=f.Builder.GeneratedPlatformCount,sprite=r.sprite.name,flip=r.flipX,camera=Camera.main.transform.position,ortho=Camera.main.orthographicSize,visualPosition=r.transform.position,visualScale=r.transform.lossyScale,visualFoot=sole,lowestVisibleFootDelta=sole.y-f.Body.GetComponent<Collider2D>().bounds.min.y,ropeOrigin=bodyVisual.RopeOrigin,ropeOriginError=err,shadowVisible=shadow&&shadow.enabled&&shadow.isVisible,shadowEnabled=shadow&&shadow.enabled,shadowPosition=shadow?shadow.transform.position:Vector3.zero,particles=particles,visibleParticleRenderers=visible,trail=presentation?presentation.Trail:0,brace=presentation?presentation.Brace:0,impact=presentation?presentation.Impact:0,material=r.sharedMaterial?r.sharedMaterial.name:""};
+        }
+        void CaptureStill(string name)
+        {
+            f.Capture("Presentation_"+name);
+            f.Write("Presentation_"+name+".json",Observe());
+        }
+        void Save()
+        {
+            File.WriteAllText(Path.Combine(output,"Result.json"),JsonUtility.ToJson(new Result{variant=PlayerPresentationProof.Candidate?"candidate":"baseline",checks=checks,events=events,samples=samples,finished=true,status=checks.TrueForAll(c=>c.status=="PASS")?"PASS":"FAIL"},true));
+        }
+        IEnumerator MergeRegression()
+        {
+            // Diagnostic positioning in the existing S9, not an A2 traversal claim.
+            CheckResult("S9 regression uses original MainStage",f.Variant=="A");
+            f.Rope.DetachAndRefund();input.Move=0;
+            f.Body.position=new Vector2(179.6f,-1.535f);f.Body.transform.position=f.Body.position;
+            f.Body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();
+            f.Respawn.TryReachSection(9,f.Body.position,0);f.Resource.RestoreCapacityAndCurrent(50,50);
+            yield return Delay(.3f);
+            f.Rope.RestoreSelectedRopeLength(6);
+            bool left=f.Rope.TryAttach(MainStageSectionNineSetup.CenterHookPosition)&&f.Builder.TryBuildCurrentPlatform();
+            yield return Delay(.25f);CaptureStill("Merge_FirstBridge");
+            f.Body.position=new Vector2(191.4f,-1.535f);f.Body.transform.position=f.Body.position;
+            f.Body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();yield return Delay(.3f);
+            f.Rope.RestoreSelectedRopeLength(6);
+            bool right=f.Rope.TryAttach(MainStageSectionNineSetup.CenterHookPosition)&&f.Builder.TryBuildCurrentPlatform();
+            yield return Delay(.3f);
+            CheckResult("Original S9 two bridges",left&&right&&f.Builder.GeneratedPlatformCount==2);
+            typeof(RopeController).GetField("keyboardAimDirection",A2Fixture.Private).SetValue(f.Rope,(MainStageSectionNineSetup.CenterHookPosition-f.Body.position).normalized);
+            float before=f.Resource.CurrentLength;bool merged=f.Builder.TryRemoveAimedHook();
+            yield return Delay(.15f);CaptureStill("Merge_Animation");
+            CheckResult("Original F merge / resource / center state",merged&&f.Builder.GeneratedPlatformCount==1&&f.Resource.CurrentLength==before&&!GameObject.Find(MainStageSectionNineSetup.CenterHookName));
+            yield return Delay(.55f);CaptureStill("Merge_Complete");
+            CheckResult("No Gameplay error",f.Errors==0);
+        }
+    }
+}
--- /dev/null
+++ isolated/Assets/PlayerPresentation/PresentationCapture.cs.meta
@@ -0,0 +1,11 @@
+fileFormatVersion: 2
+guid: 4c9fa5c69a5f4045871b02c56687a018
+MonoImporter:
+  externalObjects: {}
+  serializedVersion: 2
+  defaultReferences: []
+  executionOrder: 0
+  icon: {instanceID: 0}
+  userData:
+  assetBundleName:
+  assetBundleVariant:
--- /dev/null
+++ isolated/Assets/Resources/PlayerPresentationYarn.shader
@@ -0,0 +1,71 @@
+Shader "HimoHitoProof/PlayerPresentationYarn"
+{
+    Properties
+    {
+        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
+        _Color ("Tint", Color) = (1,1,1,1)
+        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
+        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
+        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
+        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
+        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
+    }
+    SubShader
+    {
+        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
+        Cull Off Lighting Off ZWrite Off
+        Blend One OneMinusSrcAlpha
+        Pass
+        {
+            CGPROGRAM
+            #pragma vertex ProofVertex
+            #pragma fragment ProofFragment
+            #pragma target 3.0
+            #pragma multi_compile_instancing
+            #pragma multi_compile _ PIXELSNAP_ON
+            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
+            #include "UnitySprites.cginc"
+            float4 _SpriteBounds, _Motion;
+            struct ProofOutput {float4 vertex:SV_POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0;float2 local:TEXCOORD1;};
+            ProofOutput ProofVertex(appdata_t input)
+            {
+                UNITY_SETUP_INSTANCE_ID(input);
+                ProofOutput output;
+                float2 normalized=(input.vertex.xy-_SpriteBounds.xy)/max(_SpriteBounds.zw,float2(.001,.001));
+                float y=saturate(normalized.y);
+                float lower=pow(1-y,2);
+                float side=(normalized.x-.5)*2;
+                // Crown stays fixed: every deformation is zero at y=1.
+                input.vertex.x += _Motion.x*_SpriteBounds.w*lower*(.5+.5*abs(side));
+                input.vertex.x += side*_Motion.w*.025*_SpriteBounds.w*sin(y*3.14159)*lower;
+                input.vertex.x += side*_Motion.z*_SpriteBounds.z*sin(y*3.14159);
+                // Airborne bracing lifts loose ends a little, not the physics root.
+                input.vertex.y += _Motion.y*.035*_SpriteBounds.w*lower;
+                output.vertex=UnityObjectToClipPos(UnityFlipSprite(input.vertex,_Flip));
+                #ifdef PIXELSNAP_ON
+                output.vertex=UnityPixelSnap(output.vertex);
+                #endif
+                output.uv=input.texcoord; output.local=normalized;
+                output.color=input.color*_Color*_RendererColor;
+                return output;
+            }
+            fixed4 ProofFragment(ProofOutput input):SV_Target
+            {
+                fixed4 source=SampleSpriteTexture(input.uv);
+                float luminance=dot(source.rgb,float3(.2126,.7152,.0722));
+                float pink=saturate((source.r-source.g)*3.5);
+                // Selective wool-tone remap: retain eye blacks and bright fibre ridges.
+                float3 wool=lerp(luminance.xxx,source.rgb,.76);
+                wool=pow(max(wool,.001),float3(.98,.94,.94));
+                float ridge=saturate((luminance-.19)*1.5);
+                wool+=float3(.025,.012,.005)*ridge;
+                float roomLight=.94+.09*saturate(input.local.y)+.035*(1-saturate(input.local.x));
+                float3 rgb=lerp(source.rgb,wool*roomLight,pink);
+                fixed4 color=fixed4(saturate(rgb),source.a)*input.color;
+                color.rgb*=color.a;
+                return color;
+            }
+            ENDCG
+        }
+    }
+}
--- /dev/null
+++ isolated/Assets/Resources/PlayerPresentationYarn.shader.meta
@@ -0,0 +1,9 @@
+fileFormatVersion: 2
+guid: 2500f0ffd6424e28837018102c29a840
+ShaderImporter:
+  externalObjects: {}
+  defaultTextures: []
+  nonModifiableTextures: []
+  userData:
+  assetBundleName:
+  assetBundleVariant:
--- /dev/null
+++ isolated/Assets/PlayerPresentation.meta
@@ -0,0 +1,8 @@
+fileFormatVersion: 2
+guid: 75bc164a5d194312adaabeaa34fb8652
+folderAsset: yes
+DefaultImporter:
+  externalObjects: {}
+  userData:
+  assetBundleName:
+  assetBundleVariant:
```
