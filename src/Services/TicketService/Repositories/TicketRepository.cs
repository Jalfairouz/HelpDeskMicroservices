using Microsoft.EntityFrameworkCore;
using TicketService.Data;
using TicketService.Domain;
using TicketService.DTOs.Requests;
using TicketService.Models;

namespace TicketService.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly TicketDbContext _context;

    public TicketRepository(
        TicketDbContext context)
    {
        _context = context;
    }

    public async Task<List<Ticket>> GetAllAsync()
    {
        return await _context.Tickets.AsNoTracking()
            .OrderByDescending(ticket =>ticket.CreatedAt)
            .ToListAsync();
    }
    public async Task<IEnumerable<Ticket>> GetAdminTicketsAsync( Guid? userId, AdminTicketFilter filter)
    {
        IQueryable<Ticket> query = _context.Tickets.AsNoTracking();

        if (filter.Mine == true && userId.HasValue)
        {
            query = query.Where(t => t.CreatedByUserId == userId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(t => t.Status == filter.Status.Value);
        }

        if (filter.Unassigned == true)
        {
            query = query.Where(t => t.AssignedTechnicianId == null);
        }
        else if (filter.Unassigned == false)
        {
            query = query.Where(t => t.AssignedTechnicianId != null);
        }

        return await query.ToListAsync();
    }

    public async Task<Ticket?> GetByIdAsync(Guid id)
    {
        return await _context.Tickets
            .Include(ticket => ticket.Comments)
            .Include(ticket => ticket.TicketHistories)
            
            .FirstOrDefaultAsync(ticket => ticket.Id == id);
    }

    public async Task<List<Ticket>> GetByCreatorIdAsync(Guid userId)
    {
        return await _context.Tickets
            .AsNoTracking()
            .Where(ticket =>ticket.CreatedByUserId == userId)
            .OrderByDescending(ticket => ticket.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Ticket>> GetByTechnicianIdAsync(Guid technicianId)
    {
        return await _context.Tickets
            
            .Where(ticket => ticket.AssignedTechnicianId == technicianId)
            .AsNoTracking()
            .OrderByDescending(ticket => ticket.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> CountActiveByTechnicianIdAsync( Guid technicianId)
    {
        return await _context.Tickets.CountAsync(
            ticket => ticket.AssignedTechnicianId == technicianId &&
                (
                    ticket.Status == TicketStatus.Open ||
                    ticket.Status == TicketStatus.InProgress
                ));
    }

    public async Task AddAsync(Ticket ticket)
    {
        await _context.Tickets.AddAsync(ticket);
    }

    public void Delete( Ticket ticket)
    {
        _context.Tickets.Remove(ticket);
    }
}