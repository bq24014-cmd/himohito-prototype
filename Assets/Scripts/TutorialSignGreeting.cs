using UnityEngine;

namespace HimoHito
{
    /// <summary>A one-shot greeting on the artwork only; the readable sign root stays fixed.</summary>
    [DisallowMultipleComponent]
    public sealed class TutorialSignGreeting : MonoBehaviour
    {
        private const float Duration = .85f;
        private SpriteRenderer artwork;
        private Vector3 restPosition, pivot;
        private Quaternion restRotation;
        private Material originalMaterial, tailMaterial;
        private bool initialized, consumed, playing;
        private float elapsed, direction;

        public void Tick(Vector2 playerPosition, Vector2 signPosition, float readRange, bool permitted, float dt)
        {
            if (!isActiveAndEnabled || dt <= 0f) return;
            if (!Initialize()) return;
            float distance = Vector2.Distance(playerPosition, signPosition);
            if (distance > readRange + .85f) consumed = false;
            if (!permitted) { ResetPose(); return; }
            if (!consumed && !playing && distance <= readRange)
            {
                consumed = playing = true;
                elapsed = 0f;
                direction = playerPosition.x < signPosition.x ? 1f : -1f;
                if (tailMaterial != null) artwork.sharedMaterial = tailMaterial;
            }
            if (!playing) return;
            elapsed += dt;
            if (elapsed >= Duration) { ResetPose(); return; }
            Draw(elapsed / Duration);
        }

        private bool Initialize()
        {
            if (initialized) return artwork != null;
            if (GetComponent<Collider2D>() != null || GetComponent<Rigidbody2D>() != null ||
                GetComponent<HookPoint>() != null || !TryGetComponent(out artwork) || artwork.sprite == null) return false;
            restPosition = transform.localPosition;
            restRotation = transform.localRotation;
            Bounds bounds = artwork.sprite.bounds;
            // The sprite has transparent padding below the visible wooden post.
            Vector3 bottom = new Vector3(bounds.center.x, bounds.min.y + bounds.size.y * .065f, 0f);
            pivot = restPosition + restRotation * Vector3.Scale(bottom, transform.localScale);
            originalMaterial = artwork.sharedMaterial;
            Shader shader = Resources.Load<Shader>("TutorialSignGreeting");
            if (shader != null && shader.isSupported)
                tailMaterial = new Material(shader) { name = "Guide Sign Yarn Sway", hideFlags = HideFlags.HideAndDontSave };
            initialized = true;
            return true;
        }

        private void Draw(float progress)
        {
            float envelope = (1f - progress) * (1f - progress);
            float angle = direction * 4f * Mathf.Sin(progress * Mathf.PI * 2f) * envelope;
            Quaternion turn = Quaternion.Euler(0, 0, angle);
            transform.localRotation = turn * restRotation;
            transform.localPosition = pivot + turn * (restPosition - pivot);
            if (tailMaterial != null)
            {
                float delayed = Mathf.Max(0f, progress - .08f);
                float sway = direction * .012f * Mathf.Sin(delayed * Mathf.PI * 3f) *
                    Mathf.Sin(progress * Mathf.PI) * (1f - progress);
                tailMaterial.SetFloat("_TailSway", sway);
            }
        }

        private void ResetPose()
        {
            playing = false;
            elapsed = 0f;
            if (!initialized) return;
            transform.localPosition = restPosition;
            transform.localRotation = restRotation;
            if (tailMaterial != null) tailMaterial.SetFloat("_TailSway", 0f);
            if (artwork != null && artwork.sharedMaterial == tailMaterial)
                artwork.sharedMaterial = originalMaterial;
        }

        public void Cancel() => ResetPose();
        private void OnDisable() => ResetPose();
        private void OnDestroy()
        {
            ResetPose();
            if (tailMaterial == null) return;
            if (Application.isPlaying) Destroy(tailMaterial);
            else DestroyImmediate(tailMaterial);
        }
    }
}
