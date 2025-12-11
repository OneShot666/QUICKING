using UnityEngine;

[System.Serializable]
public class Inventory : MonoBehaviour
{
    public int slotCount = 8;
    public InventorySlot[] slots;
    public Transform[] slotTransforms; // Emplacements physiques dans la scène

    void Awake()
    {
        if (slots == null || slots.Length != slotCount)
        {
            slots = new InventorySlot[slotCount];
            for (int i = 0; i < slotCount; i++)
                slots[i] = new InventorySlot();
        }
    }

    public Item GetItem(int index)
    {
        if (index < 0 || index >= slots.Length) return null;
        return slots[index].item;
    }

    public void SetItem(int index, Item item, GameObject itemObject)
    {
        if (index < 0 || index >= slots.Length) return;
        // Retirer l'ancien objet du slot si présent
        if (slots[index].itemObject != null)
        {
            slots[index].itemObject.SetActive(false);
            slots[index].itemObject.transform.SetParent(null);
        }
        slots[index].item = item;
        slots[index].itemObject = itemObject;
        if (itemObject != null && slotTransforms != null && index < slotTransforms.Length)
        {
            itemObject.SetActive(true);
            itemObject.transform.SetParent(slotTransforms[index]);
            itemObject.transform.localPosition = Vector3.zero;
            itemObject.transform.localRotation = Quaternion.identity;
            itemObject.transform.localScale = Vector3.one;
        }
    }

    public void RemoveItem(int index)
    {
        if (index < 0 || index >= slots.Length) return;
        if (slots[index].itemObject != null)
        {
            slots[index].itemObject.SetActive(false);
            slots[index].itemObject.transform.SetParent(null);
        }
        slots[index].item = null;
        slots[index].itemObject = null;
    }

    public Item[] GetAllItems()
    {
        Item[] items = new Item[slots.Length];
        for (int i = 0; i < slots.Length; i++)
            items[i] = slots[i].item;
        return items;
    }

    public GameObject[] GetAllItemObjects()
    {
        GameObject[] objects = new GameObject[slots.Length];
        for (int i = 0; i < slots.Length; i++)
            objects[i] = slots[i].itemObject;
        return objects;
    }
}
