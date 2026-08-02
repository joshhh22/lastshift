using TMPro;
using UnityEngine;

public class ObjectiveUI : MonoBehaviour
{
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text objectiveText;

    public void UpdateUI(int day, string objective)
    {
        dayText.text = $"DAY {day}";
        objectiveText.text = objective;
    }
}