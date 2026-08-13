using UnityEngine;

public class CardPickupController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform card;
    [SerializeField] private RectTransform cardStartPoint;
    [SerializeField] private RectTransform readerSlot;
    [SerializeField] private Canvas canvas;
    [SerializeField] private CardSwipeController swipeController;
    [SerializeField] private float snapDistance = 250f; // Toleransi diperbesar agar jauh lebih mudah nge-snap


    private bool dragging;
    private bool picked;
    private Vector2 initialPos;
    private Vector2 dragOffset;

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
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayPutCard();

        picked = false;
        dragging = false;

        if (card != null)
            card.anchoredPosition = initialPos;
    }

    void Update()
    {
        if (picked || card == null)
            return;

        // Klik kartu
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouse = Input.mousePosition;
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, card.position);

            float distance = Vector2.Distance(mouse, screenPos);

            if (distance < 180f)
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayPickupCard();
                
                dragging = true;

                // Hitung offset agar kartu tidak mendadak loncat ke titik kursor
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    Input.mousePosition,
                    null,
                    out Vector2 mouseLocalPos);

                dragOffset = card.anchoredPosition - mouseLocalPos;
            }
        }

        // Drag kartu ikuti kursor dengan offset
        if (dragging)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                null,
                out Vector2 mouseLocalPos);

            card.anchoredPosition = mouseLocalPos + dragOffset;
        }

        // Lepas klik
        if (Input.GetMouseButtonUp(0) && dragging)
        {
            dragging = false;

            Vector2 targetSlotPos = card.anchoredPosition;
            float minDistance = float.MaxValue;

            if (readerSlot != null)
            {
                float dSlot = Vector2.Distance(card.anchoredPosition, readerSlot.anchoredPosition);
                if (dSlot < minDistance)
                {
                    minDistance = dSlot;
                    targetSlotPos = readerSlot.anchoredPosition;
                }
            }

            if (cardStartPoint != null)
            {
                float dStart = Vector2.Distance(card.anchoredPosition, cardStartPoint.anchoredPosition);
                if (dStart < minDistance)
                {
                    minDistance = dStart;
                    targetSlotPos = cardStartPoint.anchoredPosition;
                }
            }

            // Jika kartu dilepas di sekitar scanner (jarak < snapDistance)
            if (minDistance < snapDistance)
            {
                picked = true;

                // Lepas kartu pas di slot scanner
                card.anchoredPosition = targetSlotPos;

                if (swipeController != null)
                    swipeController.EnableSwipe();
            }
            else
            {
                card.anchoredPosition = initialPos;
            }
        }
    }
}