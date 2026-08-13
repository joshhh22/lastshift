using UnityEngine;

public static class TicketGenerator
{
    static readonly string[] stations =
    {
        "Sector 5",
        "Ironhold",
        "Frostgate", // Pengganti Cawang
        "Snowtrench",
        "New Avalon",
        "The Core"
    };

    static readonly string[] lines =
    {
        "Thermal Line"
    };

    static readonly string[] invalidHonest =
    {
        "Saya lupa memperbarui masa berlaku tiket saya.",
        "Dompet saya terjatuh dan hilang di jalan.",
        "Saya tidak sengaja salah membeli jenis tiket.",
        "Kereta sambungan saya yang tadi terlambat parah.",
        "Saya benar-benar harus segera pulang malam ini."
    };

    static readonly string[] invalidFake =
    {
        "Tiket saya mendadak hilang sendiri dari kantong.",
        "Mesin tiket otomatis menelan tiket saya.",
        "Tiket milik saya tertinggal di atas meja rumah.",
        "Teman saya yang membawa tiket milik saya.",
        "Petugas jaga sebelum Anda sudah memeriksa tiket saya kok."
    };

    static readonly string[] expiredHonest =
    {
        "Saya lupa memperpanjangnya tadi pagi.",
        "Saya kira tiket saya masih berlaku sampai besok.",
        "Saya sedang terburu-buru mengejar jam kerja tadi.",
        "Saya tidak sadar kalau tiket ini sudah kadaluwarsa.",
        "Saya sibuk seharian sampai tidak sempat mengeceknya."
    };

    static readonly string[] expiredFake =
    {
        "Mesin cetak tiket yang merusak tanggal berlakunya.",
        "Alat pemindaimu yang salah membaca data.",
        "Petugas di loket tadi bilang ini masih bisa dipakai.",
        "Sistem komputer stasiun yang error, bukan tiket saya.",
        "Tiket ini kadaluwarsa sendiri saat di perjalanan."
    };

    static readonly string[] wrongDestinationHonest =
    {
        "Saya tidak sengaja memilih stasiun yang salah.",
        "Ini pertama kalinya saya menggunakan mesin tiket otomatis.",
        "Saya salah menekan nama stasiun di mesin.",
        "Saya sedang terburu-buru saat membeli tiket ini.",
        "Saya salah membaca papan nama stasiun."
    };

    static readonly string[] wrongDestinationFake =
    {
        "Mesin tiket yang mengubah tujuan tiket saya sendiri.",
        "Orang lain yang membelikan tiket salah ini untuk saya.",
        "Saya tidak pernah memilih nama stasiun ini!",
        "Mesin pencetak yang salah mencetak nama stasiun.",
        "Sistem stasiun milikmu ini yang rusak."
    };

    static readonly string[] fakeReasons =
    {
        "Saya membelinya secara resmi lewat online.",
        "Ada calo yang menjual tiket ini ke saya di luar stasiun.",
        "Ini tiket asli milik saya kok!",
        "Saya tidak tahu kenapa bentuk dan tampilannya agak beda.",
        "Stasiun sebelumnya meloloskan tiket ini tanpa masalah."
    };

    static readonly string[] maleNames =
    {
        "Bagas", "Dimas", "Raka", "Yusuf", "Adrian", 
        "Bima", "Satria", "Rendi", "Tirta", "Danish",
        "Leon", "Arthur", "Danu", "Surya", "Gilang",
        "Joko", "Toni", "Vance", "Kael", "Rudi"
    };

    static readonly string[] femaleNames =
    {
        "Maya", "Widya", "Kartika", "Sari", "Risa", 
        "Nadia", "Alya", "Laras", "Diana", "Ratna",
        "Kira", "Elena", "Nisa", "Siska", "Tania",
        "Lyra", "Nova", "Elara", "Serena", "Anya"
    };

    public static string GetRandomName(NPCGender gender)
    {
        if (gender == NPCGender.Male)
        {
            return maleNames[Random.Range(0, maleNames.Length)];
        }
        else
        {
            return femaleNames[Random.Range(0, femaleNames.Length)];
        }
    }

    static readonly string[] acceptHonestReactions =
    {
        "Terima kasih banyak, pak! Semoga Tuhan membalas kebaikan Anda.",
        "Terima kasih sudah mengerti situasi saya... Saya sangat menghargainya!",
        "Terima kasih, pak! Saya bisa menyusul keluarga saya sekarang.",
        "Terima kasih banyak! Saya tidak tahu apa yang harus saya lakukan jika ditolak.",
        "Berkah untukmu, petugas. Selamat bertugas!"
    };

    static readonly string[] acceptFakeReactions =
    {
        "Haha, terima kasih bos... Akhirnya bisa lewat juga.",
        "Terima kasih... Anda membuat keputusan yang sangat bijak.",
        "Terima kasih banyak, petugas. Semoga tidak ada inspektur lain yang tahu...",
        "Baguslah... Mesin ini memang cuma buang-buang waktu.",
        "Terima kasih, kawan. Sampai jumpa di stasiun berikutnya."
    };

