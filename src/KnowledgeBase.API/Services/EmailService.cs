using System.Net;
using System.Net.Mail;
using KnowledgeBase.API.Models.Configurations;
using Microsoft.Extensions.Options;

namespace KnowledgeBase.API.Services;

public class EmailService(IOptions<EmailSettings> emailSettings,IWebHostEnvironment environment, ILogger<EmailService> logger) : IEmailService
{
    private readonly EmailSettings _emailSettings = emailSettings.Value;

    public async Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink)
    {
        try
        {
            // 读取邮件模板
            var templatePath = Path.Combine(environment.ContentRootPath, "EmailTemplates", "PasswordResetEmail.html");
            var emailTemplate = await File.ReadAllTextAsync(templatePath);

            // 替换模板中的占位符
            var emailBody = emailTemplate
                .Replace("{{Username}}", userName)
                .Replace("{{ResetLink}}", resetLink);

            using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
            {
                EnableSsl = _emailSettings.EnableSsl,
                Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword)
            };

            var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = "重置您的密码 - AI知识库",
                IsBodyHtml = true,
                Body = emailBody
            };
            message.To.Add(toEmail);
            await client.SendMailAsync(message);

            logger.LogInformation("Password reset email sent to {ToEmail}", toEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send password reset email to {ToEmail}", toEmail);
            throw new InvalidOperationException("发送重置密码邮件失败", ex);
        }
    }
}
