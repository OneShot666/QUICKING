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

        void Start()
        {
            // Associer la vue caméra à la RawImage
            if (leftHandCamera != null && leftHandRawImage != null)
                leftHandRawImage.texture = leftHandCamera.targetTexture;
            if (rightHandCamera != null && rightHandRawImage != null)
                rightHandRawImage.texture = rightHandCamera.targetTexture;
        }

        // Équipe un objet dans la main gauche
        public void SetLeftHandItem(GameObject itemObject)
        {
            ReplaceHandObject(ref _currentLeftObject, ref _leftOriginalParent, leftHandMeshSlot, itemObject);
        }

        // Équipe un objet dans la main droite
        public void SetRightHandItem(GameObject itemObject)
        {
            ReplaceHandObject(ref _currentRightObject, ref _rightOriginalParent, rightHandMeshSlot, itemObject);
        }

        // Retire l'objet de la main gauche
        public void ClearLeftHandItem()
        {
            RestoreHandObject(ref _currentLeftObject, ref _leftOriginalParent);
        }

        // Retire l'objet de la main droite
        public void ClearRightHandItem()
        {
            RestoreHandObject(ref _currentRightObject, ref _rightOriginalParent);
        }

        // Déplace l'objet dans le slot d'affichage
        private void ReplaceHandObject(ref GameObject currentObject, ref Transform originalParent, Transform slot, GameObject itemObject)
        {
            RestoreHandObject(ref currentObject, ref originalParent);
            if (itemObject != null && slot != null)
            {
                originalParent = itemObject.transform.parent;
                itemObject.transform.SetParent(slot);
                itemObject.transform.localPosition = Vector3.zero;
                itemObject.transform.localRotation = Quaternion.identity;
                itemObject.transform.localScale = Vector3.one;
                currentObject = itemObject;
            }
            else
            {
                currentObject = null;
            }
        }

        // Restaure l'objet à son parent d'origine
        private void RestoreHandObject(ref GameObject currentObject, ref Transform originalParent)
        {
            if (currentObject != null && originalParent != null)
            {
                currentObject.transform.SetParent(originalParent);
                originalParent = null;
            }
            currentObject = null;
        }
    }
}
