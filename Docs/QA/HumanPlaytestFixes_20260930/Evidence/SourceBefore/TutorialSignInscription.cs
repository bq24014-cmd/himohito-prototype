using UnityEngine;

namespace HimoHito
{
    /// <summary>Small painted control diagrams on the existing wooden sign, not a second UI.</summary>
    [ExecuteAlways, DisallowMultipleComponent]
    public sealed class TutorialSignInscription : MonoBehaviour
    {
        [SerializeField, Range(1, 4)] private int section = 1;
        private Transform drawing;
        private Material paint, yarn;
        private int drawnSection;
        private float worldWidth;
        private int sortingLayer, sortingOrder;
        private float animationTime;
        private SpriteRenderer picturePlayer;
        private float pictureHeight;
        private LineRenderer[] firstRope, secondRope, mergedRope, middleRing, connectionRope;
        private static readonly Color Cream = new Color(1f, .91f, .68f);
        private static readonly Color Ink = new Color(.27f, .10f, .055f, .85f);
        private static readonly Color Blue = new Color(.20f, .71f, .94f);
        private static readonly Color Green = new Color(.30f, .93f, .69f);

        public bool Configure(int value)
        {
            value = Mathf.Clamp(value, 1, 4);
            bool changed = section != value || drawing == null;
            section = value;
            if (changed) Rebuild();
            return changed;
        }

        private void OnEnable() => Rebuild();
        private void Update()
        {
            if (drawing == null || drawnSection != section) Rebuild();
            if (Application.isPlaying) AdvanceAnimation(Time.deltaTime);
        }

        private void Rebuild()
        {
            Clear();
            if (!TryGetComponent(out SpriteRenderer board) || board.sprite == null) return;
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) return;
            paint = new Material(shader) { name = "Sign Painted Marks", hideFlags = HideFlags.HideAndDontSave };
            yarn = new Material(shader) { name = "Sign Yarn Diagram", hideFlags = HideFlags.HideAndDontSave };
            yarn.mainTexture = YarnRopeTexture.Load();
            Bounds bounds = board.sprite.bounds;
            worldWidth = bounds.size.x * Mathf.Abs(transform.lossyScale.x);
            sortingLayer = board.sortingLayerID;
            sortingOrder = board.sortingOrder + 1;
            drawing = new GameObject("Painted Tutorial Controls") { hideFlags = HideFlags.HideAndDontSave }.transform;
            drawing.SetParent(transform, false);
            drawing.localPosition = new Vector3(bounds.min.x, bounds.min.y, -.01f);
            drawing.localScale = new Vector3(bounds.size.x, bounds.size.y, 1f);
            drawnSection = section;

            // Coordinates are normalized to the original square artwork, inside the board face.
            switch (section)
            {
                case 1:
                    Vector2 swingHead = PlayerPicture(new Vector2(.255f, .54f), .125f);
                    firstRope=Curve(swingHead, new Vector2(.34f, .72f), new Vector2(.42f, .80f), true);
                    Ring(new Vector2(.42f, .80f), Blue);
                    Arrow(new Vector2(.27f, .50f), new Vector2(.51f, .50f));
                    Key("E", new Vector2(.72f, .69f), .15f, .23f);
                    break;
                case 2:
                    LengthChoicePicture();
                    Key("W", new Vector2(.72f, .76f), .11f, .22f);
                    Key("S", new Vector2(.72f, .61f), .11f, .22f);
                    break;
                case 3:
                    Shelf(.20f, .76f, -.055f);
                    Shelf(.55f, .76f, .055f);
                    firstRope=Curve(new Vector2(.20f, .76f), new Vector2(.375f, .48f), new Vector2(.55f, .76f), true);
                    Ring(new Vector2(.20f, .76f), Green);
                    Ring(new Vector2(.55f, .76f), Green);
                    PlayerPicture(new Vector2(.375f, .625f), .125f);
                    Key("Q", new Vector2(.72f, .69f), .15f, .23f);
                    break;
                case 4:
                    firstRope=Curve(new Vector2(.20f, .76f), new Vector2(.27f, .65f), new Vector2(.375f, .81f), false);
                    secondRope=Curve(new Vector2(.375f, .81f), new Vector2(.48f, .65f), new Vector2(.55f, .76f), false);
                    mergedRope=Curve(new Vector2(.20f, .76f), new Vector2(.375f, .40f), new Vector2(.55f, .76f), true);
                    middleRing=Ring(new Vector2(.375f, .81f), Blue);
                    Ring(new Vector2(.20f, .76f), Green);
                    Ring(new Vector2(.55f, .76f), Green);
                    Arrow(new Vector2(.375f, .72f), new Vector2(.375f, .62f));
                    PlayerPicture(new Vector2(.46f, .625f), .115f);
                    Key("F", new Vector2(.72f, .69f), .15f, .23f);
                    break;
            }
            if(section>=3)
            {
                connectionRope=Curve(Vector2.zero,Vector2.zero,Vector2.zero,true);
                SetRopeAppearance(connectionRope,0f);
            }
            if(Application.isPlaying) AdvanceAnimation(0f);
        }

