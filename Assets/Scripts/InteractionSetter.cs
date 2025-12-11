using UnityEngine;
using System.Collections.Generic;

public class InteractionSetter : MonoBehaviour
{
    public KeyCode interactionKey = KeyCode.E;
    private List<InteractionReceiver> receiversInRange = new List<InteractionReceiver>();
    private InteractionReceiver currentReceiver;

    void Update()
    {
        // Déterminer le receiver à prioriser (ex : le dernier entré ou le plus proche)
        if (receiversInRange.Count > 0)
        {
            currentReceiver = receiversInRange[receiversInRange.Count - 1];
            currentReceiver.Preview();
        }
        else
        {
            currentReceiver = null;
        }

        // Interaction
        if (currentReceiver != null && Input.GetKeyDown(interactionKey))
        {
            currentReceiver.Interact();
        }
    }

    public void AddReceiverInRange(InteractionReceiver receiver)
    {
        if (!receiversInRange.Contains(receiver))
            receiversInRange.Add(receiver);
    }

    public void RemoveReceiverInRange(InteractionReceiver receiver)
    {
        if (receiversInRange.Contains(receiver))
            receiversInRange.Remove(receiver);
    }
}
