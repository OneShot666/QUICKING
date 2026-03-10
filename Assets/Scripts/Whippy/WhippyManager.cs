using System.Collections.Generic;
using UnityEngine;

namespace Whippy {
    public class WhippyManager : MonoBehaviour {
        [Header("Référence à Whippy")]
        public Whippy whippy;

        [Header("Progression du joueur")]
        public List<string> progressionSteps = new() { "Tutorial", "Level1", "Level2" };
        private int _currentStepIndex;

        void Awake() {
            // Si la référence n'est pas assignée, on essaie de la trouver automatiquement
            if (!whippy) whippy = FindFirstObjectByType<Whippy>();
            SetWhippyStep(_currentStepIndex);
        }

        // Active l'interaction avec Whippy
        public void EnableWhippy() {
            if (whippy) whippy.canInteract = true;
        }

        // Désactive l'interaction avec Whippy
        public void DisableWhippy() {
            if (whippy) whippy.canInteract = false;
        }

        // Méthode générique pour définir l'état
        public void SetCanInteract(bool value) {
            if (whippy) whippy.canInteract = value;
        }

        public void NextStep() {
            if (_currentStepIndex < progressionSteps.Count - 1) {
                _currentStepIndex++;
                SetWhippyStep(_currentStepIndex);
            }
        }

        private void SetWhippyStep(int stepIndex) {
            if (whippy && stepIndex >= 0 && stepIndex < progressionSteps.Count) {
                whippy.SetActiveSpeechList(progressionSteps[stepIndex]);
            }
        }
    }
}
