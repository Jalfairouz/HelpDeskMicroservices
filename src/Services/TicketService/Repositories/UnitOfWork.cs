using TicketService.Data;

namespace TicketService.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly TicketDbContext _context;

    public ITicketRepository Tickets { get; }
    public ICommentRepository Comments { get; }

    public ITicketHistoryRepository TicketHistories { get; }
    public UnitOfWork(
        TicketDbContext context,
        ITicketRepository ticketRepository,
        ICommentRepository commentRepository,
        ITicketHistoryRepository ticketHistoryRepository)

    {
        _context = context;
        Tickets = ticketRepository;
        Comments = commentRepository;
        TicketHistories = ticketHistoryRepository;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}