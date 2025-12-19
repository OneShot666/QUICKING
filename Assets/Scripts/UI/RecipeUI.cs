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

    private Transform playerTransform;
    private Transform facilityTransform;
    private float closeDistance = 3.0f;
    private bool autoCloseEnabled = false;
    private Utensils.KitchenFacility ownerFacility;

    public void SetOwnerFacility(Utensils.KitchenFacility facility) { ownerFacility = facility; }

    public void EnableAutoClose(Transform player, Transform facility, float distance, Utensils.KitchenFacility owner = null)
    {
        playerTransform = player;
        facilityTransform = facility;
        closeDistance = distance;
        autoCloseEnabled = true;
        if (owner != null) ownerFacility = owner;
    }

    private void Update()
    {
        if (!autoCloseEnabled || !gameObject.activeSelf) return;
        if (playerTransform && facilityTransform)
        {
            float dist = Vector3.Distance(playerTransform.position, facilityTransform.position);
            if (dist > closeDistance)
            {
                RequestMenuCloseFromUI();
                autoCloseEnabled = false;
                return;
            }
        }
        if (UnityEngine.InputSystem.Mouse.current != null &&
            UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame &&
            !IsMouseOverUI())
        {
            RequestMenuCloseFromUI();
            autoCloseEnabled = false;
        }
    }
    private void RequestMenuCloseFromUI()
    {
        if (ownerFacility != null)
            ownerFacility.CloseMenu();
        else
            OnLoseFocus();
    }

    private bool IsMouseOverUI()
    {
        var pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
        {
            position = UnityEngine.InputSystem.Mouse.current.position.ReadValue()
        };
        var results = new List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerData, results);
        foreach (var result in results)
            if (result.gameObject.layer == LayerMask.NameToLayer("UI"))
                return true;
        return false;
    }

    public void ShowMenu(List<ItemBase> items, int index)
    {
        if (items == null || items.Count == 0 || index < 0 || index >= items.Count)
        {
            recipeNameText.text = "";
            descriptionText.text = "";
            resultImage.sprite = null;
            ClearIngredientSlots();
            return;
        }
        ItemBase item = items[index];
        recipeNameText.text = item.GetName();
        descriptionText.text = item.GetDescription();
        resultImage.sprite = item.GetIcon();
        ShowIngredients(item);
    }

    private void ShowIngredients(ItemBase item)
    {
        ClearIngredientSlots();
        var foodItem = item as Food.FoodItem;
        if (foodItem == null) return;
        var ingredients = foodItem.GetIngredients();
        foreach (var ingredient in ingredients)
        {
            var slot = Instantiate(ingredientSlotPrefab, ingredientsPanel);
            var img = slot.GetComponentInChildren<Image>();
            if (img && ingredient && ingredient.GetIcon()) img.sprite = ingredient.GetIcon();
            // Set ingredient name text if present
            var text = slot.GetComponentInChildren<TMPro.TMP_Text>();
            if (text && ingredient) text.text = "- " + ingredient.GetName();
            _ingredientSlots.Add(slot);
        }
    }

    private void ClearIngredientSlots()
    {
        foreach (var slot in _ingredientSlots)
            Destroy(slot);
        _ingredientSlots.Clear();
    }

    public void OnLoseFocus()
    {
        autoCloseEnabled = false;
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }
}
