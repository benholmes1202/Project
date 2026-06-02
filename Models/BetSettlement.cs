using Project.Models;
using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class BetSettlement
    {
        [Key]
        public int SettlementId { get; set; }

        public int BetId { get; set; }

        public Bet? Bet { get; set; }

        public DateTime SettledAt { get; set; }

        public decimal PayoutAmount { get; set; }

        public decimal ProfitLoss { get; set; }

        [Required]
        [StringLength(20)]
        public string Result { get; set; } = string.Empty;
    }
}