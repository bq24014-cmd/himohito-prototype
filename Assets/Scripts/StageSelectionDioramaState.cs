using System;
using System.Collections.Generic;

namespace HimoHito
{
    /// <summary>Per-island presentation clocks only; never loads art or changes stage progress.</summary>
    internal sealed class StageSelectionDioramaState
    {
        public const double WalkDuration = 1.8;
        public const double EndpointPause = .4;
        public const double OpenDuration = .8;
        public const double CloseDuration = .5;
        public const double ActivityInDuration = .2;
        public const double ActivityOutDuration = .25;

        public readonly struct Motion
        {
            public readonly double WalkTime;
            public readonly float WalkPosition, Activity, Openness;
            public readonly bool FacingRight, IsWalking;

            internal Motion(double walkTime, float activity, float openness, bool practice)
            {
                WalkTime = walkTime;
                Activity = activity;
                Openness = openness;
                double leg = WalkDuration + EndpointPause;
                double phase = walkTime % (2 * leg);
                FacingRight = phase < leg;
                double travel = FacingRight ? phase : phase - leg;
                float position = Smooth(travel / WalkDuration);
                WalkPosition = FacingRight ? position : 1f - position;
                IsWalking = practice && travel < WalkDuration;
            }
        }

        private sealed class Island
        {
            public bool Eligible, Practice;
            public double WalkTime, Activity, Openness;
        }

        private readonly Dictionary<string, Island> islands = new Dictionary<string, Island>(StringComparer.Ordinal);
        private string selectedId;
        private bool hasTimestamp, wasActive;
        private double lastTimestamp;

        // Safe to call on each draw. Only explicit built-in dioramas opt into animation.
        public void Configure(IReadOnlyList<StageCatalog.Entry> stages)
        {
            foreach (Island island in islands.Values) island.Eligible = false;
            if (stages == null) return;
            foreach (StageCatalog.Entry stage in stages)
            {
                if (stage == null || !stage.available || string.IsNullOrWhiteSpace(stage.id) ||
                    (stage.mapArt != "practice" && stage.mapArt != "toybox")) continue;
                bool practice = stage.mapArt == "practice";
                if (!islands.TryGetValue(stage.id, out Island island) || island.Practice != practice)
                {
                    island = new Island { Practice = practice };
                    islands[stage.id] = island;
                }
                island.Eligible = true;
            }
        }

        public void Observe(string selectedStageId, double now, bool active)
        {
            bool changed = !string.Equals(selectedId, selectedStageId, StringComparison.Ordinal);
            selectedId = selectedStageId;
            bool validTime = !double.IsNaN(now) && !double.IsInfinity(now);
            double delta = 0;
            if (validTime)
            {
                // A changed selection or resumed menu starts at this sample, without inherited time.
                if (hasTimestamp && active && wasActive && !changed && now > lastTimestamp)
                    delta = now - lastTimestamp;
                lastTimestamp = hasTimestamp ? Math.Max(lastTimestamp, now) : now;
            }
            hasTimestamp = validTime;
            wasActive = active;
            if (delta <= 0 || double.IsInfinity(delta)) return;

            foreach (KeyValuePair<string, Island> pair in islands)
            {
                Island island = pair.Value;
                if (!island.Eligible) continue;
                bool selected = string.Equals(pair.Key, selectedId, StringComparison.Ordinal);
                if (island.Practice)
                {
                    if (selected) island.WalkTime = Math.Min(double.MaxValue, island.WalkTime + delta);
                    island.Activity = Approach(island.Activity, selected, delta,
                        ActivityInDuration, ActivityOutDuration);
                }
                else island.Openness = Approach(island.Openness, selected, delta, OpenDuration, CloseDuration);
            }
        }

        public Motion GetMotion(string id)
        {
            if (id == null || !islands.TryGetValue(id, out Island island) || !island.Eligible)
                return default;
            return new Motion(island.WalkTime, Smooth(island.Activity), Smooth(island.Openness), island.Practice);
        }

        private static double Approach(double value, bool increase, double delta, double upDuration, double downDuration)
            => increase ? Math.Min(1, value + delta / upDuration) : Math.Max(0, value - delta / downDuration);

        private static float Smooth(double value)
        {
            double t = Math.Max(0, Math.Min(1, value));
            return (float)(t * t * (3 - 2 * t));
        }
    }
}
