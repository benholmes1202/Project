using System.ComponentModel.DataAnnotations;

namespace Project.ViewModels.Account
{
    public class ProfileViewModel
    {
        public int ApplicationUserId { get; set; }

        public string IdentityUserId { get; set; } = string.Empty;

        [Display(Name = "ID Number")]
        public string IdNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Surname is required.")]
        [StringLength(100, ErrorMessage = "Surname cannot exceed 100 characters.")]
        public string Surname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(255, ErrorMessage = "Email address cannot exceed 255 characters.")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [StringLength(30, ErrorMessage = "Phone number cannot exceed 30 characters.")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        public string Role { get; set; } = string.Empty;
    }
}