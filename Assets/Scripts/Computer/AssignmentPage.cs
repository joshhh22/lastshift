using System.Text;
using TMPro;
using UnityEngine;

public class AssignmentPage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI timeText;

    private void OnEnable()
    {
        RefreshObjectives();
        UpdateFooter();
    }

    public void RefreshObjectives()
    {
        if (ObjectiveManager.Instance == null)
            return;

        StringBuilder builder = new StringBuilder();

        var objectives = ObjectiveManager.Instance.GetObjectives();
        int currentIndex = ObjectiveManager.Instance.GetCurrentIndex();

        Debug.Log("Current Index : " + currentIndex);

        for (int i = 0; i < objectives.Count; i++)
        {
            Debug.Log($"{i} : {objectives[i].title}");

            if (i < currentIndex)
                builder.AppendLine($"[DONE] {objectives[i].title}");
            else if (i == currentIndex)
                builder.AppendLine($"> {objectives[i].title}");
            else
                builder.AppendLine($"□ {objectives[i].title}");
        }

        objectiveText.text = builder.ToString();
    }

    private void UpdateFooter()
    {
        string dayStr = DayManager.Instance != null ? $"DAY {(int)DayManager.Instance.CurrentDay}" : "DAY 1";
        string timeStr = GameTimeManager.Instance != null ? GameTimeManager.Instance.GetCurrentTime() : "22:00";

        if (dayText != null)
            dayText.text = dayStr;

        if (timeText != null)
            timeText.text = timeStr;

        foreach (TextMeshProUGUI tmp in GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (tmp == null) continue;
            if (tmp.gameObject.name == "Day")
                tmp.text = dayStr;
            else if (tmp.gameObject.name == "Time")
                tmp.text = timeStr;
        }
    }
}