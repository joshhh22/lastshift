using System.Collections;
using UnityEngine;

public class CCTVScreamer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int targetDay = 4;
    [SerializeField] private int targetCameraIndex = 1; // 1 = CCTV 2
    [SerializeField] private float activeDuration = 1.5f;

    [Header("Audio")]
    [SerializeField] private AudioSource jumpscareSound;

    private bool hasTriggered = false;

    private void Awake()
    {
        SetMonsterVisible(false);
    }

    // Dipanggil oleh ObjectiveManager atau DayManager saat hari baru dimulai
    public void ResetForNewDay()
    {
        hasTriggered = false;
    }

    private void Update()
    {
        if (!hasTriggered && DayManager.Instance != null && (int)DayManager.Instance.CurrentDay >= targetDay)
        {
            // Hanya trigger saat objective aktif adalah "Open CCTV"
            if (ObjectiveManager.Instance == null) return;
            if (ObjectiveManager.Instance.GetCurrentObjective() != "Open CCTV") return;

            if (CCTVManager.Instance != null && CCTVManager.Instance.CurrentIndex == targetCameraIndex)
            {
                if (TerminalMenu.Instance != null && TerminalMenu.Instance.CurrentPage == TerminalPage.CCTV)
                {
                    StartCoroutine(TriggerJumpscare());
                }
            }
        }
    }

    private IEnumerator TriggerJumpscare()
    {
        hasTriggered = true;

        // Munculkan monsternya di depan kamera
        SetMonsterVisible(true);

        // Putar suara berisik
        if (jumpscareSound != null)
        {
            jumpscareSound.Play();
        }

        // Tunggu beberapa detik
        yield return new WaitForSeconds(activeDuration);

        // Hilangkan lagi monsternya
        SetMonsterVisible(false);
    }

    private void SetMonsterVisible(bool isVisible)
    {
        // Nyalakan/Matikan mesh renderer supaya terlihat/hilang
        foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
        {
            r.enabled = isVisible;
        }
    }
}
