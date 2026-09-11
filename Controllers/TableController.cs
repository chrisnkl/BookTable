using BookTable.Clients;
using BookTable.Dtos;
using BookTable.Patterns.CircuitBreaker;
using BookTable.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookTable.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TableController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly NotificationClient _notificationClient;

        public TableController(IBookService bookService, NotificationClient notificationClient)
        {
            _bookService = bookService;
            _notificationClient = notificationClient;
        }

        // 1) Get all tables
        [HttpGet]
        public async Task<ActionResult<List<TableResponse>>> GetAllTables()
        {
            try
            {
                await _notificationClient.SendNotificationAsync();
                
                var tables = await _bookService.GetAllTablesAsync();
                return Ok(tables);
            }
            catch (CircuitBreakerOpenException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Service temporarily unavailable (Circuit Breaker open).", details = ex.Message });
            }
        }

        // 2) Create table
        [HttpPost]
        public async Task<ActionResult<TableResponse>> CreateTable([FromBody] CreateTableRequest request)
        {
            try
            {
                var table = await _bookService.CreateTableAsync(request);
                return CreatedAtAction(nameof(GetTableById), new { id = table.Id }, table);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (CircuitBreakerOpenException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Service temporarily unavailable (Circuit Breaker open).", details = ex.Message });
            }
        }

        // 3) Delete table
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTable(int id)
        {
            try
            {
                var success = await _bookService.DeleteTableAsync(id);
                if (!success)
                {
                    return NotFound(new { message = $"Table with ID {id} not found." });
                }

                return NoContent();
            }
            catch (CircuitBreakerOpenException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Service temporarily unavailable (Circuit Breaker open).", details = ex.Message });
            }
        }

        // 4) View table info
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TableResponse>> GetTableById(int id)
        {
            try
            {
                var table = await _bookService.GetTableByIdAsync(id);
                if (table == null)
                {
                    return NotFound(new { message = $"Table with ID {id} not found." });
                }

                return Ok(table);
            }
            catch (CircuitBreakerOpenException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Service temporarily unavailable (Circuit Breaker open).", details = ex.Message });
            }
        }
    }
}
