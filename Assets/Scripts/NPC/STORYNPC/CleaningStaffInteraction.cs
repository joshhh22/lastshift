using UnityEngine;

public class CleaningStaffInteraction : MonoBehaviour, IInteractable
{
    [Header("Dialogue")]
    [SerializeField] private DialogueData dialogue;

    [Header("Requirements")]
    [SerializeField] private int requiredObjectiveIndex;

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

        if (ObjectiveManager.Instance.GetCurrentIndex() != requiredObjectiveIndex)
            return "";

        return "Talk";
    }

    public void Interact()
    {
        if (hasTalked)
            return;

        if (ObjectiveManager.Instance.GetCurrentIndex() != requiredObjectiveIndex)
            return;

        hasTalked = true;

        cleaningStaff.StopPatrol();

        Transform player = GameObject.FindGameObjectWithTag("Player").transform;

        cleaningStaff.FacePlayer(player);

        DialogueManager.Instance.StartDialogue(dialogue);
    }

    private void OnDialogueFinished()
    {
        if (cleaningStaff != null)
            cleaningStaff.StartPatrol();
    }
}