        private void AdvanceAnimation(float delta)
        {
            if(drawing==null || picturePlayer==null || delta<0f) return;
            // The world signs use scaled time; unlike the open explanation they pause with gameplay.
            if(delta>0f) animationTime=Mathf.Repeat(animationTime+Mathf.Min(delta,.1f),
                TutorialGuideAnimation.Duration(section));
            var frame=TutorialGuideAnimation.Sample(section,animationTime);
            float opacity=frame.Opacity;
            Vector2 feet;
            if(section<=2)
            {
                float along=Mathf.Clamp01((frame.Feet.x-.29f)/.42f);
                Vector2 a=section==1 ? new Vector2(.20f,.66f) : new Vector2(.235f,.69f);
                Vector2 b=section==1 ? new Vector2(.375f,.38f) : new Vector2(.395f,frame.Danger ? .37f : .58f);
                Vector2 c=section==1 ? new Vector2(.55f,.66f) : new Vector2(.56f,.69f);
                feet=TutorialGuideAnimation.Curve(a,b,c,along);
                Vector2 hook=section==1 ? new Vector2(.42f,.80f) : new Vector2(.395f,.838f);
                Vector2 head=PictureCrown(feet);
                AnimateCurve(firstRope,head,Vector2.Lerp(head,hook,.5f),hook,
                    frame.Connection,opacity*frame.Connection);
            }
            else if(section==3)
            {
                Vector2 a=new Vector2(.20f,.76f), b=new Vector2(.375f,.48f), c=new Vector2(.55f,.76f);
                float along=(frame.Feet.x-.30f)/.42f;
                feet=along<0f || along>1f ? new Vector2(Mathf.Clamp(.20f+along*.35f,.17f,.60f),.76f) :
                    TutorialGuideAnimation.Curve(a,b,c,along);
                AnimateCurve(firstRope,a,b,c,frame.Bridge,opacity*(frame.Bridge>0f ? 1f : 0f));
                Vector2 head=PictureCrown(feet);
                AnimateCurve(connectionRope,head,Vector2.Lerp(head,c,.5f),c,
                    frame.Connection,opacity*frame.Connection);
            }
            else
            {
                Vector2 a=new Vector2(.20f,.76f), center=new Vector2(.375f,.81f), c=new Vector2(.55f,.76f);
                AnimateCurve(firstRope,a,new Vector2(.27f,.65f),center,frame.LeftBridge,
                    frame.Merge>0f ? 0f : opacity*frame.LeftBridge);
                AnimateCurve(secondRope,center,new Vector2(.48f,.65f),c,frame.RightBridge,
                    frame.Merge>0f ? 0f : opacity*frame.RightBridge);
                for(int i=0;i<25;i++)
                {
                    Vector2 point=SmallMergedPoint(i/24f,frame.Merge);
                    foreach(var line in mergedRope) line.SetPosition(i,point);
                }
                SetRopeAppearance(mergedRope,frame.Merge>0f ? opacity : 0f);
                float along=(frame.Feet.x-.24f)/.52f;
                feet=along<0f || along>1f ? new Vector2(Mathf.Clamp(.20f+along*.35f,.155f,.59f),.76f) :
                    SmallMergedPoint(along,frame.Merge);
                Vector2 head=PictureCrown(feet);
                Vector2 target=frame.ConnectionTarget.x<.6f ? center : c;
                AnimateCurve(connectionRope,head,Vector2.Lerp(head,target,.5f),target,
                    frame.Connection,opacity*frame.Connection);
                for(int i=0;i<middleRing.Length;i++)
                {
                    Color color=i==0 ? Ink : Blue;
                    color.a*=1f-frame.Merge;
                    middleRing[i].startColor=middleRing[i].endColor=color;
                }
            }
            SetPictureFeet(feet);
            picturePlayer.color=new Color(1f,1f,1f,opacity);
        }

