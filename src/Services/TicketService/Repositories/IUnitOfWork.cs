namespace TicketService.Repositories;

public interface IUnitOfWork
{
    ITicketRepository Tickets { get; }
    ICommentRepository Comments { get; }
    ITicketHistoryRepository TicketHistories { get; }
    Task<int> SaveChangesAsync();
}