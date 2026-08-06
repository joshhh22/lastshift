using System;

[Serializable]
public class PassengerData
{
    public string passengerName;

    public TicketData ticket = new TicketData();

    public string reason;
}