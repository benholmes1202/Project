using Project.Models;

public interface IApplicationUserService
{
    Task<List<ApplicationUser>> GetUsersAsync(string? searchTerm);
    Task<ApplicationUser?> GetByIdAsync(int id);
    Task<bool> CreateAsync(ApplicationUser user);
    Task<bool> UpdateAsync(ApplicationUser user);
    Task<bool> DeleteAsync(int id);
    Task<bool> CanDeleteAsync(int id);
    Task<bool> IdNumberExistsAsync(string idNumber, int? excludeId = null);
}
