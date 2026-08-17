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

    // Tambahan event C# agar benda-benda bisa bereaksi jika jadi target
    public System.Action<string> OnObjectiveChanged;

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
        if (objectives == null || objectives.Count == 0 || currentObjectiveIndex >= objectives.Count)
        {
            Debug.LogWarning("CompleteObjective dipanggil tapi semua objective sudah selesai.");
            return;
        }

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
        if (objectives == null || objectives.Count == 0 || currentObjectiveIndex >= objectives.Count)
            return;

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
        if (objectives == null || objectives.Count == 0 || currentObjectiveIndex >= objectives.Count)
            return;

        int day = dayManager != null ? dayManager.CurrentDayNumber : 1;

        Objective obj = objectives[currentObjectiveIndex];

        string text = obj.title;

        if (obj.targetAmount > 0)
        {
            text += $" ({obj.currentAmount}/{obj.targetAmount})";
        }

        if (objectiveUI != null)
        {
            objectiveUI.UpdateUI(day, text);
        }

        switch (obj.title)
        {
            case "Check Phone":
                if (PhoneManager.Instance != null)
                    PhoneManager.Instance.ReceiveNotification();
                break;
        }

        OnObjectiveChanged?.Invoke(obj.title);
    }

    public void RefreshCurrentObjective()
    {
        ShowCurrentObjective();
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

    public void ResetObjectives()
    {
        currentObjectiveIndex = 0;

        foreach (Objective obj in objectives)
        {
            obj.completed = false;
            obj.currentAmount = 0;
        }

        ShowCurrentObjective();

        Debug.Log("Objectives Reset");
    }
}