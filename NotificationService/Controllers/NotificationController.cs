using Microsoft.AspNetCore.Mvc;

namespace BookTable.NotificationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{

    private static int _attempts = 0;

    
    [HttpPost("sendNotification")]
    public IActionResult SendNotification()
    {
        _attempts++;

        if (_attempts <= 3)
        {
            return StatusCode(503, new
            {
                message = "Notification service temporarily unavailable."
            });
        }

        return Ok(new
        {
            message = "Notification sent successfully."
        });
    }
    
}