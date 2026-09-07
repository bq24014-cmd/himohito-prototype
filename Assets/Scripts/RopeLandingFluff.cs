using UnityEngine;

namespace HimoHito
{
    /// <summary>Short, non-colliding yarn flecks at the player's landing point.</summary>
    public sealed class RopeLandingFluff : MonoBehaviour
    {
        private Material runtimeMaterial;

        public static void Play(Collider2D feet)
        {
            if (feet == null) return;
            Texture2D yarn = YarnRopeTexture.Load();
            Shader shader = Shader.Find("Sprites/Default");
            if (yarn == null || shader == null) return;

            GameObject effect = new GameObject("Landing Yarn Fluff");
            effect.transform.SetParent(feet.transform, false);
            RopeLandingFluff owner = effect.AddComponent<RopeLandingFluff>();
            ParticleSystem particles = effect.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.7f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.6f);
            main.startSpeed = 0f;
            main.startSize3D = true;
            main.startSizeX = new ParticleSystem.MinMaxCurve(0.07f, 0.11f);
            main.startSizeY = new ParticleSystem.MinMaxCurve(0.02f, 0.035f);
            main.startSizeZ = 1f;
            main.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
            main.startColor = Color.white;
            main.gravityModifier = 0.03f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Shape;
            main.useUnscaledTime = false;
            main.maxParticles = 5;
            main.stopAction = ParticleSystemStopAction.Destroy;
            var emission = particles.emission;
            emission.enabled = false;
            var shape = particles.shape;
            shape.enabled = false;
            var collision = particles.collision;
            collision.enabled = false;
            var rotation = particles.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-1.2f, 1.2f);
            var fade = particles.colorOverLifetime;
            fade.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0.75f, 0f), new GradientAlphaKey(0.5f, 0.3f), new GradientAlphaKey(0f, 1f) });
            fade.color = gradient;
            owner.runtimeMaterial = new Material(shader)
            {
                name = "Landing Yarn Fluff Material",
                hideFlags = HideFlags.HideAndDontSave,
                mainTexture = yarn
            };
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = owner.runtimeMaterial;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.minParticleSize = 0f;
            if (feet.TryGetComponent(out SpriteRenderer playerRenderer))
            {
                renderer.sortingLayerID = playerRenderer.sortingLayerID;
                renderer.sortingOrder = playerRenderer.sortingOrder + 1;
            }
            Bounds bounds = feet.bounds;
            particles.Play();
            for (int i = 0; i < 5; i++)
            {
                float side = (i - 2f) / 2f;
                particles.Emit(new ParticleSystem.EmitParams
                {
                    position = new Vector3(bounds.center.x + side * bounds.extents.x * 0.75f,
                        bounds.min.y + 0.04f, feet.transform.position.z),
                    velocity = new Vector3(side * 0.35f, 0.28f + (i % 3) * 0.07f, 0f)
                }, 1);
            }
        }

        private void OnDestroy()
        {
            if (runtimeMaterial != null) Destroy(runtimeMaterial);
        }
    }
}
