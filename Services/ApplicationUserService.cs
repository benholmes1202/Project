using Microsoft.EntityFrameworkCore;
using Project.Models;
using Project.Services;


public class ApplicationUserService : IApplicationUserService
{
    private readonly ApplicationDbContext _context;

    public ApplicationUserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ApplicationUser>> GetUsersAsync(string? searchTerm)
    {
        var query = _context.AppUsers
            .Include(u => u.BettingAccounts)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(u =>
                u.IdNumber.Contains(searchTerm) ||
                u.Surname.Contains(searchTerm) ||
                u.BettingAccounts.Any(a => a.AccountNumber.Contains(searchTerm))
            );
        }

        return await query.ToListAsync();
    }

    public async Task<ApplicationUser?> GetByIdAsync(int id)
    {
        return await _context.AppUsers
            .Include(u => u.BettingAccounts)
            .FirstOrDefaultAsync(u => u.UserId == id);
    }

    public async Task<bool> CreateAsync(ApplicationUser user)
    {
        if (await IdNumberExistsAsync(user.IdNumber))
            return false;

        _context.Add(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateAsync(ApplicationUser user)
    {
        if (await IdNumberExistsAsync(user.IdNumber, user.UserId))
            return false;

        _context.Update(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IdNumberExistsAsync(string idNumber, int? excludeId = null)
    {
        var query = _context.AppUsers
            .Where(u => u.IdNumber == idNumber);

        if (excludeId.HasValue)
        {
            query = query.Where(u => u.UserId != excludeId);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> CanDeleteAsync(int id)
    {
        var user = await _context.AppUsers
            .Include(u => u.BettingAccounts)
            .FirstOrDefaultAsync(u => u.UserId == id);

        if (user == null) return false;

        return !user.BettingAccounts.Any() ||
               user.BettingAccounts.All(a => a.Status.ToLower() == "closed");
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (!await CanDeleteAsync(id))
            return false;

        var user = await _context.AppUsers.FindAsync(id);
        if (user == null) return false;

        _context.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
}
