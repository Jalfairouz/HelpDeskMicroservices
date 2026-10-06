using TicketService.DTOs.Requests;
using TicketService.DTOs.Responses;

namespace TicketService.Services;

public interface ITicketsService
{
    Task<TicketResponse> CreateAsync(CreateTicketRequest request, Guid createdByUserId);

    Task<IEnumerable<TicketResponse>> GetAllTicketsAsync(Guid userId, TicketFilter filter);
    Task<IEnumerable<TicketResponse>> GetSystemTicketsAsync(Guid adminId, TicketFilter filter);
    Task<IEnumerable<TicketResponse>> GetAssignedTicketsAsync(Guid technicianId);


    Task<TicketResponse?> GetByIdAsync(Guid ticketId,Guid userId,string role);
    
    Task<TicketResponse?> UpdateAsync( Guid ticketId,UpdateTicketRequest request, Guid userId, string role);

    Task<TicketResponse?> ChangeStatusAsync(Guid ticketId,ChangeTicketStatusRequest request,Guid userId,string role);

    Task<IEnumerable<TicketHistoryResponse>?>GetHistoryAsync(Guid ticketId, Guid userId,string role);

    Task<bool> DeleteAsync( Guid ticketId);
}