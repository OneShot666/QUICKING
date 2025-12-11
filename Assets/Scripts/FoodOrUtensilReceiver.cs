using UnityEngine;
using UnityEngine.UI;

public class FoodOrUtensilReceiver : InteractionReceiver
{
    public enum HandType { Left, Right }
    public string foodName;
    public bool isKnife;
    public GameObject uiButton;
    public string sliceText = "Trancher";
    public string pickUpText = "Prendre";

    [Header("UI de choix de main")]
    public GameObject leftHandButton;
    public GameObject rightHandButton;
    public Text feedbackText;

    private Player player;

    public override void Preview()
    {
        // Afficher un aperçu visuel ou UI de l'objet
    }

    public override void Interact()
    {
        player = FindFirstObjectByType<Player>();
        if (player == null) return;

        // Afficher les boutons de choix de main
        if (leftHandButton != null)
        {
            leftHandButton.SetActive(true);
            leftHandButton.GetComponent<Button>().onClick.RemoveAllListeners();
            leftHandButton.GetComponent<Button>().onClick.AddListener(() => TryEquip(true));
        }
        if (rightHandButton != null)
        {
            rightHandButton.SetActive(true);
            rightHandButton.GetComponent<Button>().onClick.RemoveAllListeners();
            rightHandButton.GetComponent<Button>().onClick.AddListener(() => TryEquip(false));
        }
    }

    private void TryEquip(bool leftHand)
    {
        if (player.CanEquipInHand(leftHand))
        {
            player.EquipItem(this, leftHand);
            HideHandButtons();
            if (feedbackText != null) feedbackText.text = "";
        }
        else
        {
            if (feedbackText != null) feedbackText.text = "La main est déjà occupée !";
        }
    }

    private void HideHandButtons()
    {
        if (leftHandButton != null) leftHandButton.SetActive(false);
        if (rightHandButton != null) rightHandButton.SetActive(false);
    }

    private void ShowUIButton(string text)
    {
        if (uiButton != null)
        {
            uiButton.SetActive(true);
            var btnText = uiButton.GetComponentInChildren<Text>();
            if (btnText != null)
                btnText.text = text;
        }
    }
}
