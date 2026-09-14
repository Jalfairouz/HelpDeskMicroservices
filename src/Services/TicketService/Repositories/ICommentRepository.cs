using TicketService.Models;

namespace TicketService.Repositories;

public interface ICommentRepository
{
    Task<List<Comment>> GetByTicketIdAsync(Guid ticketId);

    Task AddAsync(Comment comment);
}