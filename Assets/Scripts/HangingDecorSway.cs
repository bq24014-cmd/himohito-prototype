using System;
using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Local texture-coordinate movement for decorations baked into the
    /// adopted room illustration. No scene transforms or physics are animated.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class HangingDecorSway : MonoBehaviour
    {
        private const string SupportedSprite = "TutorialNightChildRoom-v1";
        private static readonly int StarAngles = Shader.PropertyToID("_StarAngles");
        private static readonly int FlagAngles = Shader.PropertyToID("_FlagAngles");
        private static readonly int LastFlagAngle = Shader.PropertyToID("_LastFlagAngle");
        private SpriteRenderer source;
        private Material originalMaterial;
        private Material swayMaterial;
        private double elapsed;

        public static void Ensure(SpriteRenderer renderer)
        {
            if (renderer == null || renderer.sprite == null ||
                renderer.sprite.name != SupportedSprite ||
                renderer.TryGetComponent(out HangingDecorSway _)) return;
            renderer.gameObject.AddComponent<HangingDecorSway>();
        }

        private void OnEnable()
        {
            if (swayMaterial != null) return;
            source = GetComponent<SpriteRenderer>();
            if (source.sprite == null || source.sprite.name != SupportedSprite) return;
            Shader shader = Resources.Load<Shader>("HangingDecorSway");
            // Missing/unsupported visual resources must never hide the room.
            if (shader == null || !shader.isSupported) return;
            originalMaterial = source.sharedMaterial;
            swayMaterial = new Material(shader)
            {
                name = "Room Decorations (Runtime)",
                hideFlags = HideFlags.HideAndDontSave
            };
            source.sharedMaterial = swayMaterial;
            ApplyAngles();
        }

        private void Update() => Advance(Time.deltaTime);

        private void Advance(float deltaTime)
        {
            if (deltaTime <= 0f || swayMaterial == null || source == null ||
                !source.enabled || source.forceRenderingOff) return;
            elapsed += deltaTime;
            ApplyAngles();
        }

        private float Angle(double period, double phase, float amplitude)
        {
            float fade = Mathf.SmoothStep(0f, 1f, (float)Math.Min(1d, elapsed / 1.2d));
            return (float)Math.Sin((elapsed % period) * Math.PI * 2d / period + phase)
                * amplitude * fade;
        }

        private void ApplyAngles()
        {
            swayMaterial.SetVector(StarAngles, new Vector4(
                Angle(4.7, 0.2, .128f), Angle(5.5, 2.1, .112f),
                Angle(4.3, 4.3, .120f), 0f));
            swayMaterial.SetVector(FlagAngles, new Vector4(
                Angle(4.6, 0.7, .120f), Angle(5.2, 2.4, .112f),
                Angle(5.8, 4.1, .115f), Angle(4.4, 1.5, .108f)));
            swayMaterial.SetFloat(LastFlagAngle, Angle(5.4, 3.5, .112f));
        }

        private void OnDisable() => ReleaseMaterial();
        private void OnDestroy() => ReleaseMaterial();

        private void ReleaseMaterial()
        {
            if (swayMaterial == null) return;
            if (source != null && source.sharedMaterial == swayMaterial)
                source.sharedMaterial = originalMaterial;
            if (Application.isPlaying) Destroy(swayMaterial);
            else DestroyImmediate(swayMaterial);
            swayMaterial = null;
        }
    }
}
