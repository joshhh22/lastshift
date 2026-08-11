using UnityEngine;

public class ShiftEndController : MonoBehaviour
{
    private bool completed = false;

    private void Update()
    {
        if (completed)
            return;

        // Shift belum selesai
        if (!GameTimeManager.Instance.IsShiftEnded)
            return;

        // Masih ada NPC di antrean
        if (QueueManager.Instance.GetQueueCount() > 0)
            return;

        // Masih ada NPC di counter atau berjalan menuju counter
        if (CounterManager.Instance.IsOccupied() || CounterManager.Instance.IsReserved())
            return;

        completed = true;

        Debug.Log("All passengers served.");

        ObjectiveManager.Instance.CompleteObjective();
    }
}