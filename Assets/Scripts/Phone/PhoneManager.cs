using UnityEngine;

public class PhoneManager : MonoBehaviour
{
    public static PhoneManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject phoneUI;

    private bool isOpen;
    private bool phoneUnlocked;
    private bool hasNotification;
    private float lastCloseTime;
    private int lastCloseFrame;

    public bool IsOpen => isOpen;
    public bool JustClosedThisFrame => Time.frameCount == lastCloseFrame || (Time.unscaledTime - lastCloseTime < 0.15f);
    public bool HasNotification => hasNotification;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        phoneUI.SetActive(false);
    }

    private void Update()
    {
        if (!phoneUnlocked)
            return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isOpen)
                ClosePhone();
            else
                OpenPhone();
        }
        else if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePhone();
        }
    }

    public void ReceiveNotification()
    {
        phoneUnlocked = true;
        hasNotification = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPhoneNotification();
        }

        string sender = "Pesan";
        if (DayManager.Instance != null)
        {
            switch (DayManager.Instance.CurrentDay)
            {
                case GameDay.Day1:
                case GameDay.Day7:
                    sender = "Ibu";
                    break;
                case GameDay.Day2:
                case GameDay.Day3:
                    sender = "Info Pusat";
                    break;
                case GameDay.Day4:
                case GameDay.Day5:
                    sender = "Supervisor";
                    break;
                case GameDay.Day6:
                    sender = "Nomor Tidak Dikenal";
                    break;
            }
        }

        if (PhoneToastNotification.Instance != null)
        {
            PhoneToastNotification.Instance.ShowNotification(sender, "");
        }
    }

    public void OpenPhone()
    {
        if (isOpen)
            return;

        isOpen = true;

        if (PhoneToastNotification.Instance != null)
        {
            PhoneToastNotification.Instance.HideImmediate();
        }

        if (phoneUI != null)
        {
            phoneUI.SetActive(true);
        }

        // Sembunyikan pemikiran objektif aktif agar tidak menutupi tampilan chat HP
        if (PlayerMonologueManager.Instance != null)
        {
            PlayerMonologueManager.Instance.HideActiveThought();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        CrosshairManager.ShowCrosshair(false);
        if (CameraHeadBob.Instance != null)
            CameraHeadBob.Instance.SetBobbingDisabled(true);

        if (PlayerLockManager.Instance != null)
        {
            PlayerLockManager.Instance.LockPlayer();
        }

        if (hasNotification)
        {
            hasNotification = false;

            if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.GetCurrentObjective() == "Check Phone")
            {
                ObjectiveManager.Instance.CompleteObjective();
            }
        }
    }

    public void ClosePhone()
    {
        if (!isOpen)
            return;

        isOpen = false;
        lastCloseFrame = Time.frameCount;
        lastCloseTime = Time.unscaledTime;

        if (phoneUI != null)
        {
            phoneUI.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        CrosshairManager.ShowCrosshair(true);
        if (CameraHeadBob.Instance != null)
            CameraHeadBob.Instance.SetBobbingDisabled(false);

        if (PlayerLockManager.Instance != null)
        {
            PlayerLockManager.Instance.UnlockPlayer();
        }
    }
}