using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhoneToastNotification : MonoBehaviour
{
    public static PhoneToastNotification Instance;

    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform container;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text hintText;

    private Coroutine showRoutine;

    private void Awake()
    {
        Instance = this;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        if (container != null)
        {
            container.anchoredPosition = new Vector2(0, 100);
        }
    }

    public void ShowNotification(string senderName, string preview)
    {
        if (showRoutine != null) StopCoroutine(showRoutine);
        showRoutine = StartCoroutine(NotificationRoutine(senderName, preview));
    }

    public void HideImmediate()
    {
        if (showRoutine != null) StopCoroutine(showRoutine);

        if (canvasGroup != null) canvasGroup.alpha = 0f;
        if (container != null) container.anchoredPosition = new Vector2(0, 100);
    }

    private IEnumerator NotificationRoutine(string senderName, string preview)
    {
        if (titleText != null)
        {
            titleText.text = $"💬 <b>Pesan Baru dari {senderName}</b>";
        }

        if (hintText != null)
        {
            hintText.text = "Tekan [TAB] untuk membuka ponsel";
        }

        if (canvasGroup == null || container == null) yield break;

        // Slide In Animation
        float duration = 0.3f;
        float elapsed = 0f;
        Vector2 startPos = new Vector2(0, 100);
        Vector2 targetPos = new Vector2(0, -40);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            container.anchoredPosition = Vector2.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, t));
            canvasGroup.alpha = t;
            yield return null;
        }

        container.anchoredPosition = targetPos;
        canvasGroup.alpha = 1f;

        // Tunggu 4 detik
        yield return new WaitForSecondsRealtime(4.0f);

        // Slide Out Animation
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            container.anchoredPosition = Vector2.Lerp(targetPos, startPos, Mathf.SmoothStep(0f, 1f, t));
            canvasGroup.alpha = 1f - t;
            yield return null;
        }

        container.anchoredPosition = startPos;
        canvasGroup.alpha = 0f;
    }
}
