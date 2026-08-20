using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Releases an attached rope when the player enters a main-stage hazard.
    /// The trigger is not a floor, so the detached player continues falling.
    /// </summary>
    public sealed class MainStageRopeHazard : MonoBehaviour
    {
        private void Awake()
        {
            Collider2D detectionArea = GetComponent<Collider2D>();
            if (detectionArea != null)
            {
                detectionArea.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryDetach(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryDetach(other);
        }

        private void TryDetach(Collider2D other)
        {
            RopeController contactedRope = other.GetComponentInParent<RopeController>();
            if (contactedRope != null && contactedRope.IsAttached)
            {
                contactedRope.DetachAndRefund();
            }
        }
    }
}
