using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardSwipeController : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    [Header("References")]
    [SerializeField] private RectTransform card;
    [SerializeField] private RectTransform startPoint;
    [SerializeField] private RectTransform endPoint;
    [SerializeField] private RectTransform swipeZone;
    [SerializeField] private Canvas canvas;
    [SerializeField] private TMP_Text statusText;

    [Header("Swipe Speed (Pixel/Second)")]
    [SerializeField] private float minSpeed = 450f;
    [SerializeField] private float maxSpeed = 900f;
    [SerializeField, Range(0.5f, 1f)] private float requiredSwipePercent = 0.85f;

    private Vector2 startPos;

    private bool dragging;
    private bool canSwipe;
    private bool swiping;

    private float swipeStartTime;
    private float swipeStartX;

    private bool enteredZone;
    private bool passedZone;
    private float requiredDistance;

    void Start()
    {
        startPos = startPoint.anchoredPosition;

        statusText.text = "Take Passenger Card";
    }

    public void EnableSwipe()
    {
        canSwipe = true;

        requiredDistance =
            (endPoint.anchoredPosition.x -
            startPoint.anchoredPosition.x)
            * requiredSwipePercent;

        statusText.text = "READY TO SCAN";
    }

    void Update()
    {
        if (!canSwipe)
            return;

        SwipeUpdate();
    }

    void SwipeUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                card,
                Input.mousePosition,
                null))
            {
                swiping = true;
                swipeStartX = card.anchoredPosition.x;
                swipeStartTime = Time.time;
            }
        }

        if (swiping)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                null,
                out Vector2 pos);

            Vector2 current = card.anchoredPosition;

            float halfCardWidth = card.rect.width * card.lossyScale.x * 0.5f;

            current.x = Mathf.Clamp(
                pos.x,
                startPoint.anchoredPosition.x,
                endPoint.anchoredPosition.x - halfCardWidth);

            card.anchoredPosition = current;
        }

        if (Input.GetMouseButtonUp(0))
        {
            swiping = false;

            float distance =
                card.anchoredPosition.x - swipeStartX;

            float duration =
                Time.time - swipeStartTime;

            float speed =
                distance / duration;

            Debug.Log("Distance : " + distance);
            Debug.Log("Duration : " + duration);
            Debug.Log("Speed : " + speed);

            ValidateSwipe(distance, speed);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!canSwipe)
            return;

        dragging = true;

        enteredZone = false;
        passedZone = false;

        swipeStartTime = Time.time;
        swipeStartX = card.anchoredPosition.x;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out Vector2 localPoint);

        // hanya horizontal
        float x = Mathf.Clamp(localPoint.x, -600f, 600f);

        card.anchoredPosition = new Vector2(
            x,
            startPos.y);

        CheckReader();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!dragging)
            return;

        dragging = false;

        // harus benar-benar melewati reader
        if (!enteredZone || !passedZone)
        {
            Fail("Swipe Again");
            return;
        }

        float distance = Mathf.Abs(card.anchoredPosition.x - swipeStartX);

        float time = Time.time - swipeStartTime;

        float speed = distance / time;

        Debug.Log("Swipe Speed = " + speed);

        if (speed < minSpeed)
        {
            Fail("Too Slow");
            return;
        }

        if (speed > maxSpeed)
        {
            Fail("Too Fast");
            return;
        }

        Success();
    }

    void ValidateSwipe(float distance, float speed)
    {
        if (distance <= 0)
        {
            statusText.text = "WRONG DIRECTION";
            return;
        }

        if (distance < requiredDistance)
        {
            statusText.text = "SWIPE FURTHER";
            return;
        }

        if (speed < minSpeed)
        {
            statusText.text = "TOO SLOW";
            return;
        }

        if (speed > maxSpeed)
        {
            statusText.text = "TOO FAST";
            return;
        }

        statusText.text = "ACCESS GRANTED";

        Debug.Log("SUCCESS");
    }

    void CheckReader()
    {
        float cardX = card.position.x;

        float left =
            swipeZone.position.x -
            swipeZone.rect.width * 0.5f;

        float right =
            swipeZone.position.x +
            swipeZone.rect.width * 0.5f;

        // mulai masuk area reader
        if (cardX <= right)
            enteredZone = true;

        // sudah melewati seluruh reader
        if (cardX <= left)
            passedZone = true;
    }

    void Fail(string message)
    {
        StopAllCoroutines();

        statusText.text = message;

        StartCoroutine(ResetRoutine());
    }

    IEnumerator ResetRoutine()
    {
        yield return new WaitForSeconds(0.8f);

        card.anchoredPosition = startPos;

        enteredZone = false;
        passedZone = false;

        statusText.text = "Swipe Card";
    }

    void Success()
    {
        StopAllCoroutines();

        statusText.text = "ACCESS GRANTED";

        Debug.Log("SUCCESS");

        // nanti disini:
        // currentNPC.Serve();

        // ServePassengerUIController.Instance.Close();
    }
}