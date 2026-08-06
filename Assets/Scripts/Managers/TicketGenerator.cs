using UnityEngine;

public static class TicketGenerator
{
    static readonly string[] names =
    {
        "Andi",
        "Budi",
        "Citra",
        "Dimas",
        "Eka",
        "Farhan",
        "Gilang",
        "Hana",
        "Indra",
        "Joshua",
        "Kevin",
        "Nadia"
    };

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

    static readonly string[] invalidReasons =
    {
        "I forgot to renew my ticket.",
        "I lost my wallet.",
        "I bought the wrong ticket.",
        "Please let me through.",
        "I really need to get home."
    };

    public static PassengerData GeneratePassenger()
    {
        PassengerData data = new PassengerData();

        data.passengerName =
            names[Random.Range(0, names.Length)];

        data.ticket.ticketID =
            Random.Range(100000, 999999).ToString();

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
        }
        else
        {
            data.reason =
                invalidReasons[
                    Random.Range(0, invalidReasons.Length)];
        }

        return data;
    }
}