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

    public bool IsOpen => isOpen;

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

        computerUI.SetActive(true);
        StartCoroutine(bootSequence.PlayBoot());

        playerController.enabled = false;
        playerInteractor.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        crosshair.SetActive(false);
    }

    public void Close()
    {
        if (!isOpen)
            return;

        isOpen = false;

        computerUI.SetActive(false);

        playerController.enabled = true;
        playerInteractor.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        crosshair.SetActive(true);
    }

}