using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mengelola Outline QuickOutline berdasarkan objective aktif dan arah raycast kamera/crosshair:
/// - "Clock In" / "Clock Out" => Outline menyala di CardReader HANYA saat crosshair mengarah ke CardReader.
/// - "Open Computer" / "Check CCTV" => Outline menyala di Komputer HANYA saat crosshair mengarah ke Komputer.
/// - Saat melihat ke tempat lain => Outline otomatis mati (bersih & imersif).
/// </summary>
public class ObjectiveOutlineManager : MonoBehaviour
{
    public static ObjectiveOutlineManager Instance { get; private set; }

    [Header("Target GameObjects")]
    [Tooltip("GameObject CardReader / MeshReader")]
    [SerializeField] private GameObject cardReaderTarget;

    [Tooltip("GameObject Computer / Monitor")]
    [SerializeField] private GameObject computerTarget;

    [Header("Raycast & Aim Settings")]
    [SerializeField] private float maxAimDistance = 4.5f;

    [Header("Outline Settings")]
    [SerializeField] private Color outlineColor = new Color(0.15f, 0.95f, 0.85f, 1f); // Neon Cyan Gold
    [SerializeField] private float outlineWidth = 2.2f;
    [SerializeField] private Outline.Mode outlineMode = Outline.Mode.OutlineVisible;

    // Objective titles yang memicu target CARD READER
    private static readonly HashSet<string> cardReaderObjectives = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
    {
        "Clock In",
        "Clock Out"
    };

    // Objective titles yang memicu target COMPUTER
    private static readonly HashSet<string> computerObjectives = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
    {
        "Open Computer",
        "Use Computer",
        "Check Computer",
        "Check CCTV",
        "CCTV",
        "Check Monitor"
    };

    private Outline cardOutline;
    private Outline computerOutline;

    private bool isCardObjectiveActive = false;
    private bool isComputerObjectiveActive = false;

    private Camera playerCam;

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

    private void Update()
    {
        // Jika tidak ada objective outline yang aktif, pastikan outline mati
        if (!isCardObjectiveActive && !isComputerObjectiveActive)
        {
            SetOutlineActive(cardOutline, false);
            SetOutlineActive(computerOutline, false);
            return;
        }

        if (playerCam == null)
        {
            playerCam = Camera.main;
            if (playerCam == null) return;
        }

        int layerMask = ~LayerMask.GetMask("Player", "Ignore Raycast");
        Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward);

        bool aimingAtCard = false;
        bool aimingAtComp = false;

        if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, layerMask, QueryTriggerInteraction.Collide))
        {
            GameObject hitObj = hit.collider.gameObject;

            // Cek apakah crosshair mengarah ke CardReader saat objektifnya aktif
            if (isCardObjectiveActive && cardReaderTarget != null)
            {
                if (hitObj == cardReaderTarget || 
                    hit.transform.IsChildOf(cardReaderTarget.transform) || 
                    hit.collider.GetComponentInParent<CardReaderInteractable>() != null)
                {
                    aimingAtCard = true;
                }
            }

            // Cek apakah crosshair mengarah ke Computer saat objektifnya aktif
            if (isComputerObjectiveActive && computerTarget != null)
            {
                if (hitObj == computerTarget || 
                    hit.transform.IsChildOf(computerTarget.transform) || 
                    hit.collider.GetComponentInParent<ComputerInteractable>() != null)
                {
                    aimingAtComp = true;
                }
            }
        }

        SetOutlineActive(cardOutline, aimingAtCard);
        SetOutlineActive(computerOutline, aimingAtComp);
    }

    private void AutoFindTargetsIfMissing()
    {
        if (cardReaderTarget == null)
        {
            CardReaderInteractable cardReader = FindFirstObjectByType<CardReaderInteractable>(FindObjectsInactive.Include);
            if (cardReader != null)
            {
                cardReaderTarget = cardReader.gameObject;
            }
            else
            {
                foreach (ObjectiveMarkerHUD hud in FindObjectsByType<ObjectiveMarkerHUD>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (hud != null && (hud.TargetObjectiveTitle == "Clock In" || hud.TargetObjectiveTitle == "Clock Out") && hud.TargetObject != null)
                    {
                        cardReaderTarget = hud.TargetObject.gameObject;
                        break;
                    }
                }
            }
        }

        if (computerTarget == null)
        {
            ComputerInteractable comp = FindFirstObjectByType<ComputerInteractable>(FindObjectsInactive.Include);
            if (comp != null)
            {
                computerTarget = comp.gameObject;
            }
            else
            {
                foreach (ObjectiveMarkerHUD hud in FindObjectsByType<ObjectiveMarkerHUD>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (hud != null && (hud.TargetObjectiveTitle == "Open Computer" || hud.TargetObjectiveTitle == "Check CCTV") && hud.TargetObject != null)
                    {
                        computerTarget = hud.TargetObject.gameObject;
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
            isCardObjectiveActive = false;
            isComputerObjectiveActive = false;
            SetOutlineActive(cardOutline, false);
            SetOutlineActive(computerOutline, false);
            return;
        }

        string baseTitle = objectiveTitle;
        int parenIdx = baseTitle.IndexOf('(');
        if (parenIdx > 0) baseTitle = baseTitle.Substring(0, parenIdx).Trim();

        string lower = baseTitle.ToLower();

        isCardObjectiveActive = cardReaderObjectives.Contains(baseTitle) || lower.Contains("clock in") || lower.Contains("clock out");
        
        // Komputer aktif pada "Open Computer", "Check CCTV", atau monster cctv anomaly
        isComputerObjectiveActive = computerObjectives.Contains(baseTitle) || lower.Contains("computer") || lower.Contains("cctv");

        // Jangan pernah aktifkan outline komputer saat masih "Go To Office"
        if (lower.Contains("office") || lower.Contains("go to"))
        {
            isComputerObjectiveActive = false;
        }

        // Reset outline display sampai di-hover oleh crosshair
        SetOutlineActive(cardOutline, false);
        SetOutlineActive(computerOutline, false);
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
        if (outline.enabled != active)
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
