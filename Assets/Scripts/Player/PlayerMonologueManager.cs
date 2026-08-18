using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMonologueManager : MonoBehaviour
{
    public static PlayerMonologueManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject monologueContainer;
    [SerializeField] private TMP_Text monologueText;
    [SerializeField] private Image monologueBackground;

    [Header("Settings")]
    [SerializeField] private float displayDuration = 5.0f;
    [SerializeField] private float typingSpeed = 0.03f;

    private Coroutine activeMonologueRoutine;
    private TMP_FontAsset regularFont;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (monologueContainer == null)
        {
            CreateMonologueUI();
        }
        else
        {
            monologueContainer.SetActive(false);
        }
    }

    private void Start()
    {
        // Listen to ObjectiveManager changes
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveChanged += OnObjectiveUpdated;
            // Pemicu awal saat start
            OnObjectiveUpdated(ObjectiveManager.Instance.GetCurrentObjective());
        }
    }

    private void OnDestroy()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveChanged -= OnObjectiveUpdated;
        }
    }

    private void CreateMonologueUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject container = new GameObject("PlayerMonologuePanel", typeof(RectTransform));
        container.transform.SetParent(canvas.transform, false);

        RectTransform rt = container.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0, 55);
        rt.sizeDelta = new Vector2(850, 50);

        Image bg = container.AddComponent<Image>();
        bg.color = new Color(0.02f, 0.03f, 0.05f, 0.85f);
        monologueBackground = bg;

        GameObject txtObj = new GameObject("MonologueText", typeof(RectTransform));
        txtObj.transform.SetParent(container.transform, false);

        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.sizeDelta = new Vector2(-30, 0);

        TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 17;
        tmp.fontStyle = FontStyles.Italic;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.92f, 0.65f, 1f); // Warm retro yellow/amber

#if UNITY_EDITOR
        TMP_FontAsset font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/OpenType (.otf)/HomeVideo-Regular SDF.asset");
        if (font != null) tmp.font = font;
#endif

        monologueText = tmp;
        monologueContainer = container;
        monologueContainer.SetActive(false);
    }

    public void OnObjectiveUpdated(string objectiveTitle)
    {
        if (string.IsNullOrEmpty(objectiveTitle)) return;

        string thought = GetContextualThought(objectiveTitle);
        if (!string.IsNullOrEmpty(thought))
        {
            ShowThought(thought);
        }
    }

    public void ShowThought(string text, float customDuration = -1f)
    {
        if (activeMonologueRoutine != null)
        {
            StopCoroutine(activeMonologueRoutine);
        }
        activeMonologueRoutine = StartCoroutine(DisplayThoughtRoutine(text, customDuration > 0 ? customDuration : displayDuration));
    }

    private IEnumerator DisplayThoughtRoutine(string text, float duration)
    {
        if (monologueContainer != null) monologueContainer.SetActive(true);

        if (monologueText != null)
        {
            monologueText.text = "";
            string formatted = $"\"{text}\"";

            for (int i = 0; i <= formatted.Length; i++)
            {
                monologueText.text = formatted.Substring(0, i);
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        yield return new WaitForSeconds(duration);

        if (monologueContainer != null)
        {
            monologueContainer.SetActive(false);
        }

        activeMonologueRoutine = null;
    }

    private string GetContextualThought(string objectiveTitle)
    {
        string lower = objectiveTitle.ToLower();

        if (lower.Contains("office") || lower.Contains("go to"))
        {
            return "Shift malam dimulai... Aku harus segera menuju ke ruang loket untuk memulai tugas.";
        }
        if (lower.Contains("phone"))
        {
            return "Ponselku bergetar... Sebaiknya kuperiksa pesan masuk sekarang [Tekan TAB].";
        }
        if (lower.Contains("pc") || lower.Contains("computer") || lower.Contains("turn on"))
        {
            return "Aku perlu menyalakan komputer dan memeriksa database tiket serta CCTV stasiun.";
        }
        if (lower.Contains("serve") || lower.Contains("passenger"))
        {
            return "Penumpang sudah tiba di loket. Periksa tiket dan dokumen mereka dengan teliti.";
        }
        if (lower.Contains("cctv") || lower.Contains("check cctv"))
        {
            return "Ada sinyal aneh di sistem keamanan! Segera periksa feed CCTV di komputer!";
        }
        if (lower.Contains("continue") || lower.Contains("working"))
        {
            return "Shift masih berjalan. Tetap fokus melayani penumpang dan pantau CCTV bila ada yang ganjil.";
        }
        if (lower.Contains("clock out") || lower.Contains("end shift") || lower.Contains("finish"))
        {
            return "Semua penumpang malam ini sudah terlayani. Waktunya mencatat laporan shift di terminal.";
        }

        return $"Tugasku sekarang: {objectiveTitle}.";
    }
}
