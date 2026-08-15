using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Dialogue")]
    [SerializeField] private GameObject dialogueUI;

    [Header("Computer")]
    [SerializeField] private GameObject computerUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // =========================
    // Dialogue
    // =========================

    public void ShowDialogue()
    {
        dialogueUI.SetActive(true);
    }

    public void HideDialogue()
    {
        dialogueUI.SetActive(false);
    }

    // =========================
    // Computer
    // =========================

    public void ShowComputer()
    {
        if (ComputerUIController.Instance != null)
        {
            ComputerUIController.Instance.gameObject.SetActive(true);
        }
        else if (computerUI != null)
        {
            computerUI.SetActive(true);
        }
    }

    public void HideComputer()
    {
        if (ComputerUIController.Instance != null)
        {
            ComputerUIController.Instance.gameObject.SetActive(false);
        }
        else if (computerUI != null)
        {
            computerUI.SetActive(false);
        }
    }
}