using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Models;
using Project.Services;
using Project.ViewModels.Account;

namespace Project.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string cleanIdNumber = model.IdNumber.Trim();
            string cleanEmail = model.Email.Trim();

            bool idNumberExists = await _context.AppUsers
                .AnyAsync(u => u.IdNumber == cleanIdNumber);

            if (idNumberExists)
            {
                ModelState.AddModelError(nameof(model.IdNumber), "A user with this ID Number already exists.");
                return View(model);
            }

            var identityUser = new IdentityUser
            {
                UserName = cleanEmail,
                Email = cleanEmail,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(identityUser, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            await _userManager.AddToRoleAsync(identityUser, "User");

            var applicationUser = new ApplicationUser
            {
                IdentityUserId = identityUser.Id,
                IdNumber = cleanIdNumber,
                FirstName = model.FirstName.Trim(),
                Surname = model.Surname.Trim(),
                Email = cleanEmail,
                PhoneNumber = model.PhoneNumber?.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.AppUsers.Add(applicationUser);
            await _context.SaveChangesAsync();

            await _signInManager.SignInAsync(identityUser, isPersistent: false);

            TempData["SuccessMessage"] = "Your account has been created successfully.";
            return RedirectToAction("MyProfile", "ApplicationUsers");
        }

        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }


        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var appUser = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.IdentityUserId == user.Id);

            if (appUser == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var model = new ProfileViewModel
            {
                ApplicationUserId = appUser.UserId,
                IdentityUserId = user.Id,
                FirstName = appUser.FirstName,
                Surname = appUser.Surname,
                IdNumber = appUser.IdNumber,
                Email = appUser.Email,
                PhoneNumber = appUser.PhoneNumber,
                Role = roles.FirstOrDefault() ?? "User"
            };

            return View(model);
        }


            [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string cleanEmail = model.Email.Trim();

            var result = await _signInManager.PasswordSignInAsync(
                cleanEmail,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                var identityUser = await _userManager.FindByEmailAsync(cleanEmail);

                if (identityUser != null && await _userManager.IsInRoleAsync(identityUser, "Admin"))
                {
                    return RedirectToAction("Index", "ApplicationUsers");
                }

                return RedirectToAction("MyProfile", "ApplicationUsers");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["SuccessMessage"] = "You have been signed out.";
            return RedirectToAction(nameof(Login), "Account");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}