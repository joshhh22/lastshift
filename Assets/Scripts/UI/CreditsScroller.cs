using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsScroller : MonoBehaviour
{
    [Header("Settings")]
    public float scrollSpeed = 50f;
    public string mainMenuSceneName = "MainMenu";

    [Header("End Behavior")]
    [Tooltip("Jarak pendorong ekstra setelah seluruh teks lewat di atas layar sebelum pindah ke Main Menu")]
    public float endPadding = 150f;

    private RectTransform rectTransform;
    private bool isEnding = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        isEnding = false;

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -Screen.height);
        }

        // Tampilkan dan buka kunci cursor agar player siap di Main Menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (isEnding) return;

        // Gerakkan teks ke atas
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

            // Hitung posisi Y saat seluruh teks sudah naik melewati batas atas layar
            float textHeight = rectTransform.rect.height;
            float endYPosition = Screen.height + textHeight + endPadding;

            if (rectTransform.anchoredPosition.y >= endYPosition)
            {
                ReturnToMainMenu();
            }
        }

        // Skip Credits tekan Escape, Return (Enter), atau Space
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            ReturnToMainMenu();
        }
    }

    public void ReturnToMainMenu()
    {
        if (isEnding) return;
        isEnding = true;

        StartCoroutine(ReturnToMainMenuRoutine());
    }

    private IEnumerator ReturnToMainMenuRoutine()
    {
        if (FadeController.Instance != null)
        {
            yield return FadeController.Instance.FadeOut();
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}

