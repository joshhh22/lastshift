using UnityEngine;

public class ShiftEndController : MonoBehaviour
{
    private bool completed = false;

    public void ResetController()
    {
        completed = false;
    }

    private void Update()
    {
        if (completed)
            return;

        // Shift belum selesai
        if (GameTimeManager.Instance == null || !GameTimeManager.Instance.IsShiftEnded)
            return;

        // Jangan selesaikan jika player belum sampai pada tahap kerja shift (misal masih "Go to Office" atau "Check Phone")
        if (ObjectiveManager.Instance == null)
            return;

        string currentObj = ObjectiveManager.Instance.GetCurrentObjective();
        if (string.IsNullOrEmpty(currentObj))
            return;

        string lower = currentObj.ToLower();
        // Hanya selesaikan jika objective saat ini adalah melayani penumpang atau shift kerja
        if (!lower.Contains("serve") && !lower.Contains("work") && !lower.Contains("shift") && !lower.Contains("continue"))
            return;

        // Masih ada NPC di antrean
        if (QueueManager.Instance != null && QueueManager.Instance.GetQueueCount() > 0)
            return;

        // Masih ada NPC di counter atau berjalan menuju counter
        if (CounterManager.Instance != null && (CounterManager.Instance.IsOccupied() || CounterManager.Instance.IsReserved()))
            return;

        completed = true;

        Debug.Log("<color=green>[ShiftEndController]</color> All passengers served and shift officially completed.");

        ObjectiveManager.Instance.CompleteObjective();
    }
}