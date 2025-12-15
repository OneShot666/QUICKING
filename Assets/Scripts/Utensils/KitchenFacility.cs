using UnityEngine;
using Inventory;
using System;
using Food;
using UI;

namespace Utensils {
    public class KitchenFacility : BaseFacilityInteraction {
        public enum RotationAxis { X, Y, Z }

        [Header("Settings")]
        public bool isCookingSpot;
        public bool isDoorOpen;
        public bool isLightOn;
        public string interactionText = "Open";
        public string openInteractionText = "Close";                            // Display text when open

        [Header("Door animation")]
        public GameObject doorObject;
        public RotationAxis rotationAxis = RotationAxis.Y;
        public float closedAngle;
        public float openedAngle = 90f;
        public float doorAnimationSpeed = 5f;                                   // Use lerp speed

        [Header("Inventory")]
        public FacilityInventory facilityInventory;
        public Light facilityLight;
        public int previewSlotCount = 5;

        private Quaternion _initialRotation;
        private float _targetAngle;

        private void Start() {
            if (!facilityInventory) facilityInventory = GetComponent<FacilityInventory>();

            if (doorObject) {                                                   // Setup init door animation
                _initialRotation = doorObject.transform.localRotation;
                _targetAngle = isDoorOpen ? openedAngle : closedAngle;
            }
            if (facilityLight) facilityLight.enabled = isLightOn;
        }

        private void Update() {
            if (doorObject) {
                Vector3 axis = Vector3.up;

                switch (rotationAxis) {
                    case RotationAxis.X: axis = Vector3.right; break;
                    case RotationAxis.Y: axis = Vector3.up; break;
                    case RotationAxis.Z: axis = Vector3.forward; break;
                }

                Quaternion targetRot = _initialRotation * Quaternion.AngleAxis(_targetAngle, axis);
                doorObject.transform.localRotation = Quaternion.Lerp(doorObject.transform.localRotation, 
                    targetRot, Time.deltaTime * doorAnimationSpeed);            // Fluid animation
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

            if (isDoorOpen && facilityInventory) {                              // UI inventory logic
                ItemBase[] allItems = facilityInventory.GetAllItems();
                InventoryUIManager.Instance.ShowFullContent(allItems);
            } else {
                InventoryUIManager.Instance.HideFullContent();
            }

            if (isLightOn && isCookingSpot) {                                   // ? Use/call CookingStation.cs
                Debug.Log("Cooking Logic Start..."); 
            }
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
                InventoryUIManager.Instance.HideFullContent();                  // Hide inventory
            }
        }

        private void ToggleDoor() {
            isDoorOpen = !isDoorOpen;
            _targetAngle = isDoorOpen ? openedAngle : closedAngle;

            isLightOn = isDoorOpen;
            if (facilityLight) facilityLight.enabled = isLightOn;                    // Turn on/off light
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
