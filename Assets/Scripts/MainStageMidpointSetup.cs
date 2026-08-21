using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Replaces the retired upper/lower branch with one broad rest platform.
    /// The platform also carries the midpoint checkpoint before section six.
    /// </summary>
    public static class MainStageMidpointSetup
    {
        public const string MidpointName = "Main Midpoint Checkpoint";

        private static readonly Vector2 MidpointPosition = new Vector2(63.5f, -2.7f);
        private static readonly Vector2 MidpointSize = new Vector2(26.4f, 0.7f);
        private static readonly Vector2 RespawnPosition = new Vector2(69.7f, -1.65f);

        private static readonly string[] RetiredRouteNames =
        {
            "Main Upper Route Hook",
            "Main Upper Route Landing",
            "Main Upper Route Descent",
            "Main Upper Route Descent Marker",
            "Main Lower Walking Route"
        };

        public static GameObject EnsureCreated()
        {
            RemoveRetiredRoutes();

            GameObject midpoint = FindSceneObject(MidpointName);
            if (midpoint == null)
            {
                midpoint = new GameObject(MidpointName);
            }

            midpoint.SetActive(true);
            midpoint.transform.position = MidpointPosition;
            midpoint.transform.localScale = new Vector3(
                MidpointSize.x,
                MidpointSize.y,
                1f);

            if (!midpoint.TryGetComponent(out SpriteRenderer _))
            {
                midpoint.AddComponent<SpriteRenderer>();
            }

            if (!midpoint.TryGetComponent(out SolidSprite visual))
            {
                visual = midpoint.AddComponent<SolidSprite>();
            }

            visual.Color = new Color(0.38f, 0.41f, 0.52f);

            if (!midpoint.TryGetComponent(out BoxCollider2D collider))
            {
                collider = midpoint.AddComponent<BoxCollider2D>();
            }

            collider.size = Vector2.one;

            if (!midpoint.TryGetComponent(out MainStageCheckpoint checkpoint))
            {
                checkpoint = midpoint.AddComponent<MainStageCheckpoint>();
            }

            checkpoint.Configure(RespawnPosition);
            return midpoint;
        }

        private static void RemoveRetiredRoutes()
        {
            foreach (string objectName in RetiredRouteNames)
            {
                GameObject retiredObject = FindSceneObject(objectName);
                if (retiredObject == null)
                {
                    continue;
                }

                retiredObject.SetActive(false);
                if (Application.isPlaying)
                {
                    Object.Destroy(retiredObject);
                }
                else
                {
                    Object.DestroyImmediate(retiredObject);
                }
            }
        }

        private static GameObject FindSceneObject(string name)
        {
            foreach (GameObject candidate in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (candidate.scene.IsValid() && candidate.name == name)
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
