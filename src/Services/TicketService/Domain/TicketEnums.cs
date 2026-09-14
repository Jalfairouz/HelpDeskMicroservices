namespace TicketService.Domain;

public enum TicketStatus
{
    Open,
    InProgress,
    Closed
}

public enum TicketPriority
{
    Low,
    Medium,
    High
}

public enum TicketType
{
    Incident,
    ServiceRequest
}

public enum TicketCategory
{
    Hardware,
    Software,
    Network,
    Access,
    Email,
    Security,
    Other
}