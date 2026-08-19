using System;
using UnityEngine;

[Serializable]
public class SaveData
{
    public bool hasSaveData = false;
    public int currentDay = 1;
    public int currentObjectiveIndex = 0;
    public int objectiveCurrentAmount = 0;
    public int performanceScore = 50;
    public int humanityScore = 50;
    public int correctDecisions = 0;
    public int wrongDecisions = 0;
    public int passengersServed = 0;
    public int gameHour = 22;
    public int gameMinute = 0;
    public string savedObjectiveTitle = "";
    public string saveDateFormatted = "";
}

/// <summary>
/// Sistem Penyimpanan Otomatis (Auto-Save):
/// Menyimpan progress pemain setiap kali objective selesai, pergantian hari, dan saat keluar game.
/// Mendukung fitur 'Continue' dan 'New Game' di Main Menu.
/// </summary>
public class SaveManager : MonoBehaviour
{
    private const string SAVE_KEY = "LAST_SHIFT_SAVE_DATA";

    public static SaveManager Instance { get; private set; }

    /// <summary>
    /// Flag penanda apakah pemain memilih 'Continue' dari Main Menu.
    /// </summary>
    public static bool IsContinuingGame { get; set; } = false;

    private static SaveData cachedSaveData;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInitialize()
    {
        if (Instance == null)
        {
            GameObject obj = new GameObject("SaveManager");
            obj.AddComponent<SaveManager>();
            DontDestroyOnLoad(obj);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (scene.name == "Gameplay" && IsContinuingGame)
        {
            StartCoroutine(ApplySaveDelayed());
        }
    }

    private System.Collections.IEnumerator ApplySaveDelayed()
    {
        // Tunggu 1 frame agar semua Start() manager selesai terinisialisasi
        yield return null;
        ApplySaveToGame();
        IsContinuingGame = false;
    }

    /// <summary>
    /// Mengecek apakah ada file save tersimpan.
    /// </summary>
    public static bool HasSaveData()
    {
        if (cachedSaveData != null && cachedSaveData.hasSaveData)
            return true;

        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    SaveData data = JsonUtility.FromJson<SaveData>(json);
                    if (data != null && data.hasSaveData)
                    {
                        cachedSaveData = data;
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[SaveManager] Gagal membaca save data: " + e.Message);
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Mengambil data save terakhir.
    /// </summary>
    public static SaveData GetSaveData()
    {
        if (cachedSaveData != null)
            return cachedSaveData;

        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    cachedSaveData = JsonUtility.FromJson<SaveData>(json);
                    return cachedSaveData;
                }
                catch
                {
                    // ignored
                }
            }
        }

        return new SaveData();
    }

    /// <summary>
    /// Menyimpan state game saat ini secara otomatis ke PlayerPrefs.
    /// </summary>
    public static void SaveCurrentGame()
    {
        SaveData data = new SaveData
        {
            hasSaveData = true,
            currentDay = DayManager.Instance != null ? DayManager.Instance.CurrentDayNumber : 1,
            currentObjectiveIndex = ObjectiveManager.Instance != null ? ObjectiveManager.Instance.GetCurrentIndex() : 0,
            savedObjectiveTitle = ObjectiveManager.Instance != null ? ObjectiveManager.Instance.GetCurrentObjective() : "",
            performanceScore = PerformanceManager.Instance != null ? PerformanceManager.Instance.Performance : 50,
            humanityScore = PerformanceManager.Instance != null ? PerformanceManager.Instance.Humanity : 50,
            correctDecisions = PerformanceManager.Instance != null ? PerformanceManager.Instance.CorrectDecisions : 0,
            wrongDecisions = PerformanceManager.Instance != null ? PerformanceManager.Instance.WrongDecisions : 0,
            passengersServed = PerformanceManager.Instance != null ? PerformanceManager.Instance.PassengersServed : 0,
            gameHour = GameTimeManager.Instance != null ? GameTimeManager.Instance.Hour : 22,
            gameMinute = GameTimeManager.Instance != null ? GameTimeManager.Instance.Minute : 0,
            saveDateFormatted = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
        };

        if (ObjectiveManager.Instance != null)
        {
            var objectives = ObjectiveManager.Instance.GetObjectives();
            if (objectives != null && data.currentObjectiveIndex < objectives.Count)
            {
                data.objectiveCurrentAmount = objectives[data.currentObjectiveIndex].currentAmount;
            }
        }

        cachedSaveData = data;
        string json = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();

        Debug.Log($"<color=cyan>[SaveManager]</color> Auto-Save Berhasil: Day {data.currentDay} | {data.savedObjectiveTitle} ({data.saveDateFormatted})");
    }

    /// <summary>
    /// Memulihkan state permainan dari save data yang ada ke seluruh manager gameplay.
    /// </summary>
    public static void ApplySaveToGame()
    {
        SaveData data = GetSaveData();
        if (data == null || !data.hasSaveData)
            return;

        Debug.Log($"<color=green>[SaveManager]</color> Memulihkan Save Data: Day {data.currentDay}, Obj {data.currentObjectiveIndex} ({data.savedObjectiveTitle})");

        // 1. Pulihkan Hari
        if (DayManager.Instance != null)
        {
            DayManager.Instance.SetDay((GameDay)Mathf.Clamp(data.currentDay, 1, 7));
        }

        // 2. Pulihkan Skor & Statistik
        if (PerformanceManager.Instance != null)
        {
            PerformanceManager.Instance.LoadSavedScores(
                data.performanceScore,
                data.humanityScore,
                data.correctDecisions,
                data.wrongDecisions,
                data.passengersServed);
        }

        // 3. Pulihkan Waktu Shift
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.LoadSavedTime(data.gameHour, data.gameMinute);
        }

        // 4. Pulihkan Objective
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.LoadSavedObjective(data.currentObjectiveIndex, data.objectiveCurrentAmount);
        }
    }

    /// <summary>
    /// Menghapus seluruh data save (dipanggil saat New Game).
    /// </summary>
    public static void ClearSaveData()
    {
        cachedSaveData = null;
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
        Debug.Log("<color=yellow>[SaveManager]</color> Save Data Direset (New Game)");
    }

    private void OnApplicationQuit()
    {
        // Pastikan save otomatis saat game di-close / Alt+F4
        if (DayManager.Instance != null && ObjectiveManager.Instance != null)
        {
            SaveCurrentGame();
        }
    }
}
