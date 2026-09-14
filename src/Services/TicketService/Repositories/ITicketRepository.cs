using TicketService.Models;

namespace TicketService.Repositories;

public interface ITicketRepository
{
    Task<List<Ticket>> GetAllAsync();

    Task<List<Ticket>> GetByCreatorIdAsync(Guid userId);

    Task<List<Ticket>> GetByTechnicianIdAsync(Guid technicianId);

    Task<Ticket?> GetByIdAsync(Guid id);

    Task AddAsync(Ticket ticket);

    void Delete(Ticket ticket);
}