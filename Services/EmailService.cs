using SendGrid;
using SendGrid.Helpers.Mail;

namespace TokenManagerApi.Services;

public class EmailService
{
    private readonly Options.EmailOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;

    public EmailService(
            Microsoft.Extensions.Options.IOptions<Options.EmailOptions> options,
            IHttpClientFactory httpClientFactory)
    {
            _options = options.Value;
            _httpClientFactory = httpClientFactory;
    }

    public async Task SendRegistrationEmailAsync(string toEmail, string username)
    {
            var client = new SendGridClient(_options.ApiKey);
            var from = new EmailAddress(_options.FromEmail, _options.FromName);
            var to = new EmailAddress(toEmail, username);
            var subject = "Welcome to RateDrinks!";
            var registrationTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
            var plainTextContent = $"Hello {username}, thanks for registering at RateDrinks! Registered at: {registrationTime}";

            // Load HTML template (local file path or URL)
            string template;
            var templatePath = "Templates/RegistrationEmail.html";
            if (Uri.TryCreate(templatePath, UriKind.Absolute, out var uri) && (uri.Scheme == "http" || uri.Scheme == "https"))
            {
                    var httpClient = _httpClientFactory.CreateClient("HtmlTemplateClient");
                    template = await httpClient.GetStringAsync(templatePath);
            }
            else
            {
                    template = await File.ReadAllTextAsync(templatePath);
            }

            var htmlContent = template
                    .Replace("{{username}}", System.Net.WebUtility.HtmlEncode(username))
                    .Replace("{{email}}", System.Net.WebUtility.HtmlEncode(toEmail))
                    .Replace("{{registrationTime}}", registrationTime);

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            await client.SendEmailAsync(msg);
    }
}
