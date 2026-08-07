using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Keeps testing fast: falling resets the square and restores the rope resource.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(RopeResource), typeof(RopeController))]
    public sealed class RespawnOnFall : MonoBehaviour
    {
        [SerializeField] private float fallThreshold = -9f;

        private Rigidbody2D body;
        private RopeResource ropeResource;
        private RopeController ropeController;
        private Vector2 startPosition;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            ropeResource = GetComponent<RopeResource>();
            ropeController = GetComponent<RopeController>();
            startPosition = body.position;
        }

        private void Update()
        {
            if (transform.position.y >= fallThreshold)
            {
                return;
            }

            ropeController.DetachAndRefund();
            ropeResource.ResetToMaximum();
            body.position = startPosition;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }
    }
}
