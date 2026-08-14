using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Restarts the current main-stage test from its beginning after a fall.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(RopeResource), typeof(RopeController))]
    [RequireComponent(typeof(WeaveResource))]
    public sealed class MainStageRespawnOnFall : MonoBehaviour
    {
        [SerializeField] private float fallThreshold = -9f;

        private Rigidbody2D body;
        private RopeResource ropeResource;
        private RopeController ropeController;
        private WeaveResource weaveResource;
        private Vector2 startPosition;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            ropeResource = GetComponent<RopeResource>();
            ropeController = GetComponent<RopeController>();
            weaveResource = GetComponent<WeaveResource>();
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
            weaveResource.ResetThreads();
            body.position = startPosition;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }
    }
}
