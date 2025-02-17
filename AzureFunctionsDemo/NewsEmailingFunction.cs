using Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Communication.Email;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using AzureFunctionsDemo.NewsApi;

namespace AzureFunctionsDemo
{
    public class NewsEmailingFunction
    {
        //<docsnippet_fixed_delay_retry_example>
        [Function(nameof(NewsEmailingFunction))]
        [FixedDelayRetry(5, "00:00:10")]
        public static async Task Run([TimerTrigger("0 0 8 * * *"
            #if DEBUG
            , RunOnStartup=true
            #endif
            )] TimerInfo timerInfo,
            FunctionContext context)
        {
            var logger = context.GetLogger(nameof(NewsEmailingFunction));
            var _newsApiClient = context.InstanceServices.GetService<NewsApiClient>();
            logger.LogInformation("C# Timer trigger function executed at: {next}", timerInfo.ScheduleStatus.Next);
            string? connectionString = GetEnvironmentVariable("AZURE_COMM_CONN");

            ArgumentException.ThrowIfNullOrEmpty(connectionString, "AZURE_COMM_CONN");

            var emailClient = new EmailClient(connectionString);

            var newsApiKey = GetEnvironmentVariable("NEWS_API_KEY");

            NewsApiResponse news = await _newsApiClient.GetTopHeadlinesAsync(newsApiKey, "business", "us");

            var emailContent = new StringBuilder();
            emailContent.Append("<html><body>");
            emailContent.Append("<h1>Daily Business News</h1>");
            foreach (var article in news.Articles)
            {
                emailContent.Append($"<h2 style='margin-top:15px'>{article.Title}</h2>");
                emailContent.Append($"<img style='width:50%' src='{article.UrlToImage}'>");
                emailContent.Append($"<p>{article.Description}</p>");
                emailContent.Append($"<a href='{article.Url}'>Read more</a>");
                emailContent.Append("<hr>");
            }
            emailContent.Append("</body></html>");

            var emailList = GetEnvironmentVariable("TimeTriggerFunctionEmailList");

            var senderAddress = GetEnvironmentVariable("TimeTriggerFunctionSenderAddress");

            ArgumentException.ThrowIfNullOrEmpty(senderAddress, "TimeTriggerFunctionSenderAddress");
            ArgumentException.ThrowIfNullOrEmpty(emailList, "TimeTriggerFunctionEmailList");

            var recipients = new EmailRecipients(emailList.Split(',').Select(email => new EmailAddress(email)).ToList());

            var emailMessage = new EmailMessage(
                senderAddress: senderAddress,
                content: new EmailContent("Daily Business News by Farman")
                {
                    PlainText = "Here is your daily business news.",
                    Html = emailContent.ToString()
                },
                recipients: recipients);

            EmailSendOperation emailSendOperation = emailClient.Send(
                WaitUntil.Completed,
                emailMessage);

            logger.LogInformation("Email sent with subject: {subject}", emailMessage.Content.Subject);
            logger.LogInformation("Email send to: {emailList}", emailList);

            static string? GetEnvironmentVariable(string name)
            {
                return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process)
                    ?? Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User)
                    ?? Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine);
            }
        }
    }
}




