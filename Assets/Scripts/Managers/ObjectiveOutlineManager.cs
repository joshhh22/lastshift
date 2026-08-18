using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mengelola Outline QuickOutline berdasarkan objective aktif saat ini.
/// - "Clock In" / "Clock Out" => Outline pada CardReader (MeshReader)
/// - "Go To Office" / objective komputer => Outline pada Computer
/// Taruh script ini di scene (GameObject manager mana saja).
/// Assign referensi CardReaderOutlineTarget & ComputerOutlineTarget via Inspector.
/// </summary>
public class ObjectiveOutlineManager : MonoBehaviour
{
    public static ObjectiveOutlineManager Instance { get; private set; }

    [Header("Target GameObjects (bisa berupa parent dengan banyak MeshRenderer)")]
    [Tooltip("GameObject CardReader / MeshReader yang akan di-outline saat objective Clock In / Clock Out")]
    [SerializeField] private GameObject cardReaderTarget;

    [Tooltip("GameObject Computer yang akan di-outline saat objective computer aktif")]
    [SerializeField] private GameObject computerTarget;

    [Header("Outline Settings")]
    [SerializeField] private Color outlineColor = new Color(1f, 0.85f, 0f, 1f);
    [SerializeField] private float outlineWidth = 5f;

    // Objective titles yang memicu outline pada CARD READER
    private static readonly HashSet<string> cardReaderObjectives = new HashSet<string>
    {
        "Clock In",
        "Clock Out"
    };

    // Objective titles yang memicu outline pada COMPUTER
    private static readonly HashSet<string> computerObjectives = new HashSet<string>
    {
        "Go To Office",
        "Use Computer",
        "Check Computer",
        "Clock In Computer",
        "Report"
    };

    private Outline cardOutline;
    private Outline computerOutline;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        cardOutline = SetupOutline(cardReaderTarget);
        computerOutline = SetupOutline(computerTarget);

        SetOutlineActive(cardOutline, false);
        SetOutlineActive(computerOutline, false);

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveChanged += OnObjectiveChanged;
            OnObjectiveChanged(ObjectiveManager.Instance.GetCurrentObjective());
        }
    }

    private void OnDestroy()
    {
        if (ObjectiveManager.Instance != null)
            ObjectiveManager.Instance.OnObjectiveChanged -= OnObjectiveChanged;
    }

    private void OnObjectiveChanged(string objectiveTitle)
    {
        string baseTitle = objectiveTitle;
        int parenIdx = baseTitle.IndexOf('(');
        if (parenIdx > 0) baseTitle = baseTitle.Substring(0, parenIdx).Trim();

        bool showCard = cardReaderObjectives.Contains(baseTitle);
        bool showComputer = computerObjectives.Contains(baseTitle);

        SetOutlineActive(cardOutline, showCard);
        SetOutlineActive(computerOutline, showComputer);
    }

    private Outline SetupOutline(GameObject target)
    {
        if (target == null) return null;

        Outline outline = target.GetComponent<Outline>();
        if (outline == null)
            outline = target.AddComponent<Outline>();

        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineColor = outlineColor;
        outline.OutlineWidth = outlineWidth;
        outline.enabled = false;

        return outline;
    }

    private void SetOutlineActive(Outline outline, bool active)
    {
        if (outline == null) return;
        outline.enabled = active;
    }

    public void SetCardReaderTarget(GameObject target)
    {
        cardReaderTarget = target;
        cardOutline = SetupOutline(cardReaderTarget);
        SetOutlineActive(cardOutline, false);
        if (ObjectiveManager.Instance != null)
            OnObjectiveChanged(ObjectiveManager.Instance.GetCurrentObjective());
    }

    public void SetComputerTarget(GameObject target)
    {
        computerTarget = target;
        computerOutline = SetupOutline(computerTarget);
        SetOutlineActive(computerOutline, false);
        if (ObjectiveManager.Instance != null)
            OnObjectiveChanged(ObjectiveManager.Instance.GetCurrentObjective());
    }
}
