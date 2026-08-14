using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SwipeTutorialUI : MonoBehaviour
{
    [System.Serializable]
    public class TutorialStep
    {
        public string stepTitle;
        [TextArea(2, 4)]
        public string stepDescription;
        public Sprite stepImage;
    }

    [Header("UI Components")]
    public Image stepImageDisplay;
    public TMP_Text stepNumberText;
    public TMP_Text stepTitleText;
    public TMP_Text stepDescriptionText;
    public TMP_Text pageIndicatorText;

    [Header("Navigation Buttons")]
    public Button prevButton;
    public Button nextButton;
    public Button backButton;

    [Header("Tutorial Data (7 Langkah)")]
    public TutorialStep[] steps;

    private int currentStepIndex = 0;

    private void Awake()
    {
        if (prevButton != null) prevButton.onClick.AddListener(ShowPrevStep);
        if (nextButton != null) nextButton.onClick.AddListener(ShowNextStep);
        if (backButton != null) backButton.onClick.AddListener(OnBackClicked);
    }

    private void OnEnable()
    {
        currentStepIndex = 0;
        UpdateUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            ShowPrevStep();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            ShowNextStep();
        }
    }

    public void ShowNextStep()
    {
        if (steps == null || steps.Length == 0) return;
        if (currentStepIndex < steps.Length - 1)
        {
            currentStepIndex++;
            UpdateUI();
        }
    }

    public void ShowPrevStep()
    {
        if (steps == null || steps.Length == 0) return;
        if (currentStepIndex > 0)
        {
            currentStepIndex--;
            UpdateUI();
        }
    }

    public void OnBackClicked()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.OpenGuide();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void UpdateUI()
    {
        if (steps == null || steps.Length == 0) return;

        currentStepIndex = Mathf.Clamp(currentStepIndex, 0, steps.Length - 1);
        TutorialStep current = steps[currentStepIndex];

        if (stepNumberText != null)
            stepNumberText.text = $"LANGKAH {currentStepIndex + 1} / {steps.Length}";

        if (stepTitleText != null)
            stepTitleText.text = current.stepTitle;

        if (stepDescriptionText != null)
            stepDescriptionText.text = current.stepDescription;

        if (stepImageDisplay != null)
        {
            if (current.stepImage != null)
            {
                stepImageDisplay.sprite = current.stepImage;
                stepImageDisplay.gameObject.SetActive(true);
            }
            else
            {
                stepImageDisplay.gameObject.SetActive(false);
            }
        }

        if (pageIndicatorText != null)
        {
            string dots = "";
            for (int i = 0; i < steps.Length; i++)
            {
                dots += (i == currentStepIndex) ? "<color=#00FFCC>●</color> " : "<color=#555555>○</color> ";
            }
            pageIndicatorText.text = dots.TrimEnd();
        }

        if (prevButton != null)
            prevButton.interactable = (currentStepIndex > 0);

        if (nextButton != null)
            nextButton.interactable = (currentStepIndex < steps.Length - 1);
    }
}
