using UnityEngine;
using StarterAssets;

public class PlayerLockManager : MonoBehaviour
{
    public static PlayerLockManager Instance;

    [SerializeField] private StarterAssetsInputs starterInput;
    [SerializeField] private StarterAssets.FirstPersonController controller;

    private void Awake()
    {
        Instance = this;
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
        if (controller != null)
            controller.CanControl = false;

        starterInput.cursorInputForLook = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ExitUIMode()
    {
        if (controller != null)
            controller.CanControl = true;

        starterInput.cursorInputForLook = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}