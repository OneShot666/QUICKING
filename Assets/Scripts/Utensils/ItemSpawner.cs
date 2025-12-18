using UnityEngine;
using Food;

namespace Utensils {
    public class ItemSpawner : BaseFacilityInteraction {
        [Header("Spawner Settings")]
        [Tooltip("Item to spawn in player's hand")]
        public ItemBase itemPrefab;
        
        [Tooltip("Number of item inside (-1 for infinite)")]
        public int quantity = -1;

        [Header("Feedback")]
        [SerializeField] private AudioSource spawnSound;

        public override string GetInteractionPrompt() {                         // Action button name
            if (quantity == 0) return "Empty";
            return $"Take {itemPrefab.GetName()}";
        }

        public override void Preview() {
            throw new System.NotImplementedException();
        }

        public override void Interact() {                                       // ? Add effect if empty (visual or sound)
            throw new System.NotImplementedException();
        }

        public bool CanSpawn() {
            return itemPrefab && quantity is -1 or > 0;
        }

        public ItemBase SpawnItem() {
            if (!CanSpawn()) return null;

            if (quantity > 0) quantity--;                                       // Reduce quantity (if limited)
            if (spawnSound) spawnSound.Play();

            return Instantiate(itemPrefab);                                     // Return instance of item
        }
    }
}
