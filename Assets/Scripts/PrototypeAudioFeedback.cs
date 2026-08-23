using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Plays small gameplay sounds that also confirm the result of an input.
    /// The prototype generates its temporary sounds at runtime so their tone can
    /// be tuned before choosing final recorded assets.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PrototypeAudioFeedback : MonoBehaviour
    {
        private const int SampleRate = 44100;
        private const float HookAttachDuration = 0.075f;

        [SerializeField, Range(0f, 1f)] private float hookAttachVolume = 0.38f;

        private AudioSource audioSource;
        private AudioClip hookAttachClip;

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
            hookAttachClip = CreateHookAttachClip();
        }

        private void OnDestroy()
        {
            if (hookAttachClip != null)
            {
                Destroy(hookAttachClip);
            }
        }

        public void PlayHookAttached()
        {
            if (audioSource == null || hookAttachClip == null)
            {
                return;
            }

            audioSource.PlayOneShot(hookAttachClip, hookAttachVolume);
        }

        private static AudioClip CreateHookAttachClip()
        {
            int sampleCount = Mathf.CeilToInt(SampleRate * HookAttachDuration);
            float[] samples = new float[sampleCount];
            uint noiseState = 0x4A3B2C1Du;

            for (int i = 0; i < sampleCount; i++)
            {
                float time = i / (float)SampleRate;
                float attack = Mathf.Clamp01(time / 0.0015f);
                float bodyDecay = Mathf.Exp(-46f * time);
                float clickDecay = Mathf.Exp(-125f * time);

                float plasticBody =
                    Mathf.Sin(2f * Mathf.PI * 720f * time) * 0.52f +
                    Mathf.Sin(2f * Mathf.PI * 1180f * time) * 0.26f;

                noiseState = noiseState * 1664525u + 1013904223u;
                float noise = ((noiseState >> 8) / 8388607.5f) - 1f;

                float settlingTime = Mathf.Max(0f, time - 0.018f);
                float settling = time >= 0.018f
                    ? Mathf.Sin(2f * Mathf.PI * 430f * settlingTime) *
                      Mathf.Exp(-70f * settlingTime) * 0.16f
                    : 0f;

                samples[i] = Mathf.Clamp(
                    attack * plasticBody * bodyDecay * 0.62f +
                    noise * clickDecay * 0.18f +
                    settling,
                    -1f,
                    1f);
            }

            AudioClip clip = AudioClip.Create(
                "Prototype Toy Hook Click",
                sampleCount,
                1,
                SampleRate,
                false);
            clip.SetData(samples, 0);
            clip.hideFlags = HideFlags.HideAndDontSave;
            return clip;
        }
    }
}
