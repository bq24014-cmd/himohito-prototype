using UnityEngine;

namespace HimoHito
{
    /// <summary>A soft ellipse on actual wooden support, never a gameplay collider.</summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(50)]
    public sealed class PlayerContactShadow : MonoBehaviour
    {
        private readonly RaycastHit2D[] hits = new RaycastHit2D[16];
        private Collider2D player;
        private SpriteRenderer shadow;
        private Texture2D texture;
        private Sprite sprite;

        private void Awake()
        {
            player = GetComponent<Collider2D>();
            texture = new Texture2D(64,32,TextureFormat.RGBA32,false) { name="Soft Contact Shadow", hideFlags=HideFlags.HideAndDontSave, wrapMode=TextureWrapMode.Clamp };
            var pixels = new Color[64*32];
            for (int y=0;y<32;y++) for(int x=0;x<64;x++)
            {
                float r = Mathf.Sqrt(Mathf.Pow((x+.5f-32f)/32f,2f)+Mathf.Pow((y+.5f-16f)/16f,2f));
                float alpha = Mathf.Pow(Mathf.Clamp01(1f-r*r),2f);
                pixels[y*64+x] = new Color(.12f,.06f,.035f,alpha);
            }
            texture.SetPixels(pixels); texture.Apply(false,true);
            sprite = Sprite.Create(texture,new Rect(0,0,64,32),new Vector2(.5f,.5f),64f);
            sprite.hideFlags = HideFlags.HideAndDontSave;
            var visual = new GameObject("Player Contact Shadow") { hideFlags=HideFlags.DontSave };
            // A scene root avoids scaling/moving the ground shadow with the body animation.
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(visual, gameObject.scene);
            shadow = visual.AddComponent<SpriteRenderer>(); shadow.sprite = sprite; shadow.enabled = false;
        }

        private void LateUpdate()
        {
            if (player == null || shadow == null) return;
            if (!player.enabled || !gameObject.activeInHierarchy) { shadow.enabled=false; return; }
            if (Time.timeScale <= 0f) return;
            UpdateShadow();
        }

        private void UpdateShadow()
        {
            shadow.enabled = false;
            Bounds bounds = player.bounds;
            var filter = new ContactFilter2D();
            filter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer)); filter.useTriggers=false;
            int count = Physics2D.Raycast(new Vector2(bounds.center.x,bounds.min.y+.12f),Vector2.down,filter,hits,2.2f);
            if (count == hits.Length) return;
            for (int i=0;i<count;i++)
            {
                var hit=hits[i]; var floor=hit.collider;
                if (floor==null || floor==player || floor.attachedRigidbody==player.attachedRigidbody ||
                    floor.isTrigger || hit.normal.y < .8f || hit.fraction<=0f || Physics2D.GetIgnoreCollision(player,floor)) continue;
                // The first real support occludes anything below, including rope bridges.
                if (!floor.TryGetComponent(out WoodenPlatformDepthVisual _) ||
                    Mathf.Abs(Mathf.DeltaAngle(floor.transform.eulerAngles.z,0f)) > .1f) return;
                float height=Mathf.Max(0f,bounds.min.y-hit.point.y);
                float fade=1f-Mathf.SmoothStep(0f,1f,height/1.9f);
                if (fade<=.01f) return;
                float width=Mathf.Lerp(.68f,.94f,height/1.9f);
                float left=Mathf.Max(hit.point.x-width*.5f,floor.bounds.min.x+.02f);
                float right=Mathf.Min(hit.point.x+width*.5f,floor.bounds.max.x-.02f);
                if(right<=left) return;
                shadow.transform.position=new Vector3((left+right)*.5f,hit.point.y-WoodenPlatformDepthVisual.SurfaceInset-.10f,0f);
                shadow.transform.localScale=new Vector3(right-left,.28f,1f);
                if(floor.TryGetComponent(out SpriteRenderer source))
                { shadow.sortingLayerID=source.sortingLayerID; shadow.sortingOrder=source.sortingOrder+3; }
                shadow.color=new Color(1f,1f,1f,.32f*fade); shadow.enabled=true;
                return;
            }
        }

        private void OnDisable() { if(shadow!=null) shadow.enabled=false; }
        private void OnDestroy()
        {
            if(shadow!=null) Dispose(shadow.gameObject);
            if(sprite!=null) Dispose(sprite);
            if(texture!=null) Dispose(texture);
        }

        private static void Dispose(Object item)
        { if(Application.isPlaying) Destroy(item); else DestroyImmediate(item); }
    }
}
