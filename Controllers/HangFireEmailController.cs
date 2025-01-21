using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using NiceAdmin.Models;
using MailKit.Net.Smtp;
using System.Globalization;

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

            
            ViewBag.RemMessage = "Email reminder sent!";
            ViewBag.ShowAlert = true; // Set flag
            return View("Index");
        }

        // 🔹 Delayed Job (Runs After a Delay)
      

[HttpPost("schedule-email")]
    public IActionResult ScheduleEmailReminder(string email, string dateTime)
    {
        if (!DateTime.TryParseExact(dateTime, "yyyy-MM-ddTHH:mm",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime scheduledDateTime))
        {
            return BadRequest("Invalid date-time format. Use 'yyyy-MM-ddTHH:mm'.");
        }

        DateTime now = DateTime.Now;
        if (scheduledDateTime <= now)
        {
            return BadRequest("Scheduled time must be in the future.");
        }

        TimeSpan delay = scheduledDateTime - now;
        _backgroundJobClient.Schedule(() => SendEmail(email, $"Scheduled Email Reminder", $"Dear {email},\r\n\r\nThis is a friendly reminder for your scheduled event.\r\n\r\n📅 Date & Time: {scheduledDateTime}\r\n📌 Details: [Mention any important details related to the event or task]\r\n\r\nIf you have any questions or need to reschedule, feel free to reach out.\r\n\r\nBest regards,\r\nTo-Do\r\nMascot"), delay);

        
            ViewBag.ScheduleMessage = $"Email scheduled for {scheduledDateTime:dd-MM-yyyy HH:mm}";
            return View("Index");
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
                ViewBag.EmailMessage = "Email, Subject or Body is missing!";
                return View(); // Return to the same page without redirection
            }

            if (_mailSettings == null)
            {
                ViewBag.EmailMessage = "Mail settings are not properly configured.";
                return View(); // Return to the same page without redirection
            }

            try
            {
                var message = new MimeMessage();
                if (message == null)
                {
                    ViewBag.EmailMessage = "MimeMessage could not be created.";
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
                        ViewBag.EmailMessage = "SMTP Client could not be created.";
                        return View(); // Return to the same page without redirection
                    }

                    client.Connect(_mailSettings.Host, _mailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                    client.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                    client.Send(message);
                    client.Disconnect(true);
                }

                ViewBag.EmailMessage = "Email sent successfully!";
                return View("Index"); // Return to the same page without redirection
            }
            catch (Exception ex)
            {
                ViewBag.EmailMessage = "Error sending email: " + ex.Message;
                return View("Index"); // Return to the same page without redirection
            }
        }


        public IActionResult Index()
        {
            return View();
        }

    }
}
