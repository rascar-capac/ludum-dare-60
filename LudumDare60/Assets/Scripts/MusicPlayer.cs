using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Plays the music loops from the start of the game, after a delay, fading in from silence.
    /// With WaitForBegin, the menu loop plays until Begin is called. The theme loop then starts when the menu loop restarts,
    /// scheduled on the audio clock to stay in time, and replaces it at once: the menu loop keeps playing at volume 0.
    /// Otherwise the theme loop plays right away.
    /// The delay and fade use unscaled time, clamped like the scaled one, so they are not affected by Time.timeScale.
    /// The volume is scaled by AudioPreferences.MusicVolume.
    /// </summary>
    public class MusicPlayer : MonoBehaviour
    {
        // Margin between the scheduling of a loop and its start, so that the audio thread doesn't start it late.
        const double ScheduleLead = 0.05;

        [SerializeField] MusicSettings settings;
        [SerializeField] AudioSource menu;
        [SerializeField] AudioSource theme;

        float elapsed;
        bool started;
        bool themeScheduled;
        double menuStartTime;
        double themeStartTime;
        bool themeStarted;

        /// <summary>Set before Start, by the MainMenu, to play the menu loop until Begin is called.</summary>
        public bool WaitForBegin { get; set; }

        void Awake()
        {
            foreach (AudioSource source in new[] { menu, theme })
            {
                if (source != null)
                {
                    source.loop = true;
                    source.playOnAwake = false;
                    source.volume = 0f;
                }
            }
        }

        void Start()
        {
            elapsed = 0f;
            started = false;
            themeScheduled = false;
            themeStarted = false;
            if (menu == null)
            {
                WaitForBegin = false;
            }
        }

        /// <summary>Ends WaitForBegin: switches from the menu loop to the theme loop when the menu loop restarts.</summary>
        public void Begin() => WaitForBegin = false;

        void Update()
        {
            if (settings == null || theme == null)
            {
                return;
            }
            // Unscaled time is not clamped: the first frames after the scene load can last long enough to skip the fade.
            float deltaTime = Mathf.Min(Time.unscaledDeltaTime, Time.maximumDeltaTime);
            elapsed += deltaTime;
            float fadeTime = elapsed - settings.StartDelay;
            if (fadeTime < 0f)
            {
                return;
            }

            if (!started)
            {
                double startTime = AudioSettings.dspTime + ScheduleLead;
                if (WaitForBegin)
                {
                    menu.PlayScheduled(startTime);
                    menuStartTime = startTime;
                }
                else
                {
                    // Begin pressed before the music starts, or no menu loop: the theme loop is heard right away.
                    PlayThemeAt(startTime);
                }
                started = true;
            }
            if (!WaitForBegin && !themeScheduled)
            {
                double loopDuration = (double)menu.clip.samples / menu.clip.frequency;
                double loopCount = System.Math.Floor((AudioSettings.dspTime - menuStartTime) / loopDuration) + 1;
                PlayThemeAt(menuStartTime + loopCount * loopDuration);
            }
            if (themeScheduled && AudioSettings.dspTime >= themeStartTime)
            {
                themeStarted = true;
            }

            float fade = settings.FadeInDuration > 0f ? Mathf.Clamp01(fadeTime / settings.FadeInDuration) : 1f;
            // Squared so the perceived loudness rises evenly instead of jumping at the start.
            float volume = fade * fade * settings.Volume * AudioPreferences.MusicVolume;
            if (menu != null)
            {
                menu.volume = themeStarted ? 0f : volume;
            }
            // Already up while scheduled, since it is silent until it starts.
            theme.volume = themeScheduled ? volume : 0f;
        }

        void PlayThemeAt(double time)
        {
            theme.PlayScheduled(time);
            themeStartTime = time;
            themeScheduled = true;
        }
    }
}
