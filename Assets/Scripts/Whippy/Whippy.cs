using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Whippy {
    public class Whippy : MonoBehaviour {
        [Header("Bulle de texte TMP")]
        public TMP_Text speechBubbleText;
        public GameObject speechBubble;

        [System.Serializable]
        public class SpeechListEntry {
            public string listName;
            [TextArea] public List<string> speeches;
        }

        [Header("Listes de textes nommées")]
        public List<SpeechListEntry> speechListEntries = new();
        private readonly Dictionary<string, List<string>> _speechLists = new();
        public string activeSpeechListName = "Tutorial";

        [Header("Gestion des interactions")]
        public bool canInteract = true; // Contrôlé par un manager externe
        public bool useSpeechTimer; // Active/désactive le timer
        public float speechDuration = 3f; // Durée d'affichage de chaque bulle

        private int _currentSpeechIndex;
        private float _speechTimer;

        void Awake() {
            _speechLists.Clear();
            foreach (var entry in speechListEntries) {
                if (!_speechLists.ContainsKey(entry.listName))
                    _speechLists.Add(entry.listName, entry.speeches);
            }
        }

        void Start() {
            ShowCurrentSpeech();
            _speechTimer = 0f;
        }

        void Update() {
            if (!canInteract) return;

            // Avancer seulement si le timer est écoulé
            _speechTimer += Time.deltaTime;
            bool timerReady = _speechTimer >= speechDuration;

            // Passage à la bulle suivante par clic (seulement si timer écoulé)
            if (timerReady && Input.GetMouseButtonDown(0)) {
                NextSpeech();
                _speechTimer = 0f;
                return;
            }

            // Passage à la bulle suivante par timer automatique
            if (useSpeechTimer && timerReady && GetActiveSpeechList().Count > 0 && speechBubble.activeSelf) {
                NextSpeech();
                _speechTimer = 0f;
            }
        }

        public void SetActiveSpeechList(string listName) {
            if (_speechLists.ContainsKey(listName)) {
                activeSpeechListName = listName;
                _currentSpeechIndex = 0;
                ShowCurrentSpeech();
            }
        }

        private List<string> GetActiveSpeechList() {
            if (_speechLists.TryGetValue(activeSpeechListName, out var list))
                return list;
            return new List<string>();
        }

        void ShowCurrentSpeech() {
            speechBubble.gameObject.SetActive(true);
            _speechTimer = 0f;
            var list = GetActiveSpeechList();
            if (speechBubbleText && list.Count > 0 && _currentSpeechIndex < list.Count) {
                speechBubbleText.text = list[_currentSpeechIndex];
            } else if (speechBubbleText && list.Count > 0) {
                speechBubbleText.text = ""; // Masquer la bulle à la fin
                speechBubble.SetActive(false);
            }
        }

        void NextSpeech() {
            var list = GetActiveSpeechList();
            if (_currentSpeechIndex < list.Count - 1) {
                _currentSpeechIndex++;
                ShowCurrentSpeech();
            } else {
                // Fin du tutoriel, masquer la bulle
                speechBubbleText.text = "";
                speechBubble.gameObject.SetActive(false);
            }
        }
    }
}
