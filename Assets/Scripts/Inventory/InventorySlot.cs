using Food;

namespace Inventory {
    [System.Serializable]
    public class InventorySlot {
        public ItemBase item;
        public bool IsEmpty => !item;
    }
}
