using EventManagementServer.Dto;
using EventManagementServer.Models;
using System.Security.Claims;

namespace EventManagementServer.Interface
{
    public interface ICommentRepository
    {
        Task<IEnumerable<Comment>> GetCommentsAsync();
        Task<Comment> GetCommentByIdAsync(int id);
        Task<Comment> GetCommentByUserIdAsync(int id);
        Task<Comment> GetCommentByEventIdAsync(int id);
        Task<Comment> CreateCommentAsync(CommentDto commentDto, ClaimsPrincipal user);
        Task<Comment> UpdateCommentAsync(int id, CommentDto commentDto, ClaimsPrincipal user);
        Task<bool> DeleteCommentAsync(int id, ClaimsPrincipal user);
    }
}
