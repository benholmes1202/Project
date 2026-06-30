using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project.Models;
using Project.Services;
using Project.ViewModels.BettingAccounts;

namespace Project.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BettingAccountsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBettingAccountService _service;

        public BettingAccountsController(ApplicationDbContext context, IBettingAccountService service)
        {
            _context = context;
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _service.GetAllAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bettingAccount = await _service.GetDetailsAsync(id.Value);
            if (bettingAccount == null)
            {
                return NotFound();
            }

            return View(bettingAccount);
        }

        public async Task<IActionResult> Create(int? userId)
        {
            var model = new BettingAccountFormViewModel
            {
                UserId = userId ?? 0,
                CurrencyCode = "ZAR"
            };

            await PopulateUsersAsync(model.UserId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BettingAccountFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateUsersAsync(model.UserId);
                return View(model);
            }

            var result = await _service.CreateAsync(model);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Account could not be created.");
                await PopulateUsersAsync(model.UserId);
                return View(model);
            }

            TempData["SuccessMessage"] = "Betting account created successfully.";
            return RedirectToAction(nameof(Details), new { id = result.Value });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _service.GetForEditAsync(id.Value);
            if (account == null)
            {
                return NotFound();
            }

            var model = new BettingAccountFormViewModel
            {
                AccountId = account.AccountId,
                UserId = account.UserId,
                AccountNumber = account.AccountNumber,
                CurrencyCode = account.CurrencyCode,
                Balance = account.Balance,
                Status = account.Status
            };

            await PopulateUsersAsync(model.UserId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BettingAccountFormViewModel model)
        {
            if (id != model.AccountId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await PopulateUsersAsync(model.UserId);
                return View(model);
            }

            var result = await _service.UpdateAsync(model);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Account could not be updated.");
                await PopulateUsersAsync(model.UserId);
                return View(model);
            }

            TempData["SuccessMessage"] = "Betting account updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(int id)
        {
            var result = await _service.CloseAsync(id);
            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
                result.Succeeded ? "Account closed successfully." : result.ErrorMessage;

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reopen(int id)
        {
            var result = await _service.ReopenAsync(id);
            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
                result.Succeeded ? "Account reopened successfully." : result.ErrorMessage;

            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bettingAccount = await _context.BettingAccounts
                .AsNoTracking()
                .Include(b => b.User)
                .Include(b => b.AccountTransactions)
                .Include(b => b.Bets)
                .FirstOrDefaultAsync(m => m.AccountId == id.Value);

            if (bettingAccount == null)
            {
                return NotFound();
            }

            return View(bettingAccount);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["SuccessMessage"] = "Betting account deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateUsersAsync(int selectedUserId)
        {
            var users = await _context.AppUsers
                .AsNoTracking()
                .OrderBy(u => u.Surname)
                .ThenBy(u => u.FirstName)
                .Select(u => new
                {
                    u.UserId,
                    Name = u.FirstName + " " + u.Surname + " (" + u.IdNumber + ")"
                })
                .ToListAsync();

            ViewData["UserId"] = new SelectList(users, "UserId", "Name", selectedUserId);
        }
    }
}
