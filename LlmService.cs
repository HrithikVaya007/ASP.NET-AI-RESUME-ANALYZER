using System.Text;
using System.Text.Json;
using AIResumeAnalyzer.MVC.Config;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace AIResumeAnalyzer.MVC.Services
{
    public class LlmService
    {
        private readonly HttpClient _httpClient;
        private readonly LlmSettings _settings;
        private readonly ILogger<LlmService> _logger;

        public LlmService(HttpClient httpClient, IOptions<LlmSettings> settings, ILogger<LlmService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<string> GenerateContentAsync(string prompt)
        {
            if (_settings.Provider == "OpenAI")
            {
                return await GenerateOpenAiContentAsync(prompt);
            }
            if (_settings.Provider == "Mistral")
            {
                return await GenerateMistralContentAsync(prompt);
            }

            return await GenerateGeminiContentAsync(prompt);
        }

        private async Task<string> GenerateMistralContentAsync(string prompt)
        {
            if (string.IsNullOrEmpty(_settings.MistralApiKey))
            {
                _logger.LogWarning("Mistral API Key is missing. Falling back to mock.");
                return MockLlmResponse(prompt);
            }

            var url = "https://api.mistral.ai/v1/chat/completions";
            var requestBody = new
            {
                model = _settings.MistralModel ?? "mistral-small-latest",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            int maxRetries = 3;
            int delayMs = 1500;

            for (int i = 0; i < maxRetries; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Authorization", $"Bearer {_settings.MistralApiKey}");
                request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseString);
                    return doc.RootElement
                              .GetProperty("choices")[0]
                              .GetProperty("message")
                              .GetProperty("content").GetString() ?? "";
                }

                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Mistral API Error ({StatusCode}): {ErrorBody}", response.StatusCode, errorBody);

                if ((response.StatusCode == System.Net.HttpStatusCode.TooManyRequests || (int)response.StatusCode >= 500) && i < maxRetries - 1)
                {
                    await Task.Delay(delayMs * (i + 1));
                    continue;
                }

                throw new Exception($"Mistral API Error ({response.StatusCode}): {errorBody}");
            }

            throw new Exception("Mistral Generation failed after multiple retries.");
        }

        private async Task<string> GenerateOpenAiContentAsync(string prompt)
        {
            if (string.IsNullOrEmpty(_settings.OpenAiApiKey) || _settings.OpenAiApiKey.StartsWith("sk-..."))
            {
                _logger.LogWarning("OpenAI API Key is missing or invalid. Falling back to mock.");
                return MockLlmResponse(prompt);
            }

            var url = "https://api.openai.com/v1/chat/completions";
            var requestBody = new
            {
                model = _settings.OpenAiModel ?? "gpt-4o",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.7
            };

            int maxRetries = 3;
            int delayMs = 2000;

            for (int i = 0; i < maxRetries; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Authorization", $"Bearer {_settings.OpenAiApiKey}");
                request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseString);
                    return doc.RootElement
                              .GetProperty("choices")[0]
                              .GetProperty("message")
                              .GetProperty("content").GetString() ?? "";
                }

                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("OpenAI API Error ({StatusCode}): {ErrorBody}", response.StatusCode, errorBody);

                if ((response.StatusCode == System.Net.HttpStatusCode.TooManyRequests || (int)response.StatusCode >= 500) && i < maxRetries - 1)
                {
                    await Task.Delay(delayMs * (i + 1));
                    continue;
                }

                throw new Exception($"OpenAI API Error ({response.StatusCode}): {errorBody}");
            }

            throw new Exception("OpenAI Generation failed after multiple retries.");
        }

        private async Task<string> GenerateGeminiContentAsync(string prompt)
        {
            if (string.IsNullOrEmpty(_settings.GeminiApiKey) || _settings.GeminiApiKey == "YOUR_GEMINI_API_KEY")
            {
                return MockLlmResponse(prompt);
            }

            // Reverting to v1beta for better model compatibility (v1 was 404ing)
            var modelId = _settings.ModelName ?? "gemini-2.0-flash";
            if (!modelId.StartsWith("models/"))
            {
                modelId = "models/" + modelId;
            }

            var url = $"https://generativelanguage.googleapis.com/v1beta/{modelId}:generateContent?key={_settings.GeminiApiKey}";
            
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            int maxRetries = 3;
            int delayMs = 1500;

            for (int i = 0; i < maxRetries; i++)
            {
                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseString);
                    
                    return doc.RootElement
                              .GetProperty("candidates")[0]
                              .GetProperty("content")
                              .GetProperty("parts")[0]
                              .GetProperty("text").GetString() ?? "";
                }

                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini API Error ({StatusCode}): {ErrorBody}", response.StatusCode, errorBody);

                if ((response.StatusCode == System.Net.HttpStatusCode.TooManyRequests || (int)response.StatusCode >= 500) && i < maxRetries - 1)
                {
                    _logger.LogWarning("Retrying Gemini API call (Attempt {Attempt}/{MaxRetries})", i + 1, maxRetries);
                    await Task.Delay(delayMs * (i + 1));
                    continue;
                }

                throw new Exception($"Gemini API Error ({response.StatusCode}): {errorBody}");
            }

            throw new Exception("Gemini Generation failed after multiple retries.");
        }

        public string CleanJsonResponse(string rawResponse)
        {
            if (string.IsNullOrWhiteSpace(rawResponse)) return "{}";

            // Remove markdown code blocks if present
            var cleaned = rawResponse.Replace("```json", "").Replace("```", "").Trim();

            // Find first and last structural characters
            var firstBrace = cleaned.IndexOf('{');
            var firstBracket = cleaned.IndexOf('[');
            var lastBrace = cleaned.LastIndexOf('}');
            var lastBracket = cleaned.LastIndexOf(']');

            int start = -1;
            int end = -1;

            if (firstBrace != -1 && (firstBracket == -1 || firstBrace < firstBracket))
            {
                start = firstBrace;
                end = lastBrace;
            }
            else if (firstBracket != -1)
            {
                start = firstBracket;
                end = lastBracket;
            }

            if (start != -1 && end != -1 && end > start)
            {
                return cleaned.Substring(start, end - start + 1);
            }

            return cleaned;
        }

        private string MockLlmResponse(string prompt)
        {
            if (prompt.Contains("Analyze this resume"))
            {
                return "{\"IsValidResume\": true, \"AtsScore\": 85, \"SkillMatchPercentage\": 78, \"MissingSkills\": [\"Docker\", \"Kubernetes\", \"CI/CD\"], \"Suggestions\": [\"Quantify achievements\", \"Add a professional summary\"], \"DetectedRole\": \"Full Stack Developer\"}";
            }
            if (prompt.Contains("Generate exactly 50 interview questions"))
            {
                var qs = new List<string>();
                for (int i = 1; i <= 50; i++) qs.Add($"Mock Question {i}: Can you walk me through your experience with complex systems?");
                return JsonSerializer.Serialize(qs);
            }
            if (prompt.Contains("Analyze their suitability") && prompt.Contains("resumeA"))
            {
                return "{\"resumeA\": {\"name\": \"Candidate A\", \"atsScore\": 82, \"strengths\": [\"Deep .NET knowledge\"], \"verdict\": \"Strong overall candidate\"}, \"resumeB\": {\"name\": \"Candidate B\", \"atsScore\": 75, \"strengths\": [\"Good frontend skills\"], \"verdict\": \"Better suited for junior role\"}}";
            }
            if (prompt.Contains("next interview question"))
            {
                return "Next Mock Question: How do you handle conflict in a team setting?";
            }
            if (prompt.Contains("Evaluate the following interview answers"))
            {
                return "{\"FinalScore\": 88, \"Strengths\": [\"Clear communication\", \"Technical depth\"], \"Weaknesses\": [\"Conciseness\"], \"Suggestions\": [\"Try to give more direct answers.\"]}";
            }

            return "{\"message\": \"Mock response generated successfully. Please configure a valid Gemini or OpenAI API key.\"}";
        }
    }
}
