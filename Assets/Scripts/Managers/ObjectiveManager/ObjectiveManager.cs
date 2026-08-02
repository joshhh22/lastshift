using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    [SerializeField] private ObjectiveUI objectiveUI;
    [SerializeField] private DayManager dayManager;

    [Header("Objectives")]
    [SerializeField] private List<Objective> objectives = new();

    private int currentObjectiveIndex = 0;

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

    private void ShowCurrentObjective()
    {
        int day = dayManager.CurrentDayNumber;

        objectiveUI.UpdateUI(day, objectives[currentObjectiveIndex].title);
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