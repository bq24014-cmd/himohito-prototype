using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Detaches an active rope when the player enters this hazard's trigger.
    /// Normal refund and momentum preservation remain owned by RopeController.
    /// </summary>
    public sealed class RopeReleaseHazard : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            RopeController ropeController = other.GetComponentInParent<RopeController>();
            if (ropeController == null || !ropeController.IsAttached)
            {
                return;
            }

            ropeController.DetachAndRefund();
        }
    }
}
