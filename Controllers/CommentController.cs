﻿using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventManagementServer.Controllers
{
    [Route("api/[controller]")]
    public class CommentController : Controller
    {
        private readonly EventDbContext _context;

        public CommentController(EventDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Comment>>> GetComments()
        {
            return await _context.Comments.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Comment>> GetCommentById(int id)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentID == id);

            if (comment == null)
            {
                return NotFound();
            }

            return Ok(comment);
        }

        [HttpGet("user/{id}")]
        public async Task<ActionResult<Comment>> GetCommentByUserId(int id)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.UserID == id);

            if (comment == null)
            {
                return NotFound();
            }

            return Ok(comment);
        }

        [HttpGet("event/{id}")]
        public async Task<ActionResult<Comment>> GetCommentByEventId(int id)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.EventID == id);

            if (comment == null)
            {
                return NotFound();
            }

            return Ok(comment);
        }

        [HttpPost]
        public async Task<ActionResult<Comment>> CreateComment([FromBody] CommentDto comment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Comment newComment = new Comment
            {
                EventID = comment.EventID,
                UserID = comment.UserID,
                Content = comment.Content,
            };

            _context.Comments.Add(newComment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCommentById), new { id = newComment.CommentID }, newComment);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Comment>> UpdateComment(int id, [FromBody] CommentDto comment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingComment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentID == id);

            if (existingComment == null)
            {
                return NotFound();
            }

            existingComment.EventID = comment.EventID;
            existingComment.UserID = comment.UserID;
            existingComment.Content = comment.Content;

            await _context.SaveChangesAsync();

            return Ok(existingComment);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Comment>> DeleteComment(int id)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentID == id);

            if (comment == null)
            {
                return NotFound();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok(comment);
        }
    }
}
