using System.Collections;
using UnityEngine;
using Food;
using UI;

// ReSharper disable Unity.PerformanceCriticalCodeInvocation
namespace Utensils {
    public class CookingStation : ItemSurface {
        [Header("Visual references")]
        [Tooltip("Particles to play while cooking")]
        [SerializeField] private ParticleSystem cookingParticles;
        [Tooltip("Light to turn on while cooking")]
        [SerializeField] private Light cookingLight;
        [SerializeField] private AudioSource cookingSound;
        [SerializeField] private AudioSource burningSound;

        [Header("Cooking settings")]
        [SerializeField] private bool canGrabWhileCooking = true;
        [SerializeField] private float cookingSpeedMultiplier = 1.0f;

        [Header("UI")]
        [SerializeField] private WorldProgressBar progressBarPrefab;

        private WorldProgressBar _progressBar;
        private bool _isCooking;                                                // To prevent starting twice
        
        public bool IsCooking => _isCooking;

        private void Start() {
            if (progressBarPrefab) {
                _progressBar = Instantiate(progressBarPrefab, transform.position, Quaternion.identity);
                _progressBar.Hide();                                            // Hide by default
            }
        }

        private void OnDisable() {                                              // Clean state
            if (_isCooking) StopCookingEffects(null);
        }

        public override void OnInteract() {
            if (_isCooking) return;                                             // Can't interact if already busy

            ItemBase item = GetHeldItem();                                      // Get item to cook

            if (item is FoodItem food && food.CanBeCooked())                    // Check if can be cooked
                StartCoroutine(CookingProcess(food));
        }

        private IEnumerator CookingProcess(FoodItem food) {
            _isCooking = true;
            
            var cookData = food.GetCookInfo();                                  // Get data
            float duration = cookData.processTime;                              // Total duration
            float timer = duration;                                             // Remaining time

            if (cookingParticles) cookingParticles.Play();                      // Show particles
            if (cookingLight) cookingLight.enabled = true;                      // Turn on light
            if (_progressBar) _progressBar.Show(transform);                     // Show progress bar
            if(!canGrabWhileCooking) food.GetComponent<Collider>().enabled = false;  // Lock item : can't grab it while cooking

            while (timer > 0) {                                                 // Timer Loop
                if (!food || food.transform.parent != GetPlacementTransform()) {// If grab food mid-cooking, stop everything
                    StopCookingEffects(null);
                    yield break;
                }

                timer -= Time.deltaTime * cookingSpeedMultiplier;

                float progress = 1f - timer / duration;
                if (_progressBar) _progressBar.UpdateProgress(progress);

                yield return null;
            }

            CompleteCooking(food, cookData);                                    // Cooking Finished !
            StopCookingEffects(food);
        }

        private void CompleteCooking(FoodItem oldFood, FoodItem.TransformationData data) {
            for (int i = 0; i < data.quantity; i++) {                           // Spawn cooked food
                Vector3 offset = Vector3.up * (i * 0.05f) + Vector3.right * (i * 0.05f);
                Vector3 spawnPos = GetPlacementTransform().position + offset;

                FoodItem newFood = Instantiate(data.resultingPrefab, spawnPos, Quaternion.identity);
                newFood.OnPlace(GetPlacementTransform());

                if (data.resultingPrefab.IsBurnt() && burningSound && burningSound.clip) burningSound.Play();
                else if (cookingSound && cookingSound.clip) cookingSound.Play();
            }

            Destroy(oldFood.gameObject);                                        // Destroy raw food after
        }

        private void StopCookingEffects(FoodItem food) {                        // Visual OFF
            if (cookingParticles) cookingParticles.Stop();
            if (cookingLight) cookingLight.enabled = false;
            if (_progressBar) _progressBar.Hide();
            if (food) food.GetComponent<Collider>().enabled = true;
            _isCooking = false;
        }
    }
}
