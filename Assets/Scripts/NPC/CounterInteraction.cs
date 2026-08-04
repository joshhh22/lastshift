using UnityEngine;

public class CounterInteraction : MonoBehaviour, IInteractable
{
    public NPCController currentNPC;

    public string GetInteractionText()
    {
        if (currentNPC == null)
            return "";

        return "Serve Passenger";
    }

    public void Interact()
    {
        if (currentNPC == null)
            return;

        ServePassengerUIController.Instance.Open(currentNPC);
    }

    public NPCController GetCurrentNPC()
    {
        return currentNPC;
    }
}