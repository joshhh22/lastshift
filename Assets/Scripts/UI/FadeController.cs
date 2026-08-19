using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    public static FadeController Instance { get; private set; }

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1.0f;

    private void Awake()
    {
        Instance = this;

        if (fadeImage == null)
        {
            fadeImage = GetComponentInChildren<Image>(true);
        }

        // Mulai dengan layar hitam untuk fade-in yang halus
        if (fadeImage != null)
        {
            Color c = Color.black;
            c.a = 1f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(true);
        }
    }

    public bool IsFading { get; private set; } = false;

    private void Start()
    {
        StartCoroutine(FadeIn(1.2f));
    }

    public IEnumerator FadeOut(float customDuration = -1f)
    {
        yield return Fade(0f, 1f, customDuration > 0 ? customDuration : fadeDuration);
    }

    public IEnumerator FadeIn(float customDuration = -1f)
    {
        yield return Fade(1f, 0f, customDuration > 0 ? customDuration : fadeDuration);
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        if (fadeImage == null) yield break;

        IsFading = true;
        fadeImage.gameObject.SetActive(true);
        Color color = Color.black;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            color.a = Mathf.Lerp(from, to, time / duration);
            fadeImage.color = color;

            yield return null;
        }

        color.a = to;
        fadeImage.color = color;

        if (to <= 0f)
        {
            fadeImage.gameObject.SetActive(false);
        }

        IsFading = false;
    }
}