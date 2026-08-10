using UnityEngine;

public class RuleManager : MonoBehaviour
{
    public static RuleManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public TicketStatus CheckSpecialRules(PassengerData data)
    {
        if (DayManager.Instance == null)
            return TicketStatus.Valid;

        // ==========================================
        // ATURAN DAY 2: STASIUN FROSTGATE DITUTUP 
        // ==========================================
        if (DayManager.Instance.CurrentDay >= GameDay.Day2)
        {
            if (data.ticket.destinationStation == "Frostgate") // Dulunya Cawang
            {
                if (data.ticket.status == TicketStatus.Valid)
                {
                    data.isReasonTrue = (Random.value > 0.5f);

                    if (data.isReasonTrue)
                    {
                        string[] honestReasons = {
                            "Frostgate ditutup? Tolong, saya harus pulang, anak saya bisa mati membeku!",
                            "Mesinnya rusak? Tapi ini darurat, keluarga saya menungggu di Frostgate...",
                            "Tolong beri pengecualian, saya tidak punya sisa ransum untuk mencari rute lain."
                        };
                        data.reason = honestReasons[Random.Range(0, honestReasons.Length)];
                    }
                    else
                    {
                        string[] fakeReasons = {
                            "Frostgate ditutup? Ah, itu cuma kebohongan para elite. Buka saja portalnya!",
                            "Saya ini Jenderal dari The Core, biarkan saya lewat ke Frostgate sekarang!",
                            "Mana mungkin ditutup, pemanas di sana adalah yang terbesar di sektor ini."
                        };
                        data.reason = fakeReasons[Random.Range(0, fakeReasons.Length)];
                    }
                }
                
                return TicketStatus.WrongDestination;
            }
        }

        // ==========================================
        // ATURAN DAY 3: SNOWTRENCH DIKARANTINA (ORIGIN STATION)
        // ==========================================
        if (DayManager.Instance.CurrentDay >= GameDay.Day3)
        {
            if (data.ticket.originStation == "Snowtrench")
            {
                if (data.ticket.status == TicketStatus.Valid)
                {
                    data.isReasonTrue = (Random.value > 0.5f);

                    if (data.isReasonTrue)
                    {
                        string[] honestReasons = {
                            "Snowtrench memang dikarantina gara-gara runtuh tertimbun salju, tapi aku tidak bawa wabah paru-paru es kok! Kumohon!",
                            "Tolong, aku ini pengungsi terakhir dari Snowtrench... Semua keluargaku sudah mati membeku di sana.",
                            "Aku tahu asalku tak diterima, tapi aku masih prima bersumpah. Biarkan aku mencari hangat."
                        };
                        data.reason = honestReasons[Random.Range(0, honestReasons.Length)];
                    }
                    else
                    {
                        string[] fakeReasons = {
                            "Snowtrench? Oh... batuk-batuk ini cuma karena debu salju biasa kok. Bukan paru-paru es!",
                            "Karantina itu kan cuma buat warga yang sakit. Aku ini sehat, lho! Percayalah sedikit.",
                            "Tulisannya tertulis Snowtrench? Ah, mesin cetak loketnya sedang *error* pas ku-print."
                        };
                        data.reason = fakeReasons[Random.Range(0, fakeReasons.Length)];
                    }
                }

                // Ditolak karena stasiun asalnya kena blacklist (bermasalah/invalid di mata mesin)
                return TicketStatus.Invalid;
            }
        }

        // ==========================================
        // ATURAN DAY 4: EVAKUASI PRIORITAS (PRIA DITOLAK)
        // ==========================================
        if (DayManager.Instance.CurrentDay >= GameDay.Day4)
        {
            if (data.gender == NPCGender.Male)
            {
                if (data.ticket.status == TicketStatus.Valid)
                {
                    data.isReasonTrue = (Random.value > 0.5f);

                    if (data.isReasonTrue)
                    {
                        string[] honestReasons = {
                            "Hanya wanita yang boleh? Tap... tapi aku harus bertahan hidup! Jangan tinggalkan aku!",
                            "Tolong, istriku sudah masuk duluan kemarin! Aku harus menyusulnya!",
                            "Ini tidak adil! Aku membayar tiket ini dengan seluruh ransum dan tabunganku!"
                        };
                        data.reason = honestReasons[Random.Range(0, honestReasons.Length)];
                    }
                    else
                    {
                        string[] fakeReasons = {
                            "Aturan wanita apanya? Sistemmu pasti rusak, periksa lagi layarnya!",
                            "Aku ini staf elit dari pemerintahan, aturan konyol itu tidak berlaku untukku!",
                            "Perusahaan sudah disogok! Aku bisa bayar masuk, sebutkan hargamu."
                        };
                        data.reason = fakeReasons[Random.Range(0, fakeReasons.Length)];
                    }
                }

                // Ditolak karena Aturan Evakuasi Wanita
                return TicketStatus.Invalid; 
            }
        }

        // Jika tidak melanggar aturan harian, biarkan mesin mengecek keaslian tiket
        return TicketStatus.Valid;
    }
}
