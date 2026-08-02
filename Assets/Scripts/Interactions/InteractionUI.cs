using TMPro;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text actionText;

    public void Show(string interactionText)
    {
        gameObject.SetActive(true);
        actionText.text = interactionText;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}