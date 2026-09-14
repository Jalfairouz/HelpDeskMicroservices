using TicketService.DTOs.Requests;
using TicketService.DTOs.Responses;
using TicketService.Models;
using TicketService.Repositories;

namespace TicketService.Services;

public class TicketsService : ITicketsService
{
    private readonly IUnitOfWork _unitOfWork;

    public TicketsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TicketResponse> CreateAsync(
        CreateTicketRequest request,
        Guid createdByUserId)
    {
        var ticket = new Ticket
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Type = request.Type,
            Category = request.Category,
            Priority = request.Priority,
            Status = TicketStatus.Open,
            CreatedByUserId = createdByUserId,
            AssignedTechnicianId = null,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Tickets.AddAsync(ticket);

        await _unitOfWork.SaveChangesAsync();

        return CreateResponse(ticket);
    }

    public async Task<IEnumerable<TicketResponse>> GetAllAsync(
        Guid userId,
        string role)
    {
        List<Ticket> tickets;

        if (role == "Admin")
        {
            tickets =
                await _unitOfWork.Tickets.GetAllAsync();
        }
        else if (role == "Technician")
        {
            tickets =
                await _unitOfWork.Tickets
                    .GetByTechnicianIdAsync(userId);
        }
        else
        {
            tickets =
                await _unitOfWork.Tickets
                    .GetByCreatorIdAsync(userId);
        }

        return tickets.Select(CreateResponse);
    }

    public async Task<TicketResponse?> GetByIdAsync(
        Guid ticketId,
        Guid userId,
        string role)
    {
        var ticket =
            await _unitOfWork.Tickets.GetByIdAsync(ticketId);

        if (ticket is null)
        {
            return null;
        }

        var canAccess = role switch
        {
            "Admin" => true,

            "Technician" =>
                ticket.AssignedTechnicianId == userId,

            "Employee" =>
                ticket.CreatedByUserId == userId,

            _ => false
        };

        if (!canAccess)
        {
            throw new UnauthorizedAccessException(
                "You cannot access this ticket.");
        }

        return CreateResponse(ticket);
    }

    public async Task<TicketResponse?> UpdateAsync(
        Guid ticketId,
        UpdateTicketRequest request,
        Guid userId,
        string role)
    {
        var ticket =
            await _unitOfWork.Tickets.GetByIdAsync(ticketId);

        if (ticket is null)
        {
            return null;
        }

        if (role == "Employee")
        {
            if (ticket.CreatedByUserId != userId)
            {
                throw new UnauthorizedAccessException(
                    "You cannot update this ticket.");
            }

            if (ticket.Status != TicketStatus.Open)
            {
                throw new InvalidOperationException(
                    "Only open tickets can be updated.");
            }
        }

        ticket.Title = request.Title.Trim();
        ticket.Description = request.Description.Trim();
        ticket.Type = request.Type;
        ticket.Category = request.Category;
        ticket.Priority = request.Priority;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return CreateResponse(ticket);
    }

    public async Task<bool> DeleteAsync(Guid ticketId)
    {
        var ticket =
            await _unitOfWork.Tickets.GetByIdAsync(ticketId);

        if (ticket is null)
        {
            return false;
        }

        _unitOfWork.Tickets.Delete(ticket);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static TicketResponse CreateResponse(Ticket ticket)
    {
        return new TicketResponse
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Type = ticket.Type,
            Category = ticket.Category,
            Priority = ticket.Priority,
            Status = ticket.Status,
            CreatedByUserId = ticket.CreatedByUserId,
            AssignedTechnicianId =
                ticket.AssignedTechnicianId,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            ClosedAt = ticket.ClosedAt
        };
    }
}