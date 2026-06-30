using Project.Models;
using Project.ViewModels.Bets;

namespace Project.Services
{
    public interface IBetService
    {
        Task<IReadOnlyList<Bet>> GetAllAsync();
        Task<IReadOnlyList<Bet>> GetForUserAsync(int userId);
        Task<Bet?> GetByIdAsync(int id);
        Task<ServiceResult<int>> PlaceBetAsync(PlaceBetViewModel model, int userId);
    }
}
