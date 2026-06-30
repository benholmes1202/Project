using System.ComponentModel.DataAnnotations;

namespace Project.ViewModels.BettingAccounts
{
    public class BettingAccountFormViewModel
    {
        public int AccountId { get; set; }

        [Required]
        [Display(Name = "User")]
        public int UserId { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Account Number")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(3, MinimumLength = 3)]
        [Display(Name = "Currency")]
        public string CurrencyCode { get; set; } = "ZAR";

        [Display(Name = "Outstanding Balance")]
        public decimal Balance { get; set; }

        public string Status { get; set; } = "Open";
    }
}
