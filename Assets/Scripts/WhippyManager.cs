using UnityEngine;
using System.Collections.Generic;

public class WhippyManager : MonoBehaviour
{
    [Header("Référence à Whippy")]
    public Whippy whippy;

    [Header("Progression du joueur")]
    public List<string> progressionSteps = new List<string> { "Tutorial", "Level1", "Level2" };
    private int currentStepIndex = 0;

    void Awake()
    {
        // Si la référence n'est pas assignée, on essaie de la trouver automatiquement
        if (whippy == null)
        {
            whippy = FindFirstObjectByType<Whippy>();
        }
        SetWhippyStep(currentStepIndex);
    }

    // Active l'interaction avec Whippy
    public void EnableWhippy()
    {
        if (whippy != null)
            whippy.canInteract = true;
    }

    // Désactive l'interaction avec Whippy
    public void DisableWhippy()
    {
        if (whippy != null)
            whippy.canInteract = false;
    }

    // Méthode générique pour définir l'état
    public void SetCanInteract(bool value)
    {
        if (whippy != null)
            whippy.canInteract = value;
    }

    public void NextStep()
    {
        if (currentStepIndex < progressionSteps.Count - 1)
        {
            currentStepIndex++;
            SetWhippyStep(currentStepIndex);
        }
    }

    public void SetWhippyStep(int stepIndex)
    {
        if (whippy != null && stepIndex >= 0 && stepIndex < progressionSteps.Count)
        {
            whippy.SetActiveSpeechList(progressionSteps[stepIndex]);
        }
    }
}
