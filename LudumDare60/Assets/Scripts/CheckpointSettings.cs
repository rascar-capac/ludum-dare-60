using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Feedback of a checkpoint when the player activates it.
    /// </summary>
    [CreateAssetMenu(fileName = "CheckpointSettings", menuName = "LD60/Checkpoint Settings")]
    public class CheckpointSettings : ScriptableObject
    {
        [Tooltip("Duration of the fade of the painting to white.")]
        [SerializeField, Min(0f)] float flashInDuration = 0.1f;

        [Tooltip("Duration of the fade back from white to the original color.")]
        [SerializeField, Min(0f)] float flashOutDuration = 0.5f;

        public float FlashInDuration => flashInDuration;
        public float FlashOutDuration => flashOutDuration;
    }
}
