using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace Project.Models
{
    public class ApplicationUser
    {
        [Key]
        public int UserId { get; set; }

        public string? IdentityUserId { get; set; }

        public IdentityUser? IdentityUser { get; set; }

        [Required]
        [StringLength(20)]
        public string IdNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Surname { get; set; } = string.Empty;

        [Required]
        [EmailAddress] 
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(30)]
        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public ICollection<BettingAccount> BettingAccounts { get; set; } = new List<BettingAccount>();

    }
}