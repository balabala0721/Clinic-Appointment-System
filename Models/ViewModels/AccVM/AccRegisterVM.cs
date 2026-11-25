using System.ComponentModel.DataAnnotations;
using ClinicSystem.Models.Account;

namespace ClinicSystem.Models.ViewModels.AccVM
{
    public class AccRegisterVM
    {
        // ===== Personal Information ===== //
        [Required(ErrorMessage = "Full name is required*")]
        [Display(Name = "Full Name*")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Full name can only contain letters and spaces*")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Full name must be between 5 and 100 characters*")]
        public string FullName { get; set; } = " ";

        [Required(ErrorMessage = "Id type is required*")]
        [Display(Name = "Id Type*")]
        [EnumDataType(typeof(IdType))]
        public IdType? IdType { get; set; }  

        [Required(ErrorMessage = "Id number is required*")]
        [Display(Name = "NRIC / Passport Number*")]
        public string IdNumber { get; set; } = " ";

        [Required(ErrorMessage = "Nationality is required*")]
        [Display(Name = "Nationality*")]
        public Nationality Nationality { get; set; }

        [Required(ErrorMessage = "Gender is required*")]
        [Display(Name = "Gender*")]
        public Gender Gender { get; set; }

        [Display(Name = "Date of Birth")]
        public DateOnly? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Email is required*")]
        [Display(Name = "Email*")]
        [EmailAddress(ErrorMessage = "Invalid email address*")]
        public string Email { get; set; } = " ";

        [Required(ErrorMessage = "Password is required*")]
        [Display(Name = "Password*")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&_])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character*")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 20 characters*")]
        public string Password { get; set; } = " ";

        [Required(ErrorMessage = "Confirm password is required*")]
        [Display(Name = "Confirm Password*")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match*")]
        public string ConfirmPassword { get; set; } = " ";
    }
}