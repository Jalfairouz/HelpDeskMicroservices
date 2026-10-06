using TicketService.Domain;
using TicketService.DTOs.Requests;
using TicketService.Models;

namespace TicketService.Repositories;

public interface ITicketRepository
{

    Task<List<Ticket>> GetListAsync(Guid? createdByUserId, Guid? excludeCreatedByUserId,
    Guid? assignedTechnicianId, TicketStatus? status, bool? unassigned);

    Task<Ticket?> GetByIdAsync(Guid id);

    Task<int> CountActiveByTechnicianIdAsync(Guid technicianId);

    Task AddAsync(Ticket ticket);

    void Delete(Ticket ticket);
}