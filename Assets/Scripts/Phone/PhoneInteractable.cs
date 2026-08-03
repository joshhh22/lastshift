using UnityEngine;

public class PhoneInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionText = "Check Phone";

    public string GetInteractionText()
    {
        return interactionText;
    }

    public void Interact()
    {
        PhoneManager.Instance.OpenPhone();
    }
}