using System.Collections;
using UnityEngine;
using Food;
using UI;

// ReSharper disable Unity.PerformanceCriticalCodeInvocation
namespace Utensils {
    public class CookingFacility : KitchenFacility {
        [Header("Visual references")]
        [SerializeField] private ParticleSystem smokeParticles;
        [SerializeField] private AudioSource cookingSound;
        [SerializeField] private AudioSource burningSound;

        [Header("Cooking Settings")]
        [Tooltip("To speed up cooking timer (2 -> twice faster)")]
        [SerializeField] private bool canGrabWhileCooking = true;
        [SerializeField] private float cookingSpeedMultiplier = 1.0f;

        [Header("UI")]
        [SerializeField] private WorldProgressBar progressBarPrefab;

        private WorldProgressBar _progressBar;
        private bool _isCooking;
        private float[] _slotCookTimers;                                        // Timers of food items that are being cooked

        protected override void Start() {
            base.Start();

            if (facilityInventory) _slotCookTimers = new float[facilityInventory.slotCount];    // Init timers list

            if (progressBarPrefab) {
                _progressBar = Instantiate(progressBarPrefab, transform.position, Quaternion.identity);
                _progressBar.Hide();                                            // Hide by default
            }
        }

        public override void Interact() {
            base.Interact();                                                    // Open/close door

            if (!isDoorOpen) {
                isLightOn = true;
                if (facilityLight) facilityLight.enabled = true;
                TryStartCooking();                                              // Check if can cook items
            } else StopCookingProcess();                                        // Stop cooking items
        }

        private void TryStartCooking() {
            if (!_isCooking && facilityInventory)
                StartCoroutine(StartCookingInventory());                      // Start cooking (if not started yet)
        }

        private IEnumerator StartCookingInventory() {
            _isCooking = true;

            if (smokeParticles) smokeParticles.Play();                          // Show particles
            if (facilityLight) facilityLight.enabled = true;                    // Turn on light
            if (_progressBar) _progressBar.Show(transform);                     // Show progress bar

            while (!isDoorOpen && isLightOn) {                                  // While closed and turned on
                bool isCookingAnything = false;
                float maxProgress = 0f;

                for (int i = 0; i < facilityInventory.slots.Length; i++) {      // For every item inside
                    var slot = facilityInventory.slots[i];

                    if (slot.IsEmpty || slot.item is not FoodItem food || !food.CanBeCooked()) {    // If item invalid or slot empty
                        _slotCookTimers[i] = 0f;
                        continue;
                    }

                    if(!canGrabWhileCooking) food.GetComponent<Collider>().enabled = false;  // Lock item : can't grab it while cooking

                    isCookingAnything = true;
                    FoodItem.TransformationData cookData = food.GetCookInfo();  // Get item cooking data
                    float cookDuration = cookData.processTime;

                    _slotCookTimers[i] += Time.deltaTime * cookingSpeedMultiplier;  // Add time to timer (with bonus)
                    
                    float itemProgress = _slotCookTimers[i] / cookDuration;
                    if (itemProgress > maxProgress) maxProgress = itemProgress; // Get closest item from being cooked

                    if (_slotCookTimers[i] >= cookDuration) {                   // Check item is cooked
                        CookSlot(i, food, cookData);
                        _slotCookTimers[i] = 0f;                                // Reset timer for new item (cooked to burnt)
                    }
                }

                if (isCookingAnything && _progressBar) _progressBar.UpdateProgress(maxProgress);
                else if (_progressBar) _progressBar.Hide();

                yield return null;                                              // Wait next frame
            }

            StopCookingProcess();
        }

        // Only create one cooked item for each raw item (choose first if more than one item expected) 
        private void CookSlot(int slotIndex, FoodItem oldItem, FoodItem.TransformationData data) {
            FoodItem newItem = Instantiate(data.resultingPrefab);               // Get cooked result
            newItem.gameObject.SetActive(false);                                // Hide to avoid clipping or wrong placement
            if (data.resultingPrefab.IsBurnt() && burningSound && burningSound.clip) burningSound.Play();
            else if (cookingSound && cookingSound.clip) cookingSound.Play();
            Destroy(oldItem.gameObject);                                        // Remove previous item (ex: raw)

            facilityInventory.SetItem(slotIndex, newItem);                      // Update new item
        }

        private void StopCookingProcess() {
            if (smokeParticles) smokeParticles.Stop();
            if (facilityLight) facilityLight.enabled = false;
            if (cookingSound) cookingSound.Stop();
            if (burningSound) burningSound.Stop();
            if (_progressBar) _progressBar.Hide();
            foreach (var slot in facilityInventory.slots)
                if (slot is { item: FoodItem food } && food) food.GetComponent<Collider>().enabled = true;
            _isCooking = false;
        }
    }
}
