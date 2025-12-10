using UnityEngine.InputSystem;
using UnityEngine;

// . Making a two hands system
// . Use keys instead of player input
// ReSharper disable Unity.PerformanceCriticalCodeInvocation
public class PlayerInteraction : MonoBehaviour {
    [Header("Settings")]
    public float interactionDistance = 2.5f;
    [Tooltip("The layer for all items and surfaces that are interactable")]
    public LayerMask interactLayer;

    [Header("References")]
    public Transform handPoint;                                                 // Player's hand
    public Transform cameraTransform;                                           // For targeting

    private PlayerInput _playerInput;
    private InputAction _interactAction;
    private InputAction _dropAction;
    private ItemBase _heldItem;

    private void Awake() {
        _playerInput = GetComponent<PlayerInput>();
        _interactAction = _playerInput.actions["Interact"];
        _dropAction = _playerInput.actions["Drop"];
        
        if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;
    }

    private void Update() {
        HandleInteraction();
        HandleDrop();
    }

    private void HandleInteraction() {
        if (!_interactAction.WasPerformedThisFrame()) return;                   // Interact when pressed right key

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);   // Raycast from camera
        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.yellow, 1f);   // Display ray

        if (Physics.Raycast(ray, out var hit, interactionDistance, interactLayer)) {
            
            if (!_heldItem) {                                                   // If empty hand -> Pick up item
                ItemBase item = hit.collider.GetComponentInParent<ItemBase>();  // Check for item based of collider
                if (item) PickUpItem(item);
            } else {                                                            // Else -> Place item
                ItemSurface surface = hit.collider.GetComponent<ItemSurface>();
                if (surface && surface.IsAvailable()) PlaceItem(surface);
            }
        }
    }

    private void HandleDrop() {
        if (_dropAction.WasPerformedThisFrame() && _heldItem) DropItem();       // Try to drop item
    }

    private void PickUpItem(ItemBase item) {
        _heldItem = item;
        item.OnEquip(handPoint);
    }

    private void DropItem() {
        if (!_heldItem) return;

        _heldItem.OnDrop();
        _heldItem = null;
    }

    private void PlaceItem(ItemSurface surface) {
        if (!_heldItem) return;

        _heldItem.OnPlace(surface.GetPlacementTransform());
        _heldItem = null;
    }
}
