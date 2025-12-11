using UnityEngine;

public class InteractionReceiver : MonoBehaviour
{
    // Méthode appelée pour afficher un aperçu (ex : contenu du frigo)
    public virtual void Preview()
    {
        // Par défaut, rien. Surcharger dans les classes dérivées.
    }

    // Méthode appelée lors de l'interaction (ex : ouvrir la porte)
    public virtual void Interact()
    {
        Debug.Log($"{gameObject.name} a été interagi.");
        // Ajoutez ici le comportement spécifique à l'objet
    }

    // Détection de l'entrée du joueur dans la zone de trigger
    private void OnTriggerEnter(Collider other)
    {
        var setter = other.GetComponent<InteractionSetter>();
        if (setter != null)
        {
            setter.AddReceiverInRange(this);
        }
    }

    // Détection de la sortie du joueur de la zone de trigger
    private void OnTriggerExit(Collider other)
    {
        var setter = other.GetComponent<InteractionSetter>();
        if (setter != null)
        {
            setter.RemoveReceiverInRange(this);
        }
    }
}
