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
    private AssignmentPage cachedAssignmentPage;

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
        if (cachedAssignmentPage == null)
            cachedAssignmentPage = FindFirstObjectByType<AssignmentPage>(FindObjectsInactive.Include);

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

        RefreshAssignmentPageUI();

        SaveManager.SaveCurrentGame();

        if (currentObjectiveIndex >= objectives.Count)
        {
            Debug.Log("All Objectives Completed");
            return;
        }

        ShowCurrentObjective();
    }

    private void RefreshAssignmentPageUI()
    {
        if (cachedAssignmentPage == null)
            cachedAssignmentPage = FindFirstObjectByType<AssignmentPage>(FindObjectsInactive.Include);

        if (cachedAssignmentPage != null)
        {
            cachedAssignmentPage.RefreshObjectives();
        }
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

        SaveManager.SaveCurrentGame();

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

        SaveManager.SaveCurrentGame();

        Debug.Log("Objectives Reset");
    }

    /// <summary>
    /// Memulihkan status objective dari Save Data saat pemain memilih Continue.
    /// </summary>
    public void LoadSavedObjective(int savedIndex, int savedAmount)
    {
        if (objectives == null || objectives.Count == 0)
            return;

        currentObjectiveIndex = Mathf.Clamp(savedIndex, 0, objectives.Count);

        for (int i = 0; i < objectives.Count; i++)
        {
            if (i < currentObjectiveIndex)
            {
                objectives[i].completed = true;
                objectives[i].currentAmount = objectives[i].targetAmount;
            }
            else if (i == currentObjectiveIndex)
            {
                objectives[i].completed = false;
                objectives[i].currentAmount = savedAmount;
            }
            else
            {
                objectives[i].completed = false;
                objectives[i].currentAmount = 0;
            }
        }

        RefreshAssignmentPageUI();
        ShowCurrentObjective();

        Debug.Log($"<color=cyan>[ObjectiveManager]</color> Objective dipulihkan ke Index {currentObjectiveIndex}: {GetCurrentObjective()}");
    }
}