using UnityEngine;
using TMPro;

public class ObjectiveMarkerHUD : MonoBehaviour
{
    [Header("Targeting")]
    [Tooltip("Sama persis dengan nama objective, misal 'Clock In'")]
    [SerializeField] private string targetObjectiveTitle;
    
    [Tooltip("Titik tengah benda yang ingin ditandai")]
    [SerializeField] private Transform targetObject;

    public string TargetObjectiveTitle => targetObjectiveTitle;
    public Transform TargetObject => targetObject;

    [Header("UI Element")]
    [Tooltip("Text atau Image UI di Canvas yang jadi bordernya")]
    [SerializeField] private RectTransform markerUI;

    private void Start()
    {
        if (markerUI != null)
            markerUI.gameObject.SetActive(false);

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveChanged += CheckHighlight;
            CheckHighlight(ObjectiveManager.Instance.GetCurrentObjective());
        }
    }

    private void OnDestroy()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveChanged -= CheckHighlight;
        }
    }

    private void CheckHighlight(string newObjectiveTitle)
    {
        if (markerUI == null) return;
        markerUI.gameObject.SetActive(newObjectiveTitle == targetObjectiveTitle);
    }

    private void Update()
    {
        // Jika sedang aktif dan punya target
        if (markerUI != null && markerUI.gameObject.activeSelf && targetObject != null)
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            // Merubah titik dunia 3D ke titik layar 2D
            Vector3 screenPos = cam.WorldToScreenPoint(targetObject.position);

            // Jika z > 0, benda ada di depan mata (bukan di belakang kepala player)
            if (screenPos.z > 0)
            {
                markerUI.position = screenPos; // Tempel UI persis di posisi benda
            }
            else
            {
                // Jika benda di balik badan kita, lempar UI-nya keluar dari layar
                markerUI.position = new Vector3(-9999, -9999, 0); 
            }
        }
    }
}
