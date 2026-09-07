using System.Collections;
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
        [SerializeField, Range(0f, 1f)] private float ropeShotVolume = 0.50f;
        [SerializeField, Range(0f, 1f)] private float ropeAttachMissVolume = 0.45f;
        [SerializeField, Range(0f, 1f)] private float ropeLengthClickVolume = 0.45f;
        [SerializeField, Range(0.5f, 1.5f)] private float ropeLengthIncreasePitch = 1.10f;
        [SerializeField, Range(0.5f, 1.5f)] private float ropeLengthDecreasePitch = 0.90f;
        [SerializeField, Range(0f, 1f)] private float ropeLengthLimitVolume = 0.42f;
        [SerializeField, Range(0f, 1f)] private float playerLandingVolume = 0.55f;
        [SerializeField, Range(0f, 1f)] private float playerJumpVolume = 0.55f;
        [SerializeField, Range(0f, 1f)] private float playerFootstepVolume = 0.42f;
        [SerializeField, Range(0f, 0.1f)] private float footstepPitchVariation = 0.025f;
        [SerializeField, Range(0f, 1f)] private float ropePlatformBuildVolume = 0.38f;
        [SerializeField, Range(0f, 1.5f)] private float ropePlatformMergeVolume = 1.15f;
        [SerializeField, Range(0f, 1f)] private float goalChestVolume = 0.62f;
        [SerializeField, Range(0f, 1f)] private float clearRevealVolume = 0.58f;
        [SerializeField, Range(0f, 1f)] private float failureYarnDropVolume = 0.72f;
        [SerializeField, Range(0f, 1f)] private float failurePianoVolume = 0.14f;
        [SerializeField, Min(0f)] private float failurePianoDelay = 0.18f;
        [SerializeField, Range(0f, 1f)] private float uiPaperOpenVolume = 0.45f;
        [SerializeField, Range(0f, 1f)] private float hookAttachWoodVolume = 0.42f;
        [SerializeField, Range(0f, 1f)] private float hookAttachSoftVolume = 0.10f;
        [SerializeField, Min(0f)] private float releaseQuietSpeed = 4f;
        [SerializeField, Min(0.01f)] private float releaseFullSpeed = 14f;
        [SerializeField, Range(0f, 1f)] private float releaseQuietVolume = 0f;
        [SerializeField, Range(0f, 1f)] private float releaseFullVolume = 0.4f;

        private AudioSource audioSource;
        private AudioSource lengthSelectionAudioSource;
        private AudioSource footstepAudioSource;
        private AudioSource failureAudioSource;
        private AudioClip ropeShotClip;
        private AudioClip ropeAttachMissClip;
        private AudioClip ropeLengthClickClip;
        private AudioClip ropeLengthLimitClip;
        private AudioClip playerLandingClip;
        private AudioClip playerJumpClip;
        private AudioClip[] playerFootstepClips;
        private AudioClip ropePlatformBuildClip;
        private AudioClip ropePlatformMergeClip;
        private AudioClip goalChestClip;
        private AudioClip clearRevealClip;
        private AudioClip failureYarnDropClip;
        private AudioClip failurePianoClip;
        private AudioClip uiPaperOpenClip;
        private AudioClip hookAttachWoodClip;
        private AudioClip hookAttachSoftClip;
        private AudioClip ropeReleaseWindClip;
        private int lastFootstepIndex = -1;
        private Coroutine failureSequence;

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

            lengthSelectionAudioSource = gameObject.AddComponent<AudioSource>();
            lengthSelectionAudioSource.playOnAwake = false;
            lengthSelectionAudioSource.loop = false;
            lengthSelectionAudioSource.spatialBlend = 0f;
            lengthSelectionAudioSource.dopplerLevel = 0f;

            footstepAudioSource = gameObject.AddComponent<AudioSource>();
            footstepAudioSource.playOnAwake = false;
            footstepAudioSource.loop = false;
            footstepAudioSource.spatialBlend = 0f;
            footstepAudioSource.dopplerLevel = 0f;

            failureAudioSource = gameObject.AddComponent<AudioSource>();
            failureAudioSource.playOnAwake = false;
            failureAudioSource.loop = false;
            failureAudioSource.spatialBlend = 0f;
            failureAudioSource.dopplerLevel = 0f;

            ropeShotClip = Resources.Load<AudioClip>("Audio/RopeShotRustling");
            ropeAttachMissClip = Resources.Load<AudioClip>("Audio/RopeAttachMissWhoosh");
            ropeLengthClickClip = Resources.Load<AudioClip>("Audio/RopeLengthClick");
            ropeLengthLimitClip = Resources.Load<AudioClip>("Audio/RopeLengthLimitWood");
            playerLandingClip = Resources.Load<AudioClip>("Audio/PlayerLandingStep");
            playerJumpClip = Resources.Load<AudioClip>("Audio/PlayerJumpSoft");
            playerFootstepClips = new[]
            {
                Resources.Load<AudioClip>("Audio/PlayerFootstepSoftA"),
                Resources.Load<AudioClip>("Audio/PlayerFootstepSoftC"),
                Resources.Load<AudioClip>("Audio/PlayerFootstepSoftD")
            };
            ropePlatformBuildClip = Resources.Load<AudioClip>("Audio/RopePlatformBuildWood");
            ropePlatformMergeClip = Resources.Load<AudioClip>("Audio/RopePlatformMergeWood");
            goalChestClip = Resources.Load<AudioClip>("Audio/GoalTreasureChestOpen");
            clearRevealClip = Resources.Load<AudioClip>("Audio/GoalClearXylophone");
            failureYarnDropClip = Resources.Load<AudioClip>("Audio/FailureYarnDrop");
            failurePianoClip = Resources.Load<AudioClip>("Audio/FailurePianoNote");
            uiPaperOpenClip = Resources.Load<AudioClip>("Audio/UiPaperOpen");
            hookAttachWoodClip = Resources.Load<AudioClip>("Audio/HookAttachWood");
            hookAttachSoftClip = Resources.Load<AudioClip>("Audio/HookAttachSoft");
            ropeReleaseWindClip = Resources.Load<AudioClip>("Audio/RopeReleaseWind");
        }

        public void PlayRopeShot()
        {
            if (audioSource != null && ropeShotClip != null)
            {
                audioSource.PlayOneShot(ropeShotClip, ropeShotVolume);
            }
        }

        public void PlayRopeAttachMiss()
        {
            if (audioSource != null && ropeAttachMissClip != null)
            {
                audioSource.PlayOneShot(
                    ropeAttachMissClip,
                    ropeAttachMissVolume);
            }
        }

        public void PlayRopeLengthChanged(int direction)
        {
            if (lengthSelectionAudioSource == null ||
                ropeLengthClickClip == null ||
                direction == 0)
            {
                return;
            }

            lengthSelectionAudioSource.pitch = direction > 0
                ? ropeLengthIncreasePitch
                : ropeLengthDecreasePitch;
            lengthSelectionAudioSource.PlayOneShot(
                ropeLengthClickClip,
                ropeLengthClickVolume);
        }

        public void PlayRopeLengthLimitReached()
        {
            if (audioSource != null && ropeLengthLimitClip != null)
            {
                audioSource.PlayOneShot(
                    ropeLengthLimitClip,
                    ropeLengthLimitVolume);
            }
        }

        public void PlayPlayerLanded()
        {
            if (audioSource != null && playerLandingClip != null)
            {
                audioSource.PlayOneShot(playerLandingClip, playerLandingVolume);
            }
        }

        public void PlayPlayerJumped()
        {
            if (audioSource != null && playerJumpClip != null)
            {
                audioSource.PlayOneShot(playerJumpClip, playerJumpVolume);
            }
        }

        public void PlayPlayerFootstep()
        {
            if (footstepAudioSource == null ||
                playerFootstepClips == null ||
                playerFootstepClips.Length == 0)
            {
                return;
            }

            int clipIndex = ChooseDifferentFootstepIndex();
            AudioClip clip = playerFootstepClips[clipIndex];
            if (clip == null)
            {
                return;
            }

            footstepAudioSource.pitch = Random.Range(
                1f - footstepPitchVariation,
                1f + footstepPitchVariation);
            footstepAudioSource.PlayOneShot(
                clip,
                playerFootstepVolume);
            lastFootstepIndex = clipIndex;
        }

        private int ChooseDifferentFootstepIndex()
        {
            if (playerFootstepClips.Length <= 1)
            {
                return 0;
            }

            int index = Random.Range(0, playerFootstepClips.Length - 1);
            if (index >= lastFootstepIndex)
            {
                index++;
            }
            return index;
        }

        public void PlayRopePlatformBuilt()
        {
            if (audioSource != null && ropePlatformBuildClip != null)
            {
                audioSource.PlayOneShot(
                    ropePlatformBuildClip,
                    ropePlatformBuildVolume);
            }
        }

        public void PlayRopePlatformsMerged()
        {
            if (audioSource != null && ropePlatformMergeClip != null)
            {
                audioSource.PlayOneShot(
                    ropePlatformMergeClip,
                    ropePlatformMergeVolume);
            }
        }

        public void PlayGoalChestOpened()
        {
            if (audioSource != null && goalChestClip != null)
            {
                audioSource.PlayOneShot(goalChestClip, goalChestVolume);
            }
        }

        public void PlayClearRevealed()
        {
            if (audioSource != null && clearRevealClip != null)
            {
                audioSource.PlayOneShot(clearRevealClip, clearRevealVolume);
            }
        }

        public void PlayRopeExhausted()
        {
            StopRopeExhaustedAudio();
            if (failureAudioSource == null)
            {
                return;
            }

            if (failureYarnDropClip != null)
            {
                failureAudioSource.PlayOneShot(
                    failureYarnDropClip,
                    failureYarnDropVolume);
            }
            failureSequence = StartCoroutine(PlayFailurePianoAfterDelay());
        }

        public void StopRopeExhaustedAudio()
        {
            if (failureSequence != null)
            {
                StopCoroutine(failureSequence);
                failureSequence = null;
            }
            failureAudioSource?.Stop();
        }

        public void PlayUiPaperOpened()
        {
            if (audioSource != null && uiPaperOpenClip != null)
            {
                audioSource.PlayOneShot(uiPaperOpenClip, uiPaperOpenVolume);
            }
        }

        private IEnumerator PlayFailurePianoAfterDelay()
        {
            yield return new WaitForSecondsRealtime(failurePianoDelay);
            if (failureAudioSource != null && failurePianoClip != null)
            {
                failureAudioSource.PlayOneShot(
                    failurePianoClip,
                    failurePianoVolume);
            }
            failureSequence = null;
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
