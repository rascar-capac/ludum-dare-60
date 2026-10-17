using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Timing of the death and respawn sequence of the player.
    /// </summary>
    [CreateAssetMenu(fileName = "RespawnSettings", menuName = "LD60/Respawn Settings")]
    public class RespawnSettings : ScriptableObject
    {
        [Tooltip("Time between the death burst and the camera starting to move back.")]
        [SerializeField, Min(0f)] float deathDelay = 0.8f;

        [Tooltip("Duration of the camera move back to its start position.")]
        [SerializeField, Min(0f)] float cameraReturnDuration = 1.2f;

        [Tooltip("Duration of the alpha fade in of the respawned player.")]
        [SerializeField, Min(0f)] float fadeInDuration = 0.6f;

        public float DeathDelay => deathDelay;
        public float CameraReturnDuration => cameraReturnDuration;
        public float FadeInDuration => fadeInDuration;
    }
}
