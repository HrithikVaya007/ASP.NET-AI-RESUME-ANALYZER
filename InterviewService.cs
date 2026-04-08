using System.Text.Json;
using AIResumeAnalyzer.MVC.DTOs;
using AIResumeAnalyzer.MVC.Models;
using AIResumeAnalyzer.MVC.Repositories;

namespace AIResumeAnalyzer.MVC.Services
{
    public class InterviewService
    {
        private readonly InterviewSessionRepository _sessionRepository;
        private readonly ResumeRepository _resumeRepository;
        private readonly LlmService _llmService;

        public InterviewService(InterviewSessionRepository sessionRepository, ResumeRepository resumeRepository, LlmService llmService)
        {
            _sessionRepository = sessionRepository;
            _resumeRepository = resumeRepository;
            _llmService = llmService;
        }

        public async Task<InterviewSession> StartSessionAsync(string resumeId, string userId)
        {
            var resume = await _resumeRepository.GetResumeByIdAsync(resumeId) ?? throw new Exception("Resume not found");

            var session = new InterviewSession
            {
                UserId = userId,
                ResumeId = resumeId,
                DetectedRole = resume.DetectedRole,
                StartedAt = DateTime.UtcNow
            };

            await _sessionRepository.CreateSessionAsync(session);
            return session;
        }

        public async Task<NextQuestionResponse> GetNextQuestionAsync(string sessionId, string previousAnswer)
        {
            var session = await _sessionRepository.GetSessionByIdAsync(sessionId) ?? throw new Exception("Session not found");
            
            if (!string.IsNullOrEmpty(previousAnswer) && session.QnAs.Any())
            {
                session.QnAs.Last().Answer = previousAnswer;
            }

            var previousQnaContext = string.Join("\n", session.QnAs.Select(q => $"Q: {q.Question}\nA: {q.Answer}"));

            var prompt = $"Generate the next interview question for a candidate applying for {session.DetectedRole}. Previous Q&A context:\n{previousQnaContext}\nReturn just the string of the next question. Make it concise.";
            var nextQuestionText = await _llmService.GenerateContentAsync(prompt);

            var newQna = new InterviewQnA { Question = nextQuestionText };
            session.QnAs.Add(newQna);
            await _sessionRepository.UpdateSessionAsync(sessionId, session);

            return new NextQuestionResponse { NextQuestion = nextQuestionText, IsComplete = session.QnAs.Count >= 5 }; // End after 5 dynamically
        }

        public async Task<InterviewFeedbackResponse> CompleteSessionAsync(string sessionId)
        {
            var session = await _sessionRepository.GetSessionByIdAsync(sessionId) ?? throw new Exception("Session not found");

            var qnaContext = string.Join("\n", session.QnAs.Select(q => $"Question: {q.Question}\nAnswer: {q.Answer}"));

            var prompt = $"Evaluate the following interview answers. Return JSON matching: FinalScore (0-100), Strengths (array of strings), Weaknesses (array of strings), Suggestions (array of strings).\n\nQ&A:\n{qnaContext}";
            var llmResponseRaw = await _llmService.GenerateContentAsync(prompt);
            var cleanedJson = _llmService.CleanJsonResponse(llmResponseRaw);

            var evalDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(cleanedJson);

            session.FinalScore = evalDict != null && evalDict.ContainsKey("FinalScore") ? evalDict["FinalScore"].GetInt32() : 0;
            session.Strengths = evalDict != null && evalDict.ContainsKey("Strengths") ? evalDict["Strengths"].EnumerateArray().Select(x => x.GetString() ?? "").ToList() : new List<string>();
            session.Weaknesses = evalDict != null && evalDict.ContainsKey("Weaknesses") ? evalDict["Weaknesses"].EnumerateArray().Select(x => x.GetString() ?? "").ToList() : new List<string>();
            session.Suggestions = evalDict != null && evalDict.ContainsKey("Suggestions") ? evalDict["Suggestions"].EnumerateArray().Select(x => x.GetString() ?? "").ToList() : new List<string>();
            session.CompletedAt = DateTime.UtcNow;

            await _sessionRepository.UpdateSessionAsync(sessionId, session);

            return new InterviewFeedbackResponse
            {
                FinalScore = session.FinalScore,
                Strengths = session.Strengths,
                Weaknesses = session.Weaknesses,
                Suggestions = session.Suggestions
            };
        }
    }
}
