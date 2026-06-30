using System.ComponentModel.DataAnnotations;

namespace Project.ViewModels.BetMatches
{
    public class BetMatchFormViewModel
    {
        public int BetMatchId { get; set; }

        [Required]
        [StringLength(120)]
        [Display(Name = "Home Team")]
        public string HomeTeam { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        [Display(Name = "Away Team")]
        public string AwayTeam { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Sport { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Match Date")]
        public DateTime MatchDate { get; set; } = DateTime.Now.AddDays(1);

        [Range(1.01, 1000)]
        [Display(Name = "Home Odds")]
        public decimal HomeOdds { get; set; }

        [Range(1.01, 1000)]
        [Display(Name = "Away Odds")]
        public decimal AwayOdds { get; set; }

        [Range(1.01, 1000)]
        [Display(Name = "Draw Odds")]
        public decimal? DrawOdds { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
