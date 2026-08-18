using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mengelola Hover Outline QuickOutline pada semua objek interaktif (Pintu, Komputer, CardReader):
/// - PINTU: Outline menyala HANYA saat crosshair / raycast mengarah ke pintu.
/// - KOMPUTER: Outline menyala HANYA saat crosshair / raycast mengarah ke komputer (baik saat ada objective maupun sesudahnya).
/// - CARD READER: Outline menyala HANYA saat crosshair / raycast mengarah ke CardReader saat Clock In / Clock Out atau sesudahnya.
/// - Saat melihat ke arah lain: Outline otomatis padam (bersih & realistis).
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

    private readonly Dictionary<GameObject, Outline> outlineCache = new Dictionary<GameObject, Outline>();
    private Outline currentActiveOutline = null;
    private Camera playerCam;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        AutoFindTargetsIfMissing();

        // Pre-setup outline pada target utama
        SetupOutline(cardReaderTarget);
        SetupOutline(computerTarget);

        // Pre-setup outline pada semua pintu di scene
        foreach (DoorInteractable door in FindObjectsByType<DoorInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (door != null)
            {
                SetupOutline(door.gameObject);
            }
        }
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

    /// <summary>
    /// Dipanggil saat PlayerInteractor mendeteksi objek interaktif di bawah crosshair.
    /// </summary>
    public void OnHoverInteractable(GameObject hitObj)
    {
        if (hitObj == null)
        {
            DisableCurrentOutline();
            return;
        }

        // Cari root interactable terkait (Pintu, Komputer, CardReader)
        GameObject target = ResolveInteractableTarget(hitObj);
        if (target == null)
        {
            DisableCurrentOutline();
            return;
        }

        Outline outline = SetupOutline(target);
        if (outline != null)
        {
            if (currentActiveOutline != null && currentActiveOutline != outline)
            {
                currentActiveOutline.enabled = false;
            }

            outline.enabled = true;
            currentActiveOutline = outline;
        }
    }

    private void Update()
    {
        // Continuous raycast fallback untuk memastikan outline responsif
        if (playerCam == null)
        {
            playerCam = Camera.main;
            if (playerCam == null) return;
        }

        int layerMask = ~LayerMask.GetMask("Player", "Ignore Raycast");
        Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, layerMask, QueryTriggerInteraction.Collide))
        {
            GameObject target = ResolveInteractableTarget(hit.collider.gameObject);
            if (target != null)
            {
                Outline outline = SetupOutline(target);
                if (outline != null)
                {
                    if (currentActiveOutline != null && currentActiveOutline != outline)
                    {
                        currentActiveOutline.enabled = false;
                    }
                    outline.enabled = true;
                    currentActiveOutline = outline;
                    return;
                }
            }
        }

        DisableCurrentOutline();
    }

    private GameObject ResolveInteractableTarget(GameObject obj)
    {
        if (obj == null) return null;

        // 1. Pintu
        DoorInteractable door = obj.GetComponentInParent<DoorInteractable>();
        if (door != null) return door.gameObject;

        // 2. Card Reader
        CardReaderInteractable card = obj.GetComponentInParent<CardReaderInteractable>();
        if (card != null) return card.gameObject;
        if (cardReaderTarget != null && (obj == cardReaderTarget || obj.transform.IsChildOf(cardReaderTarget.transform)))
            return cardReaderTarget;

        // 3. Komputer / Monitor
        ComputerInteractable comp = obj.GetComponentInParent<ComputerInteractable>();
        if (comp != null) return comp.gameObject;
        if (computerTarget != null && (obj == computerTarget || obj.transform.IsChildOf(computerTarget.transform)))
            return computerTarget;

        // 4. Any generic IInteractable
        IInteractable interactable = obj.GetComponentInParent<IInteractable>();
        if (interactable != null)
        {
            MonoBehaviour mb = interactable as MonoBehaviour;
            if (mb != null) return mb.gameObject;
        }

        return null;
    }

    private void DisableCurrentOutline()
    {
        if (currentActiveOutline != null)
        {
            currentActiveOutline.enabled = false;
            currentActiveOutline = null;
        }
    }

    private Outline SetupOutline(GameObject target)
    {
        if (target == null) return null;

        if (outlineCache.TryGetValue(target, out Outline existing) && existing != null)
        {
            return existing;
        }

        Outline outline = target.GetComponent<Outline>();
        if (outline == null)
            outline = target.AddComponent<Outline>();

        outline.OutlineMode = outlineMode;
        outline.OutlineColor = outlineColor;
        outline.OutlineWidth = outlineWidth;
        outline.enabled = false;

        outlineCache[target] = outline;
        return outline;
    }

    public void SetCardReaderTarget(GameObject target)
    {
        cardReaderTarget = target;
        SetupOutline(cardReaderTarget);
    }

    public void SetComputerTarget(GameObject target)
    {
        computerTarget = target;
        SetupOutline(computerTarget);
    }
}
