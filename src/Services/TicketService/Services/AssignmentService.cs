using TicketService.Repositories;

namespace TicketService.Services;

public class AssignmentService : IAssignmentService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly UserManagementClient _userManagementClient;

    public AssignmentService(
        ITicketRepository ticketRepository,
        UserManagementClient userManagementClient)
    {
        _ticketRepository = ticketRepository;
        _userManagementClient = userManagementClient;
    }

    public async Task<Guid?> FindLeastBusyTechnicianAsync(
        Guid? excludedTechnicianId = null)
    {
        var technicians = await _userManagementClient.GetTechniciansAsync();
        Guid? selectedTechnicianId = null;
        int? lowestActiveTickets = null;

        foreach (var technician in technicians
                     .Where(item => item.Id != excludedTechnicianId)
                     .OrderBy(item => item.Id))
        {
            var activeTickets = await _ticketRepository
                .CountActiveByTechnicianIdAsync(technician.Id);

            if (lowestActiveTickets is null || activeTickets < lowestActiveTickets)
            {
                selectedTechnicianId = technician.Id;
                lowestActiveTickets = activeTickets;
            }
        }

        return selectedTechnicianId;
    }
}