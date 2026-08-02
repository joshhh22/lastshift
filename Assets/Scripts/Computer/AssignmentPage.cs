using System.Text;
using TMPro;
using UnityEngine;

public class AssignmentPage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI objectiveText;

    private void OnEnable()
    {
        RefreshObjectives();
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
}