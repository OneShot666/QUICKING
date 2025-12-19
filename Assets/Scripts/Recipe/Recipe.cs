using UnityEngine;
using Food;

namespace Recipe {
    [CreateAssetMenu(fileName = "Recipe", menuName = "Cooking/Recipe", order = 0)]
    public class Recipe : ScriptableObject {
        public string recipeName;
        public ItemBase[] ingredients;
        public GameObject result;
        [TextArea]
        public string description;

        public Sprite Icon { get; }
    }
}
