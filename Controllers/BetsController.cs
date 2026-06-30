using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project.Services;
using Project.ViewModels.Bets;

namespace Project.Controllers
{
    [Authorize]
    public class BetsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IBetService _betService;
        private readonly IBetMatchService _matchService;

        public BetsController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IBetService betService,
            IBetMatchService matchService)
        {
            _context = context;
            _userManager = userManager;
            _betService = betService;
            _matchService = matchService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                return View(await _betService.GetAllAsync());
            }

            var appUser = await GetCurrentApplicationUserAsync();
            if (appUser == null)
            {
                TempData["ErrorMessage"] = "Your application profile could not be found.";
                return RedirectToAction("MyProfile", "ApplicationUsers");
            }

            return View(await _betService.GetForUserAsync(appUser.UserId));
        }

        public async Task<IActionResult> AvailableMatches()
        {
            return View(await _matchService.GetActiveUpcomingAsync());
        }

        public async Task<IActionResult> Place(int matchId)
        {
            var appUser = await GetCurrentApplicationUserAsync();
            if (appUser == null)
            {
                return Challenge();
            }

            var match = await _matchService.GetByIdAsync(matchId);
            if (match == null || !match.IsActive)
            {
                return NotFound();
            }

            await PopulateAccountsAsync(appUser.UserId);
            ViewBag.Match = match;

            return View(new PlaceBetViewModel
            {
                BetMatchId = match.BetMatchId
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Place(PlaceBetViewModel model)
        {
            var appUser = await GetCurrentApplicationUserAsync();
            if (appUser == null)
            {
                return Challenge();
            }

            var match = await _matchService.GetByIdAsync(model.BetMatchId);
            if (match == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await PopulateAccountsAsync(appUser.UserId);
                ViewBag.Match = match;
                return View(model);
            }

            var result = await _betService.PlaceBetAsync(model, appUser.UserId);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Bet could not be placed.");
                await PopulateAccountsAsync(appUser.UserId);
                ViewBag.Match = match;
                return View(model);
            }

            TempData["SuccessMessage"] = "Bet placed successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var bet = await _betService.GetByIdAsync(id);
            if (bet == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin"))
            {
                var appUser = await GetCurrentApplicationUserAsync();
                if (appUser == null || bet.BettingAccount?.UserId != appUser.UserId)
                {
                    return Forbid();
                }
            }

            return View(bet);
        }

        private async Task PopulateAccountsAsync(int userId)
        {
            var accounts = await _context.BettingAccounts
                .AsNoTracking()
                .Where(a => a.UserId == userId && a.Status == "Open")
                .OrderBy(a => a.AccountNumber)
                .Select(a => new
                {
                    a.AccountId,
                    Display = a.AccountNumber + " - Balance: " + a.Balance
                })
                .ToListAsync();

            ViewData["AccountId"] = new SelectList(accounts, "AccountId", "Display");
        }

        private async Task<Models.ApplicationUser?> GetCurrentApplicationUserAsync()
        {
            var identityUserId = _userManager.GetUserId(User);
            return string.IsNullOrWhiteSpace(identityUserId)
                ? null
                : await _context.AppUsers.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
        }
    }
}
