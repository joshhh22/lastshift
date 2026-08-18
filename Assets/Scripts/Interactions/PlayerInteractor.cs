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

        // Raycast hit ke semua layer (kecuali Player) agar tembok memblokir interaksi
        int layerMask = ~LayerMask.GetMask("Player", "Ignore Raycast");

        if (Physics.Raycast(
            playerCamera.transform.position,
            playerCamera.transform.forward,
            out RaycastHit hit,
            interactDistance,
            layerMask,
            QueryTriggerInteraction.Collide))
        {
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable == null)
                interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                string text = interactable.GetInteractionText();

                if (!string.IsNullOrEmpty(text))
                {
                    currentInteractable = interactable;
                    interactionUI.Show(text);

                    if (ObjectiveOutlineManager.Instance != null)
                    {
                        ObjectiveOutlineManager.Instance.OnHoverInteractable(hit.collider.gameObject);
                    }
                    return;
                }
            }
        }

        interactionUI.Hide();

        if (ObjectiveOutlineManager.Instance != null)
        {
            ObjectiveOutlineManager.Instance.OnHoverInteractable(null);
        }
    }
}