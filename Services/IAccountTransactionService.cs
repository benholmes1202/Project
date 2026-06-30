using Project.Models;
using Project.ViewModels.AccountTransactions;

namespace Project.Services
{
    public interface IAccountTransactionService
    {
        Task<IReadOnlyList<AccountTransaction>> GetAllAsync();
        Task<AccountTransaction?> GetDetailsAsync(int id);
        Task<AccountTransaction?> GetForEditAsync(int id);
        Task<ServiceResult<int>> CreateAsync(AccountTransactionFormViewModel model);
        Task<ServiceResult<int>> CreateUserMoneyMovementAsync(int accountId, int userId, decimal amount, string transactionTypeName);
        Task<ServiceResult> UpdateAsync(AccountTransactionFormViewModel model);
        Task<ServiceResult> DeleteAsync(int id);
    }
}
