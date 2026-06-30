using Microsoft.EntityFrameworkCore;
using Project.Models;
using Project.ViewModels.Bets;

namespace Project.Services
{
    public class BetService : IBetService
    {
        private readonly ApplicationDbContext _context;

        public BetService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Bet>> GetAllAsync()
        {
            return await _context.Bets
                .AsNoTracking()
                .Include(b => b.BetMatch)
                .Include(b => b.BettingAccount)
                .ThenInclude(a => a!.User)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Bet>> GetForUserAsync(int userId)
        {
            return await _context.Bets
                .AsNoTracking()
                .Include(b => b.BetMatch)
                .Include(b => b.BettingAccount)
                .Where(b => b.BettingAccount != null && b.BettingAccount.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<Bet?> GetByIdAsync(int id)
        {
            return await _context.Bets
                .AsNoTracking()
                .Include(b => b.BetMatch)
                .Include(b => b.BettingAccount)
                .ThenInclude(a => a!.User)
                .FirstOrDefaultAsync(b => b.BetId == id);
        }

        public async Task<ServiceResult<int>> PlaceBetAsync(PlaceBetViewModel model, int userId)
        {
            if (model.Amount <= 0m)
            {
                return ServiceResult<int>.Failure("Stake must be greater than zero.");
            }

            var account = await _context.BettingAccounts
                .FirstOrDefaultAsync(a => a.AccountId == model.AccountId && a.UserId == userId);

            if (account == null)
            {
                return ServiceResult<int>.Failure("The selected account could not be found.");
            }

            if (!account.Status.Equals("Open", StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResult<int>.Failure("Bets can only be placed from open accounts.");
            }

            if (account.Balance < model.Amount)
            {
                return ServiceResult<int>.Failure("Insufficient funds for this bet.");
            }

            var match = await _context.BetMatches
                .FirstOrDefaultAsync(m => m.BetMatchId == model.BetMatchId);

            if (match == null || !match.IsActive || match.MatchDate < DateTime.Now)
            {
                return ServiceResult<int>.Failure("This match is no longer available for betting.");
            }

            var selection = model.Selection.Trim();
            var odds = selection switch
            {
                "Home" => match.HomeOdds,
                "Away" => match.AwayOdds,
                "Draw" when match.DrawOdds.HasValue => match.DrawOdds.Value,
                _ => 0m
            };

            if (odds <= 0m)
            {
                return ServiceResult<int>.Failure("Please choose a valid betting selection.");
            }

            var debitType = await _context.TransactionTypes.FirstOrDefaultAsync(t => t.Name == "Debit");
            if (debitType == null)
            {
                return ServiceResult<int>.Failure("Debit transaction type is missing.");
            }

            var now = DateTime.UtcNow;
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var bet = new Bet
            {
                AccountId = account.AccountId,
                BetMatchId = match.BetMatchId,
                Category = match.Sport,
                Selection = selection,
                Amount = model.Amount,
                Odds = odds,
                PotentialPayout = model.Amount * odds,
                Status = "Placed",
                CreatedAt = now
            };

            _context.Bets.Add(bet);
            await _context.SaveChangesAsync();

            account.Balance -= model.Amount;
            account.UpdatedAt = now;

            _context.AccountTransactions.Add(new AccountTransaction
            {
                AccountId = account.AccountId,
                TransactionTypeId = debitType.TransactionTypeId,
                RelatedBetId = bet.BetId,
                Amount = model.Amount,
                TransactionDate = DateTime.Today,
                CaptureDate = now,
                Reference = $"Bet #{bet.BetId}",
                Notes = $"{selection} selection on {match.HomeTeam} vs {match.AwayTeam}",
                CreatedAt = now,
                UpdatedAt = now
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ServiceResult<int>.Success(bet.BetId);
        }
    }
}
