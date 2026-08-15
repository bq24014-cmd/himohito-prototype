using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Shrinks only the player's visible body as rope is spent.
    /// The player transform, Rigidbody2D, and Collider2D keep their original size.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RopeResource), typeof(SpriteRenderer))]
    public sealed class RopeBodyVisual : MonoBehaviour
    {
        [SerializeField, Range(0.2f, 1f)] private float minimumVisualScale = 0.65f;

        private RopeResource ropeResource;
        private SpriteRenderer sourceRenderer;
        private Transform visualTransform;

        private void Awake()
        {
            ropeResource = GetComponent<RopeResource>();
            sourceRenderer = GetComponent<SpriteRenderer>();
            CreateVisualBody();
            ApplyRemainingLength();
        }

        private void LateUpdate()
        {
            ApplyRemainingLength();
        }

        private void OnDestroy()
        {
            if (sourceRenderer != null)
            {
                sourceRenderer.enabled = true;
            }
        }

        private void CreateVisualBody()
        {
            GameObject visualObject = new GameObject("Rope Body Visual");
            visualObject.layer = gameObject.layer;
            visualTransform = visualObject.transform;
            visualTransform.SetParent(transform, false);

            SpriteRenderer visualRenderer = visualObject.AddComponent<SpriteRenderer>();
            visualRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
            visualRenderer.sortingOrder = sourceRenderer.sortingOrder;
            visualRenderer.maskInteraction = sourceRenderer.maskInteraction;

            SolidSprite solidSprite = visualObject.AddComponent<SolidSprite>();
            solidSprite.Color = sourceRenderer.color;

            sourceRenderer.enabled = false;
        }

        private void ApplyRemainingLength()
        {
            if (ropeResource == null || visualTransform == null)
            {
                return;
            }

            float remainingRatio = Mathf.Clamp01(ropeResource.NormalizedLength);
            float visualScale = Mathf.Lerp(minimumVisualScale, 1f, remainingRatio);
            visualTransform.localScale = new Vector3(visualScale, visualScale, 1f);
        }
    }
}
