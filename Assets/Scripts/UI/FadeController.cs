using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    public static FadeController Instance;

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

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    public IEnumerator FadeOut()
    {
        yield return Fade(0f, 1f);
    }

    public IEnumerator FadeIn()
    {
        yield return Fade(1f, 0f);
    }

    IEnumerator Fade(float from, float to)
    {
        if (fadeImage == null) yield break;

        fadeImage.gameObject.SetActive(true);
        Color color = Color.black;

        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            color.a = Mathf.Lerp(from, to, time / fadeDuration);
            fadeImage.color = color;

            yield return null;
        }

        color.a = to;
        fadeImage.color = color;

        if (to <= 0f)
        {
            fadeImage.gameObject.SetActive(false);
        }
    }
}