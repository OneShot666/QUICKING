using UnityEngine;
using System.Collections.Generic;
using UI;
using Utensils;
using Player;

public class RecipeBook : BaseFacilityInteraction
{
    [Header("Book Animation")]
    public GameObject bookObject;
    public KitchenFacility.RotationAxis rotationAxis = KitchenFacility.RotationAxis.Y;
    public float closedAngle = 0f;
    public float openedAngle = 120f;
    public float animationSpeed = 5f;

    [Header("Recipes")]
    public List<Recipe> recipes = new List<Recipe>();
    public int currentRecipeIndex = 0;

    [Header("UI")]
    public GameObject recipeUICanvas;
    public RecipeUI recipeUI;

    private Quaternion _initialRotation;
    private float _targetAngle;
    private bool _isOpen;

    private void Start()
    {
        if (bookObject)
        {
            _initialRotation = bookObject.transform.localRotation;
            _targetAngle = closedAngle;
        }
        if (recipeUICanvas) recipeUICanvas.SetActive(false);
    }

    private void Update()
    {
        if (bookObject)
        {
            Vector3 axis = Vector3.up;
            switch (rotationAxis)
            {
                case KitchenFacility.RotationAxis.X: axis = Vector3.right; break;
                case KitchenFacility.RotationAxis.Y: axis = Vector3.up; break;
                case KitchenFacility.RotationAxis.Z: axis = Vector3.forward; break;
            }
            Quaternion targetRot = _initialRotation * Quaternion.AngleAxis(_targetAngle, axis);
            bookObject.transform.localRotation = Quaternion.Lerp(bookObject.transform.localRotation, targetRot, Time.deltaTime * animationSpeed);
        }
    }

    public void OnInteract()
    {
        if (!_isOpen)
            OpenBook();
        else
            CloseBook();
    }

    public void OpenBook()
    {
        _isOpen = true;
        _targetAngle = openedAngle;
        if (recipeUICanvas) recipeUICanvas.SetActive(true);
        if (recipeUI) recipeUI.ShowRecipe(recipes, currentRecipeIndex);
    }

    public void CloseBook()
    {
        _isOpen = false;
        _targetAngle = closedAngle;
        if (recipeUICanvas) recipeUICanvas.SetActive(false);
    }

    public void NextRecipe()
    {
        if (recipes.Count == 0) return;
        currentRecipeIndex = (currentRecipeIndex + 1) % recipes.Count;
        if (recipeUI) recipeUI.ShowRecipe(recipes, currentRecipeIndex);
    }

    public void PreviousRecipe()
    {
        if (recipes.Count == 0) return;
        currentRecipeIndex = (currentRecipeIndex - 1 + recipes.Count) % recipes.Count;
        if (recipeUI) recipeUI.ShowRecipe(recipes, currentRecipeIndex);
    }

    public override string GetInteractionPrompt()
    {
        throw new System.NotImplementedException();
    }

    public override void Preview()
    {
        throw new System.NotImplementedException();
    }

    public override void Interact()
    {
        OnInteract();
    }
}

