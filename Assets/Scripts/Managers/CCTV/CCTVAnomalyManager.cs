using System.Collections;
using UnityEngine;

public enum AnomalyMonsterType
{
    Monster1_GateBreach,
    Monster2_StareEntity
}

public class CCTVAnomalyManager : MonoBehaviour
{
    public static CCTVAnomalyManager Instance;

    [Header("Prefabs")]
    [SerializeField] private GameObject monster1Prefab;
    [SerializeField] private GameObject monster2Prefab;

    [Header("Audio")]
    [SerializeField] private AudioClip alarmAudioClip;
    [SerializeField] private AudioClip jumpscareAudioClip;
    [SerializeField] private AudioSource alarmAudioSource;

    [Header("CCTV Cameras")]
    [SerializeField] private Camera[] cctvCameras;

    [Header("Monster 1 Spawn Points (Merangkak di Lorong)")]
    [SerializeField] private Transform[] monster1SpawnPoints; // Index 0 = CAM 1, Index 1 = CAM 2, Index 2 = CAM 3

    [Header("Monster 2 Spawn Points (Dekat Kamera Jumpscare)")]
    [SerializeField] private Transform[] monster2SpawnPoints; // Index 0 = CAM 1, Index 1 = CAM 2, Index 2 = CAM 3

    [Header("Event Timing")]
    [SerializeField] private float initialDelayAfterShiftStart = 30f; // Tepat 30 detik setelah shift start
    [SerializeField] private float anomalyTimeLimit = 35f;
    [SerializeField] private float minInterval = 45f;
    [SerializeField] private float maxInterval = 75f;

    private bool isEventActive = false;
    public bool IsEventActive => isEventActive;

    private int dayEventCount = 0;
    private int maxDayEvents = 2;
    private float nextEventDelay = 30f;
    private float eventIntervalTimer = 0f;
    private bool hasShiftStarted = false;

    private Coroutine activeCountdownRoutine;
    private CCTVMonsterInstance currentSpawnedMonster;
    private AnomalyMonsterType currentType;
    private int currentTargetCamIndex = 0;
    private int currentTrackingDay = -1;

    private void Awake()
    {
        Instance = this;

        if (alarmAudioSource == null) alarmAudioSource = GetComponent<AudioSource>();
        if (alarmAudioSource == null) alarmAudioSource = gameObject.AddComponent<AudioSource>();
        alarmAudioSource.loop = true;
        alarmAudioSource.playOnAwake = false;
    }

    private void Start()
    {
        if (cctvCameras == null || cctvCameras.Length == 0)
        {
            if (CCTVManager.Instance != null)
            {
                var cams = CCTVManager.Instance.GetComponentsInChildren<Camera>(true);
                if (cams.Length > 0) cctvCameras = cams;
            }
        }

        ResetForDay();
    }

