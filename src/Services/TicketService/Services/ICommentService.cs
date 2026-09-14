using TicketService.DTOs.Requests;
using TicketService.DTOs.Responses;

namespace TicketService.Services;

public interface ICommentService
{
    Task<CommentResponse> AddAsync(
        Guid ticketId,
        AddCommentRequest request,
        Guid authorUserId);

    Task<IEnumerable<CommentResponse>> GetByTicketIdAsync(
        Guid ticketId);
}