using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Finds authored objects in loaded scenes, including inactive objects.
    /// Runtime setup code uses this instead of duplicating a Resources scan.
    /// </summary>
    internal static class SceneObjectLookup
    {
        public static GameObject Find(
            string objectName,
            string requiredSceneName = null)
        {
            foreach (GameObject candidate in
                     Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (!candidate.scene.IsValid() ||
                    candidate.name != objectName ||
                    (!string.IsNullOrEmpty(requiredSceneName) &&
                     candidate.scene.name != requiredSceneName))
                {
                    continue;
                }

                return candidate;
            }

            return null;
        }
    }
}
