using TMPro;
using UnityEngine;

public class ObjectiveUI : MonoBehaviour
{
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text objectiveText;

    public void UpdateUI(int day, string objective)
    {
        if (dayText != null) dayText.text = $"DAY {day}";
        if (objectiveText != null) objectiveText.text = objective;
    }

    public void UpdateObjectiveDisplay(string objective)
    {
        if (objectiveText != null) objectiveText.text = objective;
    }
}