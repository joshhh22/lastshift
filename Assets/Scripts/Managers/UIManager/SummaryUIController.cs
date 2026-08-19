using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SummaryUIController : MonoBehaviour
{
    public static SummaryUIController Instance { get; private set; }

    [Header("Root & Spawn")]
    [SerializeField] private GameObject root;
    [SerializeField] private Transform playerSpawnPoint;

    [Header("Header & Info")]
    [SerializeField] private TMP_Text shiftReportTitleText;
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text stationInfoText;

    [Header("Evaluation Badge & Memo")]
    [SerializeField] private GameObject evaluationBadgeContainer;
    [SerializeField] private TMP_Text evaluationBadgeText;
    [SerializeField] private Image evaluationBadgeBg;
    [SerializeField] private TMP_Text supervisorMemoText;

    [Header("Statistics Metrics")]
    [SerializeField] private TMP_Text performanceText;
    [SerializeField] private Image performanceFillBar;
    [SerializeField] private TMP_Text humanityText;
    [SerializeField] private Image humanityFillBar;
    [SerializeField] private TMP_Text correctText;
    [SerializeField] private TMP_Text wrongText;
    [SerializeField] private TMP_Text servedText;

    [Header("Failure / Violation Logs Container")]
    [SerializeField] private GameObject failureLogsContainer;
    [SerializeField] private TMP_Text failureLogsTitleText;
    [SerializeField] private TMP_Text failureLogsContentText;

    [Header("Footer & Prompt")]
    [SerializeField] private TMP_Text continueText;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip reportOpenSfx;
    [SerializeField] private AudioClip stampSfx;

    private bool opened = false;
    private bool isTransitioning = false;
    private Coroutine activeDisplayRoutine;

    public bool IsOpen => opened || (root != null && root.activeSelf);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (root != null)
            root.SetActive(false);
    }

    private void Update()
    {
        if (!opened || isTransitioning)
            return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
        {
            NextDay();
        }
    }

    public void Open()
    {
        opened = true;
        isTransitioning = false;

        if (root != null)
            root.SetActive(true);

        HideOtherUI(true);

        if (activeDisplayRoutine != null) StopCoroutine(activeDisplayRoutine);
        activeDisplayRoutine = StartCoroutine(DisplaySummarySequence());
    }

    private IEnumerator DisplaySummarySequence()
    {
        PerformanceManager p = PerformanceManager.Instance;
        int dayNum = DayManager.Instance != null ? DayManager.Instance.CurrentDayNumber : 1;

        if (reportOpenSfx != null && audioSource != null)
        {
            audioSource.PlayOneShot(reportOpenSfx, 0.8f);
        }

        // 1. Setup Header Info
        if (shiftReportTitleText != null)
            shiftReportTitleText.text = "METRO TRANSIT AUTHORITY // DAILY SHIFT REPORT";

        if (dayText != null)
            dayText.text = $"DAY {dayNum:D2} COMPLETE";

        if (stationInfoText != null)
            stationInfoText.text = $"SECTOR 04 SUBWAY // SHIFT: 00:00 - 04:00 AM // OPERATOR ID: #4092-A";

        // 2. Metrics & Scores
        int perf = p != null ? p.Performance : 50;
        int hum = p != null ? p.Humanity : 50;
        int corr = p != null ? p.CorrectDecisions : 0;
        int wrg = p != null ? p.WrongDecisions : 0;
        int srv = p != null ? p.PassengersServed : 0;

        if (performanceText != null)
            performanceText.text = $"PERFORMANCE INDEX : {perf}%";

        if (performanceFillBar != null)
            performanceFillBar.fillAmount = Mathf.Clamp01(perf / 100f);

        if (humanityText != null)
            humanityText.text = $"HUMANITY INDEX    : {hum}%";

        if (humanityFillBar != null)
            humanityFillBar.fillAmount = Mathf.Clamp01(hum / 100f);

        if (correctText != null)
            correctText.text = $"ACCURATE INSPECTIONS : {corr}";

        if (wrongText != null)
            wrongText.text = $"PROTOCOL VIOLATIONS  : {wrg}";

        if (servedText != null)
            servedText.text = $"PASSENGERS PROCESSED : {srv}";

        // 3. Render Failure / Incident Violations Log ("Alasan Kegagalan Hari Itu")
        IReadOnlyList<string> violations = p != null ? p.DayViolations : null;
        StringBuilder sb = new StringBuilder();

        if (violations != null && violations.Count > 0)
        {
            foreach (string v in violations)
            {
                sb.AppendLine($"<color=#FF4D4D>• {v}</color>");
            }
        }
        else
        {
            sb.AppendLine("<color=#00FF99>• [04:00] Tidak ada pelanggaran protokol hari ini. Shift berjalan sempurna tanpa insiden.</color>");
        }

        if (failureLogsContentText != null)
            failureLogsContentText.text = sb.ToString();

        // 4. Evaluation Badge & Supervisor Psychological Memo
        string grade;
        string memo;
        Color badgeColor;

        if (perf >= 80 && wrg == 0)
        {
            grade = "[ EVALUATION: EXCELLARY / SATISFACTORY ]";
            memo = "\"Pengawas stasiun puas dengan ketelitianmu. Protokol dipatuhi tanpa cela. Jangan lengah di shift berikutnya.\"";
            badgeColor = new Color(0f, 0.9f, 0.4f, 1f);
        }
        else if (perf >= 40)
        {
            grade = "[ EVALUATION: WARNING / SUB-OPTIMAL ]";
            memo = "\"Ada beberapa ketidaktelitian selama shift berlangsung. Pastikan kamu selalu memeriksa CCTV dan dokumen penumpang dengan teliti.\"";
            badgeColor = new Color(1f, 0.75f, 0.1f, 1f);
        }
        else
        {
            grade = "[ EVALUATION: CRITICAL RISK / PENALIZED ]";
            memo = "\"Performa stasiun berada di zona merah bahaya. Sesuatu yang ganjil menyelinap di antara bayangan. Hati-hati saat menutup pintu stasiun.\"";
            badgeColor = new Color(1f, 0.2f, 0.2f, 1f);
        }

        if (evaluationBadgeText != null)
        {
            evaluationBadgeText.text = grade;
            evaluationBadgeText.color = badgeColor;
        }

        if (evaluationBadgeBg != null)
        {
            evaluationBadgeBg.color = new Color(badgeColor.r, badgeColor.g, badgeColor.b, 0.2f);
        }

        if (supervisorMemoText != null)
        {
            supervisorMemoText.text = memo;
        }

        if (stampSfx != null && audioSource != null)
        {
            yield return new WaitForSeconds(0.4f);
            audioSource.PlayOneShot(stampSfx, 0.9f);
        }

        // 5. Blinking Continue Prompt
        if (continueText != null)
        {
            continueText.text = ">> TEKAN [ENTER / SPASI] UNTUK MEMULAI SHIFT BERIKUTNYA <<";
        }
    }

    void NextDay()
    {
        if (isTransitioning) return;
        isTransitioning = true;
        opened = false;

        if (DayManager.Instance != null && DayManager.Instance.CurrentDay == GameDay.Day7)
        {
            if (EndingManager.Instance != null)
            {
                if (root != null) root.SetActive(false);
                HideOtherUI(true);
                EndingManager.Instance.TriggerEnding();
            }
            return;
        }

        StartCoroutine(NextDayRoutine());
    }

    IEnumerator NextDayRoutine()
    {
        // Fade ke hitam
        yield return FadeController.Instance.FadeOut();

        // Tutup summary
        if (root != null) root.SetActive(false);

        // Teleport player
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (playerSpawnPoint == null)
        {
            GameObject sp = GameObject.Find("PlayerSpawnPoint");
            if (sp != null)
            {
                playerSpawnPoint = sp.transform;
            }
        }

        if (player != null && playerSpawnPoint != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            var fpc = player.GetComponent<StarterAssets.FirstPersonController>();

            if (cc != null) cc.enabled = false;
            if (fpc != null) fpc.enabled = false;

            Vector3 spawnPos = playerSpawnPoint.position;
            // Snapping presisi ke lantai agar tidak ada sensasi jatuh dari atas
            if (Physics.Raycast(spawnPos + Vector3.up * 1f, Vector3.down, out RaycastHit hit, 5f))
            {
                spawnPos = hit.point + Vector3.up * 0.05f;
            }

            player.transform.SetPositionAndRotation(spawnPos, playerSpawnPoint.rotation);

            // Reset camera pitch agar hadap lurus ke depan (tidak nengok ke atas)
            Transform camTarget = player.transform.Find("CinemachineCameraTarget");
            if (camTarget != null) camTarget.localRotation = Quaternion.identity;

            Camera mainCam = player.GetComponentInChildren<Camera>();
            if (mainCam != null) mainCam.transform.localRotation = Quaternion.identity;

            yield return null;

            if (cc != null) cc.enabled = true;
            if (fpc != null) fpc.enabled = true;
        }

        // Ganti hari tepat 1 kali
        if (DayManager.Instance != null)
        {
            DayManager.Instance.NextDay();
        }

        // Bersihkan remaining NPCs dan counter status
        if (NPCSpawner.Instance != null) NPCSpawner.Instance.ClearRuntimeNPCs();
        if (CounterManager.Instance != null) CounterManager.Instance.ReleaseCounter();
        if (NPCDatabase.Instance != null) NPCDatabase.Instance.ResetDayNPCs();

        // Reset semua manager
        if (ObjectiveManager.Instance != null) ObjectiveManager.Instance.ResetObjectives();
        if (PerformanceManager.Instance != null) PerformanceManager.Instance.ResetDay();
        if (GameTimeManager.Instance != null) GameTimeManager.Instance.ResetTime();
        if (PassengerScheduleManager.Instance != null) PassengerScheduleManager.Instance.ResetSchedules();

        foreach (ObjectiveTrigger trigger in FindObjectsByType<ObjectiveTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (trigger != null) trigger.ResetTrigger();
        }

        foreach (CleaningStaffInteraction staff in FindObjectsByType<CleaningStaffInteraction>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (staff != null) staff.ResetForNewDay();
        }

        foreach (SelfDialogueTrigger selfDiag in FindObjectsByType<SelfDialogueTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (selfDiag != null) selfDiag.ResetTrigger();
        }

        foreach (ShiftEndController shiftEnd in FindObjectsByType<ShiftEndController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (shiftEnd != null) shiftEnd.ResetController();
        }

        CleaningStaffController cleaningStaff = FindFirstObjectByType<CleaningStaffController>();
        if (cleaningStaff != null) cleaningStaff.ResetToInitialSpawn();

        foreach (CCTVScreamer screamer in FindObjectsByType<CCTVScreamer>(FindObjectsSortMode.None))
            screamer.ResetForNewDay();

        // Auto-save hari baru
        SaveManager.SaveCurrentGame();

        yield return FadeController.Instance.FadeIn();

        HideOtherUI(false);
        isTransitioning = false;
    }

    private void HideOtherUI(bool hide)
    {
        ObjectiveUI objUI = FindFirstObjectByType<ObjectiveUI>(FindObjectsInactive.Include);
        if (objUI != null) objUI.gameObject.SetActive(!hide);

        if (InteractionUI.Instance != null)
        {
            if (hide) InteractionUI.Instance.Hide();
            else InteractionUI.Instance.gameObject.SetActive(true);
        }

        foreach (ObjectiveMarkerHUD marker in FindObjectsByType<ObjectiveMarkerHUD>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (marker != null) marker.gameObject.SetActive(!hide);
        }

        foreach (ObjectiveHighlight highlight in FindObjectsByType<ObjectiveHighlight>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (highlight != null) highlight.gameObject.SetActive(!hide);
        }
    }
}