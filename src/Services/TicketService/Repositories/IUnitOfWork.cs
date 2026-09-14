namespace TicketService.Repositories;

public interface IUnitOfWork
{
    ITicketRepository Tickets { get; }
    ICommentRepository Comments { get; }
    Task<int> SaveChangesAsync();
}