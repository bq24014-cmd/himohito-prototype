using UnityEngine;

namespace HimoHito
{
    /// <summary>Shared, purely visual chest animation. Goal zones own completion.</summary>
    [ExecuteAlways]
    public sealed class GoalChestPresentation : MonoBehaviour
    {
        private const float OpeningDuration = 0.45f;
        private const float OpenDistance = 2.6f;
        private const float CloseDistance = 3.6f;
        private static Sprite[] frames;
        private static Sprite glowSprite;
        private SpriteRenderer chest;
        private SpriteRenderer glow;
        private Transform floor;
        private Rigidbody2D visitor;
        private float elapsed;
        private bool opening;

        public static bool Ensure(GameObject marker, GameObject goalFloor)
        {
            if (marker == null || goalFloor == null) return false;
            bool added = !marker.TryGetComponent(out GoalChestPresentation view);
            if (added) view = marker.AddComponent<GoalChestPresentation>();
            view.floor = goalFloor.transform;
            view.Refresh();
            return added;
        }

        private void OnEnable() { Refresh(); }

        private void Update()
        {
            if (!Application.isPlaying) { Refresh(); return; }
            if (visitor != null)
            {
                // Failure resets immediately; walking away reverses the animation silently.
                bool reset = false;
                if (visitor != null && visitor.TryGetComponent(out PrototypeRunController run))
                    reset |= run.Outcome == PrototypeRunController.RunOutcome.Failed ||
                             run.Outcome == PrototypeRunController.RunOutcome.WaitingToStart;
                if (visitor != null && visitor.TryGetComponent(out MainStageRespawnOnFall mainRun))
                    reset |= mainRun.IsFailureVisible;
                if (reset) { opening = false; elapsed = 0f; visitor = null; }
                else if (Mathf.Abs(visitor.position.x - transform.position.x) > CloseDistance ||
                         Mathf.Abs(visitor.position.y - transform.position.y) > CloseDistance)
                    opening = false;
            }
            else opening = false;
            elapsed = Mathf.MoveTowards(elapsed, opening ? OpeningDuration : 0f, Time.deltaTime);
            if (!opening && elapsed <= 0f) visitor = null;
            Refresh();
        }

        public bool Approach(Rigidbody2D player)
        {
            float distance = Mathf.Abs(player.position.x - transform.position.x);
            if (!opening && distance <= OpenDistance)
            {
                visitor = player;
                opening = true;
                // Reapproaching during closing reverses from the current frame, without a snap.
                player.GetComponent<PrototypeAudioFeedback>()?.PlayGoalChestOpened();
            }
            return opening && elapsed >= OpeningDuration && distance <= 1.05f;
        }

        public static bool ReadyToClear(Collision2D collision, Collider2D surface,
            string markerName)
        {
            Rigidbody2D player = collision.rigidbody;
            if (player == null || !player.TryGetComponent(out RopeResource _)) return false;
            float top = surface.bounds.max.y;
            Collider2D body = player.GetComponent<Collider2D>();
            if (player.worldCenterOfMass.y <= top ||
                (body != null && body.bounds.min.y < top - 0.12f)) return false;
            bool onTop = false;
            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint2D point = collision.GetContact(i);
                onTop |= point.point.y >= top - 0.12f && Mathf.Abs(point.normal.y) >= 0.6f;
            }
            if (!onTop) return false;
            GameObject marker = GameObject.Find(markerName);
            if (marker == null)
            {
                // Legacy/graybox scenes retain the original opening-to-clear sound sequence.
                player.GetComponent<PrototypeAudioFeedback>()?.PlayGoalChestOpened();
                return true;
            }
            Ensure(marker, surface.gameObject);
            return marker.GetComponent<GoalChestPresentation>().Approach(player);
        }

