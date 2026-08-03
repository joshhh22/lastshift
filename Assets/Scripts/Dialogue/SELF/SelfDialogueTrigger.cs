using UnityEngine;

public class SelfDialogueTrigger : MonoBehaviour, IInteractable
{
    [Header("Dialogue")]
    [SerializeField] private DialogueData dialogue;

    [Header("Requirements")]
    [SerializeField] private int requiredObjectiveIndex;

    private bool played;

    public string GetInteractionText()
    {
        if (played)
            return "";

        if (ObjectiveManager.Instance.GetCurrentIndex() != requiredObjectiveIndex)
            return "";

        return "Check Phone";
    }

    public void Interact()
    {
        if (played)
            return;

        if (ObjectiveManager.Instance.GetCurrentIndex() != requiredObjectiveIndex)
            return;

        played = true;

        DialogueManager.Instance.StartDialogue(dialogue);
    }
}