using Microsoft.EntityFrameworkCore;
using Project.Models;
using Project.ViewModels.BettingAccounts;

namespace Project.Services
{
    public class BettingAccountService : IBettingAccountService
    {
        private const string OpenStatus = "Open";
        private const string ClosedStatus = "Closed";
        private readonly ApplicationDbContext _context;

        public BettingAccountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<BettingAccount>> GetAllAsync()
        {
            return await _context.BettingAccounts
                .AsNoTracking()
                .Include(a => a.User)
                .OrderBy(a => a.AccountNumber)
                .ToListAsync();
        }

        public async Task<BettingAccount?> GetDetailsAsync(int id)
        {
            return await _context.BettingAccounts
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.AccountTransactions)
                .ThenInclude(t => t.TransactionType)
                .FirstOrDefaultAsync(a => a.AccountId == id);
        }

        public async Task<BettingAccount?> GetForEditAsync(int id)
        {
            return await _context.BettingAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AccountId == id);
        }

        public async Task<ServiceResult<int>> CreateAsync(BettingAccountFormViewModel model)
        {
            var cleanAccountNumber = model.AccountNumber.Trim();
            var cleanCurrency = model.CurrencyCode.Trim().ToUpperInvariant();

            if (await AccountNumberExistsAsync(cleanAccountNumber))
            {
                return ServiceResult<int>.Failure("An account with this account number already exists.");
            }

            var userExists = await _context.AppUsers.AnyAsync(u => u.UserId == model.UserId);
            if (!userExists)
            {
                return ServiceResult<int>.Failure("The selected user does not exist.");
            }

            var now = DateTime.UtcNow;
            var account = new BettingAccount
            {
                UserId = model.UserId,
                AccountNumber = cleanAccountNumber,
                CurrencyCode = cleanCurrency,
                Balance = 0m,
                Status = OpenStatus,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.BettingAccounts.Add(account);
            await _context.SaveChangesAsync();
            return ServiceResult<int>.Success(account.AccountId);
        }

        public async Task<IReadOnlyList<BettingAccount>> GetForUserAsync(int userId)
        {
            return await _context.BettingAccounts
                .AsNoTracking()
                .Include(a => a.AccountTransactions)
                .Include(a => a.Bets)
                .ThenInclude(b => b.BetMatch)
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.AccountNumber)
                .ToListAsync();
        }

        public async Task<ServiceResult<int>> CreateForUserAsync(BettingAccountFormViewModel model, int userId)
        {
            model.UserId = userId;
            return await CreateAsync(model);
        }

        public async Task<ServiceResult> UpdateAsync(BettingAccountFormViewModel model)
        {
            var account = await _context.BettingAccounts.FindAsync(model.AccountId);
            if (account == null)
            {
                return ServiceResult.Failure("Account could not be found.");
            }

            var cleanAccountNumber = model.AccountNumber.Trim();
            if (await AccountNumberExistsAsync(cleanAccountNumber, model.AccountId))
            {
                return ServiceResult.Failure("An account with this account number already exists.");
            }

            var userExists = await _context.AppUsers.AnyAsync(u => u.UserId == model.UserId);
            if (!userExists)
            {
                return ServiceResult.Failure("The selected user does not exist.");
            }

            account.UserId = model.UserId;
            account.AccountNumber = cleanAccountNumber;
            account.CurrencyCode = model.CurrencyCode.Trim().ToUpperInvariant();
            account.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> CloseAsync(int id)
        {
            var account = await _context.BettingAccounts.FindAsync(id);
            if (account == null)
            {
                return ServiceResult.Failure("Account could not be found.");
            }

            if (account.Balance != 0m)
            {
                return ServiceResult.Failure("An account can only be closed when the outstanding balance is zero.");
            }

            account.Status = ClosedStatus;
            account.ClosedAt = DateTime.UtcNow;
            account.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> CloseForUserAsync(int id, int userId)
        {
            var accountBelongsToUser = await _context.BettingAccounts
                .AnyAsync(a => a.AccountId == id && a.UserId == userId);

            if (!accountBelongsToUser)
            {
                return ServiceResult.Failure("Account could not be found.");
            }

            return await CloseAsync(id);
        }

        public async Task<ServiceResult> ReopenAsync(int id)
        {
            var account = await _context.BettingAccounts.FindAsync(id);
            if (account == null)
            {
                return ServiceResult.Failure("Account could not be found.");
            }

            account.Status = OpenStatus;
            account.ClosedAt = null;
            account.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var account = await _context.BettingAccounts
                .Include(a => a.AccountTransactions)
                .Include(a => a.Bets)
                .FirstOrDefaultAsync(a => a.AccountId == id);

            if (account == null)
            {
                return ServiceResult.Failure("Account could not be found.");
            }

            if (account.AccountTransactions.Any() || account.Bets.Any())
            {
                return ServiceResult.Failure("Accounts with transactions or bets cannot be deleted. Close the account instead.");
            }

            _context.BettingAccounts.Remove(account);
            await _context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public async Task<bool> AccountNumberExistsAsync(string accountNumber, int? excludeAccountId = null)
        {
            var cleanAccountNumber = accountNumber.Trim();
            var query = _context.BettingAccounts.Where(a => a.AccountNumber == cleanAccountNumber);

            if (excludeAccountId.HasValue)
            {
                query = query.Where(a => a.AccountId != excludeAccountId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