    static readonly string[] rejectHonestReactions =
    {
        "Tolonglah pak... Suhu di luar beku, saya bisa mati kedinginan di sini...",
        "Sistem konyol ini... Bagaimana saya bisa bertahan hidup malam ini?!",
        "Jatuh miskin dan sekarang dibuang... Kejam sekali tempat ini.",
        "Tapi anak dan istriku menunggu ransum di dalam kereta! Ku mohon!",
        "Kamu benar-benar tidak punya nurani..."
    };

    static readonly string[] rejectFakeReactions =
    {
        "Coba saja lain kali... Mesin busuk ini selalu menghalangiku!",
        "Awas kamu, petugas... Ini belum berakhir!",
        "Sialan... Harus cari cara lain untuk menembus stasiun ini.",
        "Jangan sok suci! Semua orang di sini juga menyuap mesin tiket!",
        "Tunggu sampai bosku tahu soal ini... Kamu akan menyesal."
    };

    static readonly string[] anomalyRejectReactions =
    {
        "K___A___M___U . A___K___A___N . S___E___S___A___L . . .",
        "D__I__N__G__I__N . S__E__M__A__K__I__N . M__E__N__D__E__K__A__T . . .",
        ". . . . . . . . . ."
    };

    public static string GetDecisionReaction(PassengerData data, bool accepted)
    {
        if (data == null) return "...";

        if (data.isMonster)
        {
            if (accepted)
                return "H___A___H___A___H___A . . . T___E___R___I___M___A . K___A___S___I___H . . .";
            else
                return anomalyRejectReactions[Random.Range(0, anomalyRejectReactions.Length)];
        }

        if (accepted)
        {
            if (data.isReasonTrue)
                return acceptHonestReactions[Random.Range(0, acceptHonestReactions.Length)];
            else
                return acceptFakeReactions[Random.Range(0, acceptFakeReactions.Length)];
        }
        else
        {
            if (data.isReasonTrue)
                return rejectHonestReactions[Random.Range(0, rejectHonestReactions.Length)];
            else
                return rejectFakeReactions[Random.Range(0, rejectFakeReactions.Length)];
        }
    }

    public static PassengerData GeneratePassenger()
    {
        PassengerData data = new PassengerData();

        data.ticket.ticketID =
            Random.Range(100000,999999).ToString();

        data.ticket.originStation =
            stations[Random.Range(0, stations.Length)];

        do
        {
            data.ticket.destinationStation =
                stations[Random.Range(0, stations.Length)];
        }
        while (data.ticket.destinationStation == data.ticket.originStation);

        data.ticket.trainLine =
            lines[0];

        // Generate Seat Class — Day 6+ semakin sedikit Class A
        if (DayManager.Instance != null && DayManager.Instance.CurrentDay >= GameDay.Day6)
        {
            // Day 6+: hanya 25% chance dapat Class A
            float roll = Random.value;
            if (roll < 0.25f)
                data.ticket.seatClass = SeatClass.A;
            else if (roll < 0.62f)
                data.ticket.seatClass = SeatClass.B;
            else
                data.ticket.seatClass = SeatClass.C;
        }
        else
        {
            // Day 1-5: Class A dominan (80%)
            data.ticket.seatClass = Random.value < 0.8f ? SeatClass.A :
                                    Random.value < 0.5f ? SeatClass.B : SeatClass.C;
        }

        data.ticket.status =
            TicketDifficultyManager.GenerateStatus();


        if (data.ticket.status == TicketStatus.Valid)
        {
            data.reason = "";
            data.isReasonTrue = true;
        }
        else
        {
            data.isReasonTrue = Random.value < 0.5f;

            switch (data.ticket.status)
            {
                case TicketStatus.Invalid:

                    data.reason = data.isReasonTrue
                        ? invalidHonest[Random.Range(0, invalidHonest.Length)]
                        : invalidFake[Random.Range(0, invalidFake.Length)];

                    break;

                case TicketStatus.Expired:

                    data.reason = data.isReasonTrue
                        ? expiredHonest[Random.Range(0, expiredHonest.Length)]
                        : expiredFake[Random.Range(0, expiredFake.Length)];

                    break;

                case TicketStatus.WrongDestination:

                    data.reason = data.isReasonTrue
                        ? wrongDestinationHonest[Random.Range(0, wrongDestinationHonest.Length)]
                        : wrongDestinationFake[Random.Range(0, wrongDestinationFake.Length)];

                    break;

                case TicketStatus.Fake:

                    data.isReasonTrue = false;

                    data.reason =
                        fakeReasons[
                            Random.Range(0, fakeReasons.Length)];

                    break;
            }
        }

        return data;
    }
}