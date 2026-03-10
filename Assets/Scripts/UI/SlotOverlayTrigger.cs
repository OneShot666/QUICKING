using UnityEngine.EventSystems;
using UnityEngine;
using Food;

// To show current items in hands (in UI Screen Canvas)
namespace UI {
    public class SlotOverlayTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        private ItemBase _item;

        public void SetItem(ItemBase item) {
            _item = item;
        }

        public void OnPointerEnter(PointerEventData eventData) {
            if (_item) ItemOverlayManager.Instance.Show(_item);
        }

        public void OnPointerExit(PointerEventData eventData) {
            ItemOverlayManager.Instance.Hide();
        }
        
        private void OnDisable() {
            if (ItemOverlayManager.Instance) ItemOverlayManager.Instance.Hide();
        }
    }
}
