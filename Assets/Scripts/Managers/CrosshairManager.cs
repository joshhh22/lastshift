using UnityEngine;
using UnityEngine.UI;

public class CrosshairManager : MonoBehaviour
{
    public static CrosshairManager Instance;

    [SerializeField] private GameObject crosshairRoot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (crosshairRoot == null)
        {
            FindCrosshairInScene();
        }
    }

    public void FindCrosshairInScene()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            foreach (Image img in canvas.GetComponentsInChildren<Image>(true))
            {
                string nameLower = img.gameObject.name.ToLower();
                if (nameLower.Contains("crosshair") || nameLower.Contains("reticle") || nameLower == "pointer" || nameLower == "keybackground")
                {
                    RectTransform rt = img.GetComponent<RectTransform>();
                    if (rt != null && rt.anchorMin == new Vector2(0.5f, 0.5f) && rt.anchorMax == new Vector2(0.5f, 0.5f))
                    {
                        crosshairRoot = img.gameObject;
                        break;
                    }
                }
            }
        }
    }

    public static void ShowCrosshair(bool show)
    {
        if (Instance != null && Instance.crosshairRoot != null)
        {
            Instance.crosshairRoot.SetActive(show);
        }
        else
        {
            // Fallback cari di canvas jika belum terdaftar
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                foreach (Image img in canvas.GetComponentsInChildren<Image>(true))
                {
                    string nameLower = img.gameObject.name.ToLower();
                    if (nameLower.Contains("crosshair") || nameLower.Contains("reticle") || nameLower == "pointer" || nameLower == "keybackground")
                    {
                        RectTransform rt = img.GetComponent<RectTransform>();
                        if (rt != null && rt.anchorMin == new Vector2(0.5f, 0.5f) && rt.anchorMax == new Vector2(0.5f, 0.5f))
                        {
                            img.gameObject.SetActive(show);
                        }
                    }
                }
            }
        }
    }
}
