using System.ComponentModel.DataAnnotations;

namespace Project.ViewModels.Bets
{
    public class PlaceBetViewModel
    {
        [Required]
        [Display(Name = "Betting Account")]
        public int AccountId { get; set; }

        [Required]
        public int BetMatchId { get; set; }

        [Required]
        [StringLength(20)]
        public string Selection { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 999999999.99, ErrorMessage = "Stake must be greater than zero.")]
        [Display(Name = "Stake")]
        public decimal Amount { get; set; }
    }
}
