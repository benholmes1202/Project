using Project.Models;
using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class AccountTransaction
    {
        [Key]
        public int TransactionId { get; set; }

        public int AccountId { get; set; }

        public BettingAccount? BettingAccount { get; set; }

        public int TransactionTypeId { get; set; }

        public TransactionType? TransactionType { get; set; }

        public int? RelatedBetId { get; set; }

        public Bet? RelatedBet { get; set; }

        public decimal Amount { get; set; }

        public DateTime TransactionDate { get; set; }

        public DateTime CaptureDate { get; set; }

        [StringLength(100)]
        public string? Reference { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}