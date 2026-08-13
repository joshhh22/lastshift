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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

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

        HideOtherUI(true);

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
        // Jika sudah Day 7, trigger ending bukan ganti hari
        if (DayManager.Instance.CurrentDay == GameDay.Day7)
        {
            if (EndingManager.Instance != null)
            {
                root.SetActive(false);
                opened = false;
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
        root.SetActive(false);
        opened = false;

        // Teleport player
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (playerSpawnPoint == null)
        {
            GameObject sp = GameObject.Find("PlayerSpawnPoint");
            if (sp != null)
            {
                playerSpawnPoint = sp.transform;
            }
            else
            {
                Debug.LogWarning("PlayerSpawnPoint GameObject not found in scene!");
            }
        }

        if (player != null && playerSpawnPoint != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            var fpc = player.GetComponent<StarterAssets.FirstPersonController>();

            if (cc != null)
                cc.enabled = false;
            
            if (fpc != null)
                fpc.enabled = false;

            // Naikkan posisi sedikit biar tidak clip/nembus collider lantai
            Vector3 spawnPos = playerSpawnPoint.position;
            spawnPos.y += 0.2f;

            player.transform.SetPositionAndRotation(
                spawnPos,
                playerSpawnPoint.rotation);

            // Tunggu 1 frame agar physics engine Unity mensinkronisasi koordinat baru
            yield return null;

            if (cc != null)
                cc.enabled = true;

            if (fpc != null)
                fpc.enabled = true;
        }

        // Ganti hari
        DayManager.Instance.NextDay();

        // Bersihkan remaining NPCs dan counter status
        if (NPCSpawner.Instance != null)
        {
            NPCSpawner.Instance.ClearRuntimeNPCs();
        }
        if (CounterManager.Instance != null)
        {
            CounterManager.Instance.ReleaseCounter();
        }
        if (NPCDatabase.Instance != null)
        {
            NPCDatabase.Instance.ResetDayNPCs();
        }

        // Reset semua manager
        ObjectiveManager.Instance.ResetObjectives();
        PerformanceManager.Instance.ResetDay();
        GameTimeManager.Instance.ResetTime();
        PassengerScheduleManager.Instance.ResetSchedules();

        // Stop Cleaning Staff agar tidak langsung jalan-jalan di hari baru
        // (akan mulai jalan lagi setelah dialogue selesai lewat CleaningStaffInteraction)
        CleaningStaffController cleaningStaff = FindFirstObjectByType<CleaningStaffController>();
        if (cleaningStaff != null)
            cleaningStaff.StopPatrol();

        // Reset jumpscare CCTV agar bisa trigger lagi di hari berikutnya
        foreach (CCTVScreamer screamer in FindObjectsByType<CCTVScreamer>(FindObjectsSortMode.None))
            screamer.ResetForNewDay();

        // Fade masuk lagi
        yield return FadeController.Instance.FadeIn();

        // Kembalikan UI Objective dan Markers untuk hari baru
        HideOtherUI(false);
    }

    private void HideOtherUI(bool hide)
    {
        // 1. Sembunyikan/tampilkan ObjectiveUI
        ObjectiveUI objUI = FindFirstObjectByType<ObjectiveUI>(FindObjectsInactive.Include);
        if (objUI != null)
            objUI.gameObject.SetActive(!hide);

        // 2. Sembunyikan/tampilkan InteractionUI
        if (InteractionUI.Instance != null)
        {
            if (hide)
                InteractionUI.Instance.Hide();
            else
                InteractionUI.Instance.gameObject.SetActive(true);
        }

        // 3. Sembunyikan/tampilkan semua ObjectiveMarkerHUD / marker UI
        foreach (ObjectiveMarkerHUD marker in FindObjectsByType<ObjectiveMarkerHUD>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (marker != null)
                marker.gameObject.SetActive(!hide);
        }

        // 4. Sembunyikan/tampilkan ObjectiveHighlight
        foreach (ObjectiveHighlight highlight in FindObjectsByType<ObjectiveHighlight>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (highlight != null)
                highlight.gameObject.SetActive(!hide);
        }
    }
}