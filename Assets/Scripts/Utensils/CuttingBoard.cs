using System.Collections;
using UnityEngine;
using Food;

// ReSharper disable Unity.PerformanceCriticalCodeInvocation
namespace Utensils {
    public class CuttingBoard : ItemSurface {
        [Header("Visual Feedback")]
        [Tooltip("Particles to play while cutting (chips, etc.)")]
        [SerializeField] private ParticleSystem cuttingParticles;
        [Tooltip("Sound to play (Looping chopping sound is best)")]
        [SerializeField] private AudioSource cuttingSound;

        [Header("Settings")]
        [SerializeField] private bool canGrabWhileCutting = true;

        private bool _isCutting;
        public bool IsCutting => _isCutting;

        private void OnDisable() {
            if (_isCutting) StopCuttingEffects(null);
        }

        public override void OnInteract() {
            if (_isCutting) return;                                             // Already busy

            ItemBase item = GetHeldItem();
            if (item is FoodItem food && food.CanBeSliced()) StartCoroutine(CuttingProcess(food));
        }

        private IEnumerator CuttingProcess(FoodItem food) {
            _isCutting = true;
            
            var sliceData = food.GetSliceInfo();                                // Get food info
            float timer = sliceData.processTime;

            if (timer <= 0) {                                                   // Instant create sliced food
                CompleteSlicing(food, sliceData);
                _isCutting = false;
                yield break;
            }

            if (cuttingParticles) cuttingParticles.Play();                      // Visual ON
            if (cuttingSound && !cuttingSound.isPlaying) cuttingSound.Play();

            if (!canGrabWhileCutting) food.GetComponent<Collider>().enabled = false;    // Lock item : can't grab it while cutting

            while (timer > 0) {
                if (!food || food.transform.parent != GetPlacementTransform()) {
                    StopCuttingEffects(null);
                    yield break;
                }

                timer -= Time.deltaTime;
                yield return null;
            }

            CompleteSlicing(food, sliceData);
            StopCuttingEffects(food);
        }

        private void CompleteSlicing(FoodItem oldFood, FoodItem.TransformationData data) {
            for (int i = 0; i < data.quantity; i++) {                           // Spawn slices
                Vector3 offset = Vector3.up * (i * 0.05f) + Vector3.right * (i * 0.05f);
                Vector3 spawnPos = GetPlacementTransform().position + offset;

                FoodItem newSlice = Instantiate(data.resultingPrefab, spawnPos, Quaternion.identity);
                newSlice.OnPlace(GetPlacementTransform());
            }

            Destroy(oldFood.gameObject);                                        // Destroy complete food after
        }

        private void StopCuttingEffects(FoodItem food) {                        // Visual OFF
            if (cuttingParticles) cuttingParticles.Stop();
            if (cuttingSound) cuttingSound.Stop();
            if (food) food.GetComponent<Collider>().enabled = true;
            _isCutting = false;
        }
    }
}
