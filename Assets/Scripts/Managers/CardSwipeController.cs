using System.Collections;
using TMPro;
using UnityEngine;

public class CardSwipeController : MonoBehaviour

{
    [Header("References")]
    [SerializeField] private RectTransform card;
    [SerializeField] private RectTransform startPoint;
    [SerializeField] private RectTransform endPoint;
    [SerializeField] private RectTransform swipeZone;
    [SerializeField] private Canvas canvas;
    [SerializeField] private TMP_Text statusText;

    [SerializeField] private CardPickupController pickupController;

    [Header("Swipe Speed (Pixel/Second)")]
    [SerializeField] private float minSpeed = 450f;
    [SerializeField] private float maxSpeed = 900f;
    [SerializeField, Range(0.5f, 1f)] private float requiredSwipePercent = 0.85f;

    private Vector2 startPos;

    private bool canSwipe;
    private bool swiping;

    private float swipeStartTime;
    private float swipeStartX;
    private float swipeMouseOffsetX;

    private bool enteredZone;
    private bool passedZone;
    private float requiredDistance;

    void Start()
    {
        if (startPoint != null)
            startPos = startPoint.anchoredPosition;

        if (statusText != null)
            statusText.text = "Take Passenger Card";
    }

    public void EnableSwipe()
    {
        canSwipe = true;

        if (startPoint != null && card != null)
        {
            // Pasang kartu persis di mulut scanner (startPoint) tanpa loncat
            card.anchoredPosition = startPoint.anchoredPosition;
        }

        if (endPoint != null && startPoint != null)
        {
            requiredDistance =
                (endPoint.anchoredPosition.x -
                startPoint.anchoredPosition.x)
                * requiredSwipePercent;
        }

        if (statusText != null)
            statusText.text = "READY TO SCAN";
    }

    void Update()
    {
        if (canSwipe)
        {
            SwipeUpdate();
        }
    }

    private Camera GetCamera()
    {
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            return canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
        }
        return null;
    }

    private RectTransform GetParentRect()
    {
        return (card != null && card.parent != null) ? (card.parent as RectTransform) : (canvas.transform as RectTransform);
    }

    void SwipeUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (card != null && RectTransformUtility.RectangleContainsScreenPoint(
                card,
                Input.mousePosition,
                GetCamera()))
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySwipeCard();

                swiping = true;

                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    GetParentRect(),
                    Input.mousePosition,
                    GetCamera(),
                    out Vector2 mousePos);

                // Catat selisih posisi kursor terhadap kartu agar tidak mendadak loncat
                swipeMouseOffsetX = card.anchoredPosition.x - mousePos.x;
                swipeStartX = card.anchoredPosition.x;
                swipeStartTime = Time.time;
            }
        }

        if (swiping)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                GetParentRect(),
                Input.mousePosition,
                GetCamera(),
                out Vector2 pos);

            Vector2 current = card.anchoredPosition;

            float halfCardWidth = card.rect.width * card.lossyScale.x * 0.5f;

            float targetX = pos.x + swipeMouseOffsetX;

            current.x = Mathf.Clamp(
                targetX,
                startPoint.anchoredPosition.x,
                endPoint.anchoredPosition.x - halfCardWidth);

            current.y = startPoint.anchoredPosition.y;

            card.anchoredPosition = current;
        }

        if (Input.GetMouseButtonUp(0) && swiping)
        {
            swiping = false;

            float distance =
                card.anchoredPosition.x - swipeStartX;

            float duration =
                Time.time - swipeStartTime;

            float speed =
                duration > 0f ? (distance / duration) : 0f;

            ValidateSwipe(distance, speed);
        }
    }

    void ValidateSwipe(float distance, float speed)
    {
        if (distance <= 0)
        {
            Fail("WRONG DIRECTION");
            return;
        }

        if (distance < requiredDistance)
        {
            Fail("SWIPE FURTHER");
            return;
        }

        if (speed < minSpeed)
        {
            Fail("TOO SLOW");
            return;
        }

        if (speed > maxSpeed)
        {
            Fail("TOO FAST");
            return;
        }

        Success();
    }

    void Fail(string message)
    {
        StopAllCoroutines();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAccessDenied();
        }

        if (statusText != null)
            statusText.text = message;

        StartCoroutine(ResetRoutine());
    }

    IEnumerator ResetRoutine()
    {
        yield return new WaitForSeconds(0.8f);

        ResetCard();
    }

    void Success()
    {
        StopAllCoroutines();

        statusText.text = "VALIDATING...";

        StartCoroutine(SuccessRoutine());
    }

    public void ResetCard()
    {
        canSwipe = false;
        swiping = false;

        enteredZone = false;
        passedZone = false;

        swipeStartTime = 0;
        swipeStartX = 0;

        card.anchoredPosition = startPos;

        statusText.text = "Take Passenger Card";

        if (pickupController != null)
            pickupController.ResetPickup();
    }

    private IEnumerator SuccessRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        NPCController npc = CounterManager.Instance.GetCurrentNPC();

        if (npc != null)
        {
            TicketStatus result =
                TicketValidator.Validate(
                    npc.passengerData);

            switch (result)
            {
                case TicketStatus.Valid:
                    AudioManager.Instance.PlayAccessGranted();
                    statusText.text = "ACCESS GRANTED";

                    PerformanceManager.Instance.AddPerformance(2);
                    PerformanceManager.Instance.AddCorrectDecision();
                    PerformanceManager.Instance.AddPassengerServed();

                    npc.Serve();

                    ServePassengerUIController.Instance.Close();

                    ResetCard();
                    break;

                case TicketStatus.Invalid:
                    AudioManager.Instance.PlayAccessDenied();
                    statusText.text = "INVALID TICKET";

                    ServePassengerUIController.Instance.OpenDialoguePanel(npc);
                    break;

                case TicketStatus.Expired:
                    AudioManager.Instance.PlayAccessDenied();
                    statusText.text = "TICKET EXPIRED";

                    ServePassengerUIController.Instance.OpenDialoguePanel(npc);
                    break;

                case TicketStatus.Fake:
                    AudioManager.Instance.PlayAccessDenied();
                    statusText.text = "FAKE TICKET";

                    ServePassengerUIController.Instance.OpenDialoguePanel(npc);
                    break;

                case TicketStatus.WrongDestination:
                    AudioManager.Instance.PlayAccessDenied();
                    statusText.text = "WRONG DESTINATION";

                    ServePassengerUIController.Instance.OpenDialoguePanel(npc);
                    break;
            }
        }
    }
}