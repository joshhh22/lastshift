using TMPro;
using UnityEngine;

public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance;

    [Header("Start Time")]
    [SerializeField] private int startHour = 22;
    [SerializeField] private int startMinute = 0;

    [Header("Time Settings")]
    [SerializeField] private float secondsPerGameMinute = 1f;

    [Header("Clock UI")]
    [SerializeField] private TMP_Text[] timeTexts;

    public int Hour { get; private set; }
    public int Minute { get; private set; }

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
        Hour = startHour;
        Minute = startMinute;

        UpdateClockUI();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= secondsPerGameMinute)
        {
            timer = 0f;
            AdvanceMinute();
        }
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
    }

    void UpdateClockUI()
    {
        string currentTime = $"{Hour:00}:{Minute:00}";

        foreach (TMP_Text text in timeTexts)
        {
            if (text != null)
                text.text = currentTime;
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
}