using System;

[Serializable]
public class PassengerData
{
    public string passengerName;

    public NPCGender gender;

    public TicketData ticket = new TicketData();

    public string reason;

    public bool isReasonTrue;
}