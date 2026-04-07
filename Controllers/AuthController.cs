using Microsoft.AspNetCore.Mvc;

namespace AIResumeAnalyzer.MVC.Controllers
{
    public class AuthController : Controller
    {
        [Route("/")]
        [Route("/login")]
        public IActionResult Login()
        {
            return View();
        }

        [Route("/register")]
        public IActionResult Register()
        {
            return View();
        }

        [Route("/forgotpassword")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [Route("/verify-code")]
        public IActionResult VerifyCode(string phone)
        {
            ViewBag.Phone = phone;
            return View();
        }

        [Route("/reset-password")]
        public IActionResult ResetPassword(string phone, string code)
        {
            ViewBag.Phone = phone;
            ViewBag.Code = code;
            return View();
        }
    }
}
