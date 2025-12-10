using UnityEngine;
using Food;

// ReSharper disable Unity.PerformanceCriticalCodeInvocation
namespace Utensils {
    public class CuttingBoard : ItemSurface {
        public override void OnInteract() {
            ItemBase item = GetHeldItem();                                      // Get current item
            if (item is FoodItem food && food.CanBeSliced()) SliceFood(food);
        }

        private void SliceFood(FoodItem food) {
            var sliceData = food.GetSliceInfo();

            if (!sliceData.IsValid) return;

            Destroy(food.gameObject);                                           // Destroy object

            for (int i = 0; i < sliceData.quantity; i++) {                      // Create slices
                Vector3 spawnPos = GetPlacementTransform().position + Vector3.up * (i * 0.05f) + Vector3.right * (i * 0.05f);
                FoodItem newSlice = Instantiate(sliceData.resultingPrefab, spawnPos, Quaternion.identity);
                newSlice.OnPlace(GetPlacementTransform());                      // Placed on board
            }
        }
    }
}
