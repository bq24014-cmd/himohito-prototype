using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Builds the section-two "length window" from the 0829 stage manual.
    /// Length 5 cannot reach the raised bank, length 6 clears the spikes, and
    /// length 7 or more drops low enough to touch the tall near spike.
    /// </summary>
    public static class MainStageSectionTwoSetup
    {
        public const string BoardName =
            "Main Section 2 Length Window Board";
        public const string FrontSpikeName =
            "Main Section 2 Front Spike";
        public const string FarSpikeName =
            "Main Section 2 Far Spike";

        private const string HookName = "Main Hook 2";
        private const string LandingName = "Main Landing 2";

        // Main Landing 1 is centered at x = 9.5, ends at x = 13.5,
        // and its top is y = -4.85.
        // The invisible valley baseline is therefore y = -8.35.
        private static readonly Vector2 HookPosition =
            new Vector2(17.25f, -0.85f);
        private static readonly Vector2 LandingPosition =
            new Vector2(29f, -9.35f);
        private static readonly Vector2 LandingSize =
            new Vector2(8f, 10f);
        private static readonly Vector2 FrontSpikePosition =
            new Vector2(15.35f, -7.9f);
        private static readonly Vector2 FrontSpikeSize =
            new Vector2(0.9f, 0.9f);
        private static readonly Vector2 FarSpikePosition =
            new Vector2(19.55f, -8.2f);
        private static readonly Vector2 FarSpikeSize =
            new Vector2(0.7f, 0.3f);
        private static readonly Vector2 RespawnPosition =
            new Vector2(29f, -3.65f);

        public static bool ApplyCurrentScene()
        {
            bool changed = false;

            GameObject oldBoard = FindSceneObject(BoardName);
            if (oldBoard != null && oldBoard.activeSelf)
            {
                oldBoard.SetActive(false);
                changed = true;
            }

            GameObject hook = FindSceneObject(HookName);
            if (hook != null)
            {
                changed |= SetTransform(
                    hook,
                    HookPosition,
                    new Vector2(1.6f, 0.45f));
                if (hook.TryGetComponent(out HookPoint hookPoint))
                {
                    changed |= hookPoint.ConfigureFixedAttachmentPoint(
                        Vector2.zero);
                }
            }

            GameObject landing = FindSceneObject(LandingName);
            if (landing != null)
            {
                changed |= SetTransform(
                    landing,
                    LandingPosition,
                    LandingSize);
                changed |= EnsureSolidTerrain(landing);

                if (!landing.TryGetComponent(
                        out MainStageCheckpoint checkpoint))
                {
                    checkpoint = landing.AddComponent<MainStageCheckpoint>();
                    changed = true;
                }
                checkpoint.Configure(3, RespawnPosition, 50f);
            }

            changed |= EnsureSpike(
                FrontSpikeName,
                FrontSpikePosition,
                FrontSpikeSize);
            changed |= EnsureSpike(
                FarSpikeName,
                FarSpikePosition,
                FarSpikeSize);
            return changed;
        }

        // Kept for callers made while Section 2 was a single-board test.
        public static GameObject EnsureCreated()
        {
            ApplyCurrentScene();
            return FindSceneObject(LandingName);
        }

        private static bool EnsureSolidTerrain(GameObject terrain)
        {
            bool changed = false;
            if (!terrain.TryGetComponent(out SpriteRenderer _))
            {
                terrain.AddComponent<SpriteRenderer>();
                changed = true;
            }
            if (!terrain.TryGetComponent(out SolidSprite _))
            {
                terrain.AddComponent<SolidSprite>();
                changed = true;
            }
            if (!terrain.TryGetComponent(out BoxCollider2D collider))
            {
                collider = terrain.AddComponent<BoxCollider2D>();
                changed = true;
            }
            if (collider.isTrigger)
            {
                collider.isTrigger = false;
                changed = true;
            }
            if (!terrain.TryGetComponent(out SolidSwingSurface _))
            {
                terrain.AddComponent<SolidSwingSurface>();
                changed = true;
            }
            return changed;
        }

        private static bool EnsureSpike(
            string objectName,
            Vector2 position,
            Vector2 size)
        {
            bool changed = false;
            GameObject spike = FindSceneObject(objectName);
            if (spike == null)
            {
                spike = new GameObject(objectName);
                changed = true;
            }
            if (!spike.activeSelf)
            {
                spike.SetActive(true);
                changed = true;
            }
            changed |= SetTransform(spike, position, size);

            if (!spike.TryGetComponent(out SpriteRenderer _))
            {
                spike.AddComponent<SpriteRenderer>();
                changed = true;
            }
            if (!spike.TryGetComponent(out SolidSprite visual))
            {
                visual = spike.AddComponent<SolidSprite>();
                changed = true;
            }
            Color spikeColor = new Color(1f, 0.18f, 0.25f);
            if (visual.Color != spikeColor)
            {
                visual.Color = spikeColor;
                changed = true;
            }
            if (spike.TryGetComponent(out PolygonCollider2D oldCollider))
            {
                oldCollider.enabled = false;
                if (Application.isPlaying)
                {
                    Object.Destroy(oldCollider);
                }
                else
                {
                    Object.DestroyImmediate(oldCollider);
                }
                changed = true;
            }
            if (!spike.TryGetComponent(out BoxCollider2D collider))
            {
                collider = spike.AddComponent<BoxCollider2D>();
                changed = true;
            }
            Vector2 triggerSize = new Vector2(0.86f, 0.9f);
            Vector2 triggerOffset = new Vector2(0f, -0.03f);
            if (collider.size != triggerSize)
            {
                collider.size = triggerSize;
                changed = true;
            }
            if (collider.offset != triggerOffset)
            {
                collider.offset = triggerOffset;
                changed = true;
            }
            if (!collider.isTrigger)
            {
                collider.isTrigger = true;
                changed = true;
            }
            if (!spike.TryGetComponent(out RopeSpikeHazard _))
            {
                spike.AddComponent<RopeSpikeHazard>();
                changed = true;
            }
            if (!spike.TryGetComponent(out ToySpikeVisual toyVisual))
            {
                toyVisual = spike.AddComponent<ToySpikeVisual>();
                changed = true;
            }
            changed |= toyVisual.Refresh();
            return changed;
        }

        private static bool SetTransform(
            GameObject target,
            Vector2 position,
            Vector2 size)
        {
            bool changed = false;
            Vector3 targetPosition = new Vector3(position.x, position.y, 0f);
            Vector3 targetScale = new Vector3(size.x, size.y, 1f);
            if (target.transform.position != targetPosition)
            {
                target.transform.position = targetPosition;
                changed = true;
            }
            if (target.transform.localScale != targetScale)
            {
                target.transform.localScale = targetScale;
                changed = true;
            }
            return changed;
        }

        private static GameObject FindSceneObject(string objectName)
        {
            return SceneObjectLookup.Find(objectName, "MainStage");
        }
    }
}
