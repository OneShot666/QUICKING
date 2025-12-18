using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using Inventory;
using Player;
using Food;

// Manage inventory for kitchen facilities
// ReSharper disable Unity.PerformanceCriticalCodeInvocation
namespace UI {
    public class InventoryUIManager : MonoBehaviour {
        [Header("Inventory References")]
        public GameObject inventorySlotPrefab;
        [Tooltip("Object with Grid Layout Group for preview")]
        public Transform previewContentPanel;
        [Tooltip("Object with Grid Layout Group for full inventory")]
        public Transform inventoryContentPanel;

        [Header("Preview References (Preview place)")]
        public Camera previewCamera;
        public RenderTexture previewRenderTexture;                              // Texture "pad"
        public Transform previewStage;                                          // Where to tp object (away from scene)

        [Header("Inventory Settings")]
        [Tooltip("Rotate showed items")]
        public Vector3 itemRotation = new(-15, 45, 0);
        [Tooltip("Offset of preview screen to mouse")]
        public Vector2 cursorOffset = new(20, -20);                             // Mouse offset

        private readonly List<Texture2D> _generatedTextures = new();            // For cleaner code and avoid memory leak
        private FacilityInventory _currentOpenedInventory;
        private PlayerInteraction _player;
        private bool _isFullInventoryOpen;
        private bool _isPreviewActive;

        public static InventoryUIManager Instance;
        
        void Awake() {
            Instance = this;
            if (previewCamera) previewCamera.enabled = false;                   // Turn off preview camera by default
            
            var obj = GameObject.FindGameObjectWithTag("Player");
            if (obj) _player = obj.GetComponent<PlayerInteraction>();
        }

        void Update() {
            if (_isPreviewActive && previewContentPanel.gameObject.activeInHierarchy) // Follow mouse for preview
                MoveContainerToMouse(previewContentPanel.gameObject);
        }

        private void MoveContainerToMouse(GameObject container) {
            Vector2 mousePos = Input.mousePosition;
            RectTransform rect = container.GetComponent<RectTransform>();

            float pivotX = mousePos.x / Screen.width;                           // Keep UI on screen
            float pivotY = mousePos.y / Screen.height;
            rect.pivot = new Vector2(pivotX, pivotY);                           // Avoid pivot to exit screen

            container.transform.position = mousePos + cursorOffset;
        }

        public void ShowPreview(ItemBase[] items) {
            if (_isFullInventoryOpen) return;

            previewContentPanel.gameObject.SetActive(true);
            if (previewContentPanel) previewContentPanel.parent.gameObject.SetActive(true);
            _isPreviewActive = true;

            StopAllCoroutines();
            RawImage[] uiSlots = PrepareSlots(previewContentPanel, items.Length);
            StartCoroutine(GenerateSnapshots(items, uiSlots));
        }

        public void HidePreview() {
            if (previewContentPanel.parent) previewContentPanel.parent.gameObject.SetActive(false);
            else previewContentPanel.gameObject.SetActive(false);

            _isPreviewActive = false;
        }

        public void ShowFullContent(FacilityInventory inventory) {
            _currentOpenedInventory = inventory;                                // Memorize inventory
            ItemBase[] items = inventory.GetAllItems();

            _isFullInventoryOpen = true;
            inventoryContentPanel.gameObject.SetActive(true);
            if (inventoryContentPanel.parent) inventoryContentPanel.parent.gameObject.SetActive(true);

            HidePreview();
            StopAllCoroutines();

            RawImage[] uiSlots = PrepareSlots(inventoryContentPanel, items.Length);
            SetupSlotLogic(uiSlots, inventory);
            StartCoroutine(GenerateSnapshots(items, uiSlots));
        }

        public void HideFullContent() {
            _isFullInventoryOpen = false;

            if (inventoryContentPanel.parent) inventoryContentPanel.parent.gameObject.SetActive(false);
            else inventoryContentPanel.gameObject.SetActive(false);

            HidePreview();
            ClearGeneratedTextures();                                           // Clean memory if close inventory
        }
        
        public void RefreshContentIfOpen(FacilityInventory inventory) {         // If item is add from anywhere
            if (_isFullInventoryOpen && _currentOpenedInventory == inventory)
                ShowFullContent(inventory);
        }

