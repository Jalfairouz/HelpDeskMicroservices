using TicketService.DTOs.Responses;

namespace TicketService.Services;

public interface IAssignmentService
{
    Task<Guid?> FindLeastBusyTechnicianAsync(Guid? excludedTechnicianId = null);
    Task<TicketResponse?> AssignAsync(Guid ticketId, Guid technicianId, Guid adminUserId);

}
