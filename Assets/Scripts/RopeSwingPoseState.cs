using System;

namespace HimoHito
{
    /// <summary>
    /// Selects the six hanging drawings from momentum, not a looping walk clock.
    /// No physics or Unity object state is written here.
    /// </summary>
    internal sealed class RopeSwingPoseState
    {
        private const float NeutralPose = 2.5f;
        private const float FullTrailSpeed = 7f;
        private const float ResponseSeconds = 0.12f;
        private const float MinimumFrameSeconds = 0.085f;
        private float pose = NeutralPose;
        private float frameElapsed;

        public int FrameIndex { get; private set; } = 2;

        public void Reset()
        {
            pose = NeutralPose;
            frameElapsed = 0f;
            FrameIndex = 2;
        }

        public int Advance(float tangentialSpeed, bool facingLeft, float deltaTime)
        {
            if (deltaTime <= 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime))
                return FrameIndex;
            if (float.IsNaN(tangentialSpeed) || float.IsInfinity(tangentialSpeed))
                tangentialSpeed = 0f;

            // Sheet: left-trailing -> neutral -> right-trailing. Sprite flipping
            // reverses local left/right, so mirror the speed, not the frame clock.
            float localSpeed = facingLeft ? -tangentialSpeed : tangentialSpeed;
            float target = NeutralPose - Math.Max(-1f, Math.Min(1f,
                localSpeed / FullTrailSpeed)) * NeutralPose;
            float blend = (float)(1.0 - Math.Exp(-deltaTime / ResponseSeconds));
            pose += (target - pose) * blend;
            frameElapsed += deltaTime;

            // A dead band around each half-frame prevents flicker at an apex.
            // Move only to an adjacent pose, including after a long render frame.
            if (frameElapsed >= MinimumFrameSeconds)
            {
                int previous = FrameIndex;
                if (pose > FrameIndex + 0.65f && FrameIndex < 5) FrameIndex++;
                else if (pose < FrameIndex - 0.65f && FrameIndex > 0) FrameIndex--;
                if (FrameIndex != previous) frameElapsed = 0f;
            }

            return FrameIndex;
        }
    }
}
