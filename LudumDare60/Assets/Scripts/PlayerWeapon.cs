using UnityEngine;

namespace LD60
{
    public enum WeaponType
    {
        None,
        Spear,
    }

    /// <summary>
    /// Stock of spears carried by the avatar, up to MaxSpears. The held spear is shown while the stock is not empty
    /// and HeldItemHidden is false.
    /// </summary>
    public class PlayerWeapon : MonoBehaviour
    {
        [SerializeField, Min(1)] int maxSpears = 5;
        [SerializeField, Min(0)] int spearCount;

        [Header("Held items")]
        [SerializeField] GameObject spear;

        bool heldItemHidden;

        public int MaxSpears => maxSpears;

        public int SpearCount
        {
            get => spearCount;
            set
            {
                spearCount = Mathf.Clamp(value, 0, maxSpears);
                UpdateHeldItems();
            }
        }

        /// <summary>Hides the held spear whatever the stock, e.g. while a throw is playing.</summary>
        public bool HeldItemHidden
        {
            get => heldItemHidden;
            set
            {
                heldItemHidden = value;
                UpdateHeldItems();
            }
        }

        public WeaponType Weapon => spearCount > 0 ? WeaponType.Spear : WeaponType.None;

        public bool HasWeapon => Weapon != WeaponType.None;

        void Awake()
        {
            SpearCount = spearCount;
        }

        /// <summary>Adds one weapon of the given type to the stock. Returns false when the stock is already full.</summary>
        public bool TryAdd(WeaponType type)
        {
            if (type != WeaponType.Spear || spearCount >= maxSpears)
            {
                return false;
            }
            SpearCount++;
            return true;
        }

        void UpdateHeldItems()
        {
            if (spear != null)
            {
                spear.SetActive(spearCount > 0 && !heldItemHidden);
            }
        }
    }
}
