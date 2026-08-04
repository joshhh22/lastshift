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

    public bool IsPicked => picked;

    Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (picked)
            return;

        // klik kartu
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Klik Mouse");

            Vector2 mouse = Input.mousePosition;

            Vector2 screenPos =
                RectTransformUtility.WorldToScreenPoint(
                    null,
                    card.position);

            float distance = Vector2.Distance(mouse, screenPos);

            Debug.Log(distance);

            if (distance < 180f)
            {
                Debug.Log("Kartu Dipilih");
                dragging = true;
            }
        }

        // drag
        if (dragging)
        {
            Debug.Log("Dragging");

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
            Debug.Log("Mouse Up");

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

                Debug.Log("CARD SNAPPED");

                swipeController.EnableSwipe();
            }
            else
            {
                Debug.Log("Belum sampai reader");

                card.anchoredPosition =
                    new Vector2(313, 222);
            }
        }
    }
}