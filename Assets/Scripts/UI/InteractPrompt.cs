using UnityEngine;
using TMPro;

namespace UI {
    public class InteractionPrompt : MonoBehaviour {
        [Header("References")]
        public Canvas canvas;
        public TextMeshProUGUI promptText;

        private Transform _camTransform;

        private void Start() {
            if (!canvas) canvas = GetComponent<Canvas>();
            if (Camera.main) _camTransform = Camera.main.transform;
            Hide();                                                             // Hide by default
        }

        private void LateUpdate() {
            if (canvas.enabled && _camTransform)                                // Billboard effect (face the camera)
                transform.rotation = Quaternion.LookRotation(transform.position - _camTransform.position);
        }

        public void Show(string text, Vector3 position) {
            promptText.text = text;
            transform.position = position + Vector3.up * 2f;                    // Float slightly above object
            canvas.enabled = true;
        }

        public void Hide() {
            canvas.enabled = false;
        }
    }
}
