using TMPro;
using UnityEngine;

/// <summary>
/// Mengatur navigasi halaman Logs di Terminal:
/// Layer 0 → Menu Utama (GUIDE / FAKE REASON / HONEST REASON)
/// Layer 1 → Sub-menu FAKE (INVALID / EXPIRED / WRONG DEST / FAKE REASON)
/// Layer 2 → Detail page tiap kategori
/// </summary>
public class LogsPageController : MonoBehaviour
{
    // ── LAYER 0: Menu Utama ────────────────────────────────────
    [Header("Layer 0 — Main Menu")]
    [SerializeField] private GameObject mainMenuPanel;        // "Menu" di dalam Logs
    [SerializeField] private TMP_Text[] mainMenuItems;        // GUIDE, FAKE REASON, HONEST REASON
    // Elemen-elemen Logs yang disembunyikan saat masuk sub-layer (Background, Header, Hint, Footer, dll.)
    [SerializeField] private GameObject[] logsBaseElements;

    // ── LAYER 1: Sub Menu FAKE ─────────────────────────────────
    [Header("Layer 1 — Fake Sub Menu")]
    [SerializeField] private GameObject fakeMenuPanel;
    [SerializeField] private TMP_Text[] fakeMenuItems;   // INVALID, EXPIRED, WRONG DEST, FAKE REASON

    // ── LAYER 1: Sub Menu HONEST ───────────────────────────────
    [Header("Layer 1 — Honest Sub Menu")]
    [SerializeField] private GameObject honestMenuPanel;
    [SerializeField] private TMP_Text[] honestMenuItems; // INVALID, EXPIRED, WRONG DEST, FAKE REASON

    // ── LAYER 2: FAKE Detail Pages ─────────────────────────────
    [Header("Layer 2 — Fake Detail Pages")]
    [SerializeField] private GameObject invalidPage;
    [SerializeField] private GameObject expiredPage;
    [SerializeField] private GameObject wrongDestPage;
    [SerializeField] private GameObject fakeReasonPage;

    [Header("Detail Page Content Texts — Fake")]
    [SerializeField] private TMP_Text invalidContentText;
    [SerializeField] private TMP_Text expiredContentText;
    [SerializeField] private TMP_Text wrongDestContentText;
    [SerializeField] private TMP_Text fakeReasonContentText;

    // ── LAYER 2: HONEST Detail Pages ───────────────────────────
    [Header("Layer 2 — Honest Detail Pages")]
    [SerializeField] private GameObject honestInvalidPage;
    [SerializeField] private GameObject honestExpiredPage;
    [SerializeField] private GameObject honestWrongDestPage;
    [SerializeField] private TMP_Text honestInvalidText;
    [SerializeField] private TMP_Text honestExpiredText;
    [SerializeField] private TMP_Text honestWrongDestText;

    // ──────────────────────────────────────────────────────────
    // Digunakan oleh TerminalMenu: true = ESC harus dihandle TerminalMenu
    public bool IsAtRoot => layer == 0;

    private int layer = 0;  // 0=main, 1=fake sub, 1b=honest sub, 2=detail
    private bool inHonest = false; // apakah layer 1 adalah honest (bukan fake)
    private int mainIndex = 0;
    private int fakeIndex = 0;
    private int honestIndex = 0;

    // ── Isi teks panduan ──────────────────────────────────────
    private const string InvalidContent =
        "ALASAN BOHONG — TIKET TIDAK VALID\n" +
        "━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n" +
        "Penumpang dengan tiket INVALID sering menggunakan\n" +
        "alasan berikut untuk mengelabui petugas:\n\n" +
        "• \"My ticket disappeared.\"\n" +
        "• \"The machine ate my ticket.\"\n" +
        "• \"I left it at home.\"\n" +
        "• \"My friend has my ticket.\"\n" +
        "• \"The inspector already checked it.\"\n\n" +
        "[!] Tidak ada satu pun alasan di atas yang valid.\n" +
        "    Tolak dengan tegas menggunakan protokol standar.";

    private const string ExpiredContent =
        "ALASAN BOHONG — TIKET KEDALUWARSA\n" +
        "━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n" +
        "Penumpang dengan tiket EXPIRED sering berdalih:\n\n" +
        "• \"The ticket machine expired it.\"\n" +
        "• \"Your scanner is wrong.\"\n" +
        "• \"The staff told me it was still valid.\"\n" +
        "• \"The system made a mistake.\"\n" +
        "• \"It expired by itself.\"\n\n" +
        "[!] Sistem pemindai tidak pernah salah baca.\n" +
        "    Tanggal kedaluwarsa tercetak di tiket. Cek manual.";

