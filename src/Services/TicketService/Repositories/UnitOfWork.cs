using TicketService.Data;

namespace TicketService.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly TicketDbContext _context;

    public ITicketRepository Tickets { get; }
    public ICommentRepository Comments { get; }
    public UnitOfWork(
        TicketDbContext context,
        ITicketRepository ticketRepository,
        ICommentRepository commentRepository)

    {
        _context = context;
        Tickets = ticketRepository;
        Comments = commentRepository;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}