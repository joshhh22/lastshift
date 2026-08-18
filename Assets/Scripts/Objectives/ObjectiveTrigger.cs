using UnityEngine;

public class ObjectiveTrigger : MonoBehaviour
{
    [SerializeField] private string targetObjectiveKeyword = "office";
    private bool triggered;

    public void ResetTrigger()
    {
        triggered = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryTrigger(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryTrigger(other);
    }

    private void TryTrigger(Collider other)
    {
        if (triggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        // JANGAN trigger jika pemain sedang monolog awal atau terkunci di awal game
        if (PlayerLockManager.Instance != null && PlayerLockManager.Instance.IsLocked)
            return;

        if (ObjectiveManager.Instance == null)
            return;

        string curObj = ObjectiveManager.Instance.GetCurrentObjective();
        if (string.IsNullOrEmpty(curObj))
            return;

        // PASTIKAN HANYA MENYELESAIKAN JIKA OBJECTIVE SEKARANG ADALAH "Go To Office"!
        // JANGAN pernah menyelesaikan "Clock In", "Open Computer", atau objektif lain lewat trigger lorong!
        if (!curObj.ToLower().Contains(targetObjectiveKeyword.ToLower()) && !curObj.ToLower().Contains("go to"))
            return;

        triggered = true;

        Debug.Log("<color=green>[ObjectiveTrigger]</color> Selesai: 'Go To Office'. Objektif berikutnya: 'Clock In'.");
        ObjectiveManager.Instance.CompleteObjective();
    }
}