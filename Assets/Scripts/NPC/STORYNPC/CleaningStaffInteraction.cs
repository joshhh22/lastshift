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

    public void ResetForNewDay()
    {
        hasTalked = false;
    }

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

        if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.GetCurrentIndex() != requiredObjectiveIndex)
        {
            string curObj = ObjectiveManager.Instance.GetCurrentObjective();
            if (string.IsNullOrEmpty(curObj) || !curObj.ToLower().Contains("cleaning"))
                return "";
        }

        return "Talk";
    }

    public void Interact()
    {
        if (hasTalked)
            return;

        if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.GetCurrentIndex() != requiredObjectiveIndex)
        {
            string curObj = ObjectiveManager.Instance.GetCurrentObjective();
            if (string.IsNullOrEmpty(curObj) || !curObj.ToLower().Contains("cleaning"))
                return;
        }

        hasTalked = true;

        if (cleaningStaff != null)
        {
            cleaningStaff.StopPatrol();

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                cleaningStaff.FacePlayer(playerObj.transform);
            }
        }

        DialogueData dialogueToPlay = fallbackDialogue;

        if (DayManager.Instance != null)
        {
            int dayIndex = (int)DayManager.Instance.CurrentDay - 1;
            if (dailyDialogues != null && dayIndex >= 0 && dayIndex < dailyDialogues.Length && dailyDialogues[dayIndex] != null)
            {
                dialogueToPlay = dailyDialogues[dayIndex];
            }
        }

        if (dialogueToPlay != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(dialogueToPlay);
        }
    }

    private void OnDialogueFinished()
    {
        if (hasTalked && cleaningStaff != null)
        {
            // Buka patroli bebas ke seluruh stasiun
            cleaningStaff.UnlockFullPatrol();
            cleaningStaff.StartPatrol();

            // Selesaikan objektif 'Talk To Cleaning Staff'
            if (ObjectiveManager.Instance != null)
            {
                string curObj = ObjectiveManager.Instance.GetCurrentObjective();
                if (!string.IsNullOrEmpty(curObj) && curObj.ToLower().Contains("cleaning"))
                {
                    ObjectiveManager.Instance.CompleteObjective();
                }
            }
        }
    }
}