using Microsoft.EntityFrameworkCore;
using TicketService.Data;
using TicketService.Models;

namespace TicketService.Repositories;

public class TicketHistoryRepository : ITicketHistoryRepository
{
    private readonly TicketDbContext _context;

    public TicketHistoryRepository(TicketDbContext context)
    {
        _context = context;
    }

    public async Task<List<TicketHistory>> GetByTicketIdAsync(Guid ticketId)
    {
        return await _context.TicketHistories
            .AsNoTracking()
            .Where(h => h.TicketId == ticketId)
            .OrderBy(h => h.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(TicketHistory history)
    {
        await _context.TicketHistories.AddAsync(history);
    }
}