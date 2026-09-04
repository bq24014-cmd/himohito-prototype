using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace HimoHitoEditor
{
    /// <summary>
    /// Applies the submission-facing settings from the seventh-session deck.
    /// The build command calls this immediately before creating the player.
    /// </summary>
    public static class ProjectPresentationSettings
    {
        private const string IconPath =
            "Assets/Resources/Art/HimoHitoAppIcon-v1.png";

        [InitializeOnLoadMethod]
        private static void ScheduleApplyAfterImport()
        {
            EditorApplication.delayCall += ApplyIfIconIsReady;
        }

        [MenuItem("HimoHito/Apply Submission Presentation Settings")]
        public static void ApplyWithFeedback()
        {
            Apply();
            Debug.Log("HimoHito submission presentation settings applied.");
        }

        public static void Apply()
        {
            PlayerSettings.companyName = "bq24014-cmd";
            PlayerSettings.productName = "ヒモヒト";
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = false;

            PlayerSettings.SplashScreen.show = true;
            PlayerSettings.SplashScreen.showUnityLogo = true;
            PlayerSettings.SplashScreen.backgroundColor =
                new Color(0.035f, 0.031f, 0.094f, 1f);

            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            Sprite splashLogo = AssetDatabase.LoadAssetAtPath<Sprite>(IconPath);
            if (icon == null || splashLogo == null)
            {
                throw new InvalidOperationException(
                    $"Submission icon was not imported correctly: {IconPath}");
            }

            int iconSlotCount = PlayerSettings.GetIconSizes(
                NamedBuildTarget.Standalone,
                IconKind.Application).Length;
            if (iconSlotCount <= 0)
            {
                throw new InvalidOperationException(
                    "No Standalone application icon slots are available.");
            }

            Texture2D[] icons = new Texture2D[iconSlotCount];
            Array.Fill(icons, icon);
            PlayerSettings.SetIcons(
                NamedBuildTarget.Standalone,
                icons,
                IconKind.Application);

            PlayerSettings.SplashScreen.logos = new[]
            {
                PlayerSettings.SplashScreenLogo.Create(2f, splashLogo),
                PlayerSettings.SplashScreenLogo.CreateWithUnityLogo(2f)
            };
        }

        private static void ApplyIfIconIsReady()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += ApplyIfIconIsReady;
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath) != null)
            {
                Apply();
            }
        }
    }
}
