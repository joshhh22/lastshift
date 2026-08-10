public static class TicketValidator
{
    public static TicketStatus Validate(PassengerData data)
    {
        if (RuleManager.Instance != null)
        {
            TicketStatus ruleStatus = RuleManager.Instance.CheckSpecialRules(data);
            
            if (ruleStatus != TicketStatus.Valid)
            {
                return ruleStatus;
            }
        }

        return data.ticket.status;
    }
}