using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project.Models;
using Project.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;


namespace Project.Controllers
{
    public class ApplicationUsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IApplicationUserService _service;
        private readonly UserManager<IdentityUser> _userManager;

        public ApplicationUsersController(ApplicationDbContext context, IApplicationUserService service, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _service = service;
            _userManager = userManager;
        }

        // GET: ApplicationUsers
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.AppUsers.ToListAsync());
        }

        // GET: ApplicationUsers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _context.AppUsers
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (applicationUser == null)
            {
                return NotFound();
            }

            return View(applicationUser);
        }

        // GET: ApplicationUsers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ApplicationUsers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationUser applicationUser)
        {
            if (!ModelState.IsValid)
            {
                return View(applicationUser);
            }

            bool idExists = await _service.IdNumberExistsAsync(applicationUser.IdNumber);

            if (idExists)
            {
                ModelState.AddModelError("IdNumber", "A user with this ID Number already exists.");
                return View(applicationUser);
            }

            bool created = await _service.CreateAsync(applicationUser);

            if (!created)
            {
                ModelState.AddModelError("IdNumber", "A user with this ID Number already exists.");
                return View(applicationUser);
            }

            TempData["SuccessMessage"] = "User created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: ApplicationUsers/Edit/5
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,IdNumber,FirstName,Surname,Email,PhoneNumber,IsActive,CreatedAt,UpdatedAt")] ApplicationUser applicationUser)
        {
            if (id != applicationUser.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(applicationUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApplicationUserExists(applicationUser.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(applicationUser);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _context.AppUsers
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (applicationUser == null)
            {
                return NotFound();
            }

            return View(applicationUser);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var applicationUser = await _context.AppUsers.FindAsync(id);
            if (applicationUser != null)
            {
                _context.AppUsers.Remove(applicationUser);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ApplicationUserExists(int id)
        {
            return _context.AppUsers.Any(e => e.UserId == id);
        }

        [Authorize]
        public async Task<IActionResult> MyProfile()
        {
            string? identityUserId = _userManager.GetUserId(User);

            if (identityUserId == null)
            {
                return Challenge();
            }

            var applicationUser = await _context.AppUsers
                .Include(u => u.BettingAccounts)
                .FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);

            if (applicationUser == null)
            {
                return NotFound();
            }

            return View("Details", applicationUser);
        }

    }
}
