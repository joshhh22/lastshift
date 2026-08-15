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
        if (objectiveText == null)
            objectiveText = GetComponentInChildren<TextMeshProUGUI>(true);

        if (objectiveText == null)
            return;

        if (ObjectiveManager.Instance == null)
        {
            objectiveText.text =
                "<color=#00F0FF><b>> [AKTIF]</b></color>  <color=#FFFFFF><b>Periksa Sistem Komputer & Loket</b></color>\n\n" +
                "<color=#607D8B><b>- [PENDING]</b></color>  <color=#78909C>Periksa CCTV Keamanan Stasiun</color>\n\n" +
                "<color=#607D8B><b>- [PENDING]</b></color>  <color=#78909C>Layani & Saring Penumpang Kereta</color>";
            return;
        }

        StringBuilder builder = new StringBuilder();

        var objectives = ObjectiveManager.Instance.GetObjectives();
        int currentIndex = ObjectiveManager.Instance.GetCurrentIndex();

        if (objectives == null || objectives.Count == 0)
        {
            objectiveText.text = "<color=#00E676><b>* SEMUA TUGAS SHIFT MALAM SELESAI</b></color>";
            return;
        }

        for (int i = 0; i < objectives.Count; i++)
        {
            if (i < currentIndex)
            {
                builder.AppendLine($"<color=#00E676><b>* [SELESAI]</b></color>  <color=#B0BEC5><s>{objectives[i].title}</s></color>\n");
            }
            else if (i == currentIndex)
            {
                builder.AppendLine($"<color=#00F0FF><b>> [AKTIF]</b></color>  <color=#FFFFFF><b>{objectives[i].title}</b></color>\n");
            }
            else
            {
                builder.AppendLine($"<color=#607D8B><b>- [PENDING]</b></color>  <color=#78909C>{objectives[i].title}</color>\n");
            }
        }

        objectiveText.text = builder.ToString().TrimEnd();
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