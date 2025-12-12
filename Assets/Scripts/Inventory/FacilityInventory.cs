using UnityEngine;
using Food;

namespace Inventory {
    public class FacilityInventory : MonoBehaviour {
        [Header("Configuration")]
        public int slotCount = 8;
        public InventorySlot[] slots;
        
        [Header("Visuals")]
        [Tooltip("Points d'ancrage 3D pour afficher les objets (ex: étagères du frigo)")]
        public Transform[] slotTransforms; 

        private void Awake() {
            if (slots == null || slots.Length != slotCount) {                   // Initiate facility inventory
                slots = new InventorySlot[slotCount];
                for (int i = 0; i < slotCount; i++)
                    slots[i] = new InventorySlot();
            }
        }

        public ItemBase GetItem(int index) {
            if (index < 0 || index >= slots.Length) return null;
            return slots[index].item;
        }

        public ItemBase[] GetAllItems() {
            ItemBase[] items = new ItemBase[slots.Length];
            for (int i = 0; i < slots.Length; i++)
                items[i] = slots[i].item;
            return items;
        }

        public bool AddItem(ItemBase newItem) {
            for (int i = 0; i < slots.Length; i++) {                            // Look for empty slot
                if (slots[i].IsEmpty) {
                    SetItem(i, newItem);
                    return true;
                }
            }
            return false;                                                       // Wasn't enable to add item
        }

        public void SetItem(int index, ItemBase newItem) {                      // Place in specific slot
            if (index < 0 || index >= slots.Length) return;
            if (!newItem) return;

            slots[index].item = newItem;                                        // Update data

            Transform targetParent = slotTransforms != null && index < slotTransforms.Length ? 
                slotTransforms[index] : transform;                              // Put in slot or this

            newItem.OnPlace(targetParent);                                      // Place item in slot (disable physics)
            newItem.gameObject.SetActive(true);                                 // Display item
        }

        public ItemBase RemoveItem(int index) {                                 // Remove item from inventory
            if (index < 0 || index >= slots.Length) return null;
            if (slots[index].IsEmpty) return null;

            ItemBase itemToRemove = slots[index].item;

            slots[index].item = null;                                           // Empty slot

            return itemToRemove;                                                // Physics is still disable
        }
    }
}
