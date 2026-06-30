using Project.Models;

namespace Project.Services
{
    public interface IApplicationUserService
    {
        Task<PagedResult<ApplicationUser>> GetUsersAsync(string? searchTerm, int pageNumber, int pageSize);
        Task<ApplicationUser?> GetByIdAsync(int id);
        Task<bool> CreateAsync(ApplicationUser user);
        Task<bool> UpdateAsync(ApplicationUser user);
        Task<ServiceResult> DeleteAsync(int id, string? currentIdentityUserId);
        Task<bool> CanDeleteAsync(int id);
        Task<bool> IdNumberExistsAsync(string idNumber, int? excludeId = null);
    }
}
