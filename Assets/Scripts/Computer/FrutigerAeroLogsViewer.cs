using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FrutigerAeroLogsViewer : MonoBehaviour
{
    [Header("UI Elements")]
    public Button[] categoryButtons;
    public Image[] buttonHighlights;
    public TMP_Text detailText;

    private int selectedIndex = 0;

    private readonly string[] categoryContents = new string[]
    {
        // 0. Tiket Tidak Valid
        "<color=#FF5252><b>[SECURITY ALERT] TIKET TIDAK VALID (INVALID)</b></color>\n" +
        "<color=#00F0FF>----------------------------------------</color>\n\n" +
        "Penumpang dengan tiket <b>INVALID</b> sering menggunakan dalih berikut:\n\n" +
        "  - <i>\"Tiket saya mendadak hilang sendiri dari kantong.\"</i>\n" +
        "  - <i>\"Mesin tiket otomatis menelan tiket saya.\"</i>\n" +
        "  - <i>\"Tiket milik saya tertinggal di atas meja rumah.\"</i>\n" +
        "  - <i>\"Teman saya yang membawa tiket milik saya.\"</i>\n" +
        "  - <i>\"Petugas jaga sebelum Anda sudah memeriksa tiket saya kok.\"</i>\n\n" +
        "<color=#FFD600><b>[!] PROTOKOL KEAMANAN:</b></color>\n" +
        "Tidak ada satu pun alasan di atas yang dapat diterima. <b>TOLAK TIKET!</b>",

        // 1. Tiket Kedaluwarsa
        "<color=#FF9100><b>[SECURITY ALERT] TIKET KEDALUWARSA (EXPIRED)</b></color>\n" +
        "<color=#00F0FF>----------------------------------------</color>\n\n" +
        "Penumpang dengan tiket <b>EXPIRED</b> sering berdalih:\n\n" +
        "  - <i>\"Mesin cetak tiket yang merusak tanggal berlakunya.\"</i>\n" +
        "  - <i>\"Alat pemindaimu yang salah membaca data.\"</i>\n" +
        "  - <i>\"Petugas di loket tadi bilang ini masih bisa dipakai.\"</i>\n" +
        "  - <i>\"Sistem komputer stasiun yang error, bukan tiket saya.\"</i>\n" +
        "  - <i>\"Tiket ini kadaluwarsa sendiri saat di perjalanan.\"</i>\n\n" +
        "<color=#FFD600><b>[!] PROTOKOL KEAMANAN:</b></color>\n" +
        "Sistem pemindai tidak pernah salah baca. Tanggal tercetak di fisik kartu. <b>TOLAK TIKET!</b>",

        // 2. Salah Stasiun Tujuan
        "<color=#FFD600><b>[SECURITY ALERT] SALAH STASIUN TUJUAN (WRONG DESTINATION)</b></color>\n" +
        "<color=#00F0FF>----------------------------------------</color>\n\n" +
        "Penumpang dengan rute <b>SALAH TUJUAN</b> sering mengklaim:\n\n" +
        "  - <i>\"Mesin tiket yang mengubah tujuan tiket saya sendiri.\"</i>\n" +
        "  - <i>\"Orang lain yang membelikan tiket salah ini untuk saya.\"</i>\n" +
        "  - <i>\"Saya tidak pernah memilih nama stasiun ini!\"</i>\n" +
        "  - <i>\"Mesin pencetak yang salah mencetak nama stasiun.\"</i>\n" +
        "  - <i>\"Sistem stasiun milikmu ini yang rusak.\"</i>\n\n" +
        "<color=#FFD600><b>[!] PROTOKOL KEAMANAN:</b></color>\n" +
        "Mesin tidak mengubah rute secara sepihak. Tolak penumpang dengan stasiun tidak sesuai.",

        // 3. Tiket Palsu / Anomali
        "<color=#FF1744><b>[CRITICAL WARNING] TIKET PALSU / ENTITAS ANOMALI</b></color>\n" +
        "<color=#00F0FF>----------------------------------------</color>\n\n" +
        "Penumpang atau <b>ENTITAS ANOMALI</b> dengan tiket palsu berkata:\n\n" +
        "  - <i>\"Saya membelinya secara resmi lewat online.\"</i>\n" +
        "  - <i>\"Ada calo yang menjual tiket ini ke saya di luar stasiun.\"</i>\n" +
        "  - <i>\"Ini tiket asli milik saya kok!\"</i>\n" +
        "  - <i>\"Saya tidak tahu kenapa bentuk dan tampilannya agak beda.\"</i>\n" +
        "  - <i>\"Stasiun sebelumnya meloloskan tiket ini tanpa masalah.\"</i>\n\n" +
        "<color=#FF1744><b>[!] WASPADA TINGGI:</b></color>\n" +
        "Entitas anomali mencoba menyusup ke stasiun inti. Periksa fisik kartu dan segera tolak!"
    };

    private void Awake()
    {
        BindButtons();
    }

    private void OnEnable()
    {
        BindButtons();
        SelectCategory(selectedIndex);
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy) return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            int next = selectedIndex - 1;
            if (next < 0) next = categoryButtons.Length - 1;
            SelectCategory(next);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            int next = selectedIndex + 1;
            if (next >= categoryButtons.Length) next = 0;
            SelectCategory(next);
        }
    }

    public void BindButtons()
    {
        if (categoryButtons == null) return;

        for (int i = 0; i < categoryButtons.Length; i++)
        {
            int idx = i;
            if (categoryButtons[i] != null)
            {
                categoryButtons[i].onClick.RemoveAllListeners();
                categoryButtons[i].onClick.AddListener(() => SelectCategory(idx));
            }
        }
    }

    public void SelectCategory(int index)
    {
        if (categoryButtons == null || index < 0 || index >= categoryContents.Length) return;

        selectedIndex = index;

        if (detailText != null)
        {
            detailText.text = categoryContents[index];
        }

        for (int i = 0; i < categoryButtons.Length; i++)
        {
            if (categoryButtons[i] == null) continue;

            Image img = categoryButtons[i].GetComponent<Image>();
            if (img != null)
            {
                if (i == index)
                {
                    img.color = new Color(0f, 0.45f, 0.75f, 0.95f); // Glowing active Aero blue
                }
                else
                {
                    img.color = new Color(0.08f, 0.2f, 0.32f, 0.6f); // Inactive glass
                }
            }

            if (buttonHighlights != null && i < buttonHighlights.Length && buttonHighlights[i] != null)
            {
                buttonHighlights[i].gameObject.SetActive(i == index);
            }
        }
    }
}
