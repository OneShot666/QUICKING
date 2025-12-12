using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using Food;

// ReSharper disable Unity.PerformanceCriticalCodeInvocation
namespace UI {
    public class InventoryUIManager : MonoBehaviour {
        public static InventoryUIManager Instance;

        [Header("UI References")]
        public RawImage[] previewSlotImages; 
        public RawImage[] fullSlotImages;
        public GameObject previewContainer;
        public GameObject fullContainer;

        [Header("3D Preview System")]
        public Camera previewCamera;
        public RenderTexture previewRenderTexture;                              // Texture "pad"
        public Transform previewStage;                                          // Where to tp object (away from scene)

        [Header("Settings")]
        public Vector2 cursorOffset = new(20, -20); // Décalage souris

        private readonly List<Texture2D> _generatedTextures = new();            // For cleaner code and avoid memory leak
        private bool _isFullInventoryOpen;
        private bool _isPreviewActive;
        
        void Awake() {
            Instance = this;
            if (previewCamera) previewCamera.enabled = false;                   // Turn off preview camera by default
        }

        void Update() {
            if (_isPreviewActive && previewContainer.activeSelf)                // Follow mouse for preview
                MoveContainerToMouse(previewContainer);
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
            
            previewContainer.SetActive(true);
            _isPreviewActive = true;
            
            StopAllCoroutines();
            StartCoroutine(GenerateSnapshots(items, previewSlotImages));
        }

        public void HidePreview() {
            previewContainer.SetActive(false);
            _isPreviewActive = false;
        }

        public void ShowFullContent(ItemBase[] items) {
            _isFullInventoryOpen = true;
            fullContainer.SetActive(true);
            HidePreview();
            
            StopAllCoroutines();
            StartCoroutine(GenerateSnapshots(items, fullSlotImages));
        }

        public void HideFullContent() {
            _isFullInventoryOpen = false;
            fullContainer.SetActive(false);
            ClearGeneratedTextures();                                           // Clean memory if close inventory
        }

        private IEnumerator GenerateSnapshots(ItemBase[] items, RawImage[] uiSlots) {
            foreach (var slot in uiSlots) slot.gameObject.SetActive(false);     // Disable all inventory slots
            ClearGeneratedTextures();                                           // Clean old textures

            for (int i = 0; i < uiSlots.Length; i++) {
                if (i >= items.Length || !items[i]) continue;                   // If not item to display

                ItemBase item = items[i];
                GameObject obj = item.gameObject;

                var originalParent = obj.transform.parent;                      // Save state
                var originalPos = obj.transform.position;
                var originalRot = obj.transform.rotation;
                var originalScale = obj.transform.localScale;
                int originalLayer = obj.layer;

                obj.SetActive(true);                                            // Update preview
                obj.transform.SetParent(previewStage);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;                         // Adjust scale size
                obj.layer = LayerMask.NameToLayer("Preview");                   // Use specific layer

                previewCamera.Render();                                         // Manual render

                Texture2D snapshot = new Texture2D(previewRenderTexture.width, 
                    previewRenderTexture.height, TextureFormat.RGBA32, false);  // Snapshot : create 2D texture
                RenderTexture.active = previewRenderTexture;
                snapshot.ReadPixels(new Rect(0, 0, previewRenderTexture.width, previewRenderTexture.height), 0, 0);
                snapshot.Apply();
                RenderTexture.active = null;                                    // Reset

                _generatedTextures.Add(snapshot);                               // Add snapshot to avoid recreating it
                uiSlots[i].texture = snapshot;
                uiSlots[i].gameObject.SetActive(true);

                obj.transform.SetParent(originalParent);                        // Restaure default state
                obj.transform.position = originalPos;
                obj.transform.rotation = originalRot;
                obj.transform.localScale = originalScale;
                obj.layer = originalLayer;

                if (items.Length > 10) yield return null;                       // Wait a frame to avoid freeze
            }
        }

        private void ClearGeneratedTextures() {
            foreach (var tex in _generatedTextures) if (tex) Destroy(tex);
            _generatedTextures.Clear();
        }
    }
}
