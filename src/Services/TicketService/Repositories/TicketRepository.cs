using Microsoft.EntityFrameworkCore;
using TicketService.Data;
using TicketService.Domain;
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


    public async Task<List<Ticket>> GetListAsync(Guid? createdByUserId,Guid? excludeCreatedByUserId,
    Guid? assignedTechnicianId, TicketStatus? status, bool? unassigned)
    {
        IQueryable<Ticket> query = _context.Tickets.AsNoTracking();

        if (createdByUserId.HasValue)
            query = query.Where(t => t.CreatedByUserId == createdByUserId.Value);

        if (excludeCreatedByUserId.HasValue)
            query = query.Where(t => t.CreatedByUserId != excludeCreatedByUserId.Value);

        if (assignedTechnicianId.HasValue)
            query = query.Where(t => t.AssignedTechnicianId == assignedTechnicianId.Value);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (unassigned == true)
            query = query.Where(t => t.AssignedTechnicianId == null);
        else if (unassigned == false)
            query = query.Where(t => t.AssignedTechnicianId != null);

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<Ticket?> GetByIdAsync(Guid id)
    {
        return await _context.Tickets
            .Include(ticket => ticket.Comments)
            .Include(ticket => ticket.TicketHistories)
            
            .FirstOrDefaultAsync(ticket => ticket.Id == id);
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