using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Areas.Admin.Models;
using EduLearn.Models;

namespace EduLearn.Areas.Admin.Controllers
{
	// Manual payment channels (bKash, Nagad, bank...) shown to students on the enroll page
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class PaymentMethodController : Controller
	{
		private readonly ApplicationDbContext _context;

		public PaymentMethodController(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index()
		{
			var gateways = await _context.PaymentGetways
				.Include(g => g.GetwayInfo)
				.OrderBy(g => g.Name)
				.ToListAsync();
			return View(gateways);
		}

		[HttpPost]
		public async Task<IActionResult> Create(string name, string? description)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				TempData["Error"] = "Name is required.";
				return RedirectToAction(nameof(Index));
			}

			_context.PaymentGetways.Add(new PaymentGetway { Name = name.Trim(), Description = description?.Trim() });
			await _context.SaveChangesAsync();
			TempData["Success"] = $"Payment method \"{name}\" added. Now add its account number.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		public async Task<IActionResult> Update(int id, string name, string? description)
		{
			var gateway = await _context.PaymentGetways.FindAsync(id);
			if (gateway == null) return NotFound();
			if (string.IsNullOrWhiteSpace(name))
			{
				TempData["Error"] = "Name is required.";
				return RedirectToAction(nameof(Index));
			}

			gateway.Name = name.Trim();
			gateway.Description = description?.Trim();
			await _context.SaveChangesAsync();
			TempData["Success"] = "Payment method updated.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			var gateway = await _context.PaymentGetways.Include(g => g.GetwayInfo).FirstOrDefaultAsync(g => g.Id == id);
			if (gateway == null) return NotFound();

			_context.RemoveRange(gateway.GetwayInfo);
			_context.PaymentGetways.Remove(gateway);
			await _context.SaveChangesAsync();
			TempData["Success"] = "Payment method deleted.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		public async Task<IActionResult> AddAccount(int gatewayId, string name, string? no, string? branch, string? description)
		{
			if (!await _context.PaymentGetways.AnyAsync(g => g.Id == gatewayId)) return NotFound();
			if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(no))
			{
				TempData["Error"] = "Account type and number are required.";
				return RedirectToAction(nameof(Index));
			}

			_context.Add(new GetwayInfo
			{
				GetwayId = gatewayId,
				Name = name.Trim(),
				No = no.Trim(),
				Branch = branch?.Trim(),
				Description = description?.Trim()
			});
			await _context.SaveChangesAsync();
			TempData["Success"] = "Account added.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		public async Task<IActionResult> DeleteAccount(int id)
		{
			var account = await _context.Set<GetwayInfo>().FindAsync(id);
			if (account == null) return NotFound();

			_context.Remove(account);
			await _context.SaveChangesAsync();
			TempData["Success"] = "Account removed.";
			return RedirectToAction(nameof(Index));
		}
	}
}
