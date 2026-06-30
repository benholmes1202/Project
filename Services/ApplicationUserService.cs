using Microsoft.EntityFrameworkCore;
using Project.Models;

namespace Project.Services
{
    public class ApplicationUserService : IApplicationUserService
    {
        private readonly ApplicationDbContext _context;

        public ApplicationUserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<ApplicationUser>> GetUsersAsync(string? searchTerm, int pageNumber, int pageSize)
        {
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, 10);

            var query = _context.AppUsers
                .AsNoTracking()
                .Include(u => u.BettingAccounts)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var cleanSearch = searchTerm.Trim();
                query = query.Where(u =>
                    u.IdNumber.Contains(cleanSearch) ||
                    u.Surname.Contains(cleanSearch) ||
                    u.BettingAccounts.Any(a => a.AccountNumber.Contains(cleanSearch)));
            }

            var totalItems = await query.CountAsync();
            var totalPages = Math.Max((int)Math.Ceiling(totalItems / (double)pageSize), 1);
            pageNumber = Math.Min(pageNumber, totalPages);

            var users = await query
                .OrderBy(u => u.Surname)
                .ThenBy(u => u.FirstName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<ApplicationUser>
            {
                Items = users,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }

        public async Task<ApplicationUser?> GetByIdAsync(int id)
        {
            return await _context.AppUsers
                .Include(u => u.BettingAccounts)
                .ThenInclude(a => a.AccountTransactions)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<bool> CreateAsync(ApplicationUser user)
        {
            user.IdNumber = user.IdNumber.Trim();
            user.FirstName = user.FirstName.Trim();
            user.Surname = user.Surname.Trim();
            user.Email = user.Email.Trim();
            user.PhoneNumber = user.PhoneNumber?.Trim();
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            if (await IdNumberExistsAsync(user.IdNumber))
            {
                return false;
            }

            _context.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(ApplicationUser user)
        {
            if (await IdNumberExistsAsync(user.IdNumber.Trim(), user.UserId))
            {
                return false;
            }

            var existingUser = await _context.AppUsers.FindAsync(user.UserId);
            if (existingUser == null)
            {
                return false;
            }

            existingUser.IdNumber = user.IdNumber.Trim();
            existingUser.FirstName = user.FirstName.Trim();
            existingUser.Surname = user.Surname.Trim();
            existingUser.Email = user.Email.Trim();
            existingUser.PhoneNumber = user.PhoneNumber?.Trim();
            existingUser.IsActive = user.IsActive;
            existingUser.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IdNumberExistsAsync(string idNumber, int? excludeId = null)
        {
            var cleanIdNumber = idNumber.Trim();
            var query = _context.AppUsers
                .Where(u => u.IdNumber == cleanIdNumber);

            if (excludeId.HasValue)
            {
                query = query.Where(u => u.UserId != excludeId);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> CanDeleteAsync(int id)
        {
            var user = await _context.AppUsers
                .AsNoTracking()
                .Include(u => u.BettingAccounts)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return false;
            }

            return !user.BettingAccounts.Any() ||
                   user.BettingAccounts.All(a => a.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase));
        }

        public async Task<ServiceResult> DeleteAsync(int id, string? currentIdentityUserId)
        {
            var user = await _context.AppUsers
                .Include(u => u.BettingAccounts)
                .ThenInclude(a => a.AccountTransactions)
                .Include(u => u.BettingAccounts)
                .ThenInclude(a => a.Bets)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return ServiceResult.Failure("User could not be found.");
            }

            if (!string.IsNullOrWhiteSpace(currentIdentityUserId) &&
                user.IdentityUserId == currentIdentityUserId)
            {
                return ServiceResult.Failure("You cannot delete your own user profile while signed in.");
            }

            if (user.BettingAccounts.Any(a => !a.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase)))
            {
                return ServiceResult.Failure("Only users with no accounts or only closed accounts may be deleted.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            foreach (var account in user.BettingAccounts)
            {
                _context.AccountTransactions.RemoveRange(account.AccountTransactions);
                _context.Bets.RemoveRange(account.Bets);
            }

            _context.BettingAccounts.RemoveRange(user.BettingAccounts);
            _context.AppUsers.Remove(user);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ServiceResult.Success();
        }
    }
}
