using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using System;
using Food;

// Manage slots in Inventory UI Manager (display, mouse click and overlay)
namespace UI {
    public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        [Header("References")]
        public Button button;
        public RawImage itemImage;

        private ItemBase _item;

        public void Setup(ItemBase item, Action onClickAction) {                // Init function called by Manager
            _item = item;

            if (button) {                                                       // Set up click event
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onClickAction?.Invoke());
            }

            if (itemImage) itemImage.gameObject.SetActive(item);                // Activate slot (if has item)
        }

        public void OnPointerEnter(PointerEventData eventData) {
            if (_item) ItemOverlayManager.Instance.Show(_item);                 // Display overlay
        }

        public void OnPointerExit(PointerEventData eventData) {
            ItemOverlayManager.Instance.Hide();
        }

        private void OnDisable() {
            if (ItemOverlayManager.Instance) ItemOverlayManager.Instance.Hide();
        }
    }
}
