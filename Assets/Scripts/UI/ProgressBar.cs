using UnityEngine.UI;
using UnityEngine;

namespace UI {
    public class WorldProgressBar : MonoBehaviour {
        [Header("References")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private Image fillImage;

        [Header("Settings")]
        [SerializeField] private bool hideOnComplete = true;
        [SerializeField] private Vector3 offset = new(0, 1.5f, 0);              // Above object by default

        private Transform _target;
        private Camera _cam;

        private void Start() {
            _cam = Camera.main;
            canvas.enabled = false;                                             // Hide by default
        }

        private void LateUpdate() {
            if (canvas.enabled && _target) {
                transform.position = _target.position + offset;                 // Follow object
                transform.rotation = Quaternion.LookRotation(transform.position - _cam.transform.position); // Look at camera
                // transform.rotation = _cam.transform.rotation;                // For instant facing camera
            }
        }

        public void Show(Transform target) {
            _target = target;
            canvas.enabled = true;
            fillImage.fillAmount = 0f;
        }

        public void Hide() {
            canvas.enabled = false;
        }

        public void UpdateProgress(float progress) {
            fillImage.fillAmount = Mathf.Clamp01(progress);                     // Between 0 and 1

            if (hideOnComplete && progress >= 1f) Hide();
        }
    }
}
