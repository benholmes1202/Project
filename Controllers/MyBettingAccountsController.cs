using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Services;
using Project.ViewModels.BettingAccounts;
using Project.ViewModels.MyBettingAccounts;

namespace Project.Controllers
{
    [Authorize]
    public class MyBettingAccountsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IBettingAccountService _accountService;
        private readonly IAccountTransactionService _transactionService;

        public MyBettingAccountsController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IBettingAccountService accountService,
            IAccountTransactionService transactionService)
        {
            _context = context;
            _userManager = userManager;
            _accountService = accountService;
            _transactionService = transactionService;
        }

        public async Task<IActionResult> Index()
        {
            var appUser = await GetCurrentApplicationUserAsync();
            if (appUser == null)
            {
                TempData["ErrorMessage"] = "Your application profile could not be found.";
                return RedirectToAction("MyProfile", "ApplicationUsers");
            }

            return View(await _accountService.GetForUserAsync(appUser.UserId));
        }

        public IActionResult Create()
        {
            return View(new BettingAccountFormViewModel { CurrencyCode = "ZAR" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BettingAccountFormViewModel model)
        {
            var appUser = await GetCurrentApplicationUserAsync();
            if (appUser == null)
            {
                return Challenge();
            }

            ModelState.Remove(nameof(BettingAccountFormViewModel.UserId));

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _accountService.CreateForUserAsync(model, appUser.UserId);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Account could not be created.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Betting account created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Deposit(int id)
        {
            var account = await GetOwnedAccountAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            return View("MoneyMovement", new AccountMoneyViewModel
            {
                AccountId = account.AccountId,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(AccountMoneyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("MoneyMovement", model);
            }

            var appUser = await GetCurrentApplicationUserAsync();
            if (appUser == null)
            {
                return Challenge();
            }

            var result = await _transactionService.CreateUserMoneyMovementAsync(model.AccountId, appUser.UserId, model.Amount, "Credit");
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Deposit could not be created.");
                return View("MoneyMovement", model);
            }

            TempData["SuccessMessage"] = "Deposit captured successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Withdraw(int id)
        {
            var account = await GetOwnedAccountAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            return View("MoneyMovement", new AccountMoneyViewModel
            {
                AccountId = account.AccountId,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(AccountMoneyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("MoneyMovement", model);
            }

            var appUser = await GetCurrentApplicationUserAsync();
            if (appUser == null)
            {
                return Challenge();
            }

            var result = await _transactionService.CreateUserMoneyMovementAsync(model.AccountId, appUser.UserId, model.Amount, "Debit");
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Withdrawal could not be created.");
                return View("MoneyMovement", model);
            }

            TempData["SuccessMessage"] = "Withdrawal captured successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(int id)
        {
            var appUser = await GetCurrentApplicationUserAsync();
            if (appUser == null)
            {
                return Challenge();
            }

            var result = await _accountService.CloseForUserAsync(id, appUser.UserId);
            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
                result.Succeeded ? "Account closed successfully." : result.ErrorMessage;

            return RedirectToAction(nameof(Index));
        }

        private async Task<Models.ApplicationUser?> GetCurrentApplicationUserAsync()
        {
            var identityUserId = _userManager.GetUserId(User);
            return string.IsNullOrWhiteSpace(identityUserId)
                ? null
                : await _context.AppUsers.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
        }

        private async Task<Models.BettingAccount?> GetOwnedAccountAsync(int accountId)
        {
            var appUser = await GetCurrentApplicationUserAsync();
            if (appUser == null)
            {
                return null;
            }

            return await _context.BettingAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AccountId == accountId && a.UserId == appUser.UserId);
        }
    }
}
