using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerScheduleManager : MonoBehaviour
{
    public static PassengerScheduleManager Instance;

    [Header("Schedules")]
    [SerializeField] private List<PassengerSchedule> schedules = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (ObjectiveManager.Instance == null)
            return;

        if (GameTimeManager.Instance == null)
            return;

        if (ObjectiveManager.Instance.GetCurrentObjective() != "Continue Working Until Shift Ends")
            return;

        foreach (PassengerSchedule schedule in schedules)
        {
            if (schedule.triggered)
                continue;

            if (GameTimeManager.Instance.Hour == schedule.hour &&
                GameTimeManager.Instance.Minute == schedule.minute)
            {
                schedule.triggered = true;
                StartCoroutine(SpawnRoutine(schedule));
            }
        }
    }

    private IEnumerator SpawnRoutine(PassengerSchedule schedule)
    {
        Debug.Log($"Train Arrived ({schedule.hour:00}:{schedule.minute:00})");

        SpawnManager.Instance.SpawnPassenger(
            schedule.spawnCount,
            schedule.spawnInterval,
            schedule.spawnInterval);

        yield break;
    }
}