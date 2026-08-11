using UnityEngine;

/// <summary>
/// Pasang script ini pada benda interaktif (komputer, absen, dsb).
/// Script akan otomatis menyalakan "highlightObject" saat title objective cocok.
/// </summary>
public class ObjectiveHighlight : MonoBehaviour
{
    [Header("Target Objective Name")]
    [Tooltip("Tulis sama persis dengan nama objective, misal: 'Clock In' atau 'Open System'")]
    [SerializeField] private string targetObjectiveTitle;

    [Header("Visual Highlight")]
    [Tooltip("Benda yang akan menyala/muncul. Bisa berupa Light, Particle, atau Icon.")]
    [SerializeField] private GameObject highlightObject;

    private void Start()
    {
        // Langsung sembunyikan fitur menyala di awal
        if (highlightObject != null)
        {
            highlightObject.SetActive(false);
        }

        if (ObjectiveManager.Instance != null)
        {
            // Daftarkan fungsi ke sistem untuk dipanggil setiap objective berubah
            ObjectiveManager.Instance.OnObjectiveChanged += CheckHighlight;
            
            // Cek saat pertama game mulai
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
        if (highlightObject == null) return;

        // Jika nama objective saat ini sama dengan yang dicari, nyalakan!
        if (newObjectiveTitle == targetObjectiveTitle)
        {
            highlightObject.SetActive(true);
        }
        else
        {
            highlightObject.SetActive(false);
        }
    }
}
