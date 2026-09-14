using TicketService.Models;

namespace TicketService.Repositories;

public interface ITicketHistoryRepository
{
    Task<List<TicketHistory>> GetByTicketIdAsync(Guid ticketId);

    Task AddAsync(TicketHistory history);
}