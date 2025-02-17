using Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Communication.Email;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using NewsAPI;
using NewsAPI.Models;

namespace AzureFunctionsDemo
{
    public class NewsEmailingFunction
    {
        //<docsnippet_fixed_delay_retry_example>
        [Function(nameof(NewsEmailingFunction))]
        [FixedDelayRetry(5, "00:00:10")]
        public static async Task Run([TimerTrigger("0 0 8 * * *" // Run every day at 8:00 AM UTC
            #if DEBUG
            , RunOnStartup=true
            #endif
            )] TimerInfo timerInfo,
            FunctionContext context)
        {
            var logger = context.GetLogger(nameof(NewsEmailingFunction));
            logger.LogInformation("C# Timer trigger function executed at: {time}", DateTime.UtcNow);

            var azureCommunicationServicesConnectionString = GetEnvironmentVariable("AZURE_COMM_CONN");
            var newsApiKey = GetEnvironmentVariable("NEWS_API_KEY");
            var emailList = GetEnvironmentVariable("TimeTriggerFunctionEmailList");
            var senderAddress = GetEnvironmentVariable("TimeTriggerFunctionSenderAddress");

            ArgumentException.ThrowIfNullOrEmpty(azureCommunicationServicesConnectionString, "AZURE_COMM_CONN");
            ArgumentException.ThrowIfNullOrEmpty(newsApiKey, "NEWS_API_KEY");
            ArgumentException.ThrowIfNullOrEmpty(senderAddress, "TimeTriggerFunctionSenderAddress");
            ArgumentException.ThrowIfNullOrEmpty(emailList, "TimeTriggerFunctionEmailList");


            var emailClient = new EmailClient(azureCommunicationServicesConnectionString);
            var newsApiClient = new NewsApiClient(newsApiKey);

            var news = await newsApiClient.GetTopHeadlinesAsync(new NewsAPI.Models.TopHeadlinesRequest
            {
                Country = NewsAPI.Constants.Countries.US,
                Category = NewsAPI.Constants.Categories.Business
            });

            if (news == null || news.Articles == null || news.Articles.Count == 0)
            {
                logger.LogError("No news found. Exiting function.");
                return;
            }

            string emailContent = GenerateEmailContent(news);

            var recipients = new EmailRecipients(emailList.Split(',').Select(email => new EmailAddress(email)).ToList());

            if(recipients.To.Count == 0)
            {
                logger.LogError("No recipients found. Exiting function.");
                return;
            }

            var emailMessage = new EmailMessage(
                senderAddress: senderAddress,
                content: new EmailContent("Daily Business News by Farman - Automated")
                {
                    PlainText = "Here is your daily business news.",
                    Html = emailContent
                },
                recipients: recipients);

            EmailSendOperation emailSendOperation = emailClient.Send(
                WaitUntil.Completed,
                emailMessage);

            logger.LogInformation("Email sent with subject: {subject}", emailMessage.Content.Subject);
            logger.LogInformation("Email send to: {emailList}", emailList);
            logger.LogInformation("News sent: {count}", news.Articles.Count);
        }

        private static string GenerateEmailContent(ArticlesResult news)
        {
            var emailContent = new StringBuilder();
            emailContent.Append("<html><body>");
            emailContent.Append("<h1>Daily Business News by Farman - Automated Azure Function</h1>");
            foreach (var article in news.Articles)
            {
                emailContent.Append($"<h2 style='margin-top:15px'>{article.Title}</h2>");
                emailContent.Append($"<img style='width:50%' src='{article.UrlToImage}'>");
                emailContent.Append($"<p>{article.Description}</p>");
                emailContent.Append($"<a href='{article.Url}'>Read more</a>");
                emailContent.Append("<hr>");
            }
            emailContent.Append("</body></html>");
            return emailContent.ToString();
        }

        static string? GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process)
                ?? Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User)
                ?? Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine);
        }
    }
}




