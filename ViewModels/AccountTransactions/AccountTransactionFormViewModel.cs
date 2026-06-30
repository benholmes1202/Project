using System.ComponentModel.DataAnnotations;

namespace Project.ViewModels.AccountTransactions
{
    public class AccountTransactionFormViewModel
    {
        public int TransactionId { get; set; }

        [Required]
        [Display(Name = "Betting Account")]
        public int AccountId { get; set; }

        [Required]
        [Display(Name = "Transaction Type")]
        public int TransactionTypeId { get; set; }

        [Display(Name = "Related Bet")]
        public int? RelatedBetId { get; set; }

        [Required]
        [Range(0.01, 999999999.99, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Transaction Date")]
        public DateTime TransactionDate { get; set; } = DateTime.Today;

        [StringLength(100)]
        public string? Reference { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Display(Name = "Capture Date")]
        public DateTime? CaptureDate { get; set; }
    }
}
