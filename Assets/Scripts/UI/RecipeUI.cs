using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

using Food;
using TMPro;

public class RecipeUI : MonoBehaviour
{
    public TMP_Text recipeNameText;
    public TMP_Text descriptionText;
    public Transform ingredientsPanel;
    public GameObject ingredientSlotPrefab;
    public Image resultImage;

    private List<GameObject> _ingredientSlots = new();

    public void ShowRecipe(List<Recipe> recipes, int index)
    {
        if (recipes == null || recipes.Count == 0 || index < 0 || index >= recipes.Count)
        {
            recipeNameText.text = "";
            descriptionText.text = "";
            resultImage.sprite = null;
            ClearIngredientSlots();
            return;
        }
        Recipe recipe = recipes[index];
        recipeNameText.text = recipe.recipeName;
        descriptionText.text = recipe.description;
        resultImage.sprite = recipe.result ? recipe.result.icon : null;
        ShowIngredients(recipe.ingredients);
    }

    private void ShowIngredients(ItemBase[] ingredients)
    {
        ClearIngredientSlots();
        foreach (var ingredient in ingredients)
        {
            var slot = Instantiate(ingredientSlotPrefab, ingredientsPanel);
            var img = slot.GetComponentInChildren<Image>();
            if (img && ingredient && ingredient.icon) img.sprite = ingredient.icon;
            _ingredientSlots.Add(slot);
        }
    }

    private void ClearIngredientSlots()
    {
        foreach (var slot in _ingredientSlots)
            Destroy(slot);
        _ingredientSlots.Clear();
    }
}

