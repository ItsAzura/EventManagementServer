using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Models;
using EventManagementServer.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventManagementServer.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ControllerName("Comment")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CommentController : Controller
    {
        private readonly CommentRepository _repository;

        public CommentController(CommentRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<IEnumerable<Comment>>> GetComments()
        {
            return Ok(await _repository.GetCommentsAsync());
        }

        [HttpGet("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<Comment>> GetCommentById(int id)
        {
            var comment = await _repository.GetCommentByIdAsync(id);

            if (comment == null) return NotFound();
            return Ok(comment);
        }

        [HttpGet("user/{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<Comment>> GetCommentByUserId(int id)
        {
            var comment = await _repository.GetCommentByUserIdAsync(id);

            if (comment == null) return NotFound();

            return Ok(comment);
        }

        [HttpGet("event/{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<Comment>> GetCommentByEventId(int id)
        {
            var comment = await _repository.GetCommentByEventIdAsync(id);

            if (comment == null) return NotFound();

            return Ok(comment);
        }

        [Authorize]
        [HttpPost]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<Comment>> CreateComment([FromBody] CommentDto comment)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var newComment = await _repository.CreateCommentAsync(comment, User);

            if (newComment == null) return Forbid();

            return CreatedAtAction(nameof(GetCommentById), new { id = newComment.CommentID }, newComment);
        }

        [Authorize]
        [HttpPut("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<Comment>> UpdateComment(int id, [FromBody] CommentDto comment)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updatedComment = await _repository.UpdateCommentAsync(id, comment, User);
            if (updatedComment == null) return Forbid();

            return Ok(updatedComment);
        }

        [Authorize]
        [HttpDelete("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<Comment>> DeleteComment(int id)
        {
            var success = await _repository.DeleteCommentAsync(id, User);
            
            if(!success) return Forbid();

            return Ok();


        }
    }
}
