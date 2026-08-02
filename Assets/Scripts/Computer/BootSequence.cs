using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BootSequence : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Image progressFill;

    [SerializeField] private GameObject bootScreen;
    [SerializeField] private GameObject mainMenu;

    public IEnumerator PlayBoot()
    {
        bootScreen.SetActive(true);
        mainMenu.SetActive(false);

        progressFill.fillAmount = 0;

        yield return Step("Initializing Terminal...", 0.25f, 0.25f);

        yield return Step("Loading Employee Data...", 0.50f, 0.30f);

        yield return Step("Loading Today's Assignment...", 0.80f, 0.30f);

        yield return Step("Welcome, Joshua", 1f, 0.40f);

        bootScreen.SetActive(false);
        mainMenu.SetActive(true);
    }

    IEnumerator Step(string text, float progress, float delay)
    {
        statusText.text = text;

        progressFill.fillAmount = progress;

        yield return new WaitForSecondsRealtime(delay);
    }
}