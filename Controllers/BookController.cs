using Microsoft.AspNetCore.Mvc;

namespace BookTable.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BookController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }
    }
}
