using UnityEngine;

namespace HimoHito
{
    // Presentation-only side probe and clock. No writes to player physics.
    public sealed class WallPushPoseState
    {
        private readonly RaycastHit2D[] hits = new RaycastHit2D[16];
        private float elapsed, releaseElapsed;
        private bool releasing;
        public int Side { get; private set; }
        public int Frame { get; private set; } = 5;
        public float WallX { get; private set; }
        public bool IsVisible => Side != 0;

        public void Reset() { Side = 0; Frame = 5; elapsed = releaseElapsed = 0f; releasing = false; }

        public void Advance(Collider2D player, float input, float speed, bool eligible, float dt)
        {
            if (!eligible || player == null || !player.enabled) { Reset(); return; }
            if (dt <= 0f) return;
            int desired = Mathf.Abs(input) > .1f ? (int)Mathf.Sign(input) : 0;
            if (Side != 0 && (Mathf.Abs(speed) > .5f || (desired != 0 && desired != Side))) Reset();
            if (desired != 0 && Mathf.Abs(speed) < .5f && TryFindWall(player, desired, out float wallX))
            {
                if (Side != desired || releasing) { elapsed = 0f; releasing = false; }
                Side = desired; WallX = wallX; elapsed += dt;
                // Reach/contact first, then hold a slow effort cycle without walking in place.
                if (elapsed < .12f) Frame = 0;
                else if (elapsed < .24f) Frame = 1;
                else
                {
                    int phase = Mathf.FloorToInt((elapsed - .24f) / .22f) % 4;
                    Frame = phase == 0 ? 2 : phase == 2 ? 4 : 3;
                }
                return;
            }
            if (!IsVisible) return;
            // Release only when the input is let go; losing the wall cancels immediately.
            if (desired != 0) { Reset(); return; }
            if (!releasing) { releasing = true; releaseElapsed = 0f; }
            releaseElapsed += dt;
            Frame = releaseElapsed < .06f ? 1 : 0;
            if (releaseElapsed >= .12f) Reset();
        }

        private bool TryFindWall(Collider2D player, int side, out float x)
        {
            Bounds bounds = player.bounds;
            var filter = new ContactFilter2D { useTriggers = false };
            int count = Physics2D.Raycast(bounds.center, Vector2.right * side, filter,
                hits, bounds.extents.x + .10f);
            for (int i = 0; i < count; i++)
            {
                RaycastHit2D hit = hits[i];
                if (hit.collider == null || hit.collider.attachedRigidbody == player.attachedRigidbody ||
                    hit.collider.isTrigger || hit.collider.GetComponentInParent<HookPoint>() != null ||
                    hit.collider.GetComponentInParent<GeneratedRopePlatform>() != null ||
                    hit.normal.x * side > -.8f || hit.distance < .001f) continue;
                x = hit.point.x;
                return true;
            }
            x = 0f;
            return false;
        }
    }
}
