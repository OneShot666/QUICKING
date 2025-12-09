using UnityEngine;

public class CuttingBoard : MonoBehaviour {
    [Header("References")]
    [Tooltip("Where to place food item")]
    public Transform slot;

    public void CutFood() {
        FoodItem food = slot.GetComponentInChildren<FoodItem>();                // Get food part of food

        if (food && food.CanBeSliced()) {
            var info = food.GetSliceInfo();                                     // Get data part of food

            Destroy(food.gameObject);                                           // Destroy old food

            for (int i = 0; i < info.quantity; i++)                             // Create parts
                Instantiate(info.resultingPrefab, slot.position + Vector3.up * (i + 0.05f), Quaternion.identity);
        }
    }
}
