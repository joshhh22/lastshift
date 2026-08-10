using UnityEngine;
using TMPro;

public class PhonePage : MonoBehaviour
{
    [Header("UI Text")]
    [SerializeField] private TMP_Text senderText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text replyText; // Teks jawaban player

    private void OnEnable()
    {
        RefreshMessage();
    }

    public void RefreshMessage()
    {
        if (DayManager.Instance == null) return;
        if (senderText == null || messageText == null) return;

        switch (DayManager.Instance.CurrentDay)
        {
            case GameDay.Day1:
                senderText.text = "Ibu";
                messageText.text = "Nak, kamu sudah sampai di stasiun? Cuaca malam ini katanya makin beku, pakai jaket tebalmu ya. Ibu dan adikmu menunggu jatah ransum darimu besok pagi.";
                if (replyText != null) replyText.text = "Sudah bu, aku baru mulai shift. Jangan lupa rapatkan jendela, kabarnya badai salju malam ini memburuk.";
                break;
                
            case GameDay.Day2:
                senderText.text = "Info Pusat (Broadcast)";
                messageText.text = "[PENGUMUMAN PENTING]\nPipa termal penghangat di Stasiun Frostgate meledak. Sektor dikarantina total. Dilarang keras meloloskan penumpang menuju ke sana!";
                if (replyText != null) replyText.text = "Dimengerti. Tidak akan ada penumpang gelap yang lolos ke Frostgate di awasanku.";
                break;

            case GameDay.Day3:
                senderText.text = "Info Pusat (Broadcast)";
                messageText.text = "[DARURAT MEDIS]\nWabah mematikan 'Paru-paru Es' dikonfirmasi merebak di Stasiun Snowtrench.\nTOLAK semua tiket yang BERASAL (Origin) dari Snowtrench demi mencegah penularan di kereta!";
                if (replyText != null) replyText.text = "Mengerti. Asal stasiun = Snowtrench akan masuk daftar hitam (*Blacklist*).";
                break;

            case GameDay.Day4:
                senderText.text = "Supervisor";
                messageText.text = "[PROTOKOL EVAKUASI KRITIS]\nGenerator utama The Core melemah! Kuota pengungsi dipangkas!\n\nMulai hari ini, HANYA WANITA yang diizinkan naik. TOLAK SEMUA PRIA (Male) tanpa terkecuali!";
                if (replyText != null) replyText.text = "Ini gila... Pria dengan tiket Valid juga dibuang ke suhu minus derajat?";
                break;
                
            case GameDay.Day5:
                senderText.text = "Supervisor";
                messageText.text = "PERINGATAN: Kereta kita kelebihan beban dan generator hampir korsleting. Pastikan tidak ada tiket palsu yang masuk. Tolak mereka atau kita semua mati kedinginan di dalam kereta.";
                if (replyText != null) replyText.text = "Baik, pak. Saya akan memastikan mesin kereta tetap stabil dengan mengurangi beban penumpang liar.";
                break;
                
            case GameDay.Day6:
                senderText.text = "Nomor Tidak Dikenal";
                messageText.text = "Kami tahu lokasimu. Malam ini ratusan orang beringas dari permukaan akan mencoba menerobos stasiunmu demi kehangatan. Lindungi dirimu sendiri.";
                if (replyText != null) replyText.text = "Gerbang baja luar sudah terkunci rapat. Kalian tidak akan bisa menembus mesin tiket kami hidup-hidup.";
                break;

            case GameDay.Day7:
                senderText.text = "Ibu";
                messageText.text = "(Pesan Gagal Terkirim) ... \nSinyal Terputus.\n\nSuhu Darurat: -50°C Terdeteksi di Sektor Permukiman.";
                if (replyText != null) replyText.text = "Ibu?! Tolong jawab pesanku! Kumohon katakan kalian masih hidup di sana... Ya Tuhan.";
                break;

            default:
                senderText.text = "Tidak Ada Pesan";
                messageText.text = "Kosong.";
                if (replyText != null) replyText.text = "";
                break;
        }
    }
}
