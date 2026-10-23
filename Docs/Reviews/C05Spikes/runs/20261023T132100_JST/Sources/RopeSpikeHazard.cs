using UnityEngine;

namespace HimoHito
{
    /// <summary>Spike contact cuts an active rope and lets the player fall.</summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class RopeSpikeHazard : MonoBehaviour
    {
        private Collider2D detectionArea;
        private RopeController playerRope;
        private Collider2D playerCollider;
        private bool UsesRopeContact => name == CraftScissorsMotion.TargetName && GroundingReview.Arg("-bodyCutBaseline") != "1";
        // Presentation notification only: no changes to contact, detach or refund.
        public event System.Action<RopeController> RopeCutSucceeded;
        [System.NonSerialized] public int confirmedCuts,refundCalls,successNotifications;

        private void Awake()
        {
            CacheReferences();
        }

        private void FixedUpdate()
        {
            if (UsesRopeContact) return;
            CacheReferences();
            if (detectionArea == null ||
                !detectionArea.enabled ||
                playerCollider == null ||
                !playerCollider.enabled ||
                playerRope == null ||
                !playerRope.IsAttached)
            {
                return;
            }

            // A fast pendulum can cross a small trigger between callbacks.
            // Confirm the physical overlap every physics step as a fallback.
            if (Physics2D.Distance(detectionArea, playerCollider).isOverlapped)
            {
                Cut(playerCollider);
            }
        }

        private void OnTriggerEnter2D(Collider2D other) => Cut(other);
        private void OnTriggerStay2D(Collider2D other) => Cut(other);

        private void Cut(Collider2D other)
        {
            if (UsesRopeContact) return;
            RopeController rope = other.GetComponentInParent<RopeController>();
            if (rope == null && other.attachedRigidbody != null)
            {
                rope = other.attachedRigidbody.GetComponent<RopeController>();
            }
            if (rope != null && rope.IsAttached)
            {
                CacheReferences();
                Vector2 point = other.bounds.center;
                if (detectionArea != null)
                {
                    ColliderDistance2D contact = Physics2D.Distance(detectionArea, other);
                    point = contact.isValid ? contact.pointA : detectionArea.ClosestPoint(point);
                }
                Rigidbody2D body = rope.GetComponent<Rigidbody2D>();
                Vector2 velocity = body != null ? body.linearVelocity : Vector2.zero;
                var cutVfx = GetComponent<ScissorsCutVfx>();
                bool replaceFluff = body != null && body.simulated && Time.timeScale > 0f &&
                    cutVfx != null && cutVfx.CaptureBeforeCut(rope, detectionArea, point);
                // Preserve the existing detach/refund first. Presentation adds no force.
                rope.DetachAndRefund();
                if (body != null && body.simulated && Time.timeScale > 0f)
                {
                    GetComponent<ToySpikeVisual>()?.PlayContact(point, velocity);
                    if (!replaceFluff) RopeLandingFluff.PlaySpikeContact(other, point, velocity);
                    RopeCutSucceeded?.Invoke(rope);
                }
            }
        }

        private void CacheReferences()
        {
            if (detectionArea == null || !detectionArea.enabled)
            {
                foreach (Collider2D candidate in GetComponents<Collider2D>())
                {
                    if (candidate.enabled && candidate.isTrigger)
                    {
                        detectionArea = candidate;
                        break;
                    }
                }
            }

            if (playerRope == null)
            {
                playerRope = FindFirstObjectByType<RopeController>();
            }
            if (playerRope != null && playerCollider == null)
            {
                playerCollider = playerRope.GetComponent<Collider2D>();
            }
        }

        // Only the front scissors may use this authoritative rendered-rope contact path.
        public bool TryCutAtRopeContact(RopeController rope,RopeBladeContact.Impact impact,LineRenderer line)
        {
            if(!UsesRopeContact||!rope||!rope.IsAttached||Time.timeScale<=0)return false;
            var body=rope.GetComponent<Rigidbody2D>();
            if(!body||!body.simulated)return false;
            var vfx=GetComponent<ScissorsCutVfx>();
            impact.vfxStatus=vfx==null?"component missing":!vfx.enabled?"component disabled":"capture pending";
            try {impact.vfxCaptured=vfx!=null&&vfx.enabled&&vfx.CaptureConfirmed(rope,impact,line);if(vfx&&vfx.enabled)impact.vfxStatus=impact.vfxCaptured?"captured":"capture failed";}
            catch(System.Exception ex){impact.vfxCaptured=false;impact.vfxStatus="capture exception: "+ex.GetType().Name;}
            if(!impact.vfxCaptured&&vfx)vfx.DiscardPendingCapture();
            Vector2 velocity=body.linearVelocity;
            confirmedCuts++;rope.DetachAndRefund();refundCalls++;
            GetComponent<ToySpikeVisual>()?.PlayContact(impact.bladePoint,velocity);
            // No legacy fluff here: the successful cut owns exactly one yarn-copy effect.
            successNotifications++;
            if(RopeCutSucceeded!=null)foreach(System.Action<RopeController> subscriber in RopeCutSucceeded.GetInvocationList()){
                try{subscriber(rope);}catch(System.Exception ex){impact.vfxStatus+="; notification failure: "+ex.GetType().Name;}
            }
            return true;
        }
    }
}
