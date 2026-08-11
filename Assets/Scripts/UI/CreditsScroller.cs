using UnityEngine;

public class CreditsScroller : MonoBehaviour
{
    [Header("Settings")]
    public float scrollSpeed = 50f;
    
    // RectTransform dari Teks yang mau digulung
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        // Reset posisi ke bawah layar setiap kali credits dinyalakan
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -Screen.height);
    }

    private void Update()
    {
        // Gerakkan teks ke atas
        rectTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
        
        // Opsional: Tekan Escape untuk Quit Game saat credits jalan
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }
}
