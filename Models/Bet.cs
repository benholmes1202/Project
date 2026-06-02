using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Bet
    {
        [Key]
        public int BetId { get; set; }

        [MaxLength(100)] 
        public int AccountId { get; set; }

        public BettingAccount? BettingAccount { get; set; }

        [MaxLength(100)]
        public string Category { get; set; } = "";

        [Precision(16, 2)]
        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}
