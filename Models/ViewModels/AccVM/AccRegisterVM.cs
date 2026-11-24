using System.ComponentModel.DataAnnotations;
using ClinicSystem.Models.Account;

namespace ClinicSystem.Models.ViewModels.AccVM
{
    public class AccRegisterVM
    {
        // ===== Personal Information ===== //
        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Full name must be 5 - 100 characters")]
        public string FullName { get; set; } = " ";

        [Required(ErrorMessage = "ID type is required")]
        [Display(Name = "ID Type")]
        public IdType IdType { get; set; }

        [Required(ErrorMessage = "ID number is required")]
        [Display(Name = "NRIC/Passport Number")]
        public string IdNumber { get; set; } = " ";

        [Required(ErrorMessage = "Nationality is required")]
        [Display(Name = "Nationality")]
        public Nationality Nationality { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [Display(Name = "Gender")]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "Date of birth is required")]
        [Display(Name = "Date of Birth")]
        public DateOnly DateOfBirth { get; set; }

        // ===== Contact Information ===== //
        [Required(ErrorMessage = "Email is required")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = " ";

        [Required(ErrorMessage = "Phone number is required")]
        [Display(Name = "Phone Number")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string PhoneNo { get; set; } = " ";


        // ===== Account Information ===== //
        [Required(ErrorMessage = "Password is required")]
        [Display(Name = "Password")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; } = " ";

        [Required(ErrorMessage = "Confirm password is required")]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = " ";
    }
}
