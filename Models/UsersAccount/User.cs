using System.ComponentModel.DataAnnotations;

namespace ClinicSystem.Models.UsersAccount
{
    public enum Role
    {
        Patient,
        Doctor,
        Admin
    }

    public enum AccountStatus
    {
        Active,
        Blocked,
        Deactivated
    }

    public class User
    {
        [Required]
        [MaxLength(11)]
        public string UserID { get; set; }

        [Required]
        [MaxLength(30)]
        public string Username { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNo { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public Role Role { get; set; }

        [Required]
        public AccountStatus AccountStatus { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Url]
        public string PhotoURL { get; set; }
    }
}
