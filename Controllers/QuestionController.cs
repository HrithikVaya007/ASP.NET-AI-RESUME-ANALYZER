using Microsoft.AspNetCore.Mvc;

namespace AIResumeAnalyzer.MVC.Controllers
{
    public class QuestionController : Controller
    {
        [HttpGet("question-gen")]
        public IActionResult Generate()
        {
            return View();
        }
    }
}
