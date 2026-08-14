using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Saves tutorial progress after the player lands and releases the rope.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class TutorialCheckpoint : MonoBehaviour
    {
        [SerializeField, Range(2, 4)] private int sectionNumber = 2;
        [SerializeField] private Vector2 respawnPosition;

        public int SectionNumber => sectionNumber;
        public Vector2 RespawnPosition => respawnPosition;

        public void Configure(int section, Vector2 position)
        {
            sectionNumber = Mathf.Clamp(section, 2, 4);
            respawnPosition = position;
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.rigidbody == null ||
                collision.rigidbody.position.y <= transform.position.y)
            {
                return;
            }

            PrototypeRunController runController =
                collision.rigidbody.GetComponent<PrototypeRunController>();
            if (runController != null)
            {
                runController.TryReachTutorialSection(sectionNumber, respawnPosition);
            }
        }
    }
}
