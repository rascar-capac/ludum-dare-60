using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Kills the player when they touch one of this object's colliders.
    /// Works with static colliders as well: the player's Rigidbody2D is enough to receive the collision.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class KillsPlayerOnContact : MonoBehaviour
    {
        void OnCollisionEnter2D(Collision2D collision) => TryKill(collision);

        void OnCollisionStay2D(Collision2D collision) => TryKill(collision);

        static void TryKill(Collision2D collision)
        {
            if (collision.rigidbody != null && collision.rigidbody.TryGetComponent(out PlayerDeath player))
            {
                player.Kill();
            }
        }
    }
}
