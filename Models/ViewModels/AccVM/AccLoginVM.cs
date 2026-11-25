using System.ComponentModel.DataAnnotations;

namespace ClinicSystem.Models.ViewModels.AccVM
{
    public class AccLoginVM
    {
        [Required(ErrorMessage = "Email is required*")]
        [EmailAddress(ErrorMessage = "Invalid email address*")]
        public string Email { get; set; } = " ";

        [Required(ErrorMessage = "Password is required*")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = " ";
    }
}
