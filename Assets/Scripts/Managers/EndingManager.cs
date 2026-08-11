using System.Collections;
using UnityEngine;

/// <summary>
/// Menentukan dan menjalankan ending berdasarkan akumulasi Performance & Humanity
/// di akhir Day 7. Pakai DialogueManager yang sudah ada.
/// </summary>
public class EndingManager : MonoBehaviour
{
    public static EndingManager Instance { get; private set; }

    [Header("Ending Dialogues")]
    [Tooltip("Ending 'The Machine' — Performance ≥ 70, Humanity ≤ 30")]
    [SerializeField] private DialogueData endingTheMachine;

    [Tooltip("Ending 'The Human' — Humanity ≥ 70, Performance ≤ 40")]
    [SerializeField] private DialogueData endingTheHuman;

    [Tooltip("Ending 'The Lost' — Kondisi di antara keduanya")]
    [SerializeField] private DialogueData endingTheLost;

    [Header("Credits")]
    [SerializeField] private GameObject creditsScreen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (creditsScreen != null)
            creditsScreen.SetActive(false);
    }

    /// <summary>
    /// Dipanggil dari SummaryUIController saat Day 7 selesai.
    /// </summary>
    public void TriggerEnding()
    {
        StartCoroutine(EndingRoutine());
    }

    private IEnumerator EndingRoutine()
    {
        // Fade ke hitam dulu
        yield return FadeController.Instance.FadeOut();

        int perf = PerformanceManager.Instance.Performance;
        int human = PerformanceManager.Instance.Humanity;

        DialogueData selectedEnding = DetermineEnding(perf, human);

        Debug.Log($"[Ending] Perf={perf} Human={human} → {selectedEnding?.name}");

        // Fade balik
        yield return FadeController.Instance.FadeIn();

        // Mulai dialogue ending
        if (selectedEnding != null)
        {
            // Kunci player selama ending
            PlayerLockManager.Instance.LockPlayer();

            DialogueManager.Instance.onDialogueFinished.AddListener(OnEndingDialogueFinished);
            DialogueManager.Instance.StartDialogue(selectedEnding);
        }
        else
        {
            ShowCredits();
        }
    }

    private DialogueData DetermineEnding(int performance, int humanity)
    {
        // "The Machine" — Kamu mengikuti aturan tanpa kompromi
        if (performance >= 70 && humanity <= 30)
            return endingTheMachine;

        // "The Human" — Kamu memprioritaskan manusia di atas sistem
        if (humanity >= 70 && performance <= 40)
            return endingTheHuman;

        // "The Lost" — Di antara keduanya, tidak benar-benar memilih
        return endingTheLost;
    }

    private void OnEndingDialogueFinished()
    {
        DialogueManager.Instance.onDialogueFinished.RemoveListener(OnEndingDialogueFinished);
        ShowCredits();
    }

    private void ShowCredits()
    {
        if (creditsScreen != null)
        {
            creditsScreen.SetActive(true);
        }
        else
        {
            Debug.Log("[Ending] Credits screen not assigned.");
        }
    }
}
