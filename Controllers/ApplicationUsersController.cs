using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Models;
using Project.Services;
using Project.ViewModels.Account;

namespace Project.Controllers
{
    [Authorize]
    public class ApplicationUsersController : Controller
    {
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

        // GET: ApplicationUsers
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var users = await _context.AppUsers
                .AsNoTracking()
                .OrderBy(u => u.Surname)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            return View(users);
        }

        // GET: ApplicationUsers/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _context.AppUsers
                .AsNoTracking()
                .Include(u => u.BettingAccounts)
                .FirstOrDefaultAsync(m => m.UserId == id);

            if (applicationUser == null)
            {
                return NotFound();
            }

            return View(applicationUser);
        }

        // GET: ApplicationUsers/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: ApplicationUsers/Create
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

            applicationUser.IdNumber = applicationUser.IdNumber.Trim();
            applicationUser.FirstName = applicationUser.FirstName.Trim();
            applicationUser.Surname = applicationUser.Surname.Trim();
            applicationUser.Email = applicationUser.Email.Trim();
            applicationUser.PhoneNumber = applicationUser.PhoneNumber?.Trim();

            bool idExists = await _service.IdNumberExistsAsync(applicationUser.IdNumber);

            if (idExists)
            {
                ModelState.AddModelError(nameof(applicationUser.IdNumber), "A user with this ID Number already exists.");
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

        // GET: ApplicationUsers/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _context.AppUsers.FindAsync(id);

            if (applicationUser == null)
            {
                return NotFound();
            }

            return View(applicationUser);
        }

        // POST: ApplicationUsers/Edit/5
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

            var existingUser = await _context.AppUsers.FindAsync(id);

            if (existingUser == null)
            {
                return NotFound();
            }

            bool duplicateIdNumberExists = await _context.AppUsers
                .AnyAsync(u => u.UserId != id && u.IdNumber == applicationUser.IdNumber.Trim());

            if (duplicateIdNumberExists)
            {
                ModelState.AddModelError(nameof(applicationUser.IdNumber), "A user with this ID Number already exists.");
                return View(applicationUser);
            }

            existingUser.IdNumber = applicationUser.IdNumber.Trim();
            existingUser.FirstName = applicationUser.FirstName.Trim();
            existingUser.Surname = applicationUser.Surname.Trim();
            existingUser.Email = applicationUser.Email.Trim();
            existingUser.PhoneNumber = applicationUser.PhoneNumber?.Trim();
            existingUser.IsActive = applicationUser.IsActive;
            existingUser.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "User updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApplicationUserExists(applicationUser.UserId))
                {
                    return NotFound();
                }

                throw;
            }
        }

        // GET: ApplicationUsers/Delete/5
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
                .FirstOrDefaultAsync(m => m.UserId == id);

            if (applicationUser == null)
            {
                return NotFound();
            }

            return View(applicationUser);
        }

        // POST: ApplicationUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var applicationUser = await _context.AppUsers
                .Include(u => u.BettingAccounts)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (applicationUser == null)
            {
                return NotFound();
            }

            string? currentIdentityUserId = _userManager.GetUserId(User);

            if (!string.IsNullOrWhiteSpace(currentIdentityUserId) &&
                applicationUser.IdentityUserId == currentIdentityUserId)
            {
                TempData["ErrorMessage"] = "You cannot delete your own user profile while signed in.";
                return RedirectToAction(nameof(Index));
            }

            if (applicationUser.BettingAccounts != null && applicationUser.BettingAccounts.Any())
            {
                TempData["ErrorMessage"] = "This user cannot be deleted because they have linked betting accounts.";
                return RedirectToAction(nameof(Index));
            }

            _context.AppUsers.Remove(applicationUser);
            await _context.SaveChangesAsync();

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
                model.ApplicationUserId = applicationUser.UserId;
                model.IdentityUserId = identityUserId;
                model.IdNumber = applicationUser.IdNumber;

                var roles = await _userManager.GetRolesAsync(identityUser);
                model.Role = roles.FirstOrDefault() ?? "User";

                return View(model);
            }

            string cleanEmail = model.Email.Trim();

            var existingIdentityUserWithEmail = await _userManager.FindByEmailAsync(cleanEmail);

            if (existingIdentityUserWithEmail != null && existingIdentityUserWithEmail.Id != identityUserId)
            {
                ModelState.AddModelError(nameof(model.Email), "This email address is already used by another account.");

                model.ApplicationUserId = applicationUser.UserId;
                model.IdentityUserId = identityUserId;
                model.IdNumber = applicationUser.IdNumber;

                var roles = await _userManager.GetRolesAsync(identityUser);
                model.Role = roles.FirstOrDefault() ?? "User";

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

                model.ApplicationUserId = applicationUser.UserId;
                model.IdentityUserId = identityUserId;
                model.IdNumber = applicationUser.IdNumber;

                var roles = await _userManager.GetRolesAsync(identityUser);
                model.Role = roles.FirstOrDefault() ?? "User";

                return View(model);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your profile has been updated successfully.";
            return RedirectToAction(nameof(MyProfile));
        }

        private bool ApplicationUserExists(int id)
        {
            return _context.AppUsers.Any(e => e.UserId == id);
        }
    }
}