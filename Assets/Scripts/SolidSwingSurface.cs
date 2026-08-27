using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Marks a floor as solid from every direction, including during a fast swing.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class SolidSwingSurface : MonoBehaviour
    {
        private void Awake()
        {
            ConfigureComponents();
        }

        private void Reset()
        {
            ConfigureComponents();
        }

        private void OnValidate()
        {
            ConfigureComponents();
        }

        private void ConfigureComponents()
        {
            Collider2D solidCollider = GetComponent<Collider2D>();
            if (solidCollider == null)
            {
                return;
            }

            solidCollider.enabled = true;
            solidCollider.isTrigger = false;
            solidCollider.usedByEffector = false;

            Rigidbody2D solidBody = GetComponent<Rigidbody2D>();
            solidBody.bodyType = RigidbodyType2D.Static;
            solidBody.simulated = true;

            if (TryGetComponent(out PlatformEffector2D effector))
            {
                effector.enabled = false;
            }
        }
    }
}
