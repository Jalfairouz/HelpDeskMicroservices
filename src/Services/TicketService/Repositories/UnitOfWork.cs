using TicketService.Data;

namespace TicketService.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly TicketDbContext _context;

    public ITicketRepository Tickets { get; }

    public UnitOfWork(
        TicketDbContext context,
        ITicketRepository ticketRepository)
    {
        _context = context;
        Tickets = ticketRepository;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}