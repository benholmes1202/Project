using System.ComponentModel.DataAnnotations;

namespace Project.ViewModels.BetMatches
{
    public class ResultMatchViewModel
    {
        public int BetMatchId { get; set; }

        public string MatchName { get; set; } = string.Empty;

        public bool HasDrawOdds { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Winning Selection")]
        public string WinningSelection { get; set; } = string.Empty;
    }
}