    public void ResetForDay()
    {
        isEventActive = false;
        hasShiftStarted = false;
        dayEventCount = 0;
        eventIntervalTimer = 0f;
        nextEventDelay = initialDelayAfterShiftStart;
        lastMonsterType = -1;
        lastCamIndex = -1;

        if (DayManager.Instance != null)
        {
            currentTrackingDay = (int)DayManager.Instance.CurrentDay;
            // Hitung jatah event acak sesuai Day (Hanya Day 3 - 7)
            switch (DayManager.Instance.CurrentDay)
            {
                case GameDay.Day3: maxDayEvents = Random.Range(2, 4); break; // 2 - 3 kali
                case GameDay.Day4: maxDayEvents = Random.Range(3, 5); break; // 3 - 4 kali
                case GameDay.Day5: maxDayEvents = Random.Range(3, 5); break; // 3 - 4 kali
                case GameDay.Day6: maxDayEvents = Random.Range(4, 6); break; // 4 - 5 kali
                case GameDay.Day7: maxDayEvents = Random.Range(4, 6); break; // 4 - 5 kali
                default: maxDayEvents = 0; break; // Day 1 & 2 TIDAK MUNCUL sama sekali
            }
        }

        if (activeCountdownRoutine != null)
        {
            StopCoroutine(activeCountdownRoutine);
            activeCountdownRoutine = null;
        }

        if (alarmAudioSource != null && alarmAudioSource.isPlaying)
        {
            alarmAudioSource.Stop();
        }

        if (currentSpawnedMonster != null)
        {
            Destroy(currentSpawnedMonster.gameObject);
            currentSpawnedMonster = null;
        }

        if (CCTVAnomalyUIController.Instance != null)
        {
            CCTVAnomalyUIController.Instance.HideAllAnomalyUI();
        }

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.RefreshCurrentObjective();
        }
    }

    private void Update()
    {
        // Debug Key [F9] untuk memicu event secara instan saat testing
        if (Input.GetKeyDown(KeyCode.F9))
        {
            Debug.Log("<color=yellow>[CCTVAnomalyManager]</color> Force Trigger Anomaly Event (F9)");
            TriggerAnomalyEvent();
            return;
        }

        // Pastikan reset saat ganti hari
        if (DayManager.Instance != null && (int)DayManager.Instance.CurrentDay != currentTrackingDay)
        {
            ResetForDay();
        }

        // Event hanya aktif di Day 3 - 7
        if (DayManager.Instance == null || (int)DayManager.Instance.CurrentDay < (int)GameDay.Day3)
            return;

        // Event hanya muncul saat objective adalah Continue work / working / shift end
        if (ObjectiveManager.Instance == null) return;
        string curObj = ObjectiveManager.Instance.GetCurrentObjective();
        
        if (!IsTargetObjective(curObj) && !isEventActive)
        {
            hasShiftStarted = false;
            return;
        }

        // Tandai saat shift work baru saja dimulai
        if (!hasShiftStarted)
        {
            hasShiftStarted = true;
            eventIntervalTimer = 0f;
            nextEventDelay = initialDelayAfterShiftStart; // 30 detik pertama
        }

        // Cek kuota event hari ini
        if (dayEventCount >= maxDayEvents)
            return;

        if (!isEventActive)
        {
            eventIntervalTimer += Time.deltaTime;
            if (eventIntervalTimer >= nextEventDelay)
            {
                TriggerAnomalyEvent();
            }
        }
    }

    private bool IsTargetObjective(string objTitle)
    {
        if (string.IsNullOrEmpty(objTitle)) return false;
        string lower = objTitle.ToLower();
        return lower.Contains("continue") || lower.Contains("working") || lower.Contains("shift end");
    }

    private int lastMonsterType = -1;
    private int lastCamIndex = -1;

    public void TriggerAnomalyEvent()
    {
        if (isEventActive) return;

        isEventActive = true;
        dayEventCount++;
        eventIntervalTimer = 0f;
        nextEventDelay = Random.Range(minInterval, maxInterval);

        // Bergantian tipe monster (Jika sebelumnya Monster 2, maka sekarang Monster 1, dan sebaliknya)
        if (lastMonsterType == -1)
        {
            currentType = (Random.value > 0.5f) ? AnomalyMonsterType.Monster1_GateBreach : AnomalyMonsterType.Monster2_StareEntity;
        }
        else if (lastMonsterType == (int)AnomalyMonsterType.Monster1_GateBreach)
        {
            currentType = AnomalyMonsterType.Monster2_StareEntity;
        }
        else
        {
            currentType = AnomalyMonsterType.Monster1_GateBreach;
        }
        lastMonsterType = (int)currentType;

        // Pilih kamera yang berbeda dari event sebelumnya
        int camCount = (cctvCameras != null && cctvCameras.Length > 0) ? cctvCameras.Length : 3;
        int newCam = Random.Range(0, camCount);
        if (newCam == lastCamIndex && camCount > 1)
        {
            newCam = (newCam + 1) % camCount;
        }
        currentTargetCamIndex = newCam;
        lastCamIndex = currentTargetCamIndex;

        Camera targetCam = (cctvCameras != null && currentTargetCamIndex < cctvCameras.Length) ? cctvCameras[currentTargetCamIndex] : null;

        // Spawn Monster tunggal (tidak akan pernah berbarengan)
        if (currentSpawnedMonster != null)
        {
            Destroy(currentSpawnedMonster.gameObject);
            currentSpawnedMonster = null;
        }

        GameObject prefabToSpawn = (currentType == AnomalyMonsterType.Monster1_GateBreach) ? monster1Prefab : monster2Prefab;
        if (prefabToSpawn != null)
        {
            Transform customSpawn = null;
            if (currentType == AnomalyMonsterType.Monster1_GateBreach)
            {
                if (monster1SpawnPoints != null && currentTargetCamIndex < monster1SpawnPoints.Length)
                    customSpawn = monster1SpawnPoints[currentTargetCamIndex];
            }
            else
            {
                if (monster2SpawnPoints != null && currentTargetCamIndex < monster2SpawnPoints.Length)
                    customSpawn = monster2SpawnPoints[currentTargetCamIndex];
            }

            Vector3 spawnPos = customSpawn != null ? customSpawn.position : (targetCam != null ? targetCam.transform.position + targetCam.transform.forward * 3f : Vector3.zero);
            Quaternion spawnRot = customSpawn != null ? customSpawn.rotation : (targetCam != null ? targetCam.transform.rotation : Quaternion.identity);

            GameObject monsterObj = Instantiate(prefabToSpawn, spawnPos, spawnRot);

            // Bersihkan script penumpang/AI bawaan prefab jika ada
            var anom = monsterObj.GetComponent<AnomalyPassenger>();
            if (anom != null) DestroyImmediate(anom);

            var npc = monsterObj.GetComponent<NPCController>();
            if (npc != null) DestroyImmediate(npc);

            var agent = monsterObj.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) DestroyImmediate(agent);

            monsterObj.transform.position = spawnPos;
            monsterObj.transform.rotation = spawnRot;

            currentSpawnedMonster = monsterObj.GetComponent<CCTVMonsterInstance>();
            if (currentSpawnedMonster == null) currentSpawnedMonster = monsterObj.AddComponent<CCTVMonsterInstance>();

            if (currentType == AnomalyMonsterType.Monster1_GateBreach)
            {
                currentSpawnedMonster.SetupMonster1(customSpawn, targetCam);
            }
            else
            {
                currentSpawnedMonster.SetupMonster2(customSpawn, targetCam);
            }
        }

        // Putar suara alarm darurat
        if (alarmAudioClip != null && alarmAudioSource != null)
        {
            alarmAudioSource.clip = alarmAudioClip;
            alarmAudioSource.Play();
        }

        // Buka UI di CCTV
        if (CCTVAnomalyUIController.Instance != null)
        {
            if (currentType == AnomalyMonsterType.Monster1_GateBreach)
                CCTVAnomalyUIController.Instance.ShowMonster1UI(currentTargetCamIndex);
            else
                CCTVAnomalyUIController.Instance.ShowMonster2GlitchPhase(currentTargetCamIndex);
        }

        // Mulai Countdown Timer
        if (activeCountdownRoutine != null) StopCoroutine(activeCountdownRoutine);
        activeCountdownRoutine = StartCoroutine(AnomalyCountdownRoutine());
    }

    private IEnumerator AnomalyCountdownRoutine()
    {
        float timeLeft = anomalyTimeLimit;

        while (timeLeft > 0f && isEventActive)
        {
            timeLeft -= Time.deltaTime;
            int seconds = Mathf.CeilToInt(timeLeft);

            // Update objective HUD display menjadi Check CCTV [35s]
            if (ObjectiveManager.Instance != null)
            {
                ObjectiveUI ui = FindFirstObjectByType<ObjectiveUI>();
                if (ui != null)
                {
                    ui.UpdateObjectiveDisplay($"Check CCTV [{seconds}s]");
                }
            }

            yield return null;
        }

        // Jika waktu habis dan event belum selesai -> GAGAL!
        if (isEventActive)
        {
            OnAnomalyFailedTimeout();
        }
    }

    public void OnPlayerViewedMonster1Camera()
    {
        if (currentSpawnedMonster != null && currentType == AnomalyMonsterType.Monster1_GateBreach)
        {
            currentSpawnedMonster.StartCrawling();
        }
    }

    public void OnQTEGateSuccess()
    {
        if (!isEventActive) return;

        if (currentSpawnedMonster != null)
        {
            currentSpawnedMonster.TriggerMonster1Dying(() => {
                currentSpawnedMonster = null;
            });
        }

        EndAnomalyEvent(true);
    }

    public void OnStare3SecondsCompleted()
    {
        if (!isEventActive) return;

        // Pemicu jumpscare lunge monster 2
        if (currentSpawnedMonster != null)
        {
            currentSpawnedMonster.TriggerMonster2Scream(jumpscareAudioClip, () => {
                // Munculkan tombol Hold Lockdown setelah jumpscare
                if (CCTVAnomalyUIController.Instance != null)
                {
                    CCTVAnomalyUIController.Instance.ShowMonster2HoldLockdownPhase();
                }
            });
        }
    }

    public void OnMonster2LockdownSuccess()
    {
        if (!isEventActive) return;

        if (currentSpawnedMonster != null)
        {
            currentSpawnedMonster.DestroyMonster();
            currentSpawnedMonster = null;
        }

        EndAnomalyEvent(true);
    }

    private void OnAnomalyFailedTimeout()
    {
        if (!isEventActive) return;

        if (currentSpawnedMonster != null)
        {
            Destroy(currentSpawnedMonster.gameObject);
            currentSpawnedMonster = null;
        }

        // Penalti performa dan log kegagalan karena membiarkan anomali menerobos
        if (PerformanceManager.Instance != null)
        {
            PerformanceManager.Instance.RecordCCTVAnomalyFailure($"CAM 0{currentTargetCamIndex + 1}");
        }

        EndAnomalyEvent(false);
    }

    private void EndAnomalyEvent(bool success)
    {
        isEventActive = false;
        eventIntervalTimer = 0f;
        nextEventDelay = Random.Range(minInterval, maxInterval);

        if (activeCountdownRoutine != null)
        {
            StopCoroutine(activeCountdownRoutine);
            activeCountdownRoutine = null;
        }

        if (alarmAudioSource != null && alarmAudioSource.isPlaying)
        {
            alarmAudioSource.Stop();
        }

        if (CCTVAnomalyUIController.Instance != null)
        {
            CCTVAnomalyUIController.Instance.HideAllAnomalyUI();
        }

        // Kembalikan teks objective ke semula sesuai ObjectiveManager
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.RefreshCurrentObjective();
        }

        if (success && PerformanceManager.Instance != null)
        {
            PerformanceManager.Instance.AddPerformance(5);
        }
    }
}
