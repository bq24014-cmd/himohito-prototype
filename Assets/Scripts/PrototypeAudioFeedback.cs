using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Plays small gameplay sounds that also confirm the result of an input.
    /// A light wooden impact forms the core of the Hook connection, with a quiet
    /// soft impact layered underneath to suggest the player's yarn body.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PrototypeAudioFeedback : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float hookAttachWoodVolume = 0.42f;
        [SerializeField, Range(0f, 1f)] private float hookAttachSoftVolume = 0.10f;

        private AudioSource audioSource;
        private AudioClip hookAttachWoodClip;
        private AudioClip hookAttachSoftClip;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
            audioSource.dopplerLevel = 0f;
            hookAttachWoodClip = Resources.Load<AudioClip>("Audio/HookAttachWood");
            hookAttachSoftClip = Resources.Load<AudioClip>("Audio/HookAttachSoft");
        }

        public void PlayHookAttached()
        {
            if (audioSource == null)
            {
                return;
            }

            if (hookAttachWoodClip != null)
            {
                audioSource.PlayOneShot(hookAttachWoodClip, hookAttachWoodVolume);
            }

            if (hookAttachSoftClip != null)
            {
                audioSource.PlayOneShot(hookAttachSoftClip, hookAttachSoftVolume);
            }
        }
    }
}
