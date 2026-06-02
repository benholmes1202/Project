using Project.Models;
using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class BettingAccount
    {
        [Key]
        public int AccountId { get; set; }

        public int UserId { get; set; }

        public ApplicationUser? User { get; set; }

        [Required]
        [StringLength(30)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(3)]
        public string CurrencyCode { get; set; } = "ZAR";

        public decimal Balance { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Open";

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        public ICollection<AccountTransaction> AccountTransactions { get; set; } = new List<AccountTransaction>();

        public ICollection<Bet> Bets { get; set; } = new List<Bet>();
    }
}