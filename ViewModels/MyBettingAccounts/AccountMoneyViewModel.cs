using System.ComponentModel.DataAnnotations;

namespace Project.ViewModels.MyBettingAccounts
{
    public class AccountMoneyViewModel
    {
        public int AccountId { get; set; }

        public string AccountNumber { get; set; } = string.Empty;

        public decimal Balance { get; set; }

        [Required]
        [Range(0.01, 999999999.99, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }
    }
}
