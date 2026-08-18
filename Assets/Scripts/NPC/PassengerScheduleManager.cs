using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerScheduleManager : MonoBehaviour
{
    public static PassengerScheduleManager Instance { get; private set; }

    [Header("Schedules")]
    [SerializeField] private List<PassengerSchedule> schedules = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (schedules == null || schedules.Count == 0)
            return;

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

        AudioManager.Instance.PlayTrainArrive();

        yield return new WaitForSeconds(3f);

        SpawnManager.Instance.SpawnPassenger(
            schedule.spawnCount,
            schedule.spawnInterval,
            schedule.spawnInterval);

        yield break;
    }

    public void ResetSchedules()
    {
        foreach (PassengerSchedule schedule in schedules)
        {
            schedule.triggered = false;
        }

        Debug.Log("Passenger Schedule Reset");
    }
}