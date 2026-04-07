using System.Security.Claims;
using AIResumeAnalyzer.MVC.Models;
using AIResumeAnalyzer.MVC.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIResumeAnalyzer.MVC.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserRepository _userRepository;
        private readonly ResumeRepository _resumeRepository;
        private readonly InterviewSessionRepository _interviewRepository;

        public DashboardController(
            UserRepository userRepository, 
            ResumeRepository resumeRepository, 
            InterviewSessionRepository interviewRepository)
        {
            _userRepository = userRepository;
            _resumeRepository = resumeRepository;
            _interviewRepository = interviewRepository;
        }

        /// <summary>
        /// Helper: reads the logged-in user's ID from JWT claims and fetches from MongoDB.
        /// </summary>
        private async Task<User?> GetCurrentUserAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return null;

            return await _userRepository.GetUserByIdAsync(userId);
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Redirect("/login");

            var latestResume = await _resumeRepository.GetLatestResumeByUserIdAsync(user.Id!);
            var latestSession = await _interviewRepository.GetLatestSessionByUserIdAsync(user.Id!);
            var totalSessions = await _interviewRepository.GetTotalSessionsCountByUserIdAsync(user.Id!);

            var viewModel = new AIResumeAnalyzer.MVC.Models.ViewModels.DashboardHomeViewModel
            {
                User = user,
                LatestAtsScore = latestResume?.AtsScore ?? 0,
                LatestSkillMatch = latestResume?.SkillMatchPercentage ?? 0,
                LatestReadinessScore = latestSession?.FinalScore ?? 0,
                TotalSessions = (int)totalSessions
            };

            return View(viewModel);
        }

        [HttpGet("dashboard/history")]
        public async Task<IActionResult> History()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Redirect("/login");
            return View(user);
        }

        [HttpGet("dashboard/compare")]
        public async Task<IActionResult> Compare()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Redirect("/login");
            return View(user);
        }

        [HttpGet("dashboard/settings")]
        public async Task<IActionResult> Settings()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Redirect("/login");
            return View(user);
        }

        [HttpGet("dashboard/profile")]
        public async Task<IActionResult> Profile()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Redirect("/login");
            return View(user);
        }
    }
}
