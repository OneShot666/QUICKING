using UnityEngine.UI;
using UnityEngine;

namespace UI {
    public class HandUIManager : MonoBehaviour {
        [Header("UI Slots")]
        public RawImage leftHandRawImage;
        public RawImage rightHandRawImage;

        [Header("Hand Cameras")]
        public Camera leftHandCamera;
        public Camera rightHandCamera;

        [Header("Mesh Display Slots")]
        public Transform leftHandMeshSlot;
        public Transform rightHandMeshSlot;

        private GameObject _currentLeftObject;
        private GameObject _currentRightObject;
        private Transform _leftOriginalParent;
        private Transform _rightOriginalParent;

        void Start() {
            if (leftHandCamera && leftHandRawImage)
                leftHandRawImage.texture = leftHandCamera.targetTexture;        // Associate camera to images
            if (rightHandCamera && rightHandRawImage)
                rightHandRawImage.texture = rightHandCamera.targetTexture;
        }

        public void SetLeftHandItem(GameObject itemObject) {                    // Equip item in left hand
            ReplaceHandObject(ref _currentLeftObject, ref _leftOriginalParent, leftHandMeshSlot, itemObject);
        }

        public void SetRightHandItem(GameObject itemObject) {                   // Equip item in right hand
            ReplaceHandObject(ref _currentRightObject, ref _rightOriginalParent, rightHandMeshSlot, itemObject);
        }

        public void ClearLeftHandItem() {                                       // Remove object from left hand
            RestoreHandObject(ref _currentLeftObject, ref _leftOriginalParent);
        }

        public void ClearRightHandItem() {                                      // Remove object from right hand
            RestoreHandObject(ref _currentRightObject, ref _rightOriginalParent);
        }

        private void ReplaceHandObject(ref GameObject currentObject, ref Transform originalParent, 
            Transform slot, GameObject itemObject) {                            // Move object in display slot
            RestoreHandObject(ref currentObject, ref originalParent);
            if (itemObject && slot) {
                originalParent = itemObject.transform.parent;
                itemObject.transform.SetParent(slot);
                itemObject.transform.localPosition = Vector3.zero;
                itemObject.transform.localRotation = Quaternion.identity;
                itemObject.transform.localScale = Vector3.one;
                currentObject = itemObject;
            } else currentObject = null;
        }

        private void RestoreHandObject(ref GameObject currentObject, ref Transform originalParent) {    // Place item in old parent
            if (currentObject && originalParent) {
                currentObject.transform.SetParent(originalParent);
                originalParent = null;
            }
            currentObject = null;
        }
    }
}
