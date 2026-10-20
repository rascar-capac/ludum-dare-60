using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Item lying in the level that the player collects by touching its trigger: the pickup is deactivated,
    /// the optional burst is spawned at its position and the weapon is added to the player's stock.
    /// Not collected while the player's stock is full.
    /// Its GameObject is only deactivated, so that ResetAll can bring it back with an alpha fade in.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Pickup : MonoBehaviour
    {
        static readonly List<Pickup> pickups = new();

        [SerializeField] WeaponType weapon;
        [SerializeField] ParticleSystem burstPrefab;
        [SerializeField] AudioClip collectSound;
        [Tooltip("Gives the duration of the fade in when the pickup comes back.")]
        [SerializeField] RespawnSettings respawnSettings;

        SpriteRenderer[] spriteRenderers;

        void Awake()
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            // Registered for the whole lifetime, not only while active, so that collected pickups can be reset.
            pickups.Add(this);
        }

        void OnDestroy() => pickups.Remove(this);

        /// <summary>Makes every collected pickup available again, with a fade in.</summary>
        public static void ResetAll()
        {
            foreach (Pickup pickup in pickups)
            {
                pickup.Respawn();
            }
        }

        void Respawn()
        {
            if (gameObject.activeSelf)
            {
                return;
            }
            gameObject.SetActive(true);
            StartCoroutine(FadeIn());
        }

        IEnumerator FadeIn()
        {
            float duration = respawnSettings != null ? respawnSettings.FadeInDuration : 0f;
            for (float time = 0f; time < duration; time += Time.deltaTime)
            {
                SetAlpha(time / duration);
                yield return null;
            }
            SetAlpha(1f);
        }

        void SetAlpha(float alpha)
        {
            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            {
                Color color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            PlayerWeapon playerWeapon = other.attachedRigidbody != null
                ? other.attachedRigidbody.GetComponent<PlayerWeapon>()
                : null;
            if (playerWeapon == null || !playerWeapon.TryAdd(weapon))
            {
                return;
            }

            gameObject.SetActive(false);
            SfxPlayer.Play(collectSound);
            if (burstPrefab != null)
            {
                Vector3 position = transform.position;
                position.z = burstPrefab.transform.position.z;
                Instantiate(burstPrefab, position, burstPrefab.transform.rotation);
            }
        }
    }
}
