using UnityEngine;

public static class TicketDifficultyManager
{
    public static TicketStatus GenerateStatus()
    {
        int day = DayManager.Instance.CurrentDayNumber;

        int random = Random.Range(0, 100);

        switch (day)
        {
            // =========================
            // DAY 1
            // =========================
            case 1:

                if (random < 95)
                    return TicketStatus.Valid;

                return TicketStatus.Invalid;

            // =========================
            // DAY 2
            // =========================
            case 2:

                if (random < 90)
                    return TicketStatus.Valid;

                if (random < 98)
                    return TicketStatus.Invalid;

                return TicketStatus.Expired;

            // =========================
            // DAY 3
            // =========================
            case 3:

                if (random < 80)
                    return TicketStatus.Valid;

                if (random < 90)
                    return TicketStatus.Invalid;

                if (random < 97)
                    return TicketStatus.Expired;

                return TicketStatus.WrongDestination;

            // =========================
            // DAY 4
            // =========================
            case 4:

                if (random < 70)
                    return TicketStatus.Valid;

                if (random < 80)
                    return TicketStatus.Invalid;

                if (random < 90)
                    return TicketStatus.Expired;

                if (random < 98)
                    return TicketStatus.WrongDestination;

                return TicketStatus.Fake;

            // =========================
            // DAY 5
            // =========================
            case 5:

                if (random < 60)
                    return TicketStatus.Valid;

                if (random < 70)
                    return TicketStatus.Invalid;

                if (random < 82)
                    return TicketStatus.Expired;

                if (random < 95)
                    return TicketStatus.WrongDestination;

                return TicketStatus.Fake;

            // =========================
            // DAY 6
            // =========================
            case 6:

                if (random < 50)
                    return TicketStatus.Valid;

                if (random < 60)
                    return TicketStatus.Invalid;

                if (random < 75)
                    return TicketStatus.Expired;

                if (random < 90)
                    return TicketStatus.WrongDestination;

                return TicketStatus.Fake;

            // =========================
            // DAY 7
            // =========================
            default:

                if (random < 40)
                    return TicketStatus.Valid;

                if (random < 50)
                    return TicketStatus.Invalid;

                if (random < 70)
                    return TicketStatus.Expired;

                if (random < 90)
                    return TicketStatus.WrongDestination;

                return TicketStatus.Fake;
        }
    }
}