using System.Collections;
using UnityEngine;
using Food;

// ReSharper disable Unity.PerformanceCriticalCodeInvocation
namespace Utensils {
    public class CookingStation : ItemSurface {
        [Header("Visual Feedback")]
        [Tooltip("Particles to play while cooking")]
        public ParticleSystem cookingParticles;
        [Tooltip("Light to turn on while cooking")]
        public Light cookingLight;

        [Header("Cooking settings")]
        [SerializeField] private bool canGrabWhileCooking = true;
        [SerializeField] private AudioSource cookingSound;
        [SerializeField] private AudioSource burningSound;

        private bool _isCooking;                                                // To prevent starting twice
        
        public bool IsCooking => _isCooking;

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
            float timer = cookData.processTime;

            if (cookingParticles) cookingParticles.Play();                      // Visuals ON
            if (cookingLight) cookingLight.enabled = true;

            if(!canGrabWhileCooking) food.GetComponent<Collider>().enabled = false;  // Lock item : can't grab it while cooking

            while (timer > 0) {                                                 // Timer Loop
                if (!food || food.transform.parent != GetPlacementTransform()) {// If grab food mid-cooking, stop everything
                    StopCookingEffects(null);
                    yield break;
                }

                timer -= Time.deltaTime;
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
            if(food) food.GetComponent<Collider>().enabled = true;
            if (cookingParticles) cookingParticles.Stop();
            if (cookingLight) cookingLight.enabled = false;
            _isCooking = false;
        }
    }
}
