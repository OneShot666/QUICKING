using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using Food;

// ReSharper disable Unity.PerformanceCriticalCodeInvocation
namespace UI {
    public class HandUIManager : MonoBehaviour {
        [Header("UI Hand Slots")]
        public InventorySlotUI leftHandSlot;
        public InventorySlotUI rightHandSlot;

        [Header("Preview References (Preview place)")]
        public Camera previewCamera;
        public RenderTexture previewRenderTexture;
        public Transform previewItemParent;

        [Header("Item Settings")]
        [Tooltip("Rotate showed items")]
        public Vector3 itemRotation = new(-15, 45, 0);

        private Texture2D _leftHandTexture;
        private Texture2D _rightHandTexture;

        private void Start() {
            if (previewCamera) previewCamera.enabled = false;                   // Disable by default
            
            ClearLeftHandItem();                                                // Empty interface
            ClearRightHandItem();
        }

        public void SetLeftHandItem(ItemBase item) {
            if (!item) { ClearLeftHandItem(); return; }

            leftHandSlot.Setup(item, null);                                     // Set up slot (image + overlay)

            StartCoroutine(GenerateHandSnapshot(item, leftHandSlot.itemImage, true));
        }

        public void SetRightHandItem(ItemBase item) {
            if (!item) { ClearRightHandItem(); return; }

            rightHandSlot.Setup(item, null);
            StartCoroutine(GenerateHandSnapshot(item, rightHandSlot.itemImage, false));
        }

        public void ClearLeftHandItem() {
            leftHandSlot.Setup(null, null);                                     // Hide image + disable overlay
            if (_leftHandTexture) Destroy(_leftHandTexture);                    // Clean memory
        }

        public void ClearRightHandItem() {
            rightHandSlot.Setup(null, null);
            if (_rightHandTexture) Destroy(_rightHandTexture);
        }

        private IEnumerator GenerateHandSnapshot(ItemBase item, RawImage targetImage, bool isLeft) {    // Create 3D image
            targetImage.color = Color.clear;                                    // Temporary hide

            GameObject previewObj = Instantiate(item.gameObject, previewItemParent); // Temporary copy of item
            previewObj.transform.localPosition = Vector3.zero;                  // Config copy
            previewObj.transform.localRotation = Quaternion.Euler(itemRotation);
            previewObj.transform.localScale = Vector3.one;

            Destroy(previewObj.GetComponent<ItemBase>());                       // Remove useless components
            Destroy(previewObj.GetComponent<Rigidbody>());
            foreach(var col in previewObj.GetComponentsInChildren<Collider>()) Destroy(col);

            int previewLayer = LayerMask.NameToLayer("Preview");                // Set correct layer
            SetLayerRecursive(previewObj, previewLayer);

            yield return null;                                                  // Wait a frame (for set up)

            previewCamera.Render();

            Texture2D snapshot = new Texture2D(previewRenderTexture.width, previewRenderTexture.height, 
                TextureFormat.RGBA32, false);                                   // Take snapshot of texture
            RenderTexture.active = previewRenderTexture;
            snapshot.ReadPixels(new Rect(0, 0, previewRenderTexture.width, previewRenderTexture.height), 0, 0);
            snapshot.Apply();
            RenderTexture.active = null;

            targetImage.texture = snapshot;
            targetImage.color = Color.white;

            if (isLeft) {                                                       // Replace old texture
                if (_leftHandTexture) Destroy(_leftHandTexture);
                _leftHandTexture = snapshot;
            } else {
                if (_rightHandTexture) Destroy(_rightHandTexture);
                _rightHandTexture = snapshot;
            }

            Destroy(previewObj);                                                // Remove item copy
        }

        private void SetLayerRecursive(GameObject obj, int layer) {
            obj.layer = layer;
            foreach (Transform child in obj.transform) child.gameObject.layer = layer;
        }
    }
}
