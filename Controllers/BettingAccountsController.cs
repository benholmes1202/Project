using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project.Models;
using Project.Services;

namespace Project.Controllers
{
    public class BettingAccountsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BettingAccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BettingAccounts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.BettingAccounts.Include(b => b.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: BettingAccounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bettingAccount = await _context.BettingAccounts
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (bettingAccount == null)
            {
                return NotFound();
            }

            return View(bettingAccount);
        }

        // GET: BettingAccounts/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.AppUsers, "UserId", "Email");
            return View();
        }

        // POST: BettingAccounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AccountId,UserId,AccountNumber,CurrencyCode,Balance,Status,CreatedAt,UpdatedAt,ClosedAt,RowVersion")] BettingAccount bettingAccount)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bettingAccount);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.AppUsers, "UserId", "Email", bettingAccount.UserId);
            return View(bettingAccount);
        }

        // GET: BettingAccounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bettingAccount = await _context.BettingAccounts.FindAsync(id);
            if (bettingAccount == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.AppUsers, "UserId", "Email", bettingAccount.UserId);
            return View(bettingAccount);
        }

        // POST: BettingAccounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AccountId,UserId,AccountNumber,CurrencyCode,Balance,Status,CreatedAt,UpdatedAt,ClosedAt,RowVersion")] BettingAccount bettingAccount)
        {
            if (id != bettingAccount.AccountId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bettingAccount);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BettingAccountExists(bettingAccount.AccountId))
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
            ViewData["UserId"] = new SelectList(_context.AppUsers, "UserId", "Email", bettingAccount.UserId);
            return View(bettingAccount);
        }

        // GET: BettingAccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bettingAccount = await _context.BettingAccounts
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (bettingAccount == null)
            {
                return NotFound();
            }

            return View(bettingAccount);
        }

        // POST: BettingAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bettingAccount = await _context.BettingAccounts.FindAsync(id);
            if (bettingAccount != null)
            {
                _context.BettingAccounts.Remove(bettingAccount);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BettingAccountExists(int id)
        {
            return _context.BettingAccounts.Any(e => e.AccountId == id);
        }
    }
}
