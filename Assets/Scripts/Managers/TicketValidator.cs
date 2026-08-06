public static class TicketValidator
{
    public static TicketStatus Validate(TicketData ticket)
    {
        return ticket.status;
    }
}