        private static Vector2 SmallMergedPoint(float along,float merge)
        {
            Vector2 a=new Vector2(.20f,.76f), center=new Vector2(.375f,.81f), c=new Vector2(.55f,.76f);
            Vector2 pair=along<=.5f ? TutorialGuideAnimation.Curve(a,new Vector2(.27f,.65f),center,along*2f) :
                TutorialGuideAnimation.Curve(center,new Vector2(.48f,.65f),c,(along-.5f)*2f);
            return Vector2.Lerp(pair,TutorialGuideAnimation.Curve(a,new Vector2(.375f,.40f),c,along),merge);
        }

        private void AnimateCurve(LineRenderer[] lines,Vector2 a,Vector2 b,Vector2 c,float progress,float alpha)
        {
            if(lines==null) return;
            for(int i=0;i<25;i++)
            {
                Vector2 point=TutorialGuideAnimation.Curve(a,b,c,i/24f*progress);
                foreach(var line in lines) line.SetPosition(i,point);
            }
            SetRopeAppearance(lines,alpha);
        }

        private void SetRopeAppearance(LineRenderer[] lines,float alpha)
        {
            if(lines==null) return;
            for(int i=0;i<lines.Length;i++)
            {
                Color color=i==0 ? Ink : Color.white;
                color.a*=alpha;
                lines[i].startColor=lines[i].endColor=color;
                lines[i].enabled=alpha>.001f;
                if(i==1 && yarn.mainTexture!=null && lines[i].sharedMaterial!=yarn)
                {
                    lines[i].sharedMaterial=yarn;
                    Texture texture=yarn.mainTexture;
                    float width=lines[i].startWidth/Mathf.Max(.001f,worldWidth);
                    lines[i].textureScale=new Vector2(1f/Mathf.Max(.001f,width*texture.width/texture.height),1f);
                }
            }
        }

        private void LengthChoicePicture()
        {
            Color danger = new Color(1f, .29f, .34f);
            // Feet trajectories, not two pre-built bridges. The longer swing reaches the spikes.
            Stroke(new[] { new Vector3(.15f, .69f), new Vector3(.235f, .69f),
                new Vector3(.235f, .515f) }, .013f, Cream, false, "Left Bank");
            Stroke(new[] { new Vector3(.56f, .515f), new Vector3(.56f, .69f),
                new Vector3(.615f, .69f) }, .013f, Cream, false, "Right Bank");
            DashedPath(.58f, Green, "Safe Swing Path");
            DashedPath(.37f, danger, "Too Long Swing Path");
            for (int i = 0; i < 3; i++)
            {
                float x = .345f + i * .045f;
                Stroke(new[] { new Vector3(x - .018f, .49f), new Vector3(x, .55f),
                    new Vector3(x + .018f, .49f), new Vector3(x - .018f, .49f) },
                    .011f, danger, false, "Valley Spike");
            }
            Stroke(new[] { new Vector3(.555f, .748f), new Vector3(.570f, .731f),
                new Vector3(.600f, .769f) }, .010f, Green, false, "Safe Check");
            Stroke(new[] { new Vector3(.50f, .545f), new Vector3(.526f, .515f) },
                .010f, danger, false, "Too Long Cross");
            Stroke(new[] { new Vector3(.50f, .515f), new Vector3(.526f, .545f) },
                .010f, danger, false, "Too Long Cross");
            Vector2 head = PlayerPicture(new Vector2(.395f, .635f), .115f);
            firstRope=Curve(head, Vector2.Lerp(head, new Vector2(.395f, .838f), .5f),
                new Vector2(.395f, .838f), true);
            Ring(new Vector2(.395f, .838f), Blue);
        }

        private void DashedPath(float controlY, Color color, string label)
        {
            Vector2 a = new Vector2(.235f, .69f), b = new Vector2(.395f, controlY), c = new Vector2(.56f, .69f);
            for (int i = 0; i < 18; i += 2)
            {
                float t0 = i / 18f, t1 = (i + 1f) / 18f;
                Vector2 p0 = (1f - t0) * (1f - t0) * a + 2f * (1f - t0) * t0 * b + t0 * t0 * c;
                Vector2 p1 = (1f - t1) * (1f - t1) * a + 2f * (1f - t1) * t1 * b + t1 * t1 * c;
                Stroke(new Vector3[] { p0, p1 }, .006f, color, false, label);
            }
        }

        private Vector2 PlayerPicture(Vector2 feet, float height)
        {
            Sprite sprite = CraftPlayerArt.LoadStandingSprite("Art/HimoHitoPlayer-v1");
            if (sprite == null) return feet + Vector2.up * height;
            var child = new GameObject("Small Player Picture") { hideFlags = HideFlags.HideAndDontSave };
            child.transform.SetParent(drawing, false);
            var renderer = child.AddComponent<SpriteRenderer>();
            picturePlayer=renderer;
            pictureHeight=height;
            renderer.sprite = sprite;
            renderer.sharedMaterial = paint;
            renderer.sortingLayerID = sortingLayer;
            renderer.sortingOrder = sortingOrder + 3;
            SetPictureFeet(feet);
            return PictureCrown(feet);
        }

        private void SetPictureFeet(Vector2 feet)
        {
            Bounds bounds = picturePlayer.sprite.bounds;
            float scale = pictureHeight / Mathf.Max(.001f, bounds.size.y);
            // Keep the existing diagram height and path, while cancelling any stretch
            // inherited from the board's normalized drawing coordinates.
            Vector3 parentScale = drawing.lossyScale;
            float aspectCorrection = Mathf.Abs(parentScale.y) / Mathf.Max(.001f, Mathf.Abs(parentScale.x));
            Vector3 localScale = new Vector3(scale * aspectCorrection, scale, scale);
            picturePlayer.transform.localScale = localScale;
            picturePlayer.transform.localPosition = (Vector3)feet -
                Vector3.Scale(new Vector3(bounds.center.x, bounds.min.y, 0f), localScale);
        }

        private Vector2 PictureCrown(Vector2 feet)
        {
            if (picturePlayer == null ||
                !CraftPlayerArt.TryGetStandingCrownPoint(picturePlayer.sprite, out Vector2 crown))
                return feet + Vector2.up * (pictureHeight * .94f);
            Bounds bounds = picturePlayer.sprite.bounds;
            return feet + Vector2.Scale(crown - new Vector2(bounds.center.x, bounds.min.y),
                picturePlayer.transform.localScale);
        }

        private LineRenderer[] Curve(Vector2 a, Vector2 b, Vector2 c, bool active)
        {
            var points = new Vector3[25];
            for (int i = 0; i < points.Length; i++)
            {
                float t = i / (points.Length - 1f);
                points[i] = (1f - t) * (1f - t) * a + 2f * (1f - t) * t * b + t * t * c;
            }
            return Stroke(points, active ? .018f : .013f,
                active ? Color.white : new Color(.62f, .47f, .61f), active);
        }

