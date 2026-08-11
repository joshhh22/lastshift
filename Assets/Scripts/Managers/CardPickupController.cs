using UnityEngine;

public class CardPickupController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform card;
    [SerializeField] private RectTransform cardStartPoint;
    [SerializeField] private RectTransform readerSlot;
    [SerializeField] private Canvas canvas;
    [SerializeField] private CardSwipeController swipeController;
    [SerializeField] private float snapDistance = 80f;
    

    private bool dragging;
    private bool picked;
    private Vector2 initialPos;

    public bool IsPicked => picked;

    private void Start()
    {
        if (card != null)
        {
            initialPos = card.anchoredPosition;
        }
    }

    public void ResetPickup()
    {
        AudioManager.Instance.PlayPutCard();

        picked = false;
        dragging = false;

        card.anchoredPosition = initialPos;
    }

    void Update()
    {
        if (picked)
            return;

        // klik kartu
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouse = Input.mousePosition;

            Vector2 screenPos =
                RectTransformUtility.WorldToScreenPoint(
                    null,
                    card.position);

            float distance = Vector2.Distance(mouse, screenPos);

            if (distance < 180f)
            {
                AudioManager.Instance.PlayPickupCard();
                dragging = true;
            }
        }

        // drag
        if (dragging)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                null,
                out Vector2 pos);

            card.anchoredPosition = pos;
        }

        // lepas
        if (Input.GetMouseButtonUp(0))
        {
            dragging = false;

            float distance =
                Vector2.Distance(
                    card.anchoredPosition,
                    readerSlot.anchoredPosition);

            if (distance < snapDistance)
            {
                picked = true;

                card.anchoredPosition =
                    readerSlot.anchoredPosition;

                swipeController.EnableSwipe();
            }
            else
            {
                card.anchoredPosition = initialPos;
            }
        }
    }
}