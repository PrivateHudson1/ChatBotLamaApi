using Microsoft.AspNetCore.Mvc;

namespace ChatBotLamaApi.Controllers
{
    public class WorkController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
