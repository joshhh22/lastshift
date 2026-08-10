using UnityEngine;

public static class TicketDifficultyManager
{
    public static TicketStatus GenerateStatus()
    {
        float random = Random.value;

        // Jika DayManager belum ada, peluang Valid 50%
        float validChance = 0.5f;

        if (DayManager.Instance != null)
        {
            // Di Day 1, kemungkinan tiket Valid 70%
            if (DayManager.Instance.CurrentDay == GameDay.Day1)
            {
                validChance = 0.7f;
            }
            // Di Day 2, mulai agak kacau, kemungkinan Valid 50%
            else if (DayManager.Instance.CurrentDay == GameDay.Day2)
            {
                validChance = 0.5f;
            }
            // Day 3 ke atas, Valid cuma 40%
            else
            {
                validChance = 0.4f;
            }
        }

        if (random <= validChance)
        {
            return TicketStatus.Valid;
        }
        else
        {
            // Jika tidak valid, acak di antara 4 error lainnya
            int randomError = Random.Range(0, 4);

            switch (randomError)
            {
                case 0: return TicketStatus.Invalid;
                case 1: return TicketStatus.Expired;
                case 2: return TicketStatus.WrongDestination;
                default: return TicketStatus.Fake;
            }
        }
    }
}