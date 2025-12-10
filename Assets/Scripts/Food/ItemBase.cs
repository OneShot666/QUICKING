using UnityEngine;

// ReSharper disable MemberCanBePrivate.Global
public enum ItemType { Food, Utensil, Misc }                                    // To filter interactions

[SelectionBase]                                                                 // Helps to select the root object in Scene View
public abstract class ItemBase : MonoBehaviour {
    [Header("Data / UI Info")]
    [Tooltip("Name displayed in the UI")]
    [SerializeField] protected string itemName;
    [Tooltip("Description displayed in the overlay or inventory")]
    [SerializeField] [TextArea] protected string itemDescription;
    [Tooltip("Icon for the Inventory UI")]
    [SerializeField] protected Sprite icon;
    [Tooltip("Category of the item")]
    [SerializeField] protected ItemType itemType;

    [Header("State")]
    [Tooltip("Is the item currently pickable?")]
    protected bool IsInteractable = true;

    public virtual void OnInteract() {
        if (!IsInteractable) return;
        
        OnCollect();                                                            // Default behavior: Collect the item
    }

    protected virtual void OnCollect() {                                        // ! On going function
        // TODO: Call InventoryManager.Instance.AddItem(this);
        gameObject.SetActive(false);                                            // Disable renderer/collider
    }

    public virtual void OnEquip(Transform handTransform) {
        IsInteractable = false;                                                 // Cannot be picked up again while held
        
        if (TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;      // Stop physics
        if (TryGetComponent<Collider>(out var col)) col.enabled = false;        // Prevent collision with player

        transform.SetParent(handTransform);                                     // Attach to hand
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public virtual void OnStore(Transform containerTransform) {                 // Store inside a container
        // TODO: Call containerTransform.Instance.AddItem(this);
        Destroy(gameObject);
    }

    public virtual void OnDrop() {                                              // Drop item
        IsInteractable = true;
        transform.SetParent(null);                                              // Detach from hand

        if (TryGetComponent<Rigidbody>(out var rb)) {                           // Reactivate physics
            rb.isKinematic = false;
            rb.detectCollisions = true;
            rb.AddForce(transform.forward * 2f + Vector3.up * 1f, ForceMode.Impulse);   // Small push forward
        }

        if (TryGetComponent<Collider>(out var col)) col.enabled = true;
    }

    public virtual void OnPlace(Transform surfacePoint) {                       // Place item in object
        IsInteractable = true;
        transform.SetParent(surfacePoint);                                      // Place item as children of object

        transform.localPosition = Vector3.zero;                                 // Place object
        transform.localRotation = Quaternion.identity;

        if (TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;      // Disable physics
        if (TryGetComponent<Collider>(out var col)) col.enabled = true;         // Keep collider for raycast
    }

    /* Getters */
    public string GetName() => itemName;

    public string GetDescription() => itemDescription;

    public Sprite GetIcon() => icon;

    public ItemType GetItemType() => itemType;

    protected virtual void Reset() {
        itemName = gameObject.name.Replace("(Clone)", "").Replace("_", " ");
    }
}
