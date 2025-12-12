using UnityEngine;

// ! Add sound when done cooking + burning
// ! Make utensil class that heritage from ItemBase
// ReSharper disable Unity.PerformanceCriticalCodeInvocation
// ReSharper disable MemberCanBePrivate.Global
namespace Food {
    public enum ItemType { Food, Utensil, Misc }                                // To filter interactions

    [SelectionBase]                                                             // Helps to select the root object in Scene View
    public abstract class ItemBase : MonoBehaviour {
        [Header("References")]
        [Tooltip("Parent object where to place food items")]
        [SerializeField] protected Transform foodContainer;

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
        protected bool isInteractable = true;

        private void Awake() {
            gameObject.layer = LayerMask.NameToLayer("Interactable");
        }

        public virtual void OnInteract() {
            if (!isInteractable) return;
        
            OnCollect();                                                        // Default behavior: Collect the item
        }

        protected virtual void OnCollect() {                                    // ! On going function
            // TODO: Call InventoryManager.Instance.AddItem(this);
            gameObject.SetActive(false);                                        // Disable renderer/collider
        }

        public virtual void OnEquip(Transform handTransform) {
            isInteractable = false;                                             // Cannot be picked up again while held
        
            if (TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;  // Stop physics
            if (TryGetComponent<Collider>(out var col)) col.enabled = false;    // Prevent collision with player

            transform.SetParent(handTransform);                                 // Attach to hand
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        public virtual void OnStore(Transform containerTransform) {             // Store inside a container
            // TODO: Call containerTransform.Instance.AddItem(this);
            Destroy(gameObject);
        }

        public virtual void OnDrop() {                                          // Drop item
            isInteractable = true;

            if (!foodContainer) {
                GameObject container = GameObject.Find("Foods");
                if (!container) container = new GameObject("Foods");            // Create if doesn't exists
                foodContainer = container.transform;
            }
            transform.SetParent(foodContainer);                                 // Replace in parent

            if (TryGetComponent<Rigidbody>(out var rb)) {                       // Reactivate physics
                rb.isKinematic = false;
                rb.detectCollisions = true;
                rb.AddForce(transform.forward * 2f + Vector3.up * 1f, ForceMode.Impulse);   // Small push forward
            }

            if (TryGetComponent<Collider>(out var col)) col.enabled = true;
        }

        public virtual void OnPlace(Transform surfacePoint) {                   // Place item in object
            isInteractable = true;
            transform.SetParent(surfacePoint);                                  // Place item as children of object

            transform.localPosition = Vector3.zero;                             // Place object
            transform.localRotation = Quaternion.identity;

            if (TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;  // Disable physics
            if (TryGetComponent<Collider>(out var col)) col.enabled = true;     // Keep collider for raycast
        }

        /* Getters */
        public string GetName() => itemName;

        public string GetDescription() => itemDescription;

        public Sprite GetIcon() => icon;

        public ItemType GetItemType() => itemType;

        protected virtual void Reset() {
            itemName = gameObject.name.Replace("(Clone)", "").Replace("_", " ");    // Create proper name
        }
    }
}