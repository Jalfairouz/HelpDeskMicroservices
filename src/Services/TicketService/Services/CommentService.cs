using TicketService.Domain;
using TicketService.DTOs.Requests;
using TicketService.DTOs.Responses;
using TicketService.Models;
using TicketService.Repositories;

namespace TicketService.Services;

public class CommentService : ICommentService
{
    private readonly IUnitOfWork _unitOfWork;

    public CommentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CommentResponse> AddAsync(
        Guid ticketId,
        AddCommentRequest request,
        Guid authorUserId)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException(
                "Comment content is required.");
        }

        var now = DateTime.UtcNow;

        var comment = new Comment
        {
            TicketId = ticketId,
            AuthorUserId = authorUserId,
            Content = request.Content.Trim(),
            CreatedAt = now
        };

        await _unitOfWork.Comments.AddAsync(comment);

        var history = new TicketHistory
        {
            TicketId = ticketId,
            PerformedByUserId = authorUserId,
            ActionType = ActionType.CommentAdded,
            CreatedAt = now
        };

        await _unitOfWork.TicketHistories.AddAsync(
            history);

        await _unitOfWork.SaveChangesAsync();

        return MapToResponse(comment);
    }

    public async Task<IEnumerable<CommentResponse>>
        GetByTicketIdAsync(Guid ticketId)
    {
        var comments = await _unitOfWork.Comments
            .GetByTicketIdAsync(ticketId);

        return comments.Select(MapToResponse);
    }

    private static CommentResponse MapToResponse(
        Comment comment)
    {
        return new CommentResponse
        {
            Id = comment.Id,
            TicketId = comment.TicketId,
            AuthorUserId = comment.AuthorUserId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt
        };
    }
}