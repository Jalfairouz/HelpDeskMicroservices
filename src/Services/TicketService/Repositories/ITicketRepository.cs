using TicketService.Models;

namespace TicketService.Repositories;

public interface ITicketRepository
{
    Task<List<Ticket>> GetAllAsync();

    Task<Ticket?> GetByIdAsync(Guid id);

    Task<List<Ticket>> GetByCreatorIdAsync(Guid userId);

    Task<List<Ticket>> GetByTechnicianIdAsync( Guid technicianId);

    Task<int> CountActiveByTechnicianIdAsync(Guid technicianId);

    Task AddAsync(Ticket ticket);

    void Delete(Ticket ticket);
}