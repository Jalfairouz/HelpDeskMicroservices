using TicketService.Domain;
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

        if (role == RoleNames.Admin)
        {
            tickets =
                await _unitOfWork.Tickets.GetAllAsync();
        }
        else if (role == RoleNames.Technician)
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
            RoleNames.Admin => true,

            RoleNames.Technician =>
                ticket.AssignedTechnicianId == userId,

            RoleNames.Employee =>
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

        if (ticket.Status == TicketStatus.Closed)
        {
            throw new InvalidOperationException(
                "A closed ticket cannot be updated.");
        }

        if (role == RoleNames.Employee)
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
        else if (role == RoleNames.Technician)
        {
            if (ticket.AssignedTechnicianId != userId)
            {
                throw new UnauthorizedAccessException(
                    "You can only update tickets assigned to you.");
            }
        }
        else if (role != RoleNames.Admin)
        {
            throw new UnauthorizedAccessException(
                "You cannot update this ticket.");
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

    public async Task<TicketResponse?> ChangeStatusAsync(
        Guid ticketId,
        ChangeTicketStatusRequest request,
        Guid userId,
        string role)
    {
        var ticket =
            await _unitOfWork.Tickets.GetByIdAsync(ticketId);

        if (ticket is null)
        {
            return null;
        }

        var newStatus = request.Status;

        if (role == RoleNames.Technician)
        {
            if (ticket.AssignedTechnicianId != userId)
            {
                throw new UnauthorizedAccessException(
                    "You can only change the status of tickets assigned to you.");
            }
        }
        else if (role != RoleNames.Admin)
        {
            throw new UnauthorizedAccessException(
                "You cannot change the status of this ticket.");
        }

        if (ticket.Status == TicketStatus.Closed)
        {
            throw new InvalidOperationException(
                "A closed ticket cannot change status.");
        }

        if (newStatus == ticket.Status)
        {
            throw new InvalidOperationException(
                $"The ticket is already {ticket.Status}.");
        }

        var transitionAllowed = (ticket.Status, newStatus) switch
        {
            (TicketStatus.Open, TicketStatus.InProgress) => true,

            (TicketStatus.Open, TicketStatus.Closed) =>
                role == RoleNames.Admin,

            (TicketStatus.InProgress, TicketStatus.Closed) => true,

            _ => false
        };

        if (!transitionAllowed)
        {
            throw new InvalidOperationException(
                $"Status cannot change from {ticket.Status} to {newStatus}.");
        }

        ticket.Status = newStatus;
        ticket.UpdatedAt = DateTime.UtcNow;

        if (newStatus == TicketStatus.Closed)
        {
            ticket.ClosedAt = DateTime.UtcNow;
        }

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