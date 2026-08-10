using UnityEngine;

public class CleaningStaffInteraction : MonoBehaviour, IInteractable
{
    [Header("Daily Dialogues")]
    [Tooltip("Index 0 = Day 1, Index 1 = Day 2, etc.")]
    [SerializeField] private DialogueData[] dailyDialogues;

    [Header("Fallback Dialogue")]
    [SerializeField] private DialogueData fallbackDialogue;

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

        DialogueData dialogueToPlay = fallbackDialogue;

        if (DayManager.Instance != null)
        {
            int dayIndex = (int)DayManager.Instance.CurrentDay - 1;
            if (dailyDialogues != null && dayIndex >= 0 && dayIndex < dailyDialogues.Length && dailyDialogues[dayIndex] != null)
            {
                dialogueToPlay = dailyDialogues[dayIndex];
            }
        }

        if (dialogueToPlay != null)
        {
            DialogueManager.Instance.StartDialogue(dialogueToPlay);
        }
    }

    private void OnDialogueFinished()
    {
        if (cleaningStaff != null)
            cleaningStaff.StartPatrol();
    }
}