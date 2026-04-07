using Microsoft.AspNetCore.Mvc;

namespace AIResumeAnalyzer.MVC.Controllers
{
    public class SimulatorController : Controller
    {
        [HttpGet("ai-simulator")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
