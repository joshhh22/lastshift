using System.Collections;
using TMPro;
using UnityEngine;

public class SummaryUIController : MonoBehaviour
{
    public static SummaryUIController Instance;

    [SerializeField] private GameObject root;
    [SerializeField] private Transform playerSpawnPoint;

    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text performanceText;
    [SerializeField] private TMP_Text humanityText;
    [SerializeField] private TMP_Text correctText;
    [SerializeField] private TMP_Text wrongText;
    [SerializeField] private TMP_Text servedText;

    [SerializeField] private TMP_Text continueText;

    private bool opened;

    private void Awake()
    {
        Instance = this;

        root.SetActive(false);
    }

    private void Update()
    {
        if (!opened)
            return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            NextDay();
        }
    }

    public void Open()
    {
        opened = true;

        root.SetActive(true);

        PerformanceManager p = PerformanceManager.Instance;

        dayText.text =
            $"DAY {DayManager.Instance.CurrentDayNumber} COMPLETE";

        performanceText.text =
            $"Performance : {p.Performance}";

        humanityText.text =
            $"Humanity : {p.Humanity}";

        correctText.text =
            $"Correct : {p.CorrectDecisions}";

        wrongText.text =
            $"Wrong : {p.WrongDecisions}";

        servedText.text =
            $"Served : {p.PassengersServed}";
    }

    void NextDay()
    {
        StartCoroutine(NextDayRoutine());
    }

    IEnumerator NextDayRoutine()
    {
        // Fade ke hitam
        yield return FadeController.Instance.FadeOut();

        // Tutup summary
        root.SetActive(false);
        opened = false;

        // Teleport player
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null && playerSpawnPoint != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();

            if (cc != null)
                cc.enabled = false;

            player.transform.SetPositionAndRotation(
                playerSpawnPoint.position,
                playerSpawnPoint.rotation);

            if (cc != null)
                cc.enabled = true;
        }

        // Ganti hari
        DayManager.Instance.NextDay();

        // Reset semua manager
        ObjectiveManager.Instance.ResetObjectives();
        PerformanceManager.Instance.ResetDay();
        GameTimeManager.Instance.ResetTime();
        PassengerScheduleManager.Instance.ResetSchedules();

        // Fade masuk lagi
        yield return FadeController.Instance.FadeIn();
    }
}