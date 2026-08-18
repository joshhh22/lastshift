using TMPro;
using UnityEngine;

public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance;

    [Header("Start Time (22:00 - 04:00 AM Shift)")]
    [SerializeField] private int startHour = 22;
    [SerializeField] private int startMinute = 0;
    [SerializeField] private int endHour = 4;
    [SerializeField] private int endMinute = 0;

    [Header("Time Settings")]
    [SerializeField] private float secondsPerGameMinute = 1.2f;

    [Header("Clock UI")]
    [SerializeField] private TMP_Text[] timeTexts;

    public int Hour { get; private set; }
    public int Minute { get; private set; }
    public bool IsShiftEnded { get; private set; }
    public bool IsTimeRunning { get; private set; } = true;
    public string FormattedTime => $"{Hour:00}:{Minute:00}";

    private float timer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ResetTime();
    }

    private void Update()
    {
        // Jangan jalankan waktu jika shift sudah selesai atau waktu dijeda
        if (!IsTimeRunning || IsShiftEnded)
            return;

        // Jika player masih di tahap intro awal (Go to office / Check phone), waktu tidak boleh jalan terlalu cepat / ditahan agar tidak desync jika AFK
        if (ObjectiveManager.Instance != null)
        {
            string curObj = ObjectiveManager.Instance.GetCurrentObjective();
            if (!string.IsNullOrEmpty(curObj))
            {
                string lower = curObj.ToLower();
                // Jika masih berada di persiapan (belum kerja shift), waktu tetap di awal shift
                if (lower.Contains("office") || lower.Contains("phone") || lower.Contains("pc") || lower.Contains("computer"))
                {
                    return;
                }
            }
        }

        timer += Time.deltaTime;

        if (timer >= secondsPerGameMinute)
        {
            timer = 0f;
            AdvanceMinute();
        }
    }

    public void PauseTime()
    {
        IsTimeRunning = false;
    }

    public void ResumeTime()
    {
        IsTimeRunning = true;
    }

    void AdvanceMinute()
    {
        Minute++;

        if (Minute >= 60)
        {
            Minute = 0;
            Hour++;

            if (Hour >= 24)
                Hour = 0;
        }

        UpdateClockUI();

        // Cek apakah waktu sudah mencapai jam selesai shift (04:00 AM)
        if (!IsShiftEnded && Hour == endHour && Minute >= endMinute)
        {
            IsShiftEnded = true;
            IsTimeRunning = false; // Kunci jam di 04:00 agar tidak melaju ke 05:00 atau 06:00
            Hour = endHour;
            Minute = endMinute;
            UpdateClockUI();

            Debug.Log("<color=yellow>[GameTimeManager]</color> SHIFT ENDED AT " + FormattedTime);
        }
    }

    void UpdateClockUI()
    {
        string currentTime = $"{Hour:00}:{Minute:00}";

        if (timeTexts != null)
        {
            foreach (TMP_Text text in timeTexts)
            {
                if (text != null)
                    text.text = currentTime;
            }
        }
    }

    public string GetCurrentTime()
    {
        return $"{Hour:00}:{Minute:00}";
    }

    public bool IsTime(int hour, int minute)
    {
        return Hour == hour && Minute == minute;
    }

    public int TotalMinutes => Hour * 60 + Minute;

    public void ResetTime()
    {
        Hour = startHour;
        Minute = startMinute;

        timer = 0f;
        IsShiftEnded = false;
        IsTimeRunning = true;

        UpdateClockUI();

        Debug.Log("<color=green>[GameTimeManager]</color> Time Reset to " + FormattedTime);
    }
}