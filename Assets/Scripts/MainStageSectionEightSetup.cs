using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Disables the former rope-allocation section after normal detach became free.
    /// </summary>
    public static class MainStageSectionEightSetup
    {
        public const string FirstHookName = "Main Section 8 Hook A";
        public const string PlanningLandingName = "Main Section 8 Planning Landing";
        public const string SecondHookName = "Main Section 8 Hook B";
        public const string FinalLandingName = "Main Section 8 Legacy Landing";

        public static void DisableLegacyObjects()
        {
            GameObject sectionSevenLanding =
                FindSceneObject(MainStageSectionSevenSetup.LandingName);
            if (sectionSevenLanding != null &&
                sectionSevenLanding.TryGetComponent(
                    out MainStageSectionEightCheckpoint checkpoint))
            {
                checkpoint.enabled = false;
            }

            string[] legacyObjectNames =
            {
                FirstHookName,
                PlanningLandingName,
                SecondHookName,
                FinalLandingName
            };

            foreach (string objectName in legacyObjectNames)
            {
                GameObject legacyObject = FindSceneObject(objectName);
                if (legacyObject != null)
                {
                    legacyObject.SetActive(false);
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
