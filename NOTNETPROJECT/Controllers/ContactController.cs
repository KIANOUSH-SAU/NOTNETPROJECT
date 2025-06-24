using Microsoft.AspNetCore.Mvc;
using NOTNETPROJECT.Models;
using NOTNETPROJECT.Data;
using NOTNETPROJECT.Services;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NOTNETPROJECT.Controllers
{
    public class ContactController : Controller
    {

        private readonly EmailService _emailService;
        private readonly ApplicationDbContext _db;

        public ContactController(EmailService emailService, ApplicationDbContext db)
        {
            _emailService = emailService;
            _db = db;
        }

        [HttpGet]
        [Route("/legacyverify")]
        public IActionResult LegacyVerify(string code)
        {
            // Redirect to the HTTPS version
            return RedirectToAction("Verify", new { code });
        }


        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Contact(string Name, string Email, string Message)
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Message))
            {
                ViewBag.Error = "All fields are required.";
                return View();
            }

            // Generate verification code
            var code = Guid.NewGuid().ToString("N");
            var pending = new PendingContactMessage
            {
                Code = code,
                Name = Name,
                Email = Email,
                Message = Message
            };
            _db.PendingContactMessages.Add(pending);
            await _db.SaveChangesAsync();

            //string verificationUrl = Url.Action("Verify", "Contact", new { code }, Request.Scheme);
            string verificationUrl = Url.Action("Verify", "Contact", new { code }, "https");
            // Send verification email to user
            await _emailService.SendVerificationEmailAsync(Name, Email, verificationUrl);

            ViewBag.Success = "A verification link has been sent to your email. Please check your inbox to complete the process.";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Verify(string code)
        {
            var pending = await _db.PendingContactMessages.FirstOrDefaultAsync(x => x.Code == code);
            if (pending != null)
            {
                await _emailService.SendEmailAsync(pending.Name, pending.Email, pending.Message);
                _db.PendingContactMessages.Remove(pending);
                await _db.SaveChangesAsync();
                return View("Verified");
            }
            return View("InvalidOrExpired");
        }


    }
}
