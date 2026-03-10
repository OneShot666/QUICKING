using UnityEngine;
using Food;

namespace Utensils {
    public class TrashBin : ItemSurface {
        [Header("Feedback")]
        [SerializeField] private ParticleSystem trashParticles;
        [SerializeField] private AudioSource trashSound;

        public override bool IsAvailable() {                                    // Always available
            return true;
        }

        public void DisposeItem(ItemBase item) {                                // Delete threw item
            if (!item) return;

            if (trashParticles) trashParticles.Play();
            if (trashSound) trashSound.Play();

            Destroy(item.gameObject);
        }
    }
}
