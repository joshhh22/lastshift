using UnityEngine;

public static class TicketDifficultyManager
{
    public static TicketStatus GenerateStatus()
    {
        float random = Random.value;

        float validChance = 0.5f;

        if (DayManager.Instance != null)
        {
            switch (DayManager.Instance.CurrentDay)
            {
                case GameDay.Day1: validChance = 0.70f; break;
                case GameDay.Day2: validChance = 0.50f; break;
                case GameDay.Day3: validChance = 0.40f; break;
                case GameDay.Day4: validChance = 0.35f; break;
                case GameDay.Day5: validChance = 0.30f; break;
                case GameDay.Day6: validChance = 0.25f; break;
                case GameDay.Day7: validChance = 0.20f; break;
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