using Project.Models;
using Project.ViewModels.BetMatches;

namespace Project.Services
{
    public interface IBetMatchService
    {
        Task<IReadOnlyList<BetMatch>> GetAllAsync();
        Task<IReadOnlyList<BetMatch>> GetActiveUpcomingAsync();
        Task<BetMatch?> GetByIdAsync(int id);
        Task<ServiceResult<int>> CreateAsync(BetMatchFormViewModel model);
        Task<ServiceResult> UpdateAsync(BetMatchFormViewModel model);
        Task<ServiceResult> DeleteAsync(int id);
    }
}
