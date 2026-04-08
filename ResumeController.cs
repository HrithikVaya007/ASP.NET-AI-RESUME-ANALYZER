using Microsoft.AspNetCore.Mvc;

namespace AIResumeAnalyzer.MVC.Controllers
{
    public class ResumeController : Controller
    {
        [HttpGet("resume-analyze")]
        public IActionResult Analyze()
        {
            return View();
        }
    }
}
