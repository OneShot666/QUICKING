using UnityEngine;
using Inventory;
using System;
using Food;
using UI;

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

        [Header("Inventory")]
        public FacilityInventory facilityInventory;
        public int previewSlotCount = 5;

        private Quaternion _initialRotation;
        private float _currentAngle;

        protected virtual void Start() {
            if (!facilityInventory) facilityInventory = GetComponent<FacilityInventory>();

            if (doorObject) {                                                   // Setup init door animation
                _initialRotation = doorObject.transform.localRotation;
                _currentAngle = isDoorOpen ? openedAngle : closedAngle;
            }
            
            isLightOn = isDoorOpen;
            if (facilityLight) facilityLight.enabled = isLightOn;
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
        }

        public override string GetInteractionPrompt() {
            return isDoorOpen ? openInteractionText : interactionText;
        }

        public override void Preview() {
            throw new NotImplementedException();
        }

        public override void Interact() {
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

            if (isDoorOpen) {
                ToggleDoor();                                                   // Close door
                CloseInventory();                                               // Hide inventory
            }
        }

        public ItemBase[] GetPreviewItems() {
            if (!facilityInventory) return Array.Empty<ItemBase>();
            int count = Mathf.Min(previewSlotCount, facilityInventory.slots.Length);
            ItemBase[] preview = new ItemBase[count];
            for (int i = 0; i < count; i++)
                preview[i] = facilityInventory.slots[i].item;
            return preview;
        }
    }
}
