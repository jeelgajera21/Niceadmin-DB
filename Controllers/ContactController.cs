using Microsoft.AspNetCore.Mvc;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using NiceAdmin.Models;


namespace NiceAdmin.Controllers;

public class ContactController : Controller
{
    private readonly MailSettings _mailSettings;

    public ContactController(IOptions<MailSettings> mailSettings)
    {
        _mailSettings = mailSettings.Value;
    }

    public IActionResult Index(ContactModel model)
    {

        if (ModelState.IsValid)
        {
            try
            {

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail));
                message.To.Add(new MailboxAddress(model.Name, model.Email));
                message.Subject = $"Hello {model.Name}";
                message.Body = new TextPart("plain")
                {
                    Text = $"Name: {model.Name}\nEnrollment No: {model.EnrollmentNo}"
                };

                using (var client = new SmtpClient())
                {
                    client.Connect(_mailSettings.Host, _mailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                    client.Authenticate("jeelpatel7589@gmail.com", "uyzr xfdp kdkp uwdj");
                    client.Send(message);
                    client.Disconnect(true);
                }

                ViewBag.Message = "Email sent successfully!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Error sending email: " + ex.Message;
            }
        }

        return View(model);
    }

    public IActionResult CreateCookies()
    {
        string key = "Coffee_Shop_Cookies";
        string value = "Oh You accessed the coffee shop cookie.";
        CookieOptions options = new CookieOptions();
        {
            options.Expires = DateTime.Now.AddMinutes(2);
            options.Secure = true;
            HttpContext.Response.Cookies.Append(key, value, options);
        }
        return View("Cookies");
    }
    public IActionResult ReadCookies()
    {
        string cookieValue = string.Empty;

        // Check if the cookie exists and retrieve the value
        if (HttpContext.Request.Cookies.ContainsKey("Coffee_Shop_Cookies"))
        {
            cookieValue = HttpContext.Request.Cookies["Coffee_Shop_Cookies"];
        }

        // Pass the cookie value to TempData so that it can be accessed in the view
        TempData["Cookie"] = string.IsNullOrEmpty(cookieValue) ? "Cookie not found" : cookieValue;

        return View("Cookies");
    }

    public IActionResult RemoveCookies()
    {
        HttpContext.Response.Cookies.Delete("Coffee_Shop_Cookies");
        return View("Cookies");
    }
    public IActionResult Cookies()
    {
        return View();
    }


}