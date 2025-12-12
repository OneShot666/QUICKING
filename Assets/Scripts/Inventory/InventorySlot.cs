using Food;

namespace Inventory {
    [System.Serializable]
    public class InventorySlot {
        public bool IsEmpty => !item;
        public ItemBase item;
    }
}
