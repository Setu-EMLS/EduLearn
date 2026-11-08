using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Models;

namespace EduLearn.Controllers
{
    public class ContactUsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactUsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Payment accounts are listed on the contact page so people can pay before enrolling
            ViewData["Gateways"] = await _context.PaymentGetways
                .Include(g => g.GetwayInfo)
                .OrderBy(g => g.Name)
                .ToListAsync();
            return View();
        }
    }
}
