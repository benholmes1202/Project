using Project.Models;
using Project.ViewModels.BettingAccounts;

namespace Project.Services
{
    public interface IBettingAccountService
    {
        Task<IReadOnlyList<BettingAccount>> GetAllAsync();
        Task<BettingAccount?> GetDetailsAsync(int id);
        Task<BettingAccount?> GetForEditAsync(int id);
        Task<ServiceResult<int>> CreateAsync(BettingAccountFormViewModel model);
        Task<ServiceResult> UpdateAsync(BettingAccountFormViewModel model);
        Task<ServiceResult> CloseAsync(int id);
        Task<ServiceResult> ReopenAsync(int id);
        Task<ServiceResult> DeleteAsync(int id);
        Task<bool> AccountNumberExistsAsync(string accountNumber, int? excludeAccountId = null);
    }
}
