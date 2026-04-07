using System.Text.Json;
using AIResumeAnalyzer.MVC.DTOs;
using AIResumeAnalyzer.MVC.Models;
using AIResumeAnalyzer.MVC.Repositories;
using Microsoft.AspNetCore.Http;

namespace AIResumeAnalyzer.MVC.Services
{
    public class ResumeService
    {
        private readonly ResumeRepository _resumeRepository;
        private readonly PdfService _pdfService;
        private readonly LlmService _llmService;

        public ResumeService(ResumeRepository resumeRepository, PdfService pdfService, LlmService llmService)
        {
            _resumeRepository = resumeRepository;
            _pdfService = pdfService;
            _llmService = llmService;
        }

        public async Task<ResumeAnalysisResponse> ProcessResumeAsync(IFormFile file, string userId, string jobDescription)
        {
            // 1. Extract Text
            var extractedText = await _pdfService.ExtractTextFromPdfAsync(file);

            if (string.IsNullOrWhiteSpace(extractedText))
            {
                throw new Exception("The uploaded document is empty or unreadable. Please ensure it is a text-based PDF.");
            }

            // 2. Call LLM for Analysis
            var prompt = $"Analyze this resume against the Job Description. Be lenient: if the text represents a candidate's background or profile, treat it as a resume. Output ONLY plain JSON: IsValidResume(true), AtsScore(0-100), SkillMatchPercentage(0-100), MissingSkills(array), Suggestions(array), DetectedRole(string). \n\nJob Description: {jobDescription} \n\nResume Text: {extractedText}";
            var llmResponseRaw = await _llmService.GenerateContentAsync(prompt);
            
            var cleanedJson = _llmService.CleanJsonResponse(llmResponseRaw);
            var analysisDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(cleanedJson);

            // We'll proceed even if IsValidResume is false, unless empty, to provide the best possible feedback.
            bool isValid = analysisDict != null && analysisDict.ContainsKey("IsValidResume") && analysisDict["IsValidResume"].GetBoolean();
            if (!isValid && string.IsNullOrWhiteSpace(extractedText))
            {
                throw new Exception("The document appears to be empty or completely unreadable.");
            }

            var resume = new Resume
            {
                UserId = userId,
                OriginalFileName = file.FileName,
                ExtractedText = extractedText,
                JobDescription = jobDescription,
                AtsScore = analysisDict != null && analysisDict.ContainsKey("AtsScore") ? analysisDict["AtsScore"].GetInt32() : 0,
                SkillMatchPercentage = analysisDict != null && analysisDict.ContainsKey("SkillMatchPercentage") ? analysisDict["SkillMatchPercentage"].GetInt32() : 0,
                MissingSkills = analysisDict != null && analysisDict.ContainsKey("MissingSkills") ? analysisDict["MissingSkills"].EnumerateArray().Select(x => x.GetString() ?? "").ToList() : new List<string>(),
                Suggestions = analysisDict != null && analysisDict.ContainsKey("Suggestions") ? analysisDict["Suggestions"].EnumerateArray().Select(x => x.GetString() ?? "").ToList() : new List<string>(),
                DetectedRole = analysisDict != null && analysisDict.ContainsKey("DetectedRole") ? analysisDict["DetectedRole"].GetString() ?? "Unknown" : "Unknown",
                UploadedAt = DateTime.UtcNow
            };

            await _resumeRepository.CreateResumeAsync(resume);

            return new ResumeAnalysisResponse
            {
                Id = resume.Id ?? "",
                AtsScore = resume.AtsScore,
                SkillMatchPercentage = resume.SkillMatchPercentage,
                MissingSkills = resume.MissingSkills,
                Suggestions = resume.Suggestions,
                DetectedRole = resume.DetectedRole
            };
        }

        public async Task<string> GenerateInterviewQuestionsPdfAsync(string resumeId)
        {
            var resume = await _resumeRepository.GetResumeByIdAsync(resumeId) ?? throw new Exception("Resume not found");

            var prompt = $"Generate exactly 50 interview questions as a JSON array of strings for a candidate with the following resume. Role: {resume.DetectedRole}, text: {resume.ExtractedText}. Return ONLY plain JSON array.";
            var llmResponseRaw = await _llmService.GenerateContentAsync(prompt);
            var cleanedJson = _llmService.CleanJsonResponse(llmResponseRaw);

            var questions = JsonSerializer.Deserialize<List<string>>(cleanedJson) ?? new List<string>();

            return _pdfService.GenerateQuestionsPdf(resume.DetectedRole, questions);
        }

        public async Task<string> GenerateAnalysisReportAsync(string resumeId)
        {
            var resume = await _resumeRepository.GetResumeByIdAsync(resumeId) ?? throw new Exception("Resume not found");
            return _pdfService.GenerateAnalysisPdf(resume);
        }

        public async Task<object> CompareResumesAsync(IFormFile fileA, IFormFile fileB, string jobDescription)
        {
            var textA = await _pdfService.ExtractTextFromPdfAsync(fileA);
            var textB = await _pdfService.ExtractTextFromPdfAsync(fileB);

            var prompt = $@"Compare these two resumes against the following Job Description. 
            Resumes:
            A: {textA}
            B: {textB}
            
            Job Description: {jobDescription}

            Analyze their suitability, calculate an ATS score for each, and decide which one is better.
            Output plain JSON exactly matching this structure:
            {{
                ""resumeA"": {{ ""name"": ""{fileA.FileName}"", ""atsScore"": 0, ""strengths"": [""str1""], ""verdict"": ""..."" }},
                ""resumeB"": {{ ""name"": ""{fileB.FileName}"", ""atsScore"": 0, ""strengths"": [""str1""], ""verdict"": ""..."" }}
            }}";

            var raw = await _llmService.GenerateContentAsync(prompt);
            var cleanedJson = _llmService.CleanJsonResponse(raw);

            return JsonSerializer.Deserialize<object>(cleanedJson) ?? new { };
        }
    }
}
