using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Services;
using Project.ViewModels.BetMatches;

namespace Project.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BetMatchesController : Controller
    {
        private readonly IBetMatchService _service;

        public BetMatchesController(IBetMatchService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _service.GetAllAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var match = await _service.GetByIdAsync(id);
            return match == null ? NotFound() : View(match);
        }

        public IActionResult Create()
        {
            return View(new BetMatchFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BetMatchFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _service.CreateAsync(model);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Match could not be created.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Match created successfully.";
            return RedirectToAction(nameof(Details), new { id = result.Value });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var match = await _service.GetByIdAsync(id);
            if (match == null)
            {
                return NotFound();
            }

            return View(new BetMatchFormViewModel
            {
                BetMatchId = match.BetMatchId,
                HomeTeam = match.HomeTeam,
                AwayTeam = match.AwayTeam,
                Sport = match.Sport,
                MatchDate = match.MatchDate,
                HomeOdds = match.HomeOdds,
                AwayOdds = match.AwayOdds,
                DrawOdds = match.DrawOdds,
                IsActive = match.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BetMatchFormViewModel model)
        {
            if (id != model.BetMatchId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _service.UpdateAsync(model);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Match could not be updated.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Match updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var match = await _service.GetByIdAsync(id);
            return match == null ? NotFound() : View(match);
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

            TempData["SuccessMessage"] = "Match deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
