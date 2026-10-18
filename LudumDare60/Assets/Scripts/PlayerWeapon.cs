using UnityEngine;

namespace LD60
{
    public enum WeaponType
    {
        None,
        Spear,
    }

    /// <summary>
    /// Weapon currently held by the avatar, shown by activating the matching held item object.
    /// </summary>
    public class PlayerWeapon : MonoBehaviour
    {
        [SerializeField] WeaponType weapon;

        [Header("Held items")]
        [SerializeField] GameObject spear;

        public WeaponType Weapon
        {
            get => weapon;
            set
            {
                weapon = value;
                UpdateHeldItems();
            }
        }

        public bool HasWeapon => weapon != WeaponType.None;

        void Awake()
        {
            UpdateHeldItems();
        }

        void UpdateHeldItems()
        {
            if (spear != null)
            {
                spear.SetActive(weapon == WeaponType.Spear);
            }
        }
    }
}
