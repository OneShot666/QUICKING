using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using System;
using Food;

// Manage slots in Inventory UI Manager (display, mouse click and overlay)
// ReSharper disable Unity.PerformanceCriticalCodeInvocation
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

        public void OnPointerEnter(PointerEventData eventData) {                // Display overlay
            if (_item && ItemOverlayManager.Instance) ItemOverlayManager.Instance.Show(_item);
        }

        public void OnPointerExit(PointerEventData eventData) {
            if (ItemOverlayManager.Instance) ItemOverlayManager.Instance.Hide();
        }

        private void OnDisable() {
            if (ItemOverlayManager.Instance) ItemOverlayManager.Instance.Hide();
        }
    }
}
