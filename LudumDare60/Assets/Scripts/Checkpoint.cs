using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Respawn point placed in the level. It is reached as soon as the player goes past the x of transform.position,
    /// and the player then respawns at spawnPoint (see PlayerDeath). The first one is the start of the level.
    /// Activation plays the "Checked" trigger of the optional Animator and flashes the painting to white.
    /// </summary>
    public class Checkpoint : MonoBehaviour
    {
        static readonly List<Checkpoint> checkpoints = new();
        static readonly int CheckedHash = Animator.StringToHash("Checked");

        [Tooltip("Where the player respawns. Falls back to this transform when empty.")]
        [SerializeField] Transform spawnPoint;
        [SerializeField] CheckpointSettings settings;

        Animator animator;
        PaintingFlash painting;

        public float ActivationX => transform.position.x;
        public Vector3 SpawnPosition => spawnPoint != null ? spawnPoint.position : transform.position;

        void Awake()
        {
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

        void OnDrawGizmos()
        {
            Vector3 position = transform.position;
            Vector3 spawn = SpawnPosition;
            Gizmos.color = new Color(0.3f, 1f, 0.4f, 0.8f);
            Gizmos.DrawLine(position + Vector3.down * 25f, position + Vector3.up * 25f);
            Gizmos.DrawLine(position, spawn);
            Gizmos.DrawWireSphere(spawn, 0.4f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(position + new Vector3(0.3f, 9f, 0f), "Checkpoint");
            UnityEditor.Handles.Label(spawn + new Vector3(0.3f, 1f, 0f), "Spawn");
#endif
        }
    }
}
