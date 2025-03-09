using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Interface;
using EventManagementServer.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventManagementServer.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly EventDbContext _context;
        public CommentRepository(EventDbContext context)
        {
            _context = context;
        }
        public async Task<Comment> CreateCommentAsync(CommentDto commentDto, ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

            if (commentDto == null || commentDto.UserID.ToString() != userId && userRole != "1")
                return null;

            var newComment = new Comment
            {
                EventID = commentDto.EventID,
                UserID = commentDto.UserID,
                Content = commentDto.Content,
            };

            _context.Comments.Add(newComment);
            await _context.SaveChangesAsync();

            return newComment;
        }

        public async Task<bool> DeleteCommentAsync(int id, ClaimsPrincipal user)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentID == id);
            if (comment == null) return false;

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

            if (comment.UserID.ToString() != userId && userRole != "1")
                return false;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Comment?> GetCommentByEventIdAsync(int id)
        {
            return await _context.Comments
                .FirstOrDefaultAsync(c => c.EventID == id);
        }

        public async Task<Comment?> GetCommentByIdAsync(int id)
        {
            return await _context.Comments
                .FirstOrDefaultAsync(c => c.CommentID == id);
        }

        public async Task<Comment?> GetCommentByUserIdAsync(int id)
        {
            return await _context.Comments
                .FirstOrDefaultAsync(c => c.UserID == id);
        }

        public async Task<IEnumerable<Comment>> GetCommentsAsync()
        {
            return await _context.Comments.ToListAsync();
        }

        public async Task<Comment> UpdateCommentAsync(int id, CommentDto commentDto, ClaimsPrincipal user)
        {
            var existingComment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentID == id);
            if (existingComment == null) return null;

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

            if (existingComment.UserID.ToString() != userId && userRole != "1")
                return null;

            existingComment.Content = commentDto.Content;
            existingComment.EventID = commentDto.EventID;
            existingComment.UserID = commentDto.UserID;

            await _context.SaveChangesAsync();
            return existingComment;
        }
    }
}
