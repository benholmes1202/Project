using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class BetMatch
    {
        [Key]
        public int BetMatchId { get; set; }

        [Required]
        [StringLength(120)]
        public string HomeTeam { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        public string AwayTeam { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Sport { get; set; } = string.Empty;

        [Required]
        public DateTime MatchDate { get; set; }

        [Range(1.01, 1000)]
        public decimal HomeOdds { get; set; }

        [Range(1.01, 1000)]
        public decimal AwayOdds { get; set; }

        [Range(1.01, 1000)]
        public decimal? DrawOdds { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public ICollection<Bet> Bets { get; set; } = new List<Bet>();
    }
}
