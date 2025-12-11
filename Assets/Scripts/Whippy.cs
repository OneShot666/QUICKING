using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Serialization;

// Personal TO DO List
// R Add clients

// Official TO DO List
// R In-game interface allowing player to turn pages of a recipe book
// R Non-diegetic interface to manage player’s hands : can carry ingredients and objects 
// L Zoomable inventory menu
// L Second, non-blocking display mode for the inventory, accessible by clicking the minimize icon
public class Whippy : MonoBehaviour
{
    [Header("Bulle de texte TMP")]
    public TMP_Text speechBubbleText;
    public GameObject speechBubble;

    [System.Serializable]
    public class SpeechListEntry
    {
        public string listName;
        [TextArea]
        public List<string> speeches;
    }

    [Header("Listes de textes nommées")]
    public List<SpeechListEntry> speechListEntries = new List<SpeechListEntry>();
    private Dictionary<string, List<string>> speechLists = new Dictionary<string, List<string>>();
    public string activeSpeechListName = "Tutorial";

    [Header("Gestion des interactions")]
    public bool canInteract = true; // Contrôlé par un manager externe
    public bool useSpeechTimer = false; // Active/désactive le timer
    public float speechDuration = 3f; // Durée d'affichage de chaque bulle

    private int currentSpeechIndex = 0;
    private float speechTimer = 0f;

    void Awake()
    {
        speechLists.Clear();
        foreach (var entry in speechListEntries)
        {
            if (!speechLists.ContainsKey(entry.listName))
                speechLists.Add(entry.listName, entry.speeches);
        }
    }

    void Start()
    {
        ShowCurrentSpeech();
        speechTimer = 0f;
    }

    void Update()
    {
        if (!canInteract) return;

        // Avancer seulement si le timer est écoulé
        speechTimer += Time.deltaTime;
        bool timerReady = speechTimer >= speechDuration;

        // Passage à la bulle suivante par clic (seulement si timer écoulé)
        if (timerReady && Input.GetMouseButtonDown(0))
        {
            NextSpeech();
            speechTimer = 0f;
            return;
        }

        // Passage à la bulle suivante par timer automatique
        if (useSpeechTimer && timerReady && GetActiveSpeechList().Count > 0 && speechBubble.activeSelf)
        {
            NextSpeech();
            speechTimer = 0f;
        }
    }

    public void SetActiveSpeechList(string listName)
    {
        if (speechLists.ContainsKey(listName))
        {
            activeSpeechListName = listName;
            currentSpeechIndex = 0;
            ShowCurrentSpeech();
        }
    }

    private List<string> GetActiveSpeechList()
    {
        if (speechLists.ContainsKey(activeSpeechListName))
            return speechLists[activeSpeechListName];
        return new List<string>();
    }

    void ShowCurrentSpeech()
    {
        speechBubble.gameObject.SetActive(true);
        speechTimer = 0f;
        var list = GetActiveSpeechList();
        if (speechBubbleText != null && list.Count > 0 && currentSpeechIndex < list.Count)
        {
            speechBubbleText.text = list[currentSpeechIndex];
        }
        else if (speechBubbleText != null && list.Count > 0)
        {
            speechBubbleText.text = ""; // Masquer la bulle à la fin
            speechBubble.SetActive(false);
        }
    }

    void NextSpeech()
    {
        var list = GetActiveSpeechList();
        if (currentSpeechIndex < list.Count - 1)
        {
            currentSpeechIndex++;
            ShowCurrentSpeech();
        }
        else
        {
            // Fin du tutoriel, masquer la bulle
            speechBubbleText.text = "";
            speechBubble.gameObject.SetActive(false);
        }
    }
}
