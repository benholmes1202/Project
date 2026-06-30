using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Bet
    {
        [Key]
        public int BetId { get; set; }

        public int AccountId { get; set; }

        public BettingAccount? BettingAccount { get; set; }

        public int? BetMatchId { get; set; }

        public BetMatch? BetMatch { get; set; }

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Selection { get; set; } = string.Empty;

        [Precision(16, 2)]
        public decimal Amount { get; set; }

        [Precision(16, 2)]
        public decimal Odds { get; set; }

        [Precision(16, 2)]
        public decimal PotentialPayout { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Placed";

        public DateTime CreatedAt { get; set; }

    }
}
