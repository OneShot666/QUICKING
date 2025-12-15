using System.Collections;
using UnityEngine;
using Food;

// ReSharper disable Unity.PerformanceCriticalCodeInvocation
namespace Utensils {
    public class CookingStation : ItemSurface {
        [Header("Visual Feedback")]
        [Tooltip("Particles to play while cooking")]
        public ParticleSystem cookingParticles;
        [Tooltip("Light to enable while cooking")]
        public Light cookingLight;

        [Header("Cooking settings")]
        [SerializeField] private bool canGrabWhileCooking = true;
        [SerializeField] private AudioSource cookingSound;
        [SerializeField] private AudioSource burningSound;

        private bool _isCooking;                                                // To prevent starting twice

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
                    StopCookingEffects();
                    _isCooking = false;
                    yield break;
                }

                timer -= Time.deltaTime;
                yield return null;
            }

            CompleteCooking(food, cookData);                                    // Cooking Finished !
            StopCookingEffects();
            _isCooking = false;
        }

        private void CompleteCooking(FoodItem oldFood, FoodItem.TransformationData data) {
            Destroy(oldFood.gameObject);                                        // Destroy raw food

            for (int i = 0; i < data.quantity; i++) {                           // Spawn cooked food
                FoodItem newFood = Instantiate(data.resultingPrefab, GetPlacementTransform().position, Quaternion.identity);
                newFood.OnPlace(GetPlacementTransform());
                if (data.resultingPrefab.name.Contains("burnt") && burningSound.clip) burningSound.Play();
                else if (cookingSound.clip) cookingSound.Play();
            }
        }

        private void StopCookingEffects() {                                     // Visual OFF
            if (cookingParticles) cookingParticles.Stop();
            if (cookingLight) cookingLight.enabled = false;
        }
    }
}