        private void Refresh()
        {
            if (floor == null)
            {
                string floorName = gameObject.name == TutorialSectionFourSetup.GoalMarkerName
                    ? TutorialSectionFourSetup.GoalFloorName : MainStageSectionTenSetup.GoalFloorName;
                GameObject found = GameObject.Find(floorName);
                if (found == null) return;
                floor = found.transform;
            }
            if (!LoadFrames()) return;
            if (TryGetComponent(out SpriteRenderer original)) original.enabled = false;
            Transform old = transform.Find("Open Toy Box Goal Visual");
            if (old != null && old.TryGetComponent(out SpriteRenderer oldRenderer))
                oldRenderer.enabled = false;
            chest = EnsureRenderer(chest, "Animated Goal Chest", 8);
            glow = EnsureRenderer(glow, "Goal Chest Warm Glow", 9);
            float progress = Application.isPlaying ? elapsed / OpeningDuration : 0f;
            int frame = Mathf.Clamp(Mathf.RoundToInt(progress * 3f), 0, 3);
            chest.sprite = frames[frame];
            float width = Mathf.Abs(transform.lossyScale.x);
            float scale = width / chest.sprite.bounds.size.x;
            chest.transform.localScale = new Vector3(scale / Mathf.Abs(transform.lossyScale.x),
                scale / Mathf.Abs(transform.lossyScale.y), 1f);
            float top = floor.GetComponent<Collider2D>().bounds.max.y - 0.16f;
            // Bottom-centred pivots keep all four differently sized frames planted on the wood.
            chest.transform.position = new Vector3(transform.position.x, top, transform.position.z);
            glow.sprite = glowSprite;
            glow.transform.position = new Vector3(transform.position.x, top + width * 0.43f,
                transform.position.z);
            float size = width * 0.9f;
            glow.transform.localScale = new Vector3(size / Mathf.Abs(transform.lossyScale.x),
                size * 0.7f / Mathf.Abs(transform.lossyScale.y), 1f);
            glow.color = new Color(1f, 0.82f, 0.44f, Mathf.SmoothStep(0f, 0.65f, progress));
        }

        private SpriteRenderer EnsureRenderer(SpriteRenderer current, string childName, int order)
        {
            if (current == null)
            {
                Transform child = transform.Find(childName);
                if (child == null)
                {
                    child = new GameObject(childName).transform;
                    child.SetParent(transform, false);
                    child.gameObject.hideFlags = HideFlags.DontSave;
                }
                current = child.GetComponent<SpriteRenderer>();
                if (current == null) current = child.gameObject.AddComponent<SpriteRenderer>();
            }
            current.enabled = true;
            current.sortingOrder = order;
            return current;
        }

        private static bool LoadFrames()
        {
            if (frames != null && frames[0] != null) return true;
            Texture2D source = Resources.Load<Texture2D>("Art/ToyBoxOpening-v1");
            if (source == null) return false;
            frames = new Sprite[4];
            int w = source.width / 2, h = source.height / 2;
            for (int frame = 0; frame < 4; frame++)
            {
                Color[] pixels = source.GetPixels((frame % 2) * w, (1 - frame / 2) * h, w, h);
                int minX = w, maxX = 0, minY = h, maxY = 0;
                for (int i = 0; i < pixels.Length; i++)
                {
                    Color c = pixels[i];
                    float low = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
                    float high = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
                    // Same neutral-background removal as the existing toy sprites.
                    if (low >= 205f / 255f && high - low <= 28f / 255f) c.a = 0f;
                    pixels[i] = c;
                    if (c.a < 0.05f) continue;
                    minX = Mathf.Min(minX, i % w); maxX = Mathf.Max(maxX, i % w);
                    minY = Mathf.Min(minY, i / w); maxY = Mathf.Max(maxY, i / w);
                }
                Texture2D texture = new Texture2D(w, h, TextureFormat.RGBA32, false);
                texture.hideFlags = HideFlags.HideAndDontSave;
                texture.filterMode = FilterMode.Bilinear;
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.SetPixels(pixels); texture.Apply(false, true);
                frames[frame] = Sprite.Create(texture,
                    new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1),
                    new Vector2(0.5f, 0f), 100f, 0, SpriteMeshType.FullRect);
                frames[frame].hideFlags = HideFlags.HideAndDontSave;
            }
            Texture2D light = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            light.hideFlags = HideFlags.HideAndDontSave;
            light.wrapMode = TextureWrapMode.Clamp;
            Color[] glowPixels = new Color[64 * 64];
            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 64; x++)
                {
                    float falloff = Mathf.Clamp01(1f - Vector2.Distance(
                        new Vector2(x, y), new Vector2(31.5f, 31.5f)) / 31.5f);
                    glowPixels[y * 64 + x] = new Color(1f, 1f, 1f, falloff * falloff);
                }
            light.SetPixels(glowPixels); light.Apply(false, true);
            glowSprite = Sprite.Create(light, new Rect(0, 0, 64, 64), Vector2.one * 0.5f, 64f);
            glowSprite.hideFlags = HideFlags.HideAndDontSave;
            return true;
        }
    }
}
