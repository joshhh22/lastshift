using UnityEngine;

public class PhoneManager : MonoBehaviour
{
    public static PhoneManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject phoneUI;

    private bool isOpen;
    private bool phoneUnlocked;
    private bool hasNotification;

    public bool IsOpen => isOpen;
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
    }

    public void ReceiveNotification()
    {
        phoneUnlocked = true;
        hasNotification = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPhoneNotification();
        }
    }

    public void OpenPhone()
    {
        if (isOpen)
            return;

        isOpen = true;

        phoneUI.SetActive(true);

        PlayerLockManager.Instance.LockPlayer();

        if (hasNotification)
        {
            hasNotification = false;

            if (ObjectiveManager.Instance.GetCurrentObjective() == "Check Phone")
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

        phoneUI.SetActive(false);

        PlayerLockManager.Instance.UnlockPlayer();
    }
}