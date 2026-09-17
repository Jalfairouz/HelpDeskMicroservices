using TicketService.Domain;
using TicketService.DTOs.Responses;
using TicketService.Models;
using TicketService.Repositories;

namespace TicketService.Services;

public class AssignmentService : IAssignmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITicketRepository _ticketRepository;
    private readonly UserManagementClient _userManagementClient;

    public AssignmentService(IUnitOfWork unitOfWork, ITicketRepository ticketRepository, UserManagementClient userManagementClient)
    {
        _unitOfWork = unitOfWork;
        _ticketRepository = ticketRepository;
        _userManagementClient = userManagementClient;
    }

    public async Task<Guid?> FindLeastBusyTechnicianAsync(Guid? excludedTechnicianId = null)
    {
        var technicians = await _userManagementClient.GetTechniciansAsync();

        var availableTechnicians = technicians
            .Where(technician => technician.IsActive)
            .Where(technician => technician.Id != excludedTechnicianId)
            .OrderBy(technician => technician.Id);

        Guid? selectedTechnicianId = null;
        int? lowestActiveTickets = null;

        foreach (var technician in availableTechnicians)
        {
            var activeTickets = await _ticketRepository.CountActiveByTechnicianIdAsync(technician.Id);

            if (lowestActiveTickets is null || activeTickets < lowestActiveTickets)
            {
                selectedTechnicianId = technician.Id;
                lowestActiveTickets = activeTickets;
            }
        }

        return selectedTechnicianId;
    }
    public async Task<TicketResponse?> AssignAsync(Guid ticketId, Guid technicianId, Guid adminUserId)
    {
        var ticket = await _unitOfWork.Tickets.GetByIdAsync(ticketId);

        if (ticket is null)
        {
            return null;
        }

        if (ticket.Status == TicketStatus.Closed)
        {
            throw new InvalidOperationException("A closed ticket cannot be assigned.");
        }

        var technicians = await _userManagementClient.GetTechniciansAsync();
        var technician = technicians.FirstOrDefault(t => t.Id == technicianId && t.IsActive);

        if (technician is null)
        {
            throw new InvalidOperationException("Technician not found or not active.");
        }

        ticket.AssignedTechnicianId = technicianId;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.TicketHistories.AddAsync(new TicketHistory
        {
            TicketId = ticket.Id,
            PerformedByUserId = adminUserId,
            ActionType = ActionType.Assigned,
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();

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
            AssignedTechnicianId = ticket.AssignedTechnicianId,
            CreatedAt = ticket.CreatedAt.ToString(),
            UpdatedAt = ticket.UpdatedAt?.ToString(),
            ClosedAt = ticket.ClosedAt?.ToString()
        };
    }
}