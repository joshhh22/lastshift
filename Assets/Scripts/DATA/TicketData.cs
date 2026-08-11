using System;

public enum TicketStatus
{
    Valid,
    Invalid,
    Expired,
    Fake,
    WrongDestination
}

public enum SeatClass
{
    A,
    B,
    C
}

[Serializable]
public class TicketData
{
    public string ticketID;

    public string originStation;

    public string destinationStation;

    public string trainLine;

    public SeatClass seatClass = SeatClass.A;

    public TicketStatus status = TicketStatus.Valid;
}