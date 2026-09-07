using UnityEngine;

namespace HimoHito
{
    /// <summary>User volume multipliers; 100% preserves the authored mix.</summary>
    public static class HimoHitoAudioSettings
    {
        private const string MusicKey = "HimoHito.Audio.Music";
        private const string EffectsKey = "HimoHito.Audio.Effects";
        private static bool loaded;
        private static float music, effects;
        public static float Music { get { Load(); return music; } }
        public static float Effects { get { Load(); return effects; } }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { loaded = false; }

        private static float Sanitize(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value) ? 1f : Mathf.Clamp01(value);
        }

        private static void Load()
        {
            if (loaded) return;
            music = Sanitize(PlayerPrefs.GetFloat(MusicKey, 1f));
            effects = Sanitize(PlayerPrefs.GetFloat(EffectsKey, 1f));
            loaded = true;
        }

        public static void SetMusic(float value)
        {
            Load(); music = Sanitize(value); PlayerPrefs.SetFloat(MusicKey, music);
        }

        public static void SetEffects(float value)
        {
            Load(); effects = Sanitize(value); PlayerPrefs.SetFloat(EffectsKey, effects);
        }

        public static void Save() { if (loaded) PlayerPrefs.Save(); }
    }
}
