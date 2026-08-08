using UnityEngine;

public class CardReaderInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionText = "Clock In";

    public string GetInteractionText()
    {
        string currentObjective =
            ObjectiveManager.Instance.GetCurrentObjective();

        if (currentObjective == "Clock In")
            return "Clock In";

        if (currentObjective == "Clock Out")
            return "Clock Out";

        return "";
    }

    public void Interact()
    {
        string currentObjective =
            ObjectiveManager.Instance.GetCurrentObjective();

        Debug.Log("Interact -> " + currentObjective);

        switch (currentObjective)
        {
            case "Clock In":

                AudioManager.Instance.PlayClockBeep();
                Debug.Log("Shift Started");

                ObjectiveManager.Instance.CompleteObjective();

                break;

            case "Clock Out":

                AudioManager.Instance.PlayClockBeep();
                Debug.Log("Shift Ended");

                ObjectiveManager.Instance.CompleteObjective();

                SummaryUIController.Instance.Open();

                break;
        }
    }
}