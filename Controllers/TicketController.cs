using EventManagementServer.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using EventManagementServer.Interface;

namespace EventManagementServer.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ControllerName("Ticket")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TicketController : Controller
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ILogger<TicketController> _logger;

        public TicketController(ITicketRepository ticketRepository, ILogger<TicketController> logger)
        {
            _ticketRepository = ticketRepository;
            _logger = logger;
        }

        [HttpGet]
        [EnableRateLimiting("FixedWindowLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetTickets(int page = 1, int pageSize = 10, int? Quantity = null, decimal? Price = null, string? search = null)
        {
            if(page < 1 || pageSize < 1) return BadRequest("Invalid page or pageSize");
            var (tickets, totalCount) = await _ticketRepository.GetTicketsAsync(page, pageSize, Quantity, Price, search);
            if (tickets == null) return NotFound();
            var response = new
            {
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                Tickets = tickets
            };
            _logger.LogInformation($"Get tickets: {response}");
            return Ok(response);
        }

        [HttpGet("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetTicketById(int id)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(id);
            if (ticket == null) return NotFound();
            _logger.LogInformation($"Get ticket by id: {ticket}");
            return Ok(ticket);
        }

        [HttpGet("eventarea/{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetTicketByEventAreaId(int id)
        {
            var tickets = await _ticketRepository.GetTicketsByEventAreaIdAsync(id);
            if (tickets == null) return NotFound();
            _logger.LogInformation($"Get ticket by event area id: {tickets}");
            return Ok(tickets);
        }

        [Authorize(Roles = "1,2")]
        [HttpPost]
        [EnableRateLimiting("FixedWindowLimiter")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CreateTicket([FromBody] TicketDto ticket)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var newTicket = await _ticketRepository.CreateTicketAsync(ticket, User);
                _logger.LogInformation($"Create new ticket: {newTicket}");
                return CreatedAtAction(nameof(GetTicketById), new { id = newTicket.TicketID }, newTicket);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "1,2")]
        [HttpPut("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateCategory(int id, [FromBody] TicketDto ticket)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var updatedTicket = await _ticketRepository.UpdateTicketAsync(id, ticket, User);
                if (updatedTicket == null) return NotFound();
                _logger.LogInformation($"Update ticket: {updatedTicket}");
                return Ok(updatedTicket);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [Authorize(Roles = "1,2")]
        [HttpDelete("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteTicket(int id)
        {
            try
            {
                var result = await _ticketRepository.DeleteTicketAsync(id, User);
                if (!result) return NotFound();
                _logger.LogInformation($"Delete ticket: {id}");
                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [Authorize(Roles = "1,2")]
        [HttpPut("active/{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ActivateTicket(int id)
        {
            try
            {
                var ticket = await _ticketRepository.ActivateTicketAsync(id, User);
                if (ticket == null) return NotFound();
                _logger.LogInformation($"Activate ticket: {ticket}");
                return Ok(ticket);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [Authorize(Roles = "1,2")]
        [HttpPut("unactive/{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeactivateTicket(int id)
        {
            try
            {
                var ticket = await _ticketRepository.DeactivateTicketAsync(id, User);
                if (ticket == null) return NotFound();
                _logger.LogInformation($"Deactivate ticket: {ticket}");
                return Ok(ticket);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}
