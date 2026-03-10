using System.Collections;
using UnityEngine;
using Food;
using UI;

// ReSharper disable Unity.PerformanceCriticalCodeInvocation
namespace Utensils {
    public class CuttingBoard : ItemSurface {
        [Header("Visual references")]
        [Tooltip("Particles to play while cutting (chips, etc.)")]
        [SerializeField] private ParticleSystem cuttingParticles;
        [Tooltip("Sound to play (Looping chopping sound is best)")]
        [SerializeField] private AudioSource cuttingSound;

        [Header("Settings")]
        [SerializeField] private bool canGrabWhileCutting = true;
        [SerializeField] private float cuttingSpeedMultiplier = 1.0f;

        [Header("UI")]
        [SerializeField] private WorldProgressBar progressBarPrefab;

        private WorldProgressBar _progressBar;
        private bool _isCutting;

        public bool IsCutting => _isCutting;

        private void Start() {
            if (progressBarPrefab) {
                _progressBar = Instantiate(progressBarPrefab, transform.position, Quaternion.identity, transform);
                _progressBar.transform.localScale = Vector3.one;
                _progressBar.Hide();                                            // Hide by default
            }
        }

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
            float duration = sliceData.processTime;                             // Total duration
            float timer = duration;                                             // Remaining time

            if (timer <= 0) {                                                   // Instant create sliced food
                CompleteSlicing(food, sliceData);
                _isCutting = false;
                yield break;
            }

            if (cuttingParticles) cuttingParticles.Play();                      // Show particles
            if (cuttingSound && !cuttingSound.isPlaying) cuttingSound.Play();   // Play sound
            if (_progressBar) _progressBar.Show(transform);                     // Show progress bar
            if (!canGrabWhileCutting) food.GetComponent<Collider>().enabled = false;    // Lock item : can't grab it while cutting

            while (timer > 0) {
                if (!food || food.transform.parent != GetPlacementTransform()) {
                    StopCuttingEffects(null);
                    yield break;
                }

                timer -= Time.deltaTime * cuttingSpeedMultiplier;

                float progress = 1f - timer / duration;
                if (_progressBar) _progressBar.UpdateProgress(progress);

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
            if (_progressBar) _progressBar.Hide();
            if (food) food.GetComponent<Collider>().enabled = true;
            _isCutting = false;
        }
    }
}
