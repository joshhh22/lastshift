using UnityEngine;

public class CardReaderInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionText = "Clock In";

    public string GetInteractionText()
    {
        return interactionText;
    }

    public void Interact()
    {
        if (ObjectiveManager.Instance.GetCurrentObjective() != "Clock In")
            return;

        Debug.Log("Shift Started");

        ObjectiveManager.Instance.CompleteObjective();
    }
}