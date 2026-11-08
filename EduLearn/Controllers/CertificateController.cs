using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Models;
using EduLearn.Models.Lms;

namespace EduLearn.Controllers
{
    // Public page employers can use to check a certificate number
    public class CertificateController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CertificateController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Verify(string? id)
        {
            Enrollment? enrollment = null;
            if (!string.IsNullOrWhiteSpace(id))
            {
                var no = id.Trim();
                enrollment = await _context.Enrollments
                    .Include(e => e.Student)
                    .Include(e => e.Course)
                    .FirstOrDefaultAsync(e => e.CertificateNo == no && e.Status == EnrollmentStatus.Completed);
            }

            ViewBag.Number = id;
            return View(enrollment);
        }
    }
}
