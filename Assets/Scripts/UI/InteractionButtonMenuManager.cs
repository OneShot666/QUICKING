using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;

// ReSharper disable Unity.PerformanceCriticalCodeInvocation
namespace UI {
    public class InteractionButtonMenuManager : MonoBehaviour {
        [Header("References")]
        public Camera mainCamera;
        public GameObject menuContainer;                                        // Panel with buttons
        public InteractionButton buttonPrefab;
        public Transform buttonsParent;                                         // Object with GridLayoutGroup

        [Header("Settings")]
        public Vector3 uiOffset = new(4f, 2f, 0);

        private readonly List<InteractionButton> _activeButtons = new();
        private GridLayoutGroup _gridLayoutGroup;
        private Transform _target;

        private void Start() {
            if (!mainCamera) mainCamera = Camera.main;
            _gridLayoutGroup = buttonsParent.GetComponent<GridLayoutGroup>();
            Hide();
        }

        private void LateUpdate() {
            if (menuContainer.activeSelf && _target) {                          // Look at camera
                transform.position = _target.position + uiOffset;
                transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
            }
        }

        public void Show(Transform target, List<InteractionOption> options) {
            _target = target;
            menuContainer.SetActive(true);

            while (_activeButtons.Count < options.Count) {                      // Reuse or create buttons
                InteractionButton btn = Instantiate(buttonPrefab, buttonsParent);
                _activeButtons.Add(btn);
            }

            for (int i = 0; i < options.Count; i++) {                           // Set required buttons
                _activeButtons[i].gameObject.SetActive(true);
                _activeButtons[i].Setup(options[i].labelText, options[i].actionToRun);
            }

            for (int i = options.Count; i < _activeButtons.Count; i++) {        // Hide unused buttons
                _activeButtons[i].gameObject.SetActive(false);
            }
            
            UpdateLayoutMode(options.Count);
        }
        
        private void UpdateLayoutMode(int count) {
            if (!_gridLayoutGroup) return;

            _gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;

            if (count >= 4) _gridLayoutGroup.constraintCount = 2;               // 2 columns when full
            else _gridLayoutGroup.constraintCount = 1;                          // 1 column
        }

        public void Hide() {
            menuContainer.SetActive(false);
            _target = null;
        }
    }

    public struct InteractionOption {                                           // Small struct to modify infos
        public readonly string labelText;
        public readonly Action actionToRun;

        public InteractionOption(string label, Action action) {
            labelText = label;
            actionToRun = action;
        }
    }
}
