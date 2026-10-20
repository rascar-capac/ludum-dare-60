using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Volumes chosen in the settings of the MainMenu, from 0 to 1, saved in the PlayerPrefs.
    /// SoundVolume scales the SfxPlayer, MusicVolume the MusicPlayer.
    /// </summary>
    public static class AudioPreferences
    {
        const string SoundVolumeKey = "SoundVolume";
        const string MusicVolumeKey = "MusicVolume";

        static float? soundVolume;
        static float? musicVolume;

        public static float SoundVolume
        {
            get => soundVolume ??= PlayerPrefs.GetFloat(SoundVolumeKey, 1f);
            set
            {
                soundVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(SoundVolumeKey, soundVolume.Value);
            }
        }

        public static float MusicVolume
        {
            get => musicVolume ??= PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
            set
            {
                musicVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume.Value);
            }
        }

        /// <summary>Writes the volumes to disk, which some platforms (WebGL) only do on demand.</summary>
        public static void Save() => PlayerPrefs.Save();
    }
}