    private const string WrongDestContent =
        "ALASAN BOHONG — TUJUAN SALAH\n" +
        "━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n" +
        "Penumpang dengan tujuan SALAH sering mengklaim:\n\n" +
        "• \"The machine changed my destination.\"\n" +
        "• \"Someone else bought this ticket.\"\n" +
        "• \"I never selected this station.\"\n" +
        "• \"The printer printed the wrong destination.\"\n" +
        "• \"The system is broken.\"\n\n" +
        "[!] Mesin tidak mengubah tujuan secara sepihak.\n" +
        "    Penumpang bertanggung jawab atas tiket miliknya.";

    private const string FakeReasonContent =
        "ALASAN BOHONG — TIKET PALSU\n" +
        "━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n" +
        "Penumpang membawa tiket PALSU biasanya berkata:\n\n" +
        "• \"I bought it online.\"\n" +
        "• \"Someone sold me this ticket.\"\n" +
        "• \"This is my real ticket.\"\n" +
        "• \"I don't know why it looks different.\"\n" +
        "• \"The previous station accepted it.\"\n\n" +
        "[!] WASPADA: Beberapa entitas anomali juga membawa\n" +
        "    tiket palsu. Periksa konsistensi data secara fisik.\n" +
        "    Jika ada kejanggalan perilaku, aktifkan protokol 7.";

    // ──────────────────────────────────────────────────────────

    // Honest content (alasan jujur)
    private const string HonestInvalidContent =
        "ALASAN JUJUR — TIKET TIDAK VALID\n" +
        "━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n" +
        "Penumpang dengan tiket INVALID bisa berkata jujur:\n\n" +
        "• \"I forgot to renew my ticket.\"\n" +
        "• \"I lost my wallet.\"\n" +
        "• \"I bought the wrong ticket.\"\n" +
        "• \"My train was delayed.\"\n" +
        "• \"I really need to get home.\"\n\n" +
        "[i] Alasan jujur tidak otomatis berarti tiketnya valid.\n" +
        "    Tetap ikuti protokol — jika tiket tidak valid, TOLAK.";

    private const string HonestExpiredContent =
        "ALASAN JUJUR — TIKET KEDALUWARSA\n" +
        "━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n" +
        "Penumpang dengan tiket EXPIRED bisa berkata:\n\n" +
        "• \"I forgot to renew it this morning.\"\n" +
        "• \"I thought my ticket was still valid.\"\n" +
        "• \"I was rushing to work.\"\n" +
        "• \"I didn't notice it had expired.\"\n" +
        "• \"I've been busy all day.\"\n\n" +
        "[i] Kemanusiaan bukan alasan untuk melanggar protokol.\n" +
        "    Performa dan keamanan adalah prioritas utama.";

    private const string HonestWrongDestContent =
        "ALASAN JUJUR — TUJUAN SALAH\n" +
        "━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n" +
        "Penumpang yang salah tujuan bisa jujur berkata:\n\n" +
        "• \"I accidentally bought the wrong destination.\"\n" +
        "• \"It was my first time using the machine.\"\n" +
        "• \"I clicked the wrong station.\"\n" +
        "• \"I was in a hurry.\"\n" +
        "• \"I read the station name incorrectly.\"\n\n" +
        "[i] Kasihan bukan alasan untuk meloloskan.\n" +
        "    Arahkan ke loket pembelian tiket baru.";

    private const string HonestFakeReasonContent = ""; // Tidak digunakan

