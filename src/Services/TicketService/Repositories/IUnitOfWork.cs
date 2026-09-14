namespace TicketService.Repositories;

public interface IUnitOfWork
{
    ITicketRepository Tickets { get; }

    Task<int> SaveChangesAsync();
}