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
        var controller = ComputerUIController.Instance;
        if (controller != null)
        {
            if (!controller.gameObject.activeSelf)
                controller.gameObject.SetActive(true);

            controller.Open();
        }
        else
        {
            Debug.LogError("[ComputerInteractable] ComputerUIController tidak ditemukan di scene!");
        }
    }
}