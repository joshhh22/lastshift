using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLockManager : MonoBehaviour
{
    public static PlayerLockManager Instance;

    [Header("References")]
    [SerializeField] private PlayerInput playerInput;

    private void Awake()
    {
        Instance = this;
    }

    public void LockPlayer()
    {
        if (playerInput != null)
            playerInput.enabled = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockPlayer()
    {
        if (playerInput != null)
            playerInput.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}