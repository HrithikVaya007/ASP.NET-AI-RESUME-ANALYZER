using Microsoft.Extensions.Logging;

namespace AIResumeAnalyzer.MVC.Services
{
    public class SmsService
    {
        private readonly ILogger<SmsService> _logger;

        public SmsService(ILogger<SmsService> logger)
        {
            _logger = logger;
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            // For development: Log to Console/Logger
            // In Production: Use Twilio, AWS SNS, etc.
            _logger.LogInformation("\n\n----------------- [SMS MOCK] -----------------");
            _logger.LogInformation("TO: {PhoneNumber}", phoneNumber);
            _logger.LogInformation("MESSAGE: {Message}", message);
            _logger.LogInformation("---------------------------------------------\n");

            await Task.CompletedTask;
        }
    }
}
