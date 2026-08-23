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
        [SerializeField, Min(0f)] private float releaseQuietSpeed = 4f;
        [SerializeField, Min(0.01f)] private float releaseFullSpeed = 14f;
        [SerializeField, Range(0f, 1f)] private float releaseQuietVolume = 0f;
        [SerializeField, Range(0f, 1f)] private float releaseFullVolume = 0.4f;

        private AudioSource audioSource;
        private AudioClip hookAttachWoodClip;
        private AudioClip hookAttachSoftClip;
        private AudioClip ropeReleaseWindClip;

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
            ropeReleaseWindClip = Resources.Load<AudioClip>("Audio/RopeReleaseWind");
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

        public void PlayRopeReleased(float releaseSpeed)
        {
            if (audioSource == null || ropeReleaseWindClip == null)
            {
                return;
            }

            if (releaseSpeed <= releaseQuietSpeed)
            {
                return;
            }

            float speedRatio = Mathf.InverseLerp(
                releaseQuietSpeed,
                Mathf.Max(releaseQuietSpeed + 0.01f, releaseFullSpeed),
                Mathf.Max(0f, releaseSpeed));
            float volume = Mathf.Lerp(
                releaseQuietVolume,
                releaseFullVolume,
                speedRatio);
            audioSource.PlayOneShot(ropeReleaseWindClip, volume);
        }
    }
}
