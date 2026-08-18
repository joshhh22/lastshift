using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mengelola Outline QuickOutline berdasarkan objective aktif saat ini:
/// - "Clock In" / "Clock Out" => Outline pada CardReader / MeshReader
/// - "Open Computer" / "Use Computer" => Outline pada Computer / Monitor
/// Otomatis mendeteksi target object jika belum di-assign di Inspector.
/// </summary>
public class ObjectiveOutlineManager : MonoBehaviour
{
    public static ObjectiveOutlineManager Instance { get; private set; }

    [Header("Target GameObjects")]
    [Tooltip("GameObject CardReader / MeshReader")]
    [SerializeField] private GameObject cardReaderTarget;

    [Tooltip("GameObject Computer / Monitor")]
    [SerializeField] private GameObject computerTarget;

    [Header("Outline Settings")]
    [SerializeField] private Color outlineColor = new Color(0.15f, 0.95f, 0.85f, 1f); // Neon Cyan Gold
    [SerializeField] private float outlineWidth = 2.2f;
    [SerializeField] private Outline.Mode outlineMode = Outline.Mode.OutlineVisible;

    // Objective titles yang memicu outline pada CARD READER
    private static readonly HashSet<string> cardReaderObjectives = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
    {
        "Clock In",
        "Clock Out"
    };

    // Objective titles yang memicu outline pada COMPUTER
    // Catatan: "Go To Office" BUKAN objective komputer, jadi tidak akan meng-outline komputer saat awal game
    private static readonly HashSet<string> computerObjectives = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
    {
        "Open Computer",
        "Use Computer",
        "Check Computer"
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
        AutoFindTargetsIfMissing();

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

    private void AutoFindTargetsIfMissing()
    {
        if (cardReaderTarget == null)
        {
            // Cari dari CardReaderInteractable
            CardReaderInteractable cardReader = FindFirstObjectByType<CardReaderInteractable>(FindObjectsInactive.Include);
            if (cardReader != null)
            {
                cardReaderTarget = cardReader.gameObject;
            }
            else
            {
                // Cari dari ObjectiveMarkerHUD
                foreach (ObjectiveMarkerHUD hud in FindObjectsByType<ObjectiveMarkerHUD>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (hud != null && (hud.targetObjectiveTitle == "Clock In" || hud.targetObjectiveTitle == "Clock Out") && hud.targetObject != null)
                    {
                        cardReaderTarget = hud.targetObject;
                        break;
                    }
                }
            }
        }

        if (computerTarget == null)
        {
            // Cari dari ComputerInteractable
            ComputerInteractable comp = FindFirstObjectByType<ComputerInteractable>(FindObjectsInactive.Include);
            if (comp != null)
            {
                computerTarget = comp.gameObject;
            }
            else
            {
                // Cari dari ObjectiveMarkerHUD
                foreach (ObjectiveMarkerHUD hud in FindObjectsByType<ObjectiveMarkerHUD>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (hud != null && hud.targetObjectiveTitle == "Open Computer" && hud.targetObject != null)
                    {
                        computerTarget = hud.targetObject;
                        break;
                    }
                }
            }
        }
    }

    private void OnObjectiveChanged(string objectiveTitle)
    {
        if (string.IsNullOrEmpty(objectiveTitle))
        {
            SetOutlineActive(cardOutline, false);
            SetOutlineActive(computerOutline, false);
            return;
        }

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

        outline.OutlineMode = outlineMode;
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
