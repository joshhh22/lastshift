using UnityEngine;

public class ComputerInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionText = "Use Computer";

    public string GetInteractionText()
    {
        return interactionText;
    }

    public void Interact()
    {

        ComputerUIController.Instance.Open();
    }
}