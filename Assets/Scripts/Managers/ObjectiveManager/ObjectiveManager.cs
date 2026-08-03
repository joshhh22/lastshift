using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    [SerializeField] private ObjectiveUI objectiveUI;
    [SerializeField] private DayManager dayManager;

    [Header("Objectives")]
    [SerializeField] private List<Objective> objectives = new();

    private int currentObjectiveIndex = 0;

    [Header("Objective Events")]
    [SerializeField] private UnityEvent[] objectiveEvents;

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
        ShowCurrentObjective();
    }

    public void CompleteObjective()
    {
        objectives[currentObjectiveIndex].completed = true;

        Debug.Log($"Completed : {objectives[currentObjectiveIndex].title}");

        if (currentObjectiveIndex < objectiveEvents.Length)
        {
            objectiveEvents[currentObjectiveIndex]?.Invoke();
        }

        currentObjectiveIndex++;

        if (currentObjectiveIndex >= objectives.Count)
        {
            Debug.Log("All Objectives Completed");

            AssignmentPage assignmentPage = FindFirstObjectByType<AssignmentPage>();

            if (assignmentPage != null)
            {
                assignmentPage.RefreshObjectives();
            }

            return;
        }

        AssignmentPage assignmentPageAfter = FindFirstObjectByType<AssignmentPage>();

        if (assignmentPageAfter != null)
        {
            assignmentPageAfter.RefreshObjectives();
        }

        ShowCurrentObjective();
    }

    public void AddProgress(int amount = 1)
    {
        Objective obj = objectives[currentObjectiveIndex];

        if (obj.targetAmount <= 0)
            return;

        obj.currentAmount += amount;

        if (obj.currentAmount > obj.targetAmount)
            obj.currentAmount = obj.targetAmount;

        ShowCurrentObjective();

        if (obj.currentAmount >= obj.targetAmount)
        {
            CompleteObjective();
        }
    }

    private void ShowCurrentObjective()
    {
        int day = dayManager.CurrentDayNumber;

        Objective obj = objectives[currentObjectiveIndex];

        string text = obj.title;

        if (obj.targetAmount > 0)
        {
            text += $" ({obj.currentAmount}/{obj.targetAmount})";
        }

        objectiveUI.UpdateUI(day, text);

        switch (obj.title)
        {
            case "Check Phone":
                PhoneManager.Instance.ReceiveNotification();
                break;
        }
    }

    public string GetCurrentObjective()
    {
        if (currentObjectiveIndex >= objectives.Count)
            return "";

        return objectives[currentObjectiveIndex].title;
    }

    public List<Objective> GetObjectives()
    {
        return objectives;
    }

    public int GetCurrentIndex()
    {
        return currentObjectiveIndex;
    }
}