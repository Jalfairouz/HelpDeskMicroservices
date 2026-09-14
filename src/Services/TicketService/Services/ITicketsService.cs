using TicketService.DTOs.Requests;
using TicketService.DTOs.Responses;

namespace TicketService.Services;

public interface ITicketsService
{
    Task<TicketResponse> CreateAsync(
        CreateTicketRequest request,
        Guid createdByUserId);

    Task<IEnumerable<TicketResponse>> GetAllAsync(
        Guid userId,
        string role);

    Task<TicketResponse?> GetByIdAsync(
        Guid ticketId,
        Guid userId,
        string role);

    Task<TicketResponse?> UpdateAsync(
        Guid ticketId,
        UpdateTicketRequest request,
        Guid userId,
        string role);

    Task<TicketResponse?> ChangeStatusAsync(
        Guid ticketId,
        ChangeTicketStatusRequest request,
        Guid userId,
        string role);

    Task<bool> DeleteAsync(Guid ticketId);
}
