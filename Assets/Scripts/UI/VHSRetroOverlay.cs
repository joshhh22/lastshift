using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Menambahkan efek visual retro ala Fears to Fathom (VHS Overlay, Timestamp, Scanline Flicker)
/// </summary>
public class VHSRetroOverlay : MonoBehaviour
{
    [Header("VHS Display Settings")]
    [SerializeField] private bool showVHSText = true;
    [SerializeField] private TMP_Text vhsTimestampText;
    [SerializeField] private TMP_Text playStatusText;
    [SerializeField] private TMP_Text tapeLabelText;

    [Header("Scanline & Noise Animation")]
    [SerializeField] private CanvasGroup scanlineCanvasGroup;
    [SerializeField] private float flickerSpeed = 10f;
    [SerializeField] private float minAlpha = 0.12f;
    [SerializeField] private float maxAlpha = 0.22f;

    private void Start()
    {
        if (playStatusText != null)
            playStatusText.text = "PLAY  ▶";

        if (tapeLabelText != null)
            tapeLabelText.text = "SP  0:00:00";
    }

    private void Update()
    {
        // 1. Flicker scanlines secara acak agar terasa pita kaset berjalan
        if (scanlineCanvasGroup != null)
        {
            float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
            scanlineCanvasGroup.alpha = Mathf.Lerp(minAlpha, maxAlpha, noise);
        }

        // 2. Update Timestamp retro VHS di sudut layar (Tahun 2142)
        if (showVHSText && vhsTimestampText != null)
        {
            string timeStr = (GameTimeManager.Instance != null) 
                ? GameTimeManager.Instance.GetCurrentTime() 
                : System.DateTime.Now.ToString("HH:mm");

            vhsTimestampText.text = "FEB 14 2142  " + timeStr + " AM";
        }
    }
}
