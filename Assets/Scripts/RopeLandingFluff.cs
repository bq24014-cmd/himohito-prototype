using UnityEngine;

namespace HimoHito
{
    /// <summary>Short, non-colliding yarn fibres for footsteps, jumps and spike contact.</summary>
    public sealed class RopeLandingFluff : MonoBehaviour
    {
        private Material runtimeMaterial;
        private Collider2D sourceFeet;
        private static readonly RaycastHit2D[] StepHits = new RaycastHit2D[12];

        public static void PlayWoodenStep(Collider2D feet)
        {
            if (feet == null || !feet.enabled || !feet.gameObject.activeInHierarchy ||
                Time.timeScale <= 0f || feet.attachedRigidbody == null ||
                !feet.attachedRigidbody.simulated) return;
            // Read the first actual support, so wood below a bridge/rail is not mistaken for it.
            Bounds bounds = feet.bounds;
            var filter = new ContactFilter2D();
            filter.SetLayerMask(Physics2D.GetLayerCollisionMask(feet.gameObject.layer));
            filter.useTriggers = false;
            int count = Physics2D.Raycast(new Vector2(bounds.center.x, bounds.min.y + .12f),
                Vector2.down, filter, StepHits, .3f);
            if (count == StepHits.Length) return;
            for (int i = 0; i < count; i++)
            {
                Collider2D support = StepHits[i].collider;
                if (support == null || support == feet ||
                    support.attachedRigidbody == feet.attachedRigidbody ||
                    Physics2D.GetIgnoreCollision(feet, support)) continue;
                if (StepHits[i].fraction <= 0f || StepHits[i].normal.y < .8f ||
                    support.GetComponent<GeneratedRopePlatform>() != null ||
                    support.GetComponent<OneWayRailPlatform>() != null ||
                    !support.TryGetComponent(out WoodenPlatformDepthVisual _)) return;
                Emit(feet, false, false, walking: true);
                return;
            }
        }

        public static void Play(Collider2D feet, float impactSpeed = 4f)
        {
            Emit(feet, false, impactSpeed >= 8f);
        }

        public static void PlayTakeoff(Collider2D feet)
        {
            Emit(feet, true, false);
        }

        public static void PlaySpikeContact(Collider2D player, Vector2 point, Vector2 velocity)
        {
            Emit(player, false, false, contactPoint: point, contactVelocity: velocity);
        }

        private static void Emit(Collider2D feet, bool takeoff, bool strongLanding, bool walking = false,
            Vector2? contactPoint = null, Vector2 contactVelocity = default)
        {
            if (feet == null || Time.timeScale <= 0f ||
                (feet.attachedRigidbody != null && !feet.attachedRigidbody.simulated)) return;
            Texture2D yarn = YarnRopeTexture.Load();
            Shader shader = Shader.Find("Sprites/Default");
            if (yarn == null || shader == null) return;

            bool spikeContact = contactPoint.HasValue;
            int count = spikeContact ? 4 : walking ? 2 : strongLanding ? 5 : 3;
            GameObject effect = new GameObject(spikeContact ? "Spike Contact Yarn Fibres" : walking ? "Footstep Yarn Fibres" :
                takeoff ? "Takeoff Yarn Fluff" : "Landing Yarn Fluff");
            effect.transform.SetParent(feet.transform, false);
            RopeLandingFluff owner = effect.AddComponent<RopeLandingFluff>();
            owner.sourceFeet = feet;
            ParticleSystem particles = effect.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = walking ? .45f : 0.7f;
            main.startLifetime = walking ? new ParticleSystem.MinMaxCurve(.28f, .42f) :
                new ParticleSystem.MinMaxCurve(0.4f, takeoff ? 0.5f : 0.65f);
            main.startSpeed = 0f;
            main.startSize3D = true;
            main.startSizeX = spikeContact ? new ParticleSystem.MinMaxCurve(.12f, .20f) : walking ? new ParticleSystem.MinMaxCurve(.055f, .09f) :
                new ParticleSystem.MinMaxCurve(0.09f, 0.14f);
            main.startSizeY = walking ? new ParticleSystem.MinMaxCurve(.018f, .026f) :
                new ParticleSystem.MinMaxCurve(0.022f, 0.035f);
            main.startSizeZ = 1f;
            main.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
            main.startColor = Color.white;
            main.gravityModifier = walking ? .035f : 0.055f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Shape;
            main.useUnscaledTime = false;
            main.maxParticles = count;
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
                name = "Foot Yarn Fibre Material",
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
            for (int i = 0; i < count; i++)
            {
                float side = i / (float)(count - 1) * 2f - 1f;
                float spread = walking ? .09f : strongLanding ? 0.48f : 0.3f;
                float lift = walking ? .04f : takeoff ? 0.2f : strongLanding ? 0.38f : 0.28f;
                Vector3 position = new Vector3(bounds.center.x + side * bounds.extents.x * 0.75f,
                    bounds.min.y + 0.04f, feet.transform.position.z);
                Vector3 velocity = new Vector3(side * spread, lift + (i % 3) * (walking ? .015f : .05f), 0f);
                if (spikeContact)
                {
                    position = new Vector3(contactPoint.Value.x, contactPoint.Value.y, feet.transform.position.z);
                    Vector2 drift = Vector2.ClampMagnitude(contactVelocity * .06f, .55f);
                    velocity = new Vector3(drift.x + side * .55f, drift.y + .12f + (i % 2) * .18f, 0f);
                }
                particles.Emit(new ParticleSystem.EmitParams
                {
                    position = position,
                    velocity = velocity
                }, 1);
            }
        }

        private void Update()
        {
            // Failure, title and preview hide residual fibres along with the player.
            if (sourceFeet == null ||
                (sourceFeet.attachedRigidbody != null && !sourceFeet.attachedRigidbody.simulated))
                Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (runtimeMaterial == null) return;
            if (Application.isPlaying) Destroy(runtimeMaterial);
            else DestroyImmediate(runtimeMaterial);
        }
    }
}
