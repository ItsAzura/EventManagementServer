using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Interface;
using EventManagementServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EventManagementServer.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ControllerName("Comment")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CommentController : Controller
    {
        private readonly ICommentRepository _repository;
        public CommentController(ICommentRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [EnableRateLimiting("FixedWindowLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Comment>>> GetComments()
        {
            var comments = await _repository.GetCommentsAsync();

            if (comments == null || !comments.Any()) return NotFound();

            return Ok(comments);
        }

        [HttpGet("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Comment>> GetCommentById(int id)
        {
            var comment = await _repository.GetCommentByIdAsync(id);

            if (comment == null) return NotFound();

            var result = new CommentResponseDto
            {
                CommentID = comment.CommentID,
                EventID = comment.EventID,
                UserID = comment.UserID,
                Content = comment.Content,
                CreateAt = comment.CreateAt,
                Event = new SimpleEventDto
                {
                    EventID = comment.Event.EventID,
                    EventName = comment.Event.EventName,
                 
                },
                User = new SimpleUserDto
                {
                    UserID = comment.User.UserID,
                    UserName = comment.User.UserName,
                    Email = comment.User.Email
                }
            };

            return Ok(result);
        }

        [HttpGet("user/{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Comment>> GetCommentByUserId(int id)
        {
            var comment = await _repository.GetCommentByUserIdAsync(id);

            if (comment == null) return NotFound();

            var result = new CommentResponseDto
            {
                CommentID = comment.CommentID,
                EventID = comment.EventID,
                UserID = comment.UserID,
                Content = comment.Content,
                CreateAt = comment.CreateAt,
                Event = new SimpleEventDto
                {
                    EventID = comment.Event.EventID,
                    EventName = comment.Event.EventName,

                },
                User = new SimpleUserDto
                {
                    UserID = comment.User.UserID,
                    UserName = comment.User.UserName,
                    Email = comment.User.Email
                }
            };

            return Ok(result);
        }

        [HttpGet("event/{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Comment>> GetCommentByEventId(int id)
        {
            var comment = await _repository.GetCommentByEventIdAsync(id);

            if (comment == null) return NotFound();

            var result = new CommentResponseDto
            {
                CommentID = comment.CommentID,
                EventID = comment.EventID,
                UserID = comment.UserID,
                Content = comment.Content,
                CreateAt = comment.CreateAt,
                Event = new SimpleEventDto
                {
                    EventID = comment.Event.EventID,
                    EventName = comment.Event.EventName,

                },
                User = new SimpleUserDto
                {
                    UserID = comment.User.UserID,
                    UserName = comment.User.UserName,
                    Email = comment.User.Email
                }
            };

            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        [EnableRateLimiting("FixedWindowLimiter")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Comment>> DeleteComment(int id)
        {
            var success = await _repository.DeleteCommentAsync(id, User);
            
            if(!success) return Forbid();

            return Ok();


        }
    }
}
