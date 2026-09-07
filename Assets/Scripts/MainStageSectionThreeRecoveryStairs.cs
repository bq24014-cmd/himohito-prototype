using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Keeps the lower-route recovery stairs out of both valid swing paths.
    /// They become solid and visible only after the player presses the switch
    /// on the low dead end, where they are needed to prevent a soft lock.
    /// </summary>
    public sealed class MainStageSectionThreeRecoveryStairs : MonoBehaviour
    {
        private bool revealed;

        public bool IsRevealed => revealed;

        public void Configure()
        {
            if (Application.isPlaying && !revealed)
            {
                SetStairsActive(false);
            }
        }

        private void Awake()
        {
            Configure();
        }

        public void Reveal()
        {
            if (revealed)
            {
                return;
            }

            revealed = true;
            SetStairsActive(true);
        }

        public void Hide()
        {
            if (!revealed)
            {
                SetStairsActive(false);
                return;
            }

            revealed = false;
            SetStairsActive(false);
        }

        private static void SetStairsActive(bool active)
        {
            SetActive(MainStageSectionThreeSetup.ReturnStepAName, active);
            SetActive(MainStageSectionThreeSetup.ReturnStepBName, active);
            SetActive(MainStageSectionThreeSetup.ReturnStepCName, active);
            SetActive(MainStageSectionThreeSetup.ReturnStepDName, active);
        }

        private static void SetActive(string objectName, bool active)
        {
            foreach (GameObject candidate in
                     Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (candidate.scene.IsValid() &&
                    candidate.scene.name == "MainStage" &&
                    candidate.name == objectName)
                {
                    candidate.SetActive(active);
                    return;
                }
            }
        }
    }
}
