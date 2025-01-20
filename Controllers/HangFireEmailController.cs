using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using NiceAdmin.Models;
using MailKit.Net.Smtp;


namespace NiceAdmin.Controllers
{
    public class HangFireEmailController : Controller
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly MailSettings _mailSettings;

       /* HangFireEmailController(IOptions<MailSettings> mailSettings)
        {
            *//*if (mailSettings == null || mailSettings.Value == null)
            {
                throw new ArgumentNullException(nameof(mailSettings), "MailSettings is not configured properly.");
            }
            _mailSettings = mailSettings.Value;

            // Log the values to confirm they are being injected
            Console.WriteLine($"MailSettings - Mail: {_mailSettings.Mail}, Host: {_mailSettings.Host}, Port: {_mailSettings.Port}");*//*
        }*/



        public HangFireEmailController(IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager, IOptions<MailSettings> mailSettings)
        {
            if (backgroundJobClient == null)
            {
                throw new ArgumentNullException(nameof(backgroundJobClient), "IBackgroundJobClient was not provided.");
            }
            _backgroundJobClient = backgroundJobClient;
            _recurringJobManager = recurringJobManager;

            if (mailSettings == null || mailSettings.Value == null)
            {
                throw new ArgumentNullException(nameof(mailSettings), "MailSettings is not configured properly.");
            }
            _mailSettings = mailSettings.Value;

            // Log the values to confirm they are being injected
            Console.WriteLine($"MailSettings - Mail: {_mailSettings.Mail}, Host: {_mailSettings.Host}, Port: {_mailSettings.Port}");
        }


        // 🔹 Fire-and-Forget Job (Runs Once Immediately)
        [HttpPost("send-email")]
        public IActionResult SendEmailReminder(string email)
        {
            _backgroundJobClient.Enqueue(() => SendEmail(email, "Reminder Email", "This is your reminder!"));

            return Ok("Email reminder scheduled!");
        }

        // 🔹 Delayed Job (Runs After a Delay)
        [HttpPost("schedule-email")]
        public IActionResult ScheduleEmailReminder(string email, int minutes)
        {
            _backgroundJobClient.Schedule(() => SendEmail(email, "Scheduled Reminder", "Your scheduled reminder!"), TimeSpan.FromMinutes(minutes));

            return Ok($"Email scheduled to be sent in {minutes} minutes!");
        }

        // 🔹 Recurring Job (Runs Repeatedly at a Specific Time)
        [HttpPost("set-daily-reminder")]
        public IActionResult SetDailyReminder(string email)
        {
            _recurringJobManager.AddOrUpdate(
                email,
                () => SendEmail(email, "Daily Reminder", "This is your daily reminder!"),
                Cron.Daily(9)  // Runs daily at 9 AM
            );

            return Ok("Daily email reminder set at 9 AM!");
        }

        // 🔹 Email Sending Logic (Mock)
        [HttpPost]
        public IActionResult SendEmail(string email, string subject, string body)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(body))
            {
                ViewBag.Message = "Email, Subject or Body is missing!";
                return View(); // Return to the same page without redirection
            }

            if (_mailSettings == null)
            {
                ViewBag.Message = "Mail settings are not properly configured.";
                return View(); // Return to the same page without redirection
            }

            try
            {
                var message = new MimeMessage();
                if (message == null)
                {
                    ViewBag.Message = "MimeMessage could not be created.";
                    return View(); // Return to the same page without redirection
                }

                message.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail));
                message.To.Add(new MailboxAddress(email, email));
                message.Subject = $"Hello {email}";
                message.Body = new TextPart("plain")
                {
                    Text = $"Email Sent to: {email}\nSubject: {subject}\nBody: {body}"
                };

                using (var client = new SmtpClient())
                {
                    if (client == null)
                    {
                        ViewBag.Message = "SMTP Client could not be created.";
                        return View(); // Return to the same page without redirection
                    }

                    client.Connect(_mailSettings.Host, _mailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                    client.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                    client.Send(message);
                    client.Disconnect(true);
                }

                ViewBag.Message = "Email sent successfully!";
                return View("Index"); // Return to the same page without redirection
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Error sending email: " + ex.Message;
                return View("Index"); // Return to the same page without redirection
            }
        }


        public IActionResult Index()
        {
            return View();
        }

    }
}
