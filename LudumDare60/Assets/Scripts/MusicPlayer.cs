using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Plays the clip of the AudioSource in a loop from the start of the game, after a delay, fading in from silence.
    /// The delay and fade use unscaled time, clamped like the scaled one, so they are not affected by Time.timeScale.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class MusicPlayer : MonoBehaviour
    {
        [SerializeField] MusicSettings settings;

        AudioSource source;
        float elapsed;
        bool started;

        void Awake()
        {
            source = GetComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
        }

        void Start()
        {
            elapsed = 0f;
            started = false;
            source.volume = 0f;
        }

        void Update()
        {
            if (settings == null)
            {
                return;
            }
            // Unscaled time is not clamped: the first frames after the scene load can last long enough to skip the fade.
            elapsed += Mathf.Min(Time.unscaledDeltaTime, Time.maximumDeltaTime);
            float fadeTime = elapsed - settings.StartDelay;
            if (fadeTime < 0f)
            {
                return;
            }
            if (!started)
            {
                source.Play();
                started = true;
            }
            float fade = settings.FadeInDuration > 0f ? Mathf.Clamp01(fadeTime / settings.FadeInDuration) : 1f;
            // Squared so the perceived loudness rises evenly instead of jumping at the start.
            source.volume = fade * fade * settings.Volume;
        }
    }
}