        private LineRenderer[] Ring(Vector2 center, Color color)
        {
            var points = new Vector3[33];
            for (int i = 0; i < points.Length; i++)
            {
                float angle = i / (points.Length - 1f) * Mathf.PI * 2f;
                points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * .029f;
            }
            return Stroke(points, .014f, color, false);
        }

        private void Shelf(float x, float y, float direction)
        {
            Stroke(new[] { new Vector3(x + direction, y), new Vector3(x, y) }, .017f, Cream, false);
        }

        private void Arrow(Vector2 from, Vector2 to)
        {
            Vector2 direction = (to - from).normalized;
            Vector2 cross = new Vector2(-direction.y, direction.x);
            Stroke(new Vector3[] { from, to, to - direction * .028f + cross * .022f,
                to, to - direction * .028f - cross * .022f }, .009f, Cream, false);
        }

        private LineRenderer[] Stroke(Vector3[] points, float width, Color color, bool textured, string label = null)
        {
            var outline=MakeLine(label != null ? label + " Outline" : "Paint Outline", points, width + .006f, Ink, paint, sortingOrder);
            var line=MakeLine(label ?? (textured ? "Yarn Picture" : "Paint Picture"), points, width, color,
                textured && yarn.mainTexture != null ? yarn : paint, sortingOrder + 1);
            return new[] { outline, line };
        }

        private LineRenderer MakeLine(string label, Vector3[] points, float width, Color color, Material material, int order)
        {
            var child = new GameObject(label) { hideFlags = HideFlags.HideAndDontSave };
            child.transform.SetParent(drawing, false);
            var line = child.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.sharedMaterial = material;
            line.positionCount = points.Length;
            line.SetPositions(points);
            line.startWidth = line.endWidth = width * worldWidth;
            line.startColor = line.endColor = color;
            line.numCapVertices = line.numCornerVertices = 3;
            line.sortingLayerID = sortingLayer;
            line.sortingOrder = order;
            line.textureMode = LineTextureMode.Tile;
            if (material.mainTexture != null)
            {
                // Local vertices are in sprite-normalized units; keep yarn twists small and consistent.
                Texture texture = material.mainTexture;
                line.textureScale = new Vector2(1f / Mathf.Max(.001f, width * texture.width / texture.height), 1f);
            }
            return line;
        }

        private void Key(string value, Vector2 center, float height, float maxWidth)
        {
            Font font = Resources.Load<Font>("Fonts/MPlusRounded1c-Bold");
            if (font == null) return;
            var child = new GameObject("Operation Key " + value) { hideFlags = HideFlags.HideAndDontSave };
            child.transform.SetParent(drawing, false);
            var text = child.AddComponent<TextMesh>();
            text.font = font;
            text.fontSize = 64;
            text.characterSize = 1f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = Cream;
            text.text = value;
            var renderer = child.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = font.material;
            renderer.sortingLayerID = sortingLayer;
            renderer.sortingOrder = sortingOrder + 2;
            Bounds glyph = renderer.localBounds;
            float scale = Mathf.Min(height / Mathf.Max(.001f, glyph.size.y), maxWidth / Mathf.Max(.001f, glyph.size.x));
            child.transform.localScale = Vector3.one * scale;
            child.transform.localPosition = (Vector3)center - glyph.center * scale;
        }

        private void Clear()
        {
            animationTime=0f;
            picturePlayer=null;
            firstRope=secondRope=mergedRope=middleRing=connectionRope=null;
            if (drawing != null)
            {
                drawing.gameObject.SetActive(false);
                Dispose(drawing.gameObject);
                drawing = null;
            }
            Dispose(paint); Dispose(yarn);
            paint = yarn = null;
        }

        private static void Dispose(Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) Destroy(value);
            else DestroyImmediate(value);
        }

        private void OnDisable() => Clear();
        private void OnDestroy() => Clear();
    }
}
