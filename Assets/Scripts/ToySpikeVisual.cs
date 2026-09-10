using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Draws a red felt triangle with a wooden rim to match the handcrafted
    /// toy room. The painted wooden sprite remains a missing-asset fallback.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ToySpikeVisual : MonoBehaviour
    {
        private const string FaceName = "Wooden Toy Spike Face";
        private const string CraftSpikeResourcePath = "Art/HimoHitoCraftSpike-v1";
        private const string PaintedSpikeResourcePath = "Art/WoodenToySpike-v1";
        private const int TextureSize = 64;
        private static Sprite sharedSprite;
        private const float ContactDuration = .40f;
        private Transform face;
        private float contactElapsed = ContactDuration;
        private float contactDirection = 1f;

        public bool Refresh()
        {
            bool changed = false;
            SpriteRenderer sourceRenderer = GetComponent<SpriteRenderer>();
            if (sourceRenderer.enabled)
            {
                sourceRenderer.enabled = false;
                changed = true;
            }

            face = transform.Find(FaceName);
            if (face == null)
            {
                GameObject faceObject = new GameObject(FaceName);
                face = faceObject.transform;
                face.SetParent(transform, false);
                changed = true;
            }

            if (face.localPosition != Vector3.zero)
            {
                face.localPosition = Vector3.zero;
                changed = true;
            }
            if (face.localRotation != Quaternion.identity)
            {
                face.localRotation = Quaternion.identity;
                changed = true;
            }
            Sprite targetSprite = GetSharedSprite();
            // Fit the new artwork inside the same local hazard bounds; never resize the collider.
            Vector3 visualScale = new Vector3(1f / targetSprite.bounds.size.x,
                1f / targetSprite.bounds.size.y, 1f);
            if (face.localScale != visualScale)
            {
                face.localScale = visualScale;
                changed = true;
            }

            if (!face.TryGetComponent(out SpriteRenderer faceRenderer))
            {
                faceRenderer = face.gameObject.AddComponent<SpriteRenderer>();
                changed = true;
            }

            if (faceRenderer.sprite != targetSprite)
            {
                faceRenderer.sprite = targetSprite;
                changed = true;
            }
            if (faceRenderer.color != Color.white)
            {
                faceRenderer.color = Color.white;
                changed = true;
            }
            if (faceRenderer.sortingLayerID != sourceRenderer.sortingLayerID)
            {
                faceRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
                changed = true;
            }
            if (faceRenderer.sortingOrder != sourceRenderer.sortingOrder + 2)
            {
                faceRenderer.sortingOrder = sourceRenderer.sortingOrder + 2;
                changed = true;
            }
            ApplyContactPose();
            return changed;
        }

        public void PlayContact(Vector2 contactPoint, Vector2 velocity)
        {
            if (!isActiveAndEnabled || Time.timeScale <= 0f) return;
            if (face == null) Refresh();
            contactDirection = Mathf.Abs(velocity.x) > .05f ? -Mathf.Sign(velocity.x) :
                (contactPoint.x < transform.position.x ? -1f : 1f);
            contactElapsed = 0f;
            ApplyContactPose();
        }

        private void LateUpdate()
        {
            if (Application.isPlaying) AdvanceContact(Time.deltaTime);
        }

        private void AdvanceContact(float dt)
        {
            if (dt <= 0f || contactElapsed >= ContactDuration) return;
            contactElapsed = Mathf.Min(ContactDuration, contactElapsed + dt);
            ApplyContactPose();
        }

        private void ApplyContactPose()
        {
            if (face == null) return;
            float t = Mathf.Clamp01(contactElapsed / ContactDuration);
            float angle = t >= 1f ? 0f : contactDirection * 6f * Mathf.Sin(t * Mathf.PI * 2f) * (1f - t);
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
            // Rock the painted face about its bottom, never the hazard collider/root.
            Vector3 pivot = new Vector3(0f, -.5f, 0f);
            face.localRotation = rotation;
            face.localPosition = pivot - rotation * pivot;
        }

        private void OnDisable()
        {
            contactElapsed = ContactDuration;
            ApplyContactPose();
        }

        private void OnEnable()
        {
            Refresh();
        }

        private static Sprite GetSharedSprite()
        {
            Sprite craftSprite = TutorialFirstSectionVisuals.LoadProcessedToySprite(
                CraftSpikeResourcePath, useMipMaps: true);
            if (craftSprite != null) return craftSprite;

            Sprite paintedSprite = TutorialFirstSectionVisuals.LoadProcessedToySprite(
                PaintedSpikeResourcePath);
            if (paintedSprite != null) return paintedSprite;

            // Retain a visible hazard if the art has not finished importing.
            if (sharedSprite != null)
            {
                return sharedSprite;
            }

            Color clear = new Color(0f, 0f, 0f, 0f);
            Color litWood = new Color(0.95f, 0.36f, 0.29f, 1f);
            Color warmWood = new Color(0.78f, 0.18f, 0.20f, 1f);
            Color shadedWood = new Color(0.49f, 0.10f, 0.16f, 1f);
            Color rim = new Color(1f, 0.58f, 0.38f, 1f);
            Color[] pixels = new Color[TextureSize * TextureSize];

            for (int y = 0; y < TextureSize; y++)
            {
                float vertical = (y + 0.5f) / TextureSize;
                float halfWidth = 0.47f * (1f - vertical);
                for (int x = 0; x < TextureSize; x++)
                {
                    float horizontal = (x + 0.5f) / TextureSize - 0.5f;
                    Color color = clear;
                    if (vertical >= 0.04f &&
                        Mathf.Abs(horizontal) <= halfWidth)
                    {
                        float edgeDistance = halfWidth - Mathf.Abs(horizontal);
                        if (edgeDistance < 0.025f || vertical < 0.075f)
                        {
                            color = shadedWood;
                        }
                        else if (horizontal < -0.08f)
                        {
                            color = litWood;
                        }
                        else if (horizontal > 0.18f)
                        {
                            color = shadedWood;
                        }
                        else
                        {
                            color = warmWood;
                        }

                        if (horizontal < -0.13f &&
                            edgeDistance > 0.055f &&
                            vertical > 0.18f &&
                            vertical < 0.82f)
                        {
                            color = Color.Lerp(color, rim, 0.55f);
                        }
                    }
                    pixels[y * TextureSize + x] = color;
                }
            }

            Texture2D texture = new Texture2D(
                TextureSize,
                TextureSize,
                TextureFormat.RGBA32,
                false)
            {
                name = "WoodenToySpikeTexture",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixels(pixels);
            texture.Apply();

            sharedSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, TextureSize, TextureSize),
                new Vector2(0.5f, 0.5f),
                TextureSize);
            sharedSprite.name = "WoodenToySpike";
            sharedSprite.hideFlags = HideFlags.HideAndDontSave;
            return sharedSprite;
        }
    }
}
