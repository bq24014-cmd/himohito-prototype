using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace HimoHitoEditor
{
    // The licensed source is deliberately excluded from Git. Do not silently
    // produce a music-free distribution from a fresh clone.
    public sealed class BackgroundMusicBuildCheck : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (AssetDatabase.LoadAssetAtPath<AudioClip>(
                "Assets/Resources/Audio/YasashiiOdori.mp3") == null)
                throw new BuildFailedException(
                    "BGM「優しい踊り」がありません。READMEのBGM導入手順で公式音源を配置してください。");
        }
    }
}
