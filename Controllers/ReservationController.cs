using BookTable.Clients;
using BookTable.Dtos;
using BookTable.Patterns.CircuitBreaker.Exceptions;
using BookTable.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookTable.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly NotificationClient _notificationClient;

        public ReservationController(IBookService bookService, NotificationClient notificationClient)
        {
            _bookService = bookService;
            _notificationClient = notificationClient;
        }

        // Get all reservations and tables (Overview)
        [HttpGet]
        public async Task<ActionResult<ReservationsAndTablesResponse>> GetReservationsAndTables()
        {
            try
            {
                var overview = await _bookService.GetReservationsAndTablesAsync();
                return Ok(overview);
            }
            catch (CircuitBreakerOpenException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Service temporarily unavailable (Circuit Breaker open).", details = ex.Message });
            }
        }

        // Reserve a table
        [HttpPost]
        public async Task<ActionResult<ReservationResponse>> ReserveTable([FromBody] CreateReservationRequest request)
        {
            try
            {
                var reservation = await _bookService.BookTableAsync(request);

                await _notificationClient.SendNotificationAsync();
                
                return CreatedAtAction(nameof(GetReservationById), new { id = reservation.Id }, reservation);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (CircuitBreakerOpenException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Service temporarily unavailable (Circuit Breaker open).", details = ex.Message });
            }
        }

        // Cancel a reservation
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> CancelReservation(int id)
        {
            try
            {
                var success = await _bookService.CancelReservationAsync(id);
                if (!success)
                {
                    return NotFound(new { message = $"Reservation with ID {id} not found." });
                }
                
                await _notificationClient.SendNotificationAsync();
                return NoContent();
            }
            catch (CircuitBreakerOpenException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Service temporarily unavailable (Circuit Breaker open).", details = ex.Message });
            }
        }

        // View reservation info
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ReservationResponse>> GetReservationById(int id)
        {
            try
            {
                var reservation = await _bookService.GetReservationByIdAsync(id);
                if (reservation == null)
                {
                    return NotFound(new { message = $"Reservation with ID {id} not found." });
                }

                return Ok(reservation);
            }
            catch (CircuitBreakerOpenException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Service temporarily unavailable (Circuit Breaker open).", details = ex.Message });
            }
        }
    }
}
