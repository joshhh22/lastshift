using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactLayer;

    [Header("UI")]
    [SerializeField] private InteractionUI interactionUI;

    private IInteractable currentInteractable;

    private void Update()
    {
        CheckInteraction();

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E DITEKAN");
        }

        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    private void CheckInteraction()
    {
        currentInteractable = null;

        if (Physics.Raycast(
            playerCamera.transform.position,
            playerCamera.transform.forward,
            out RaycastHit hit,
            interactDistance,
            interactLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                string text = interactable.GetInteractionText();

                if (!string.IsNullOrEmpty(text))
                {
                    currentInteractable = interactable;
                    interactionUI.Show(text);
                    return;
                }
            }
        }

        interactionUI.Hide();
    }
}