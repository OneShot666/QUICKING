using UnityEngine;
using Food;

[CreateAssetMenu(fileName = "Recipe", menuName = "Cooking/Recipe", order = 0)]
public class Recipe : ScriptableObject
{
    public string recipeName;
    public ItemBase[] ingredients;
    public ItemBase result;
    [TextArea]
    public string description;
}

