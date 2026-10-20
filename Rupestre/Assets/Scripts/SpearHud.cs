using UnityEngine;
using UnityEngine.UIElements;

namespace LD60
{
    /// <summary>
    /// Shows the player's spear stock in the HUD: the first SpearCount slots (in UXML order) get the full class,
    /// the others show the empty slot. The slots are hidden until the player has picked up their first spear.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class SpearHud : MonoBehaviour
    {
        const string SlotClass = "spear-slot";
        const string FullClass = "spear-slot--full";
        const string SlotsClass = "spear-slots";
        const string LockedClass = "spear-slots--locked";

        [SerializeField] PlayerWeapon weapon;

        VisualElement slotsContainer;
        UQueryState<VisualElement> slots;
        int shownCount = -1;

        void OnEnable()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            slotsContainer = root.Q(className: SlotsClass);
            slots = root.Query(className: SlotClass).Build();
            shownCount = -1;
        }

        void Update()
        {
            slotsContainer?.EnableInClassList(LockedClass, weapon == null || !weapon.SpearsUnlocked);

            int count = weapon != null ? weapon.SpearCount : 0;
            if (count == shownCount)
            {
                return;
            }

            shownCount = count;
            int index = 0;
            slots.ForEach(slot => slot.EnableInClassList(FullClass, index++ < count));
        }
    }
}
