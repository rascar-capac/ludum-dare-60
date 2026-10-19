using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Plays the sound effects of the game as one-shots on a 2D AudioSource, so that the sound of an object
    /// that is deactivated or destroyed right away is not cut. Play does nothing when there is no SfxPlayer in the scene.
    /// The volume follows AudioPreferences.SoundVolume.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SfxPlayer : MonoBehaviour
    {
        static SfxPlayer instance;

        AudioSource source;

        void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
        }

        void OnEnable() => instance = this;

        void OnDisable()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        public static void Play(AudioClip clip)
        {
            if (instance != null && clip != null)
            {
                instance.source.PlayOneShot(clip, AudioPreferences.SoundVolume);
            }
        }
    }
}
