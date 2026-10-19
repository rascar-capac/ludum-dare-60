using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Respawn point placed in the level. It is reached as soon as the player goes past the x of transform.position,
    /// and the player then respawns at spawnPoint (see PlayerDeath).
    /// The level starts at the most advanced checkpoint marked as start point, or at the first one if none is.
    /// A debug checkpoint is disabled in builds, which makes it easy to test a section without shipping its start point.
    /// Activation plays the "Checked" trigger of the optional Animator and flashes the painting to white.
    /// Reaching an end point ends the game (see PlayerDeath).
    /// </summary>
    public class Checkpoint : MonoBehaviour
    {
        static readonly List<Checkpoint> checkpoints = new();
        static readonly int CheckedHash = Animator.StringToHash("Checked");

        [Tooltip("Where the player respawns. Falls back to this transform when empty.")]
        [SerializeField] Transform spawnPoint;
        [SerializeField] CheckpointSettings settings;
        [Tooltip("The level starts at the most advanced start point.")]
        [SerializeField] bool startPoint;
        [Tooltip("Disabled in builds.")]
        [SerializeField] bool debug;
        [Tooltip("Reaching it ends the game: the player can't move anymore and the camera stops.")]
        [SerializeField] bool endPoint;

        Animator animator;
        PaintingFlash painting;

        public float ActivationX => transform.position.x;
        public bool IsEndPoint => endPoint;
        public Vector3 SpawnPosition => spawnPoint != null ? spawnPoint.position : transform.position;

        void Awake()
        {
#if !UNITY_EDITOR
            if (debug)
            {
                gameObject.SetActive(false);
                return;
            }
#endif
            animator = GetComponent<Animator>();
            painting = GetComponent<PaintingFlash>();
        }

        void OnEnable() => checkpoints.Add(this);
        void OnDisable() => checkpoints.Remove(this);

        /// <summary>Called when the player goes past this checkpoint and it becomes the respawn point.</summary>
        public void Activate()
        {
            if (animator != null)
            {
                animator.SetTrigger(CheckedHash);
            }
            if (painting != null && settings != null)
            {
                StopAllCoroutines();
                StartCoroutine(FlashSequence());
            }
        }

        IEnumerator FlashSequence()
        {
            for (float time = 0f; time < settings.FlashInDuration; time += Time.deltaTime)
            {
                painting.Flash = time / settings.FlashInDuration;
                yield return null;
            }
            for (float time = 0f; time < settings.FlashOutDuration; time += Time.deltaTime)
            {
                painting.Flash = 1f - time / settings.FlashOutDuration;
                yield return null;
            }
            painting.Flash = 0f;
        }

        /// <summary>Returns the most advanced checkpoint activated at or before x, or null if none is.</summary>
        public static Checkpoint GetLastReached(float x)
        {
            Checkpoint last = null;
            foreach (Checkpoint checkpoint in checkpoints)
            {
                if (checkpoint.ActivationX <= x && (last == null || checkpoint.ActivationX > last.ActivationX))
                {
                    last = checkpoint;
                }
            }
            return last;
        }

        /// <summary>Returns the least advanced checkpoint, or null if there is none.</summary>
        public static Checkpoint GetFirst()
        {
            Checkpoint first = null;
            foreach (Checkpoint checkpoint in checkpoints)
            {
                if (first == null || checkpoint.ActivationX < first.ActivationX)
                {
                    first = checkpoint;
                }
            }
            return first;
        }

        /// <summary>Returns the most advanced checkpoint marked as start point, or the first one if none is, or null if there is none.</summary>
        public static Checkpoint GetStart()
        {
            Checkpoint start = null;
            foreach (Checkpoint checkpoint in checkpoints)
            {
                if (checkpoint.startPoint && (start == null || checkpoint.ActivationX > start.ActivationX))
                {
                    start = checkpoint;
                }
            }
            return start != null ? start : GetFirst();
        }

        void OnDrawGizmos()
        {
            Vector3 position = transform.position;
            Vector3 spawn = SpawnPosition;
            Gizmos.color = debug ? new Color(1f, 0.6f, 0.1f, 0.8f) : new Color(0.3f, 1f, 0.4f, 0.8f);
            Gizmos.DrawLine(position + Vector3.down * 25f, position + Vector3.up * 25f);
            Gizmos.DrawLine(position, spawn);
            Gizmos.DrawWireSphere(spawn, 0.4f);
#if UNITY_EDITOR
            string label = endPoint ? "End Point" : debug ? "Debug Checkpoint" : "Checkpoint";
            UnityEditor.Handles.Label(position + new Vector3(0.3f, 9f, 0f), startPoint ? label + " (start)" : label);
#endif
        }
    }
}
