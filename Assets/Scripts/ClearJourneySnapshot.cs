using System.Collections.Generic;
using UnityEngine;

namespace HimoHito
{
    /// <summary>Read-only, per-clear snapshot. No route is invented across gaps without bridges.</summary>
    internal sealed class ClearJourneySnapshot
    {
        internal readonly struct Floor
        {
            public readonly float Left, Right, Top;
            public readonly bool Rail;
            public Floor(Bounds bounds, bool rail)
            { Left = bounds.min.x; Right = bounds.max.x; Top = bounds.max.y; Rail = rail; }
        }
        public readonly Floor[] Floors;
        public readonly Vector2[][] Bridges;
        public readonly Vector2 Start, Goal;
        public readonly Sprite Chest;
        public readonly Rect World;
        public readonly float Remaining, Capacity;

        public ClearJourneySnapshot(RopeResource owner, RopePlatformBuilder builder, bool tutorial)
        {
            var floors = new List<Floor>();
            Bridges = builder != null && owner != null && builder.gameObject.scene == owner.gameObject.scene
                ? builder.CaptureJourneyCurves() : new Vector2[0][];
            Remaining = owner != null ? owner.CurrentLength : 0f;
            Capacity = owner != null ? owner.MaximumLength : 0f;
            Vector2? start = null, goal = null;
            string startName = tutorial ? TutorialSectionOneSetup.StartFloorName : "Main Start Ground";
            string goalName = tutorial ? TutorialSectionFourSetup.GoalMarkerName : MainStageSectionTenSetup.GoalMarkerName;
            string goalFloorName = tutorial ? TutorialSectionFourSetup.GoalFloorName : MainStageSectionTenSetup.GoalFloorName;
            float? goalTop = null;
            if (owner != null)
            foreach (GameObject target in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (target.scene != owner.gameObject.scene || !target.activeInHierarchy) continue;
                if (target.name == goalName)
                {
                    goal = target.transform.position;
                    Transform image = target.transform.Find("Animated Goal Chest");
                    if (image != null && image.TryGetComponent(out SpriteRenderer sprite)) Chest = sprite.sprite;
                }
                if (!target.TryGetComponent(out BoxCollider2D box) || !box.enabled || box.isTrigger ||
                    target.GetComponentInParent<RopeResource>() != null ||
                    target.GetComponentInParent<HookPoint>() != null ||
                    target.GetComponentInParent<RopeSpikeHazard>() != null ||
                    target.GetComponentInParent<GeneratedRopePlatform>() != null) continue;
                bool rail = target.TryGetComponent(out OneWayRailPlatform _) ||
                    target.TryGetComponent(out SwingPassThroughRailPlatform _);
                bool terrain = target.TryGetComponent(out WoodenPlatformDepthVisual _) ||
                    (target.TryGetComponent(out SolidSprite _) &&
                     (target.name.StartsWith("Main ") || target.name.StartsWith("Tutorial "))) ||
                    target.name == startName || target.name == goalFloorName;
                if (!terrain && !rail) continue;
                floors.Add(new Floor(box.bounds, rail));
                if (target.name == startName) start = new Vector2(box.bounds.center.x, box.bounds.max.y);
                if (target.name == goalFloorName) goalTop = box.bounds.max.y;
            }
            floors.Sort((a, b) => a.Left.CompareTo(b.Left));
            Floors = floors.ToArray();
            Start = start ?? (floors.Count > 0 ? new Vector2(floors[0].Left, floors[0].Top) : Vector2.zero);
            Goal = goal ?? (floors.Count > 0 ? new Vector2(floors[^1].Right, floors[^1].Top) : Vector2.right);
            if (goalTop.HasValue) Goal = new Vector2(Goal.x, goalTop.Value);
            Vector2 min = Vector2.Min(Start, Goal), max = Vector2.Max(Start, Goal);
            foreach (Floor floor in Floors)
            {
                min = Vector2.Min(min, new Vector2(floor.Left, floor.Top));
                max = Vector2.Max(max, new Vector2(floor.Right, floor.Top));
            }
            foreach (Vector2[] curve in Bridges)
            foreach (Vector2 point in curve) { min = Vector2.Min(min, point); max = Vector2.Max(max, point); }
            float height = Mathf.Max(4f, max.y - min.y);
            World = new Rect(min.x - 1f, (min.y + max.y - height) * .5f,
                Mathf.Max(2f, max.x - min.x + 2f), height);
        }

        // Independent axes make the long main stage readable; the caption identifies this as a schematic.
        public Vector2 Project(Vector2 point, Rect plot) => new Vector2(
            Mathf.Lerp(plot.xMin, plot.xMax, Mathf.InverseLerp(World.xMin, World.xMax, point.x)),
            Mathf.Lerp(plot.yMax, plot.yMin, Mathf.InverseLerp(World.yMin, World.yMax, point.y)));
    }
}
