using Microsoft.EntityFrameworkCore;
using Project.Models;
using Project.ViewModels.BetMatches;

namespace Project.Services
{
    public class BetMatchService : IBetMatchService
    {
        private readonly ApplicationDbContext _context;

        public BetMatchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<BetMatch>> GetAllAsync()
        {
            return await _context.BetMatches
                .AsNoTracking()
                .OrderByDescending(m => m.MatchDate)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<BetMatch>> GetActiveUpcomingAsync()
        {
            return await _context.BetMatches
                .AsNoTracking()
                .Where(m => m.IsActive && m.MatchDate >= DateTime.Now)
                .OrderBy(m => m.MatchDate)
                .ToListAsync();
        }

        public async Task<BetMatch?> GetByIdAsync(int id)
        {
            return await _context.BetMatches
                .AsNoTracking()
                .Include(m => m.Bets)
                .ThenInclude(b => b.BettingAccount)
                .FirstOrDefaultAsync(m => m.BetMatchId == id);
        }

        public async Task<ServiceResult<int>> CreateAsync(BetMatchFormViewModel model)
        {
            if (model.MatchDate <= DateTime.Now)
            {
                return ServiceResult<int>.Failure("Match date must be in the future.");
            }

            var now = DateTime.UtcNow;
            var match = new BetMatch
            {
                HomeTeam = model.HomeTeam.Trim(),
                AwayTeam = model.AwayTeam.Trim(),
                Sport = model.Sport.Trim(),
                MatchDate = model.MatchDate,
                HomeOdds = model.HomeOdds,
                AwayOdds = model.AwayOdds,
                DrawOdds = model.DrawOdds,
                IsActive = model.IsActive,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.BetMatches.Add(match);
            await _context.SaveChangesAsync();
            return ServiceResult<int>.Success(match.BetMatchId);
        }

        public async Task<ServiceResult> UpdateAsync(BetMatchFormViewModel model)
        {
            var match = await _context.BetMatches.FindAsync(model.BetMatchId);
            if (match == null)
            {
                return ServiceResult.Failure("Match could not be found.");
            }

            match.HomeTeam = model.HomeTeam.Trim();
            match.AwayTeam = model.AwayTeam.Trim();
            match.Sport = model.Sport.Trim();
            match.MatchDate = model.MatchDate;
            match.HomeOdds = model.HomeOdds;
            match.AwayOdds = model.AwayOdds;
            match.DrawOdds = model.DrawOdds;
            match.IsActive = model.IsActive;
            match.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var match = await _context.BetMatches
                .Include(m => m.Bets)
                .FirstOrDefaultAsync(m => m.BetMatchId == id);

            if (match == null)
            {
                return ServiceResult.Failure("Match could not be found.");
            }

            if (match.Bets.Any())
            {
                return ServiceResult.Failure("Matches with placed bets cannot be deleted. Mark it inactive instead.");
            }

            _context.BetMatches.Remove(match);
            await _context.SaveChangesAsync();
            return ServiceResult.Success();
        }
    }
}
