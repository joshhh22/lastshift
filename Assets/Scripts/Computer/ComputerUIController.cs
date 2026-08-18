using UnityEngine;
using StarterAssets;

public class ComputerUIController : MonoBehaviour
{
    private static ComputerUIController _instance;
    public static ComputerUIController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Object.FindFirstObjectByType<ComputerUIController>(FindObjectsInactive.Include);
            }
            return _instance;
        }
        private set => _instance = value;
    }

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
        _instance = this;

        if (computerUI != null && computerUI != gameObject)
            computerUI.SetActive(false);
    }

    public void Open()
    {
        if (isOpen)
            return;

        isOpen = true;

        // Buka kunci cursor terlebih dahulu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SetGameplayHUDVisible(false);

        if (playerController != null)
            playerController.enabled = false;
        if (playerInteractor != null)
            playerInteractor.enabled = false;

        if (crosshair != null)
            crosshair.SetActive(false);
        CrosshairManager.ShowCrosshair(false);

        if (CameraHeadBob.Instance != null)
            CameraHeadBob.Instance.SetBobbingDisabled(true);

        if (computerUI != null)
            computerUI.SetActive(true);

        if (FrutigerAeroComputerUI.Instance != null)
        {
            FrutigerAeroComputerUI.Instance.gameObject.SetActive(true);
            FrutigerAeroComputerUI.Instance.CloseAllWindows();
        }
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
        CrosshairManager.ShowCrosshair(true);

        if (CameraHeadBob.Instance != null)
            CameraHeadBob.Instance.SetBobbingDisabled(false);
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