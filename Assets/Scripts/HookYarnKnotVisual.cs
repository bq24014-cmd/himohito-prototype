using UnityEngine;

namespace HimoHito
{
    /// <summary>A tiny tied loop at the visible rope tip. Never modifies the hook or joint.</summary>
    [DefaultExecutionOrder(110)]
    public sealed class HookYarnKnotVisual : MonoBehaviour
    {
        private const float TieDuration = 0.20f;
        private const float UntieDuration = 0.12f;
        private const int Segments = 40;
        private LineRenderer line;
        private Material material;
        private Rigidbody2D owner;
        private RopeController ropeController;
        private DistanceJoint2D ropeJoint;
        private RopeBodyVisual playerArt;
        private PlayerMover mover;
        private RopeRetractVisual returningRope;
        private Vector3 anchor;
        private Vector3 across, along;
        private float elapsed, width, opacity;
        private float releaseVisible, releaseSize;
        private bool releasing, cancelled;
        private float load;

        public static HookYarnKnotVisual Play(LineRenderer source, Rigidbody2D player, Vector2 point)
        {
            if (source == null || !source.enabled || source.positionCount < 2 ||
                source.sharedMaterial == null || player == null || !player.simulated) return null;
            var root = new GameObject("Hook Yarn Knot Visual");
            root.transform.SetParent(player.transform, false);
            var effect = root.AddComponent<HookYarnKnotVisual>();
            effect.owner = player;
            effect.ropeController = player.GetComponent<RopeController>();
            effect.ropeJoint = player.GetComponent<DistanceJoint2D>();
            effect.playerArt = player.GetComponent<RopeBodyVisual>();
            effect.mover = player.GetComponent<PlayerMover>();
            effect.anchor = point;
            Vector3 origin = source.GetPosition(0);
            if (!source.useWorldSpace) origin = source.transform.TransformPoint(origin);
            effect.along = (origin - effect.anchor).normalized;
            if (effect.along.sqrMagnitude < 0.001f) effect.along = Vector3.down;
            effect.across = new Vector3(-effect.along.y, effect.along.x, 0f);
            effect.width = Mathf.Clamp(source.endWidth * 0.48f, 0.045f, 0.075f);
            effect.opacity = source.endColor.a;
            effect.material = new Material(source.sharedMaterial) { name = "Hook Knot Yarn Material" };
            effect.line = root.AddComponent<LineRenderer>();
            effect.line.sharedMaterial = effect.material;
            effect.line.useWorldSpace = true;
            effect.line.positionCount = Segments;
            effect.line.numCapVertices = 4;
            effect.line.numCornerVertices = 3;
            effect.line.textureMode = LineTextureMode.Tile;
            Texture texture = effect.material.mainTexture;
            float tileLength = texture != null ? effect.width * texture.width / texture.height : 1f;
            effect.line.textureScale = new Vector2(1f / Mathf.Max(0.01f, tileLength), 1f);
            effect.line.startColor = effect.line.endColor = source.endColor;
            effect.line.sortingLayerID = source.sortingLayerID;
            // In front of the existing blue/green ring, without hiding its rim.
            effect.line.sortingOrder = Mathf.Max(9, source.sortingOrder + 1);
            effect.Draw();
            return effect;
        }

        public void Release(RopeRetractVisual rope)
        {
            if (rope == null) { Cancel(); return; }
            float tied = Mathf.Clamp01(elapsed / TieDuration);
            releaseVisible = Mathf.SmoothStep(0f, 1f, tied * 1.65f);
            releaseSize = Mathf.Lerp(1.3f, 1f, Mathf.SmoothStep(0f, 1f, tied));
            returningRope = rope;
            releasing = true;
            elapsed = 0f;
        }

        public void Cancel()
        {
            if (cancelled) return;
            cancelled = true;
            if (line != null) line.enabled = false;
            Destroy(gameObject);
        }

        private void LateUpdate() => Advance(Time.timeScale > 0f ? Time.deltaTime : 0f);

        private void Advance(float deltaTime)
        {
            if (cancelled) return;
            if (owner == null || !owner.simulated || !owner.gameObject.activeInHierarchy ||
                (releasing && returningRope == null)) { Cancel(); return; }
            if (deltaTime <= 0f) return;
            elapsed += deltaTime;
            if (releasing && elapsed >= UntieDuration) { Cancel(); return; }
            if (!releasing && elapsed >= TieDuration) UpdateLoad(deltaTime);
            Draw();
        }

        private void UpdateLoad(float delta)
        {
            float target = 0f;
            if (ropeController != null && ropeController.IsAttached && ropeJoint != null &&
                (mover == null || (mover.enabled && !mover.IsGrounded)) &&
                Vector2.Distance(ropeController.AnchorPoint, anchor) < .01f)
            {
                Vector2 span = owner.position - (Vector2)anchor;
                float distance = span.magnitude;
                Vector2 radial = distance > .001f ? span / distance : Vector2.down;
                float taut = Mathf.InverseLerp(ropeJoint.distance - .18f, ropeJoint.distance, distance);
                Vector2 tangent = new Vector2(-radial.y, radial.x);
                float speed = Vector2.Dot(owner.linearVelocity, tangent);
                Vector2 gravity = Physics2D.gravity * owner.gravityScale;
                // Read-only load estimate: slack removes load; speed and hanging weight tighten it.
                float weight = Mathf.Max(0f, Vector2.Dot(radial, gravity.normalized));
                float speedLoad = speed * speed / Mathf.Max(1f, ropeJoint.distance * gravity.magnitude);
                target = taut * Mathf.Clamp01((weight + speedLoad) / 2.5f);
            }
            float blend = 1f - Mathf.Exp(-(target > load ? 10f : 5f) * delta);
            load = Mathf.Lerp(load, target, blend);
            Vector3 origin = playerArt != null ? playerArt.RopeOrigin : owner.transform.position;
            Vector3 direction = origin - anchor;
            if (direction.sqrMagnitude > .0001f)
            {
                float angle = Mathf.Atan2(along.y, along.x) * Mathf.Rad2Deg;
                float desired = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                along = Quaternion.AngleAxis(Mathf.DeltaAngle(angle, desired) * (1f - Mathf.Exp(-12f * delta)), Vector3.forward) * along;
                across = new Vector3(-along.y, along.x, 0f);
            }
        }

        private void Draw()
        {
            float tied = Mathf.Clamp01(elapsed / TieDuration);
            float undone = releasing ? Mathf.SmoothStep(0f, 1f, elapsed / UntieDuration) : 0f;
            float visible = releasing ? releaseVisible * (1f - undone) : Mathf.SmoothStep(0f, 1f, tied * 1.65f);
            float size = releasing ? Mathf.Lerp(releaseSize, 0.55f, undone) : Mathf.Lerp(1.3f, 1f, Mathf.SmoothStep(0f, 1f, tied));
            Vector3 center = releasing ? returningRope.TipPosition : anchor;
            line.enabled = visible > 0.001f;
            float pull = load * (1f - undone);
            line.startWidth = line.endWidth = width * (1f - .12f * pull);
            Color color = line.startColor;
            color.a = opacity * (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.7f, 1f, undone)));
            line.startColor = line.endColor = color;
            for (int i = 0; i < Segments; i++)
            {
                float turn = visible * i / (Segments - 1f) * Mathf.PI * 2f;
                // One continuous figure-eight: the crossing tightens into a small yarn knot.
                Vector3 offset = across * (Mathf.Sin(turn) * 0.135f * (1f - .28f * pull)) +
                                 along * (Mathf.Sin(turn * 2f) * 0.075f * (1f + .18f * pull));
                line.SetPosition(i, center + offset * size);
            }
        }

        private void OnDisable()
        {
            if (line != null) line.enabled = false;
        }

        private void OnDestroy()
        {
            if (material != null) Destroy(material);
        }
    }
}
