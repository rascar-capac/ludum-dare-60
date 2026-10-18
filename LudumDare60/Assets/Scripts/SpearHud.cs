using UnityEngine;
using UnityEngine.UIElements;

namespace LD60
{
    /// <summary>
    /// Shows the player's spear stock in the HUD: the first SpearCount slots (in UXML order) get the full class,
    /// the others show the empty slot.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class SpearHud : MonoBehaviour
    {
        const string SlotClass = "spear-slot";
        const string FullClass = "spear-slot--full";

        [SerializeField] PlayerWeapon weapon;

        UQueryState<VisualElement> slots;
        int shownCount = -1;

        void OnEnable()
        {
            slots = GetComponent<UIDocument>().rootVisualElement.Query(className: SlotClass).Build();
            shownCount = -1;
        }

        void Update()
        {
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
