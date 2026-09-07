using System;

namespace HimoHito
{
    /// <summary>One-shot ground poses. Observes input/motion; never delays or writes either.</summary>
    internal sealed class GroundMotionPoseState
    {
        private const float StartSpeed = 0.75f;
        private const float StopSpeed = 0.2f;
        private const float StartDuration = 0.18f;
        private const float StopDuration = 0.24f;
        private bool primed;
        private int motionDirection;
        private int poseDirection = 1;
        private float peakSpeed;
        private float elapsed;
        private float stopStrength;
        private int phase; // 0: normal walk/idle, 1: start, 2: stop

        public bool IsActive => phase != 0;
        public int SequenceId { get; private set; }
        public bool IsStarting => phase == 1;
        public bool FacingLeft => poseDirection < 0;
        public float Progress => phase == 0 ? 1f : Math.Min(1f,
            elapsed / (IsStarting ? StartDuration : StopDuration));
        public int FrameIndex => phase == 0 ? -1 : (IsStarting ? 0 : 3) +
            Math.Min(2, (int)(Progress * 3f));
        public float LeanDegrees
        {
            get
            {
                if (!IsActive) return 0f;
                float wave = (float)Math.Sin(Progress * Math.PI);
                return poseDirection * (IsStarting ? -4f : 3f * stopStrength) * wave * wave;
            }
        }

        public void Reset()
        {
            primed = false;
            motionDirection = 0;
            peakSpeed = elapsed = 0f;
            phase = 0;
            SequenceId = 0;
        }

        public void Advance(float input, float velocity, float deltaTime, bool eligible)
        {
            if (!eligible) { Reset(); return; }
            if (deltaTime <= 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime)) return;
            if (float.IsNaN(input) || float.IsInfinity(input) ||
                float.IsNaN(velocity) || float.IsInfinity(velocity)) { Reset(); return; }
            int requested = input > 0.01f ? 1 : input < -0.01f ? -1 : 0;
            float speed = Math.Abs(velocity);
            bool progressing = requested != 0 && velocity * requested >= StartSpeed;
            if (!primed)
            {
                // Landing or resuming control while already moving is not a new
                // takeoff. Seed history without playing another start/stop.
                primed = true;
                motionDirection = progressing ? requested : 0;
                peakSpeed = speed;
                return;
            }

            if (phase != 0)
            {
                elapsed += deltaTime;
                if (Progress >= 1f) phase = 0;
            }

            if (motionDirection != 0)
            {
                peakSpeed = Math.Max(peakSpeed, speed);
                if (requested != motionDirection || speed <= StopSpeed)
                {
                    poseDirection = motionDirection;
                    motionDirection = 0;
                    stopStrength = Math.Min(1f, Math.Max(0.2f, peakSpeed / 6.5f));
                    phase = 2;
                    elapsed = 0f;
                    SequenceId++;
                }
            }

            // A new direction or a renewed press can interrupt a stop immediately.
            // Holding against a wall cannot replay a takeoff without real motion.
            if (motionDirection == 0 && progressing)
            {
                motionDirection = poseDirection = requested;
                peakSpeed = speed;
                phase = 1;
                elapsed = 0f;
                SequenceId++;
            }
        }
    }
}
