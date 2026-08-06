using System;

public enum TicketStatus
{
    Valid,
    Invalid,
    Expired,
    Fake,
    WrongDestination
}

[Serializable]
public class TicketData
{
    public string ticketID;

    public string originStation;
    public string destinationStation;

    public string trainLine;

    public TicketStatus status = TicketStatus.Valid;
}