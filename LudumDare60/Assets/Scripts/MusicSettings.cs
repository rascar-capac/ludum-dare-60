using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Music tuning. The clips stay on the AudioSources of the MusicPlayer.
    /// </summary>
    [CreateAssetMenu(fileName = "MusicSettings", menuName = "LD60/Music Settings")]
    public class MusicSettings : ScriptableObject
    {
        [SerializeField, Range(0f, 1f)] float volume = 1f;

        [Tooltip("Delay between the start of the game and the start of the music, in seconds.")]
        [SerializeField, Min(0f)] float startDelay = 1f;

        [Tooltip("Duration of the fade in from silence when the music starts, in seconds.")]
        [SerializeField, Min(0f)] float fadeInDuration = 3f;

        public float Volume => volume;
        public float StartDelay => startDelay;
        public float FadeInDuration => fadeInDuration;
    }
}
