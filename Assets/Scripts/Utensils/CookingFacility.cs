using System.Collections;
using UnityEngine;
using Inventory;
using Food;

// ReSharper disable Unity.PerformanceCriticalCodeInvocation
namespace Utensils {
    public class CookingFacility : KitchenFacility {
        
        [Header("Cooking Settings")]
        [Tooltip("Multiplicateur de vitesse (2 = cuit 2x plus vite)")]
        public float cookingSpeedMultiplier = 1.0f;
        public ParticleSystem smokeParticles;

        private bool _isCookingProcessRunning;
        private float[] _slotCookTimers;                                        // Timers of food items that are being cooked

        private void Start() {
            if (!facilityInventory) facilityInventory = GetComponent<FacilityInventory>();
            
            if (facilityInventory) _slotCookTimers = new float[facilityInventory.slotCount];    // Init timers list
        }

        public override void Interact() {
            base.Interact();                                                    // Open/close door

            if (!isDoorOpen && isLightOn) TryStartCooking();                    // Check if can cook items
            else StopCookingVisuals();                                          // Stop cooking items
        }

        private void TryStartCooking() {
            if (!_isCookingProcessRunning && facilityInventory)
                StartCoroutine(ProcessInventoryCooking());                      // Start cooking (if not started yet)
        }

        private IEnumerator ProcessInventoryCooking() {
            _isCookingProcessRunning = true;
            if (smokeParticles) smokeParticles.Play();

            while (!isDoorOpen && isLightOn) {                                  // While closed and turned on
                for (int i = 0; i < facilityInventory.slots.Length; i++) {      // For every item inside
                    var slot = facilityInventory.slots[i];

                    if (slot.IsEmpty || slot.item is not FoodItem food || !food.CanBeCooked()) {    // If item invalid or slot empty
                        _slotCookTimers[i] = 0f;
                        continue;
                    }

                    FoodItem.TransformationData cookData = food.GetCookInfo();  // Get item cooking data
                    float cookDuration = cookData.processTime;

                    _slotCookTimers[i] += Time.deltaTime * cookingSpeedMultiplier;  // Add time to timer (with bonus)

                    if (_slotCookTimers[i] >= cookDuration) {                   // Check item is cooked
                        CookSlot(i, food, cookData);
                        _slotCookTimers[i] = 0f;                                // Reset timer for new item (cooked to burnt)
                    }
                }

                yield return null;                                              // Wait next frame
            }

            _isCookingProcessRunning = false;
            StopCookingVisuals();
        }

        // Only create one cooked item for each raw item (choose first if more than one item expected) 
        private void CookSlot(int slotIndex, FoodItem oldItem, FoodItem.TransformationData data) {
            FoodItem newItem = Instantiate(data.resultingPrefab);               // Get cooked result
            newItem.gameObject.SetActive(false);                                // Hide to avoid clipping or wrong placement
            Destroy(oldItem.gameObject);                                        // Remove previous item (ex: raw)

            facilityInventory.SetItem(slotIndex, newItem);                      // Update new item
            Debug.Log($"[Oven] Slot {slotIndex} finished cooking: {oldItem.GetName()} became {newItem.GetName()}"); // !!!
        }

        private void StopCookingVisuals() {
            if (smokeParticles) smokeParticles.Stop();
        }
    }
}
