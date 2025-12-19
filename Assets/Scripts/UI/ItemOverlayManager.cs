using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using Food;

// Manage overlay of item on screen when mouse is hover one item in an inventory (usually facilities)
namespace UI {
    public class ItemOverlayManager : MonoBehaviour {
        [Header("References")]
        [SerializeField] private GameObject overlayPanel;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI detailText;

        [Header("Settings")]
        [SerializeField] private Vector2 mouseOffset = new(15, -15);               // Mouse offset

        private RectTransform _panelRect;
        private RectTransform _canvasRect;
        private Canvas _parentCanvas;

        public static ItemOverlayManager Instance;

        private void Awake() {
            Instance = this;
            if (overlayPanel) {
                _panelRect = overlayPanel.GetComponent<RectTransform>();
                _parentCanvas = GetComponentInParent<Canvas>();
                if (_parentCanvas) _canvasRect = _parentCanvas.GetComponent<RectTransform>();
            }
            Hide();                                                             // Hide by default
        }

        private void Update() {
            if (overlayPanel.activeSelf) UpdatePosition();                      // Follow mouse
        }

        private void UpdatePosition() {                                         // Keep overlay fully on screen
            if (!_parentCanvas && !_canvasRect) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Camera cam = _parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _parentCanvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, mousePos, cam, out var localPoint);
            
            Vector2 finalLocalPos = localPoint + mouseOffset;

            float width = _panelRect.rect.width;
            float height = _panelRect.rect.height;
            
            float canvasRightEdge = _canvasRect.rect.width / 2f;
            float canvasBottomEdge = _canvasRect.rect.height / 2f;

            if (finalLocalPos.x + width > canvasRightEdge) finalLocalPos.x = localPoint.x - width -  mouseOffset.x;
            if (finalLocalPos.y - height < canvasBottomEdge) finalLocalPos.y = localPoint.y + height - mouseOffset.y;

            overlayPanel.transform.localPosition = finalLocalPos;
        }

        public void Show(ItemBase item) {
            if (!item) return;

            nameText.text = item.GetName();
            descriptionText.text = item.GetDescription();

            if (detailText) {
                string details = "";

                if (item is FoodItem food) {
                    if (food.CanBeSliced()) {
                        var info = food.GetSliceInfo();
                        string result = info.resultingPrefab ? info.resultingPrefab.GetName() : "???";
                        details += $"Can be cut into {result} (x{info.quantity})\n";
                    }

                    if (food.CanBeCooked()) {
                        var info = food.GetCookInfo();
                        string result = info.resultingPrefab ? info.resultingPrefab.GetName() : "???";
                        details += $"Can be cooked into {result} (x{info.quantity})\n";
                    }

                    if (food.CanBeEaten()) {
                        var info = food.GetEatenInfo();
                        string result = info.resultingPrefab ? info.resultingPrefab.GetName() : "???";
                        details += $"Can be eaten and gives {result} (x{info.quantity})\n";
                    }
                }

                detailText.text = details;
                detailText.gameObject.SetActive(!string.IsNullOrEmpty(details));
            }

            overlayPanel.SetActive(true);
            overlayPanel.transform.SetAsLastSibling();

            LayoutRebuilder.ForceRebuildLayoutImmediate(_panelRect);            // Force layout update to avoid size blinking
        }

        public void Hide() {
            overlayPanel.SetActive(false);
        }
    }
}
