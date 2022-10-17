using Microsoft.AspNetCore.Mvc;

namespace ReceiptServiceDotnetCore.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
