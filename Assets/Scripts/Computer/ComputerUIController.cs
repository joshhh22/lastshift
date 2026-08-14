using UnityEngine;
using StarterAssets;

public class ComputerUIController : MonoBehaviour
{
    public static ComputerUIController Instance;

    [Header("UI")]
    [SerializeField] private GameObject computerUI;
    [SerializeField] private GameObject crosshair;
    [SerializeField] private BootSequence bootSequence;
    [SerializeField] private TerminalMenu terminalMenu;

    [Header("Player")]
    [SerializeField] private FirstPersonController playerController;
    [SerializeField] private PlayerInteractor playerInteractor;

    private bool isOpen;
    private float lastCloseTime;
    private int lastCloseFrame;

    public bool IsOpen => isOpen;
    public bool JustClosedThisFrame => Time.frameCount == lastCloseFrame || (Time.unscaledTime - lastCloseTime < 0.15f);

    private void Awake()
    {
        Debug.Log("ComputerUIController Awake");

        Instance = this;

        computerUI.SetActive(false);
    }

    public void Open()
    {
        if (isOpen)
            return;

        isOpen = true;

        SetGameplayHUDVisible(false);

        computerUI.SetActive(true);
        StartCoroutine(bootSequence.PlayBoot());

        if (playerController != null)
            playerController.enabled = false;
        if (playerInteractor != null)
            playerInteractor.enabled = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (crosshair != null)
            crosshair.SetActive(false);
    }

    public void Close()
    {
        if (!isOpen)
            return;

        isOpen = false;
        lastCloseTime = Time.unscaledTime;
        lastCloseFrame = Time.frameCount;

        computerUI.SetActive(false);

        SetGameplayHUDVisible(true);

        if (playerController != null)
            playerController.enabled = true;
        if (playerInteractor != null)
            playerInteractor.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (crosshair != null)
            crosshair.SetActive(true);
    }

    private void SetGameplayHUDVisible(bool visible)
    {
        // 1. Objective UI
        ObjectiveUI objUI = FindFirstObjectByType<ObjectiveUI>(FindObjectsInactive.Include);
        if (objUI != null)
            objUI.gameObject.SetActive(visible);

        // 2. Interaction UI
        if (InteractionUI.Instance != null)
        {
            if (!visible)
                InteractionUI.Instance.Hide();
            else
                InteractionUI.Instance.gameObject.SetActive(true);
        }

        // 3. Objective Markers
        foreach (ObjectiveMarkerHUD marker in FindObjectsByType<ObjectiveMarkerHUD>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (marker != null)
                marker.gameObject.SetActive(visible);
        }

        // 4. Objective Highlights
        foreach (ObjectiveHighlight highlight in FindObjectsByType<ObjectiveHighlight>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (highlight != null)
                highlight.gameObject.SetActive(visible);
        }

        // 5. VHS Retro Overlay
        foreach (VHSRetroOverlay vhs in FindObjectsByType<VHSRetroOverlay>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (vhs != null)
                vhs.gameObject.SetActive(visible);
        }
    }
}