    private void OnEnable()
    {
        // Isi konten teks detail FAKE
        if (invalidContentText)    invalidContentText.text    = InvalidContent;
        if (expiredContentText)    expiredContentText.text    = ExpiredContent;
        if (wrongDestContentText)  wrongDestContentText.text  = WrongDestContent;
        if (fakeReasonContentText) fakeReasonContentText.text = FakeReasonContent;

        // Isi konten teks detail HONEST (tanpa FakeReason)
        if (honestInvalidText)   honestInvalidText.text   = HonestInvalidContent;
        if (honestExpiredText)   honestExpiredText.text   = HonestExpiredContent;
        if (honestWrongDestText) honestWrongDestText.text = HonestWrongDestContent;

        GoToLayer0();
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.UpArrow))    Navigate(-1);
        if (Input.GetKeyDown(KeyCode.DownArrow))  Navigate(1);
        if (Input.GetKeyDown(KeyCode.Return))     Select();
        if (Input.GetKeyDown(KeyCode.Escape))     Back();
    }

    // ── Navigasi ──────────────────────────────────────────────
    private void Navigate(int dir)
    {
        if (layer == 0)
        {
            mainIndex = (mainIndex + dir + mainMenuItems.Length) % mainMenuItems.Length;
            RefreshMenuHighlight(mainMenuItems, mainIndex);
        }
        else if (layer == 1 && !inHonest)
        {
            fakeIndex = (fakeIndex + dir + fakeMenuItems.Length) % fakeMenuItems.Length;
            RefreshMenuHighlight(fakeMenuItems, fakeIndex);
        }
        else if (layer == 1 && inHonest)
        {
            honestIndex = (honestIndex + dir + honestMenuItems.Length) % honestMenuItems.Length;
            RefreshMenuHighlight(honestMenuItems, honestIndex);
        }
        // layer 2 tidak navigasi (hanya baca)
    }

    private void Select()
    {
        if (layer == 0)
        {
            switch (mainIndex)
            {
                case 0: // FAKE REASON → layer 1 fake
                    GoToLayer1Fake();
                    break;
                case 1: // HONEST REASON → layer 1 honest
                    GoToLayer1Honest();
                    break;
            }
        }
        else if (layer == 1 && !inHonest)
        {
            GoToLayer2Fake(fakeIndex);
        }
        else if (layer == 1 && inHonest)
        {
            GoToLayer2Honest(honestIndex);
        }
    }

    private void Back()
    {
        if (layer == 2)      GoToLayer1Return();
        else if (layer == 1) GoToLayer0();
        // layer 0 → ESC ditangani TerminalMenu via IsAtRoot
    }

    // ── Layer transitions ─────────────────────────────────────
    private void SetLogsBase(bool active)
    {
        foreach (var go in logsBaseElements)
            if (go != null) go.SetActive(active);
    }

    private void GoToLayer0()
    {
        layer = 0;
        mainIndex = 0;
        inHonest = false;
        SetAllDetailPagesActive(false);
        if (fakeMenuPanel)   fakeMenuPanel.SetActive(false);
        if (honestMenuPanel) honestMenuPanel.SetActive(false);
        if (mainMenuPanel)   mainMenuPanel.SetActive(true);
        SetLogsBase(true);   // tampilkan background/header/hint Logs
        RefreshMenuHighlight(mainMenuItems, mainIndex);
    }

    private void GoToLayer1Fake()
    {
        layer = 1;
        inHonest = false;
        fakeIndex = 0;
        if (mainMenuPanel)   mainMenuPanel.SetActive(false);
        if (honestMenuPanel) honestMenuPanel.SetActive(false);
        SetAllDetailPagesActive(false);
        SetLogsBase(false);  // sembunyikan background/header/hint Logs
        if (fakeMenuPanel) fakeMenuPanel.SetActive(true);
        RefreshMenuHighlight(fakeMenuItems, fakeIndex);
    }

    private void GoToLayer1Honest()
    {
        layer = 1;
        inHonest = true;
        honestIndex = 0;
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (fakeMenuPanel) fakeMenuPanel.SetActive(false);
        SetAllDetailPagesActive(false);
        SetLogsBase(false);  // sembunyikan background/header/hint Logs
        if (honestMenuPanel) honestMenuPanel.SetActive(true);
        RefreshMenuHighlight(honestMenuItems, honestIndex);
    }

    // Kembali ke layer 1 yang sesuai (fake atau honest)
    private void GoToLayer1Return()
    {
        if (inHonest) GoToLayer1Honest();
        else          GoToLayer1Fake();
    }

    private void GoToLayer2Fake(int index)
    {
        layer = 2;
        if (fakeMenuPanel) fakeMenuPanel.SetActive(false);
        SetAllDetailPagesActive(false);

        switch (index)
        {
            case 0: if (invalidPage)    invalidPage.SetActive(true);    break;
            case 1: if (expiredPage)    expiredPage.SetActive(true);    break;
            case 2: if (wrongDestPage)  wrongDestPage.SetActive(true);  break;
            case 3: if (fakeReasonPage) fakeReasonPage.SetActive(true); break;
        }
    }

    private void GoToLayer2Honest(int index)
    {
        layer = 2;
        if (honestMenuPanel) honestMenuPanel.SetActive(false);
        SetAllDetailPagesActive(false);

        switch (index)
        {
            case 0: if (honestInvalidPage)   honestInvalidPage.SetActive(true);   break;
            case 1: if (honestExpiredPage)   honestExpiredPage.SetActive(true);   break;
            case 2: if (honestWrongDestPage) honestWrongDestPage.SetActive(true); break;
        }
    }

    private void SetAllDetailPagesActive(bool active)
    {
        if (invalidPage)         invalidPage.SetActive(active);
        if (expiredPage)         expiredPage.SetActive(active);
        if (wrongDestPage)       wrongDestPage.SetActive(active);
        if (fakeReasonPage)      fakeReasonPage.SetActive(active);
        if (honestInvalidPage)   honestInvalidPage.SetActive(active);
        if (honestExpiredPage)   honestExpiredPage.SetActive(active);
        if (honestWrongDestPage) honestWrongDestPage.SetActive(active);
    }

    private void RefreshMenuHighlight(TMP_Text[] items, int selected)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null) continue;
            string raw = items[i].text.Replace("► ", "");
            items[i].text = (i == selected) ? "► " + raw : raw;
        }
    }
}