        private IEnumerator GenerateSnapshots(ItemBase[] items, RawImage[] uiSlots) {
            foreach (var slot in uiSlots) slot.color = Color.clear;             // Make slots invisibles

            ClearGeneratedTextures();                                           // Clean old textures

            for (int i = 0; i < uiSlots.Length; i++) {
                if (i >= items.Length || !items[i]) {                           // If not item to display
                    uiSlots[i].color = Color.clear;
                    continue;
                }

                ItemBase item = items[i];
                GameObject obj = item.gameObject;
                uiSlots[i].color = Color.white;                                 // Reset color

                var originalParent = obj.transform.parent;                      // Save state
                var originalPos = obj.transform.position;
                var originalRot = obj.transform.rotation;
                var originalScale = obj.transform.localScale;
                int originalLayer = obj.layer;

                obj.SetActive(true);                                            // Update preview
                obj.transform.SetParent(previewStage);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.Euler(itemRotation);
                obj.transform.localScale = Vector3.one;                         // Adjust scale size
                int previewLayer = LayerMask.NameToLayer("Preview");            // Use specific layer
                if (previewLayer != -1) SetLayerRecursive(obj, previewLayer);

                previewCamera.Render();                                         // Manual render

                Texture2D snapshot = new Texture2D(previewRenderTexture.width, 
                    previewRenderTexture.height, TextureFormat.RGBA32, false);  // Snapshot : create 2D texture
                RenderTexture.active = previewRenderTexture;
                snapshot.ReadPixels(new Rect(0, 0, previewRenderTexture.width, previewRenderTexture.height), 0, 0);
                snapshot.Apply();
                RenderTexture.active = null;                                    // Reset

                _generatedTextures.Add(snapshot);                               // Add snapshot to avoid recreating it
                uiSlots[i].texture = snapshot;
                uiSlots[i].color = Color.white;                                 // Return previous color

                obj.transform.SetParent(originalParent);                        // Restaure default state
                obj.transform.position = originalPos;
                obj.transform.rotation = originalRot;
                obj.transform.localScale = originalScale;
                obj.layer = originalLayer;

                if (items.Length > 10) yield return null;                       // Wait a frame to avoid freeze
            }
        }

        private RawImage[] PrepareSlots(Transform gridParent, int neededCount) {
            List<RawImage> validImages = new List<RawImage>();

            while (gridParent.childCount < neededCount) {                       // Check have enough slots
                Instantiate(inventorySlotPrefab, gridParent);
            }

            for (int i = 0; i < gridParent.childCount; i++) {                   // Among all children
                Transform child = gridParent.GetChild(i);

                if (i < neededCount) {                                          // Show if require
                    child.gameObject.SetActive(true);

                    InventorySlotUI slot = child.GetComponent<InventorySlotUI>();
                    if (slot && slot.itemImage) validImages.Add(slot.itemImage);
                    else {                                                      // ! Fallback (remove after test)
                        var img = child.GetComponentInChildren<RawImage>();
                        validImages.Add(img);
                    }
                } else child.gameObject.SetActive(false);                       // Disable if not require
            }

            return validImages.ToArray();
        }
        
        private void SetupSlotLogic(RawImage[] uiImages, FacilityInventory inventory) {
            for (int i = 0; i < uiImages.Length; i++) {
                InventorySlotUI slot = uiImages[i].GetComponentInParent<InventorySlotUI>();

                if (slot) {
                    int index = i;                                              // Get current index
                    ItemBase item = inventory.GetItem(i);                       // Get item to display (can be null)
                    slot.Setup(item, () => OnSlotClicked(index, inventory));
                }
            }
        }
        
        private void OnSlotClicked(int index, FacilityInventory inventory) {
            if (!_player) return;                                               // Basic checks
            ItemBase item = inventory.GetItem(index);
            if (!item) return;

            bool isRightFree = _player.CanEquipInHand(true);
            bool isLeftFree = _player.CanEquipInHand(false);
            if (isRightFree || isLeftFree) {                                    // If a hand is free
                inventory.RemoveItem(index);                                    // Remove from facility inventory
                _player.PickUpItem(item); 
                ShowFullContent(inventory);                                     // Update inventory
            }
        }

        private static void SetLayerRecursive(GameObject obj, int layer) {
            obj.layer = layer;
            foreach (Transform child in obj.transform) child.gameObject.layer = layer;
        }

        private void ClearGeneratedTextures() {
            foreach (var tex in _generatedTextures) if (tex) Destroy(tex);
            _generatedTextures.Clear();
        }
    }
}
