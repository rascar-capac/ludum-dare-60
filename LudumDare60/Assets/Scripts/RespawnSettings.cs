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

        [Tooltip("Duration of the camera move from the main menu to the start point.")]
        [SerializeField, Min(0f)] float beginCameraDuration = 3f;

        [Tooltip("Duration of the alpha fade in of the respawned player.")]
        [SerializeField, Min(0f)] float fadeInDuration = 0.6f;

        [Tooltip("Horizontal position of the spawn point on screen once the camera is placed: 0 = left edge, 1 = right edge.")]
        [SerializeField, Range(0f, 1f)] float spawnScreenX = 0.25f;

        public float DeathDelay => deathDelay;
        public float CameraReturnDuration => cameraReturnDuration;
        public float BeginCameraDuration => beginCameraDuration;
        public float FadeInDuration => fadeInDuration;
        public float SpawnScreenX => spawnScreenX;
    }
}
