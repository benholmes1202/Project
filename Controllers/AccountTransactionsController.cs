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
    public class AccountTransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountTransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AccountTransactions
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.AccountTransactions.Include(a => a.RelatedBet).Include(a => a.TransactionType);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: AccountTransactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var accountTransaction = await _context.AccountTransactions
                .Include(a => a.RelatedBet)
                .Include(a => a.TransactionType)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
            if (accountTransaction == null)
            {
                return NotFound();
            }

            return View(accountTransaction);
        }

        // GET: AccountTransactions/Create
        public IActionResult Create()
        {
            ViewData["RelatedBetId"] = new SelectList(_context.Bets, "BetId", "BetId");
            ViewData["TransactionTypeId"] = new SelectList(_context.TransactionTypes, "TransactionTypeId", "Direction");
            return View();
        }

        // POST: AccountTransactions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TransactionId,AccountId,TransactionTypeId,RelatedBetId,Amount,TransactionDate,CaptureDate,Reference,Notes,CreatedAt,UpdatedAt")] AccountTransaction accountTransaction)
        {
            if (ModelState.IsValid)
            {
                _context.Add(accountTransaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RelatedBetId"] = new SelectList(_context.Bets, "BetId", "BetId", accountTransaction.RelatedBetId);
            ViewData["TransactionTypeId"] = new SelectList(_context.TransactionTypes, "TransactionTypeId", "Direction", accountTransaction.TransactionTypeId);
            return View(accountTransaction);
        }

        // GET: AccountTransactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var accountTransaction = await _context.AccountTransactions.FindAsync(id);
            if (accountTransaction == null)
            {
                return NotFound();
            }
            ViewData["RelatedBetId"] = new SelectList(_context.Bets, "BetId", "BetId", accountTransaction.RelatedBetId);
            ViewData["TransactionTypeId"] = new SelectList(_context.TransactionTypes, "TransactionTypeId", "Direction", accountTransaction.TransactionTypeId);
            return View(accountTransaction);
        }

        // POST: AccountTransactions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TransactionId,AccountId,TransactionTypeId,RelatedBetId,Amount,TransactionDate,CaptureDate,Reference,Notes,CreatedAt,UpdatedAt")] AccountTransaction accountTransaction)
        {
            if (id != accountTransaction.TransactionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(accountTransaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountTransactionExists(accountTransaction.TransactionId))
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
            ViewData["RelatedBetId"] = new SelectList(_context.Bets, "BetId", "BetId", accountTransaction.RelatedBetId);
            ViewData["TransactionTypeId"] = new SelectList(_context.TransactionTypes, "TransactionTypeId", "Direction", accountTransaction.TransactionTypeId);
            return View(accountTransaction);
        }

        // GET: AccountTransactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var accountTransaction = await _context.AccountTransactions
                .Include(a => a.RelatedBet)
                .Include(a => a.TransactionType)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
            if (accountTransaction == null)
            {
                return NotFound();
            }

            return View(accountTransaction);
        }

        // POST: AccountTransactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var accountTransaction = await _context.AccountTransactions.FindAsync(id);
            if (accountTransaction != null)
            {
                _context.AccountTransactions.Remove(accountTransaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AccountTransactionExists(int id)
        {
            return _context.AccountTransactions.Any(e => e.TransactionId == id);
        }
    }
}
