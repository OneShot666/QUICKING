using UnityEngine;

public class FoodItem : ItemBase {
    public enum FoodState { Raw, Chopped, Cooked, Burnt, Eaten }

    [Header("Food Specifics")]
    public FoodState currentState = FoodState.Raw;

    [Header("Transformations")]
    [Tooltip("What item became when cut")]
    [SerializeField] private TransformationData sliceResult;
    [Tooltip("What item became when cook")]
    [SerializeField] private TransformationData cookResult;
    [SerializeField] private TransformationData eatResult;

    // Structure simple pour définir le résultat d'une action
    [System.Serializable]
    public struct TransformationData {
        [Tooltip("The prefab of the resulting item")]
        public FoodItem resultingPrefab;
        [Tooltip("How many of these items are generated")]
        [Min(1)] public int quantity;
        [Tooltip("Time to create these items (cut them or cook them)")]
        public float processTime;

        public bool IsValid => resultingPrefab;                                 // Check if transformation is possible
    }

    public bool CanBeSliced() => sliceResult.IsValid;

    public bool CanBeCooked() => cookResult.IsValid;

    public TransformationData GetSliceInfo() => sliceResult;                    // Result of cut

    public TransformationData GetCookInfo() => cookResult;                      // Result of cook

    protected override void Reset() {
        base.Reset();
        GuessStatus();
        ApplyDescription();
        sliceResult = new TransformationData();
        cookResult = new TransformationData();
    }

    private void GuessStatus() {                                                // Use item name to guess current state
        string lowerName = gameObject.name.ToLower();

        if (lowerName.Contains("cooked")) currentState = FoodState.Cooked;
        else if (lowerName.Contains("burnt")) currentState = FoodState.Burnt;
        else if (lowerName.Contains("slice") || lowerName.Contains("chopped")) currentState = FoodState.Chopped;
        else if (lowerName.Contains("eaten") || lowerName.Contains("naked") || lowerName.Contains("waste")) currentState = FoodState.Eaten;
        else currentState = FoodState.Raw;
    }

    private void ApplyDescription() {
        switch (currentState) {
            case FoodState.Raw: itemDescription = "Not cooked yet"; break;
            case FoodState.Chopped: itemDescription = "Chopped in pieces"; break;
            case FoodState.Cooked: itemDescription = "Cooked to perfection"; break;
            case FoodState.Burnt: itemDescription = "Poor thing, you burnt it dry..."; break;
            case FoodState.Eaten: itemDescription = "What's left of a delicious meal"; break;
        }
    }
}
