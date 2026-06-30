using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project.Services;
using Project.ViewModels.AccountTransactions;

namespace Project.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AccountTransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAccountTransactionService _service;

        public AccountTransactionsController(ApplicationDbContext context, IAccountTransactionService service)
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

            var accountTransaction = await _service.GetDetailsAsync(id.Value);
            if (accountTransaction == null)
            {
                return NotFound();
            }

            return View(accountTransaction);
        }

        public async Task<IActionResult> Create(int? accountId)
        {
            var model = new AccountTransactionFormViewModel
            {
                AccountId = accountId ?? 0,
                TransactionDate = DateTime.Today
            };

            await PopulateSelectListsAsync(model.AccountId, model.TransactionTypeId, model.RelatedBetId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AccountTransactionFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync(model.AccountId, model.TransactionTypeId, model.RelatedBetId);
                return View(model);
            }

            var result = await _service.CreateAsync(model);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Transaction could not be created.");
                await PopulateSelectListsAsync(model.AccountId, model.TransactionTypeId, model.RelatedBetId);
                return View(model);
            }

            TempData["SuccessMessage"] = "Transaction created and account balance updated.";
            return RedirectToAction("Details", "BettingAccounts", new { id = model.AccountId });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var accountTransaction = await _service.GetForEditAsync(id.Value);
            if (accountTransaction == null)
            {
                return NotFound();
            }

            var model = new AccountTransactionFormViewModel
            {
                TransactionId = accountTransaction.TransactionId,
                AccountId = accountTransaction.AccountId,
                TransactionTypeId = accountTransaction.TransactionTypeId,
                RelatedBetId = accountTransaction.RelatedBetId,
                Amount = accountTransaction.Amount,
                TransactionDate = accountTransaction.TransactionDate,
                Reference = accountTransaction.Reference,
                Notes = accountTransaction.Notes,
                CaptureDate = accountTransaction.CaptureDate
            };

            await PopulateSelectListsAsync(model.AccountId, model.TransactionTypeId, model.RelatedBetId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AccountTransactionFormViewModel model)
        {
            if (id != model.TransactionId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync(model.AccountId, model.TransactionTypeId, model.RelatedBetId);
                return View(model);
            }

            var result = await _service.UpdateAsync(model);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Transaction could not be updated.");
                await PopulateSelectListsAsync(model.AccountId, model.TransactionTypeId, model.RelatedBetId);
                return View(model);
            }

            TempData["SuccessMessage"] = "Transaction updated and account balance recalculated.";
            return RedirectToAction("Details", "BettingAccounts", new { id = model.AccountId });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var accountTransaction = await _service.GetDetailsAsync(id.Value);
            if (accountTransaction == null)
            {
                return NotFound();
            }

            return View(accountTransaction);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var accountTransaction = await _service.GetDetailsAsync(id);
            var accountId = accountTransaction?.AccountId;

            var result = await _service.DeleteAsync(id);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return accountId.HasValue
                    ? RedirectToAction("Details", "BettingAccounts", new { id = accountId.Value })
                    : RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Transaction deleted and account balance recalculated.";
            return accountId.HasValue
                ? RedirectToAction("Details", "BettingAccounts", new { id = accountId.Value })
                : RedirectToAction(nameof(Index));
        }

        private async Task PopulateSelectListsAsync(int selectedAccountId, int selectedTransactionTypeId, int? selectedBetId)
        {
            var accounts = await _context.BettingAccounts
                .AsNoTracking()
                .Where(a => a.Status == "Open")
                .OrderBy(a => a.AccountNumber)
                .Select(a => new { a.AccountId, a.AccountNumber })
                .ToListAsync();

            var transactionTypes = await _context.TransactionTypes
                .AsNoTracking()
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .ToListAsync();

            var bets = await _context.Bets
                .AsNoTracking()
                .OrderBy(b => b.BetId)
                .Select(b => new { b.BetId, Display = b.BetId + " - " + b.Category })
                .ToListAsync();

            ViewData["AccountId"] = new SelectList(accounts, "AccountId", "AccountNumber", selectedAccountId);
            ViewData["TransactionTypeId"] = new SelectList(transactionTypes, "TransactionTypeId", "Name", selectedTransactionTypeId);
            ViewData["RelatedBetId"] = new SelectList(bets, "BetId", "Display", selectedBetId);
        }
    }
}
