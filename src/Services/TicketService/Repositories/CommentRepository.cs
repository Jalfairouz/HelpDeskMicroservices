using Microsoft.EntityFrameworkCore;
using TicketService.Data;
using TicketService.Models;

namespace TicketService.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly TicketDbContext _context;

    public CommentRepository(TicketDbContext context)
    {
        _context = context;
    }

    public async Task<List<Comment>> GetByTicketIdAsync(Guid ticketId)
    {
        return await _context.Comments
            .AsNoTracking()
            .Where(c => c.TicketId == ticketId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Comment comment)
    {
        await _context.Comments.AddAsync(comment);
    }
}