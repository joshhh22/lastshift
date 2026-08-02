using TMPro;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance;

    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text promptText;

    private void Awake()
    {
        Instance = this;

        Hide();
    }

    public void Show(string text)
    {
        root.SetActive(true);
        promptText.text = $"[E] {text}";
    }

    public void Hide()
    {
        root.SetActive(false);
    }
}