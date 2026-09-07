using UnityEngine;

namespace HimoHito
{
    /// <summary>Small, non-colliding yarn fragments emitted only after a successful Q build.</summary>
    public sealed class RopeBuildFluff : MonoBehaviour
    {
        private Material runtimeMaterial;

        public static void Play(LineRenderer bridge)
        {
            if (bridge == null || bridge.positionCount < 2) return;
            Texture2D yarn = YarnRopeTexture.Load();
            Shader shader = Shader.Find("Sprites/Default");
            if (yarn == null || shader == null) return;

            // Parenting ensures checkpoint restoration or bridge removal also clears the effect.
            GameObject effect = new GameObject("Rope Build Yarn Fluff");
            effect.transform.SetParent(bridge.transform, false);
            RopeBuildFluff owner = effect.AddComponent<RopeBuildFluff>();
            ParticleSystem particles = effect.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = particles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 1f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.65f, 0.9f);
            main.startSpeed = 0f;
            main.startSize3D = true;
            main.startSizeX = new ParticleSystem.MinMaxCurve(0.09f, 0.14f);
            main.startSizeY = new ParticleSystem.MinMaxCurve(0.025f, 0.045f);
            main.startSizeZ = 1f;
            main.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
            main.startColor = Color.white;
            main.gravityModifier = 0.035f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.useUnscaledTime = false;
            main.maxParticles = 12;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = particles.emission;
            emission.enabled = false;
            var shape = particles.shape;
            shape.enabled = false;
            var collision = particles.collision;
            collision.enabled = false;
            var rotation = particles.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-1.4f, 1.4f);
            var fade = particles.colorOverLifetime;
            fade.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0.85f, 0f), new GradientAlphaKey(0.7f, 0.35f), new GradientAlphaKey(0f, 1f) });
            fade.color = gradient;

            owner.runtimeMaterial = new Material(shader)
            {
                name = "Rope Build Fluff Material",
                hideFlags = HideFlags.HideAndDontSave,
                mainTexture = yarn
            };
            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = owner.runtimeMaterial;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingLayerID = bridge.sortingLayerID;
            renderer.sortingOrder = bridge.sortingOrder + 1;
            renderer.minParticleSize = 0f;

            float span = Vector3.Distance(bridge.GetPosition(0), bridge.GetPosition(bridge.positionCount - 1));
            int count = Mathf.Clamp(Mathf.CeilToInt(span * 0.8f), 6, 12);
            particles.Play();
            for (int i = 0; i < count; i++)
            {
                float sample = (i + 0.5f) / count * (bridge.positionCount - 1);
                int index = Mathf.FloorToInt(sample);
                Vector3 point = Vector3.Lerp(bridge.GetPosition(index), bridge.GetPosition(index + 1), sample - index);
                if (!bridge.useWorldSpace) point = bridge.transform.TransformPoint(point);
                particles.Emit(new ParticleSystem.EmitParams
                {
                    position = point + Vector3.up * bridge.startWidth * 0.4f,
                    velocity = new Vector3((i % 2 == 0 ? -1f : 1f) * 0.16f, 0.3f + (i % 3) * 0.08f, 0f)
                }, 1);
            }
        }

        private void OnDestroy()
        {
            if (runtimeMaterial != null) Destroy(runtimeMaterial);
        }
    }
}
