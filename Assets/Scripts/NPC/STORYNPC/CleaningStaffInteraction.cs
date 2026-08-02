using UnityEngine;

public class CleaningStaffInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private DialogueData dialogue;

    private CleaningStaffController cleaningStaff;

    private bool hasTalked;

    private void Awake()
    {
        cleaningStaff = GetComponent<CleaningStaffController>();

        if (DialogueManager.Instance != null)
            DialogueManager.Instance.onDialogueFinished.AddListener(OnDialogueFinished);
    }

    private void OnDestroy()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.onDialogueFinished.RemoveListener(OnDialogueFinished);
    }

    public string GetInteractionText()
    {
        if (hasTalked)
            return "";

        return "Talk";
    }

    public void Interact()
    {
        if (hasTalked)
            return;

        hasTalked = true;

        if (cleaningStaff != null)
            cleaningStaff.StopPatrol();

        DialogueManager.Instance.StartDialogue(dialogue);
    }

    private void OnDialogueFinished()
    {
        if (cleaningStaff != null)
            cleaningStaff.StartPatrol();
    }
}