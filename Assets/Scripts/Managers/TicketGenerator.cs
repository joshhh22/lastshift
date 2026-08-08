using UnityEngine;

public static class TicketGenerator
{
    static readonly string[] stations =
    {
        "Manggarai",
        "Tebet",
        "Cawang",
        "Pasar Minggu",
        "Depok",
        "Bogor"
    };

    static readonly string[] lines =
    {
        "Red Line"
    };

    static readonly string[] invalidHonest =
    {
        "I forgot to renew my ticket.",
        "I lost my wallet.",
        "I bought the wrong ticket.",
        "My train was delayed.",
        "I really need to get home."
    };

    static readonly string[] invalidFake =
    {
        "My ticket disappeared.",
        "The machine ate my ticket.",
        "I left it at home.",
        "My friend has my ticket.",
        "The inspector already checked it."
    };

    static readonly string[] expiredHonest =
    {
        "I forgot to renew it this morning.",
        "I thought my ticket was still valid.",
        "I was rushing to work.",
        "I didn't notice it had expired.",
        "I've been busy all day."
    };

    static readonly string[] expiredFake =
    {
        "The ticket machine expired it.",
        "Your scanner is wrong.",
        "The staff told me it was still valid.",
        "The system made a mistake.",
        "It expired by itself."
    };

    static readonly string[] wrongDestinationHonest =
    {
        "I accidentally bought the wrong destination.",
        "It was my first time using the machine.",
        "I clicked the wrong station.",
        "I was in a hurry.",
        "I read the station name incorrectly."
    };

    static readonly string[] wrongDestinationFake =
    {
        "The machine changed my destination.",
        "Someone else bought this ticket.",
        "I never selected this station.",
        "The printer printed the wrong destination.",
        "The system is broken."
    };

    static readonly string[] fakeReasons =
    {
        "I bought it online.",
        "Someone sold me this ticket.",
        "This is my real ticket.",
        "I don't know why it looks different.",
        "The previous station accepted it."
    };


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