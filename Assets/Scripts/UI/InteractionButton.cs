using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

// ReSharper disable Unity.PerformanceCriticalCodeInvocation
namespace UI {
    public class InteractionButton : MonoBehaviour {
        [Header("References")]
        public TextMeshProUGUI labelText;
        public Button button;
        [Tooltip("[Optional] Not implemented yet")]
        public Image iconImage;

        private Action _callback;

        public void Setup(string text, Action onClickAction) {                  // Initiate button with text and action
            labelText.text = text;
            _callback = onClickAction;

            button.onClick.RemoveAllListeners();                                // Clean previous listeners
            button.onClick.AddListener(() => _callback?.Invoke());              // Set new action
        }
    }
}
