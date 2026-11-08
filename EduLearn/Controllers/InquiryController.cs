using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Models;

namespace EduLearn.Controllers
{
    // Receives the public website forms (contact, free seminar, newsletter) and stores them for the admin
    public class InquiryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InquiryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Contact(string? name, string? email, string? phone, string? subject, string? message, string? website, string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(message) || !IsEmail(email))
            {
                TempData["Error"] = "Please enter your name, a valid email and your message.";
                return Back(returnUrl, "/ContactUs");
            }

            var type = subject?.Contains("feedback", StringComparison.OrdinalIgnoreCase) == true ? InquiryType.Feedback : InquiryType.Contact;
            await SaveAsync(website, new Inquiry { Type = type, Name = name, Email = email!, Phone = phone, Subject = subject, Message = message });
            TempData["Success"] = "Thank you! Your message has been sent. We'll get back to you shortly.";
            return Back(returnUrl, "/ContactUs");
        }

        [HttpPost]
        public async Task<IActionResult> Seminar(string? name, string? email, string? phone, string? course, string? website, string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone) || !IsEmail(email))
            {
                TempData["Error"] = "Please enter your name, phone number and a valid email to reserve a seat.";
                return Back(returnUrl, "/");
            }

            await SaveAsync(website, new Inquiry { Type = InquiryType.Seminar, Name = name, Email = email!, Phone = phone, Course = course, Subject = "Free seminar registration" });
            TempData["Success"] = "Your seat is reserved! We'll call you with the seminar date and time.";
            return Back(returnUrl, "/");
        }

        [HttpPost]
        public async Task<IActionResult> Subscribe(string? email, string? website, string? returnUrl)
        {
            if (!IsEmail(email))
            {
                TempData["Error"] = "Please enter a valid email address.";
                return Back(returnUrl, "/");
            }

            var normalized = email!.Trim();
            var exists = await _context.Inquiries.AnyAsync(i => i.Type == InquiryType.Newsletter && i.Email == normalized);
            if (!exists)
                await SaveAsync(website, new Inquiry { Type = InquiryType.Newsletter, Email = normalized, Subject = "Newsletter subscription" });

            TempData["Success"] = "You're subscribed! Watch your inbox for new courses and seminars.";
            return Back(returnUrl, "/");
        }

        private async Task SaveAsync(string? honeypot, Inquiry inquiry)
        {
            // Bots fill the hidden "website" field; pretend success without storing anything
            if (!string.IsNullOrEmpty(honeypot)) return;

            inquiry.Name = Clip(inquiry.Name, 150);
            inquiry.Email = Clip(inquiry.Email, 200)!;
            inquiry.Phone = Clip(inquiry.Phone, 30);
            inquiry.Subject = Clip(inquiry.Subject, 200);
            inquiry.Course = Clip(inquiry.Course, 200);
            inquiry.Message = Clip(inquiry.Message, 4000);
            inquiry.CreatedAt = DateTime.Now;
            _context.Inquiries.Add(inquiry);
            await _context.SaveChangesAsync();
        }

        private IActionResult Back(string? returnUrl, string fallback) =>
            LocalRedirect(!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : fallback);

        private static bool IsEmail(string? email) =>
            !string.IsNullOrWhiteSpace(email) && new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email.Trim());

        private static string? Clip(string? value, int max)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            value = value.Trim();
            return value.Length <= max ? value : value[..max];
        }
    }
}
