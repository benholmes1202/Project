using Microsoft.EntityFrameworkCore;
using Project.Models;
using Project.ViewModels.AccountTransactions;

namespace Project.Services
{
    public class AccountTransactionService : IAccountTransactionService
    {
        private readonly ApplicationDbContext _context;

        public AccountTransactionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<AccountTransaction>> GetAllAsync()
        {
            return await _context.AccountTransactions
                .AsNoTracking()
                .Include(t => t.BettingAccount)
                .Include(t => t.TransactionType)
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.CaptureDate)
                .ToListAsync();
        }

        public async Task<AccountTransaction?> GetDetailsAsync(int id)
        {
            return await _context.AccountTransactions
                .AsNoTracking()
                .Include(t => t.BettingAccount)
                .Include(t => t.RelatedBet)
                .Include(t => t.TransactionType)
                .FirstOrDefaultAsync(t => t.TransactionId == id);
        }

        public async Task<AccountTransaction?> GetForEditAsync(int id)
        {
            return await _context.AccountTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TransactionId == id);
        }

        public async Task<ServiceResult<int>> CreateAsync(AccountTransactionFormViewModel model)
        {
            var validation = await ValidateModelAsync(model);
            if (!validation.Succeeded)
            {
                return ServiceResult<int>.Failure(validation.ErrorMessage ?? "Transaction is invalid.");
            }

            var account = validation.Value!.Account;
            var transactionType = validation.Value.TransactionType;
            var now = DateTime.UtcNow;

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var accountTransaction = new AccountTransaction
            {
                AccountId = model.AccountId,
                TransactionTypeId = model.TransactionTypeId,
                RelatedBetId = model.RelatedBetId,
                Amount = model.Amount,
                TransactionDate = model.TransactionDate.Date,
                CaptureDate = now,
                Reference = model.Reference?.Trim(),
                Notes = model.Notes?.Trim(),
                CreatedAt = now,
                UpdatedAt = now
            };

            account.Balance += model.Amount * transactionType.BalanceEffect;
            account.UpdatedAt = now;

            _context.AccountTransactions.Add(accountTransaction);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ServiceResult<int>.Success(accountTransaction.TransactionId);
        }

        public async Task<ServiceResult> UpdateAsync(AccountTransactionFormViewModel model)
        {
            var existingTransaction = await _context.AccountTransactions
                .Include(t => t.BettingAccount)
                .FirstOrDefaultAsync(t => t.TransactionId == model.TransactionId);

            if (existingTransaction == null)
            {
                return ServiceResult.Failure("Transaction could not be found.");
            }

            var oldTransactionType = await _context.TransactionTypes
                .FirstOrDefaultAsync(t => t.TransactionTypeId == existingTransaction.TransactionTypeId);

            if (oldTransactionType == null)
            {
                return ServiceResult.Failure("The existing transaction type could not be found.");
            }

            if (existingTransaction.BettingAccount == null ||
                existingTransaction.BettingAccount.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResult.Failure("Transactions cannot be edited on a closed account.");
            }

            var validation = await ValidateModelAsync(model, existingTransaction.AccountId);
            if (!validation.Succeeded)
            {
                return ServiceResult.Failure(validation.ErrorMessage ?? "Transaction is invalid.");
            }

            var account = validation.Value!.Account;
            var newTransactionType = validation.Value.TransactionType;
            var now = DateTime.UtcNow;

            await using var transaction = await _context.Database.BeginTransactionAsync();

            account.Balance -= existingTransaction.Amount * oldTransactionType.BalanceEffect;
            account.Balance += model.Amount * newTransactionType.BalanceEffect;
            account.UpdatedAt = now;

            existingTransaction.TransactionTypeId = model.TransactionTypeId;
            existingTransaction.RelatedBetId = model.RelatedBetId;
            existingTransaction.Amount = model.Amount;
            existingTransaction.TransactionDate = model.TransactionDate.Date;
            existingTransaction.CaptureDate = now;
            existingTransaction.Reference = model.Reference?.Trim();
            existingTransaction.Notes = model.Notes?.Trim();
            existingTransaction.UpdatedAt = now;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var accountTransaction = await _context.AccountTransactions
                .Include(t => t.BettingAccount)
                .Include(t => t.TransactionType)
                .FirstOrDefaultAsync(t => t.TransactionId == id);

            if (accountTransaction == null)
            {
                return ServiceResult.Failure("Transaction could not be found.");
            }

            if (accountTransaction.BettingAccount == null ||
                accountTransaction.BettingAccount.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResult.Failure("Transactions cannot be deleted from a closed account.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            accountTransaction.BettingAccount.Balance -= accountTransaction.Amount * accountTransaction.TransactionType!.BalanceEffect;
            accountTransaction.BettingAccount.UpdatedAt = DateTime.UtcNow;

            _context.AccountTransactions.Remove(accountTransaction);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ServiceResult.Success();
        }

        private async Task<ServiceResult<TransactionValidationContext>> ValidateModelAsync(
            AccountTransactionFormViewModel model,
            int? originalAccountId = null)
        {
            if (model.Amount <= 0m)
            {
                return ServiceResult<TransactionValidationContext>.Failure("Transaction amount must be greater than zero.");
            }

            if (model.TransactionDate.Date > DateTime.Today)
            {
                return ServiceResult<TransactionValidationContext>.Failure("Transaction date cannot be in the future.");
            }

            if (originalAccountId.HasValue && originalAccountId.Value != model.AccountId)
            {
                return ServiceResult<TransactionValidationContext>.Failure("A transaction cannot be moved to a different account.");
            }

            var account = await _context.BettingAccounts
                .FirstOrDefaultAsync(a => a.AccountId == model.AccountId);

            if (account == null)
            {
                return ServiceResult<TransactionValidationContext>.Failure("The selected account could not be found.");
            }

            if (account.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResult<TransactionValidationContext>.Failure("No transactions may be posted to a closed account.");
            }

            var transactionType = await _context.TransactionTypes
                .FirstOrDefaultAsync(t => t.TransactionTypeId == model.TransactionTypeId && t.IsActive);

            if (transactionType == null)
            {
                return ServiceResult<TransactionValidationContext>.Failure("The selected transaction type is not available.");
            }

            return ServiceResult<TransactionValidationContext>.Success(new TransactionValidationContext(account, transactionType));
        }

        private sealed record TransactionValidationContext(BettingAccount Account, TransactionType TransactionType);
    }
}
