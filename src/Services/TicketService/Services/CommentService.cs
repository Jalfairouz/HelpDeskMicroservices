using TicketService.Domain;
using TicketService.DTOs.Requests;
using TicketService.DTOs.Responses;
using TicketService.Models;
using TicketService.Repositories;
using TicketService.Services;
namespace TicketService.Services;

public class CommentService : ICommentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManagementClient _userManagementClient;
    public CommentService(IUnitOfWork unitOfWork, UserManagementClient userManagementClient)
    {
        _unitOfWork = unitOfWork;
        _userManagementClient = userManagementClient;

    }

    public async Task<CommentResponse> AddAsync(Guid ticketId, AddCommentRequest request,Guid authorUserId)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException( "Comment content is required.");
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

        await _unitOfWork.TicketHistories.AddAsync(history);

        await _unitOfWork.SaveChangesAsync();

        return MapToResponse(comment);
    }

    public async Task<IEnumerable<CommentResponse>>GetByTicketIdAsync(Guid ticketId)
    {
        var comments = await _unitOfWork.Comments.GetByTicketIdAsync(ticketId);

        var result = new List<CommentResponse>();

        foreach (var comment in comments)
        {
            var user = await _userManagementClient.GetUserByIdAsync(comment.AuthorUserId);

             result.Add(new CommentResponse
            {
                Id = comment.Id,
                TicketId = comment.TicketId,
                AuthorUserId = comment.AuthorUserId,
                AuthorName = user?.FullName ?? "Unknown User",
                Content = comment.Content,
                CreatedAt = comment.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
        return result;
    }

    private static CommentResponse MapToResponse(Comment comment)
    {
        return new CommentResponse
        {
            Id = comment.Id,
            TicketId = comment.TicketId,
            AuthorUserId = comment.AuthorUserId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt.ToString(),
        };
    }
}