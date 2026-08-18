using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMonologueManager : MonoBehaviour
{
    public static PlayerMonologueManager Instance;

    [Header("Objective Thought Subtitle UI (No Background Box)")]
    [SerializeField] private GameObject thoughtContainer;
    [SerializeField] private TMP_Text thoughtText;

    [Header("Settings")]
    [SerializeField] private float thoughtDisplayDuration = 4.5f;
    [SerializeField] private float typingSpeed = 0.025f;

    private Coroutine activeThoughtRoutine;
    private int currentTrackedDay = -1;
    private bool isOpeningMonologueActive = false;
    private string lastThoughtObjectiveTitle = "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        CreateThoughtUI();
    }

    private void Start()
    {
        // Listen to Objective updates
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveChanged += OnObjectiveUpdated;
        }

        // Jalankan monolog pembuka harian saat game mulai
        StartCoroutine(DelayedInitialDayMonologue());
    }

    private void OnDestroy()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveChanged -= OnObjectiveUpdated;
        }
    }

    private void Update()
    {
        // Deteksi pergantian hari untuk memicu monolog harian baru
        if (DayManager.Instance != null && (int)DayManager.Instance.CurrentDay != currentTrackedDay)
        {
            currentTrackedDay = (int)DayManager.Instance.CurrentDay;
            TriggerDayOpeningMonologue(DayManager.Instance.CurrentDay);
        }
    }

    private IEnumerator DelayedInitialDayMonologue()
    {
        // Tunggu fade-in selesai sebelum monolog dimulai (~1s fade + 1.5s breathing room)
        yield return new WaitForSeconds(2.5f);

        GameDay day = DayManager.Instance != null ? DayManager.Instance.CurrentDay : GameDay.Day1;
        currentTrackedDay = (int)day;
        TriggerDayOpeningMonologue(day);
    }

    private void CreateThoughtUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // Cari atau buat container tanpa background box
        Transform existing = canvas.transform.Find("PlayerObjectiveThought");
        if (existing != null)
        {
            thoughtContainer = existing.gameObject;
            thoughtText = thoughtContainer.GetComponentInChildren<TMP_Text>(true);
            return;
        }

        GameObject container = new GameObject("PlayerObjectiveThought", typeof(RectTransform));
        container.transform.SetParent(canvas.transform, false);

        RectTransform rt = container.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0, 45);
        rt.sizeDelta = new Vector2(950, 45);

        // Tanpa background box — Tampilan subtitle bersih transparan
        GameObject txtObj = new GameObject("ThoughtText", typeof(RectTransform));
        txtObj.transform.SetParent(container.transform, false);

        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 17;
        tmp.fontStyle = FontStyles.Italic;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.92f, 0.65f, 1f); // Warm amber yellow

#if UNITY_EDITOR
        TMP_FontAsset font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/OpenType (.otf)/HomeVideo-Regular SDF.asset");
        if (font != null) tmp.font = font;
#endif

        thoughtText = tmp;
        thoughtContainer = container;
        thoughtContainer.SetActive(false);
    }

    // =========================================================================
    // 1. MONOLOG PEMBUKA HARIAN (Player Terkunci saat membaca cerita pembuka)
    // =========================================================================

    public void TriggerDayOpeningMonologue(GameDay day)
    {
        List<DialogueLine> lines = GetDayOpeningLines(day);

        if (DialogueManager.Instance != null)
        {
            isOpeningMonologueActive = true;
            // Sembunyikan pemikiran objektif jika sempat muncul
            if (thoughtContainer != null) thoughtContainer.SetActive(false);

            // Player akan terkunci otomatis oleh DialogueManager
            DialogueManager.Instance.StartDialogue(lines, () =>
            {
                isOpeningMonologueActive = false;
                // Setelah monolog pembuka selesai -> Player bisa bergerak, lalu tampilkan pemikiran objektif
                if (ObjectiveManager.Instance != null)
                {
                    OnObjectiveUpdated(ObjectiveManager.Instance.GetCurrentObjective());
                }
            });
        }
    }

    private List<DialogueLine> GetDayOpeningLines(GameDay day)
    {
        List<DialogueLine> lines = new List<DialogueLine>();

        switch (day)
        {
            case GameDay.Day1:
                lines.Add(new DialogueLine { speaker = "Aku", text = "Shift pertamaku sebagai pemeriksa tiket di Stasiun Sektor 04..." });
                lines.Add(new DialogueLine { speaker = "Aku", text = "Udara terowongan bawah tanah ini selalu terasa dingin dan pengap." });
                lines.Add(new DialogueLine { speaker = "Aku", text = "Ibu berpesan agar aku selalu teliti memeriksa tiket dan tidak ceroboh." });
                lines.Add(new DialogueLine { speaker = "Aku", text = "Baiklah, waktunya masuk ke ruang loket dan memulai shift pertamaku." });
                break;

            case GameDay.Day2:
                lines.Add(new DialogueLine { speaker = "Aku", text = "Malam kedua... Kemarin malam shift berjalan lancar, tapi stasiun ini terasa terlalu sunyi." });
                lines.Add(new DialogueLine { speaker = "Aku", text = "Manajemen pusat mengirim peringatan tentang pemalsuan dokumen yang kian marak." });
                lines.Add(new DialogueLine { speaker = "Aku", text = "Aku harus lebih waspada memeriksa kecocokan data setiap penumpang hari ini." });
                break;

            case GameDay.Day3:
                lines.Add(new DialogueLine { speaker = "Aku", text = "Malam ketiga. Ada desas-desus aneh tentang gangguan teknis di jalur rel terowongan." });
                lines.Add(new DialogueLine { speaker = "Aku", text = "Petugas keamanan mengatakan sistem CCTV sempat menangkap gerakan yang ganjil." });
                lines.Add(new DialogueLine { speaker = "Aku", text = "Semoga malam ini tidak terjadi gangguan yang membahayakan stasiun." });
                break;

            case GameDay.Day4:
                lines.Add(new DialogueLine { speaker = "Aku", text = "Malam keempat... Lampu lorong stasiun mulai berkedip-kedip tidak menentu." });
                lines.Add(new DialogueLine { speaker = "Aku", text = "Pesan masuk dari supervisor terasa semakin dingin dan menekan." });
                lines.Add(new DialogueLine { speaker = "Aku", text = "Aku tidak boleh lengah sedikitpun dalam mengambil keputusan." });
                break;

            case GameDay.Day5:
                lines.Add(new DialogueLine { speaker = "Aku", text = "Malam kelima. Stasiun ini terasa semakin mencekam... Ada suara gesekan di lorong gelap." });
                lines.Add(new DialogueLine { speaker = "Aku", text = "Bukan hanya pemalsu tiket biasa yang berkeliaran, ada sesuatu yang mencoba menyusup." });
                lines.Add(new DialogueLine { speaker = "Aku", text = "Aku harus tetap berada di ruang loket dan memastikan semua pintu terkunci." });
                break;

            case GameDay.Day6:
                lines.Add(new DialogueLine { speaker = "Aku", text = "Malam keenam... Tinggal sedikit lagi sebelum minggu kerja pertamaku selesai." });
                lines.Add(new DialogueLine { speaker = "Aku", text = "Ketegangan di stasiun ini semakin memuncak. Penumpang yang datang tampak mencurigakan." });
                lines.Add(new DialogueLine { speaker = "Aku", text = "Fokus... Cocokkan nomor ID, rute stasiun, dan alasan perjalanan mereka." });
                break;

            case GameDay.Day7:
                lines.Add(new DialogueLine { speaker = "Aku", text = "Malam ketujuh... Malam terakhir shift di Stasiun Sektor 04." });
                lines.Add(new DialogueLine { speaker = "Aku", text = "Semua keputusan dan evaluasi kerjaku selama seminggu ini akan dinilai malam ini." });
                lines.Add(new DialogueLine { speaker = "Aku", text = "Apapun yang terjadi di luar sana, aku harus bertahan sampai pukul 04:00 pagi." });
                break;
        }

        return lines;
    }

    // =========================================================================
    // 2. MONOLOG PIKIRAN OBJEKTIF (Player BISA BERGERAK bebas sambil membaca)
    // =========================================================================

    public void OnObjectiveUpdated(string objectiveTitle)
    {
        if (string.IsNullOrEmpty(objectiveTitle)) return;

        // Ambil nama dasar objektif (abaikan angka progress seperti " (1/5)")
        string baseTitle = objectiveTitle;
        int parenIndex = baseTitle.IndexOf('(');
        if (parenIndex > 0)
        {
            baseTitle = baseTitle.Substring(0, parenIndex).Trim();
        }

        // Jangan tampilkan pemikiran yang sama berulang kali (misal saat tiap penumpang selesai 1/5, 2/5, dst)
        if (baseTitle.Equals(lastThoughtObjectiveTitle, System.StringComparison.OrdinalIgnoreCase))
            return;

        // Jangan tampilkan pemikiran objektif jika monolog pembuka atau dialog lain sedang berjalan
        if (isOpeningMonologueActive || (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying()))
            return;

        lastThoughtObjectiveTitle = baseTitle;

        string thought = GetContextualThought(baseTitle);
        if (!string.IsNullOrEmpty(thought))
        {
            ShowThought(thought);
        }
    }

    public void ShowThought(string text, float duration = -1f)
    {
        if (activeThoughtRoutine != null)
        {
            StopCoroutine(activeThoughtRoutine);
        }
        activeThoughtRoutine = StartCoroutine(DisplayThoughtRoutine(text, duration > 0 ? duration : thoughtDisplayDuration));
    }

    private IEnumerator DisplayThoughtRoutine(string text, float duration)
    {
        if (thoughtContainer != null) thoughtContainer.SetActive(true);

        if (thoughtText != null)
        {
            thoughtText.text = "";
            string formatted = $"\"{text}\"";

            for (int i = 0; i <= formatted.Length; i++)
            {
                thoughtText.text = formatted.Substring(0, i);
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        yield return new WaitForSeconds(duration);

        if (thoughtContainer != null)
        {
            thoughtContainer.SetActive(false);
        }

        activeThoughtRoutine = null;
    }

    private string GetContextualThought(string objectiveTitle)
    {
        string lower = objectiveTitle.ToLower();

        if (lower.Contains("office") || lower.Contains("go to"))
        {
            return "Aku harus segera berjalan menuju ke ruang loket stasiun.";
        }
        if (lower.Contains("clock in"))
        {
            return "Aku perlu menempelkan kartu identitasku ke mesin pembaca untuk memulai absensi shift malam ini.";
        }
        if (lower.Contains("pc") || lower.Contains("computer") || lower.Contains("open computer") || lower.Contains("turn on"))
        {
            return "Waktunya menyalakan terminal komputer untuk memeriksa sistem loket dan database stasiun.";
        }
        if (lower.Contains("serve") || lower.Contains("passenger"))
        {
            return "Penumpang mulai berdatangan di depan loket. Aku harus memeriksa tiket dan identitas mereka dengan sangat teliti.";
        }
        if (lower.Contains("cleaning") || lower.Contains("staff") || lower.Contains("talk"))
        {
            return "Petugas kebersihan tampak sedang menyapu di sekitar peron. Sebaiknya aku menyapanya dan menanyakan situasi terkini di stasiun.";
        }
        if (lower.Contains("phone"))
        {
            return "Ponselku bergetar... Sepertinya ada pesan penting yang masuk. Sebaiknya segera kuperiksa [Tekan TAB].";
        }
        if (lower.Contains("cctv") || lower.Contains("camera"))
        {
            return "Sensor keamanan mendeteksi kejanggalan di area stasiun! Aku harus segera memeriksa feed kamera CCTV di monitor!";
        }
        if (lower.Contains("continue") || lower.Contains("working"))
        {
            return "Malam semakin larut... Aku harus tetap fokus melayani penumpang dan selalu waspada terhadap hal-hal yang mencurigakan.";
        }
        if (lower.Contains("clock out") || lower.Contains("end shift") || lower.Contains("finish"))
        {
            return "Semua jadwal penumpang malam ini telah selesai. Waktunya menempelkan kartu pulang di mesin pembaca dan menyelesaikan laporan shift.";
        }

        return "Aku harus menyelesaikan tugasku berikutnya dengan baik dan teliti.";
    }
}
