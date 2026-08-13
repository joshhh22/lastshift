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


    [Header("Sensitivity Settings")]
    [SerializeField] private float dragSensitivity = 1.0f;

    private bool dragging;
    private bool picked;
    private Vector2 initialPos;
    private Vector2 dragOffset;

    public bool IsPicked => picked;

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
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(GetCamera(), card.position);

            float distance = Vector2.Distance(mouse, screenPos);

            if (distance < 180f)
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayPickupCard();

                dragging = true;

                // Hitung offset presisi dalam ruang koordinat parent kartu
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    GetParentRect(),
                    Input.mousePosition,
                    GetCamera(),
                    out Vector2 mouseLocalPos);

                dragOffset = card.anchoredPosition - mouseLocalPos;
            }
        }

        // Drag kartu ikuti kursor dengan offset & sensitivitas 1:1 presisi
        if (dragging)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                GetParentRect(),
                Input.mousePosition,
                GetCamera(),
                out Vector2 mouseLocalPos);

            Vector2 targetPos = mouseLocalPos + dragOffset;
            if (dragSensitivity != 1.0f)
            {
                targetPos = Vector2.Lerp(card.anchoredPosition, targetPos, dragSensitivity);
            }

            card.anchoredPosition = targetPos;
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