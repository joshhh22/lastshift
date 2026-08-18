using UnityEngine;
using StarterAssets;

public class PlayerLockManager : MonoBehaviour
{
    public static PlayerLockManager Instance { get; private set; }

    [SerializeField] private StarterAssetsInputs starterInput;
    [SerializeField] private StarterAssets.FirstPersonController controller;

    public bool IsLocked { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (controller == null)
            controller = FindFirstObjectByType<StarterAssets.FirstPersonController>();

        if (starterInput == null && controller != null)
            starterInput = controller.GetComponent<StarterAssetsInputs>();
    }

    private void Start()
    {
        // Pastikan saat masuk scene game (dari Main Menu), cursor langsung terkunci dan tersembunyi
        ExitUIMode();
    }

    // ==========================================
    // LEGACY
    // ==========================================

    public void LockPlayer()
    {
        EnterUIMode();
    }

    public void UnlockPlayer()
    {
        ExitUIMode();
    }

    // ==========================================
    // UI MODE
    // ==========================================

    public void EnterUIMode()
    {
        IsLocked = true;

        if (controller != null)
            controller.CanControl = false;

        if (starterInput != null)
            starterInput.cursorInputForLook = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ExitUIMode()
    {
        IsLocked = false;

        if (controller != null)
            controller.CanControl = true;

        if (starterInput != null)
            starterInput.cursorInputForLook = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}