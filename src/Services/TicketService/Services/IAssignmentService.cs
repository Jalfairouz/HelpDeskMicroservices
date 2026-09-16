namespace TicketService.Services;

public interface IAssignmentService
{
    Task<Guid?> FindLeastBusyTechnicianAsync(Guid? excludedTechnicianId = null);
}
