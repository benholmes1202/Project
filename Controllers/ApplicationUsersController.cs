using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Models;
using Project.Services;
using Project.ViewModels.Account;
using Project.ViewModels.ApplicationUsers;

namespace Project.Controllers
{
    [Authorize]
    public class ApplicationUsersController : Controller
    {
        private const int PageSize = 10;
        private readonly ApplicationDbContext _context;
        private readonly IApplicationUserService _service;
        private readonly UserManager<IdentityUser> _userManager;

        public ApplicationUsersController(
            ApplicationDbContext context,
            IApplicationUserService service,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _service = service;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string? searchTerm, int page = 1)
        {
            var result = await _service.GetUsersAsync(searchTerm, page, PageSize);

            var model = new ApplicationUsersIndexViewModel
            {
                Users = result.Items,
                SearchTerm = searchTerm,
                PageNumber = result.PageNumber,
                TotalPages = result.TotalPages,
                TotalUsers = result.TotalItems
            };

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _service.GetByIdAsync(id.Value);

            if (applicationUser == null)
            {
                return NotFound();
            }

            return View(applicationUser);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(new ApplicationUser { IsActive = true });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("IdNumber,FirstName,Surname,Email,PhoneNumber,IsActive")] ApplicationUser applicationUser)
        {
            if (!ModelState.IsValid)
            {
                return View(applicationUser);
            }

            bool created = await _service.CreateAsync(applicationUser);

            if (!created)
            {
                ModelState.AddModelError(nameof(applicationUser.IdNumber), "A user with this ID Number already exists.");
                return View(applicationUser);
            }

            TempData["SuccessMessage"] = "User created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _context.AppUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == id.Value);

            if (applicationUser == null)
            {
                return NotFound();
            }

            return View(applicationUser);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("UserId,IdNumber,FirstName,Surname,Email,PhoneNumber,IsActive")] ApplicationUser applicationUser)
        {
            if (id != applicationUser.UserId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(applicationUser);
            }

            bool updated = await _service.UpdateAsync(applicationUser);

            if (!updated)
            {
                ModelState.AddModelError(nameof(applicationUser.IdNumber), "A user with this ID Number already exists.");
                return View(applicationUser);
            }

            TempData["SuccessMessage"] = "User updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _context.AppUsers
                .AsNoTracking()
                .Include(u => u.BettingAccounts)
                .FirstOrDefaultAsync(m => m.UserId == id.Value);

            if (applicationUser == null)
            {
                return NotFound();
            }

            ViewBag.CanDelete = await _service.CanDeleteAsync(applicationUser.UserId);
            return View(applicationUser);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            string? currentIdentityUserId = _userManager.GetUserId(User);
            var result = await _service.DeleteAsync(id, currentIdentityUserId);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "User deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> MyProfile()
        {
            string? identityUserId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(identityUserId))
            {
                return Challenge();
            }

            var applicationUser = await _context.AppUsers
                .AsNoTracking()
                .Include(u => u.BettingAccounts)
                .FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);

            if (applicationUser == null)
            {
                TempData["ErrorMessage"] =
                    "Your login account exists, but no application profile is linked to it. Please contact an administrator.";

                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction(nameof(Index));
                }

                return RedirectToAction("Login", "Account");
            }

            return View(applicationUser);
        }

        [Authorize]
        public async Task<IActionResult> EditMyProfile()
        {
            string? identityUserId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(identityUserId))
            {
                return Challenge();
            }

            var applicationUser = await _context.AppUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);

            if (applicationUser == null)
            {
                TempData["ErrorMessage"] = "Your profile could not be found.";
                return RedirectToAction(nameof(MyProfile));
            }

            var identityUser = await _userManager.FindByIdAsync(identityUserId);
            var roles = identityUser == null
                ? Array.Empty<string>()
                : await _userManager.GetRolesAsync(identityUser);

            var model = new ProfileViewModel
            {
                ApplicationUserId = applicationUser.UserId,
                IdentityUserId = applicationUser.IdentityUserId ?? string.Empty,
                IdNumber = applicationUser.IdNumber,
                FirstName = applicationUser.FirstName,
                Surname = applicationUser.Surname,
                Email = applicationUser.Email,
                PhoneNumber = applicationUser.PhoneNumber,
                Role = roles.FirstOrDefault() ?? "User"
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMyProfile(ProfileViewModel model)
        {
            string? identityUserId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(identityUserId))
            {
                return Challenge();
            }

            var applicationUser = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);

            if (applicationUser == null)
            {
                TempData["ErrorMessage"] = "Your profile could not be found.";
                return RedirectToAction(nameof(MyProfile));
            }

            var identityUser = await _userManager.FindByIdAsync(identityUserId);

            if (identityUser == null)
            {
                TempData["ErrorMessage"] = "Your login account could not be found.";
                return RedirectToAction(nameof(MyProfile));
            }

            ModelState.Remove(nameof(ProfileViewModel.ApplicationUserId));
            ModelState.Remove(nameof(ProfileViewModel.IdentityUserId));
            ModelState.Remove(nameof(ProfileViewModel.IdNumber));
            ModelState.Remove(nameof(ProfileViewModel.Role));

            if (!ModelState.IsValid)
            {
                await PopulateProfileReadOnlyFields(model, applicationUser, identityUser);
                return View(model);
            }

            string cleanEmail = model.Email.Trim();
            var existingIdentityUserWithEmail = await _userManager.FindByEmailAsync(cleanEmail);

            if (existingIdentityUserWithEmail != null && existingIdentityUserWithEmail.Id != identityUserId)
            {
                ModelState.AddModelError(nameof(model.Email), "This email address is already used by another account.");
                await PopulateProfileReadOnlyFields(model, applicationUser, identityUser);
                return View(model);
            }

            applicationUser.FirstName = model.FirstName.Trim();
            applicationUser.Surname = model.Surname.Trim();
            applicationUser.Email = cleanEmail;
            applicationUser.PhoneNumber = model.PhoneNumber?.Trim();
            applicationUser.UpdatedAt = DateTime.UtcNow;

            identityUser.Email = cleanEmail;
            identityUser.UserName = cleanEmail;

            var identityUpdateResult = await _userManager.UpdateAsync(identityUser);

            if (!identityUpdateResult.Succeeded)
            {
                foreach (var error in identityUpdateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                await PopulateProfileReadOnlyFields(model, applicationUser, identityUser);
                return View(model);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your profile has been updated successfully.";
            return RedirectToAction(nameof(MyProfile));
        }

        private async Task PopulateProfileReadOnlyFields(
            ProfileViewModel model,
            ApplicationUser applicationUser,
            IdentityUser identityUser)
        {
            model.ApplicationUserId = applicationUser.UserId;
            model.IdentityUserId = applicationUser.IdentityUserId ?? string.Empty;
            model.IdNumber = applicationUser.IdNumber;

            var roles = await _userManager.GetRolesAsync(identityUser);
            model.Role = roles.FirstOrDefault() ?? "User";
        }
    }
}
