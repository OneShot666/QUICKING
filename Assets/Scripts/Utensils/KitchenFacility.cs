using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine;
using Inventory;
using System;
using Player;
using Food;
using UI;

// ReSharper disable Unity.PerformanceCriticalCodeInvocation
namespace Utensils {
    public class KitchenFacility : BaseFacilityInteraction {
        public enum RotationAxis { X, Y, Z }

        [Header("Visual references")]
        public Light facilityLight;

        [Header("Settings")]
        public bool isDoorOpen;
        public bool isLightOn;
        public string interactionText = "Open door";
        public string openInteractionText = "Close door";                       // Display text when open

        [Header("Door animation")]
        public GameObject doorObject;
        public RotationAxis rotationAxis = RotationAxis.Y;
        public float closedAngle;
        public float openedAngle = 90f;
        [Tooltip("Speed of door animation (in degrees per second)")]
        public float doorAnimationSpeed = 80f;

        [Header("Auto-Close Settings")]
        public bool isAutoClose = true;
        [Tooltip("If set to 0, use player's range + 50%. Otherwise, use entered value.")]
        public float customCloseDistance; 

        [Header("Inventory")]
        public FacilityInventory facilityInventory;
        public int previewSlotCount = 5;

        private PlayerInteraction _playerScript; 
        private Quaternion _initialRotation;
        private Transform _playerTransform;
        private float _lastInteractionTime;
        private float _currentAngle;

        protected virtual void Start() {
            if (!facilityInventory) facilityInventory = GetComponent<FacilityInventory>();

            if (doorObject) {                                                   // Setup init door animation
                _initialRotation = doorObject.transform.localRotation;
                _currentAngle = isDoorOpen ? openedAngle : closedAngle;
            }
            
            isLightOn = isDoorOpen;
            if (facilityLight) facilityLight.enabled = isLightOn;

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj) {
                _playerTransform = playerObj.transform;
                _playerScript = playerObj.GetComponent<PlayerInteraction>();
            }
        }

        protected virtual void Update() {
            if (doorObject) {
                float targetAngle = isDoorOpen ? openedAngle : closedAngle;
                _currentAngle = Mathf.MoveTowards(_currentAngle, targetAngle, Time.deltaTime * doorAnimationSpeed);

                Vector3 axis = Vector3.up;
                switch (rotationAxis) {
                    case RotationAxis.X: axis = Vector3.right; break;
                    case RotationAxis.Y: axis = Vector3.up; break;
                    case RotationAxis.Z: axis = Vector3.forward; break;
                }

                doorObject.transform.localRotation = _initialRotation * Quaternion.AngleAxis(_currentAngle, axis);  // Fluid animation
            }

            if (isDoorOpen) CheckAutoClose();
        }

        private void CheckAutoClose() {
            if (Time.time < _lastInteractionTime + 0.2f) return;                // Anti-spam clic

            if (isAutoClose && _playerTransform && _playerScript) {
                float dist = Vector3.Distance(transform.position, _playerTransform.position);
                float closeThreshold = customCloseDistance > 0 ? customCloseDistance : _playerScript.InteractionRadius * 1.5f;
                if (dist > closeThreshold) {                                    // Check distance from player
                    Interact();                                                 // Close door and inventory
                    return;
                }
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame &&
                !IsMouseOverUI()) Interact();                                   // Check click outside of window
        }

        public override string GetInteractionPrompt() {
            return isDoorOpen ? openInteractionText : interactionText;
        }

        public override void Preview() {
            throw new NotImplementedException();
        }

        public override void Interact() {
            _lastInteractionTime = Time.time;

            ToggleDoor();
            
            if (isDoorOpen) OpenInventory();
            else CloseInventory();
        }

        private void ToggleDoor() {
            isDoorOpen = !isDoorOpen;

            isLightOn = isDoorOpen;
            if (facilityLight) facilityLight.enabled = isLightOn;               // Turn on/off light
        }

        public void OpenInventory() {
            if (facilityInventory) {                                            // UI inventory logic
                ItemBase[] allItems = facilityInventory.GetAllItems();
                InventoryUIManager.Instance.ShowFullContent(allItems);
            }
        }

        public void CloseInventory() {
            InventoryUIManager.Instance.HideFullContent();
        }

        public override void OnFocus() {                                        // Called while player close to this
            if (!isDoorOpen && facilityInventory) {                             // Display preview if this is closed
                 ItemBase[] previewItems = GetPreviewItems();
                 InventoryUIManager.Instance.ShowPreview(previewItems);
            }
        }

        public override void OnLoseFocus() {                                    // When player move away
            InventoryUIManager.Instance.HidePreview();                          // Hide UI preview

            // if (isDoorOpen) Interact();
        }

        public ItemBase[] GetPreviewItems() {
            if (!facilityInventory) return Array.Empty<ItemBase>();

            int count = Mathf.Min(previewSlotCount, facilityInventory.slots.Length);
            ItemBase[] preview = new ItemBase[count];
            for (int i = 0; i < count; i++)
                preview[i] = facilityInventory.slots[i].item;

            return preview;
        }
        
        private bool IsMouseOverUI() {
            PointerEventData pointerData = new PointerEventData(EventSystem.current) {
                position = Mouse.current.position.ReadValue()                   // Create virtual pointer at mouse position
            };

            System.Collections.Generic.List<RaycastResult> results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);               // Throw all raycasts

            foreach (RaycastResult result in results)                           // Check what have been touch
                if (result.gameObject.layer == LayerMask.NameToLayer("UI"))     // If touched UI object
                    return true; 

            return false;                                                       // Touch something else
        }
    }
}
