using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ClinicSystem.Models.Account
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

    [Index("Username", IsUnique = true)]
    [Index("Email", IsUnique = true)]
    [Index("PhoneNo", IsUnique = true)]
    public class User
    {
        [Key]
        [Required]
        [MaxLength(11)]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = " ";

        [Required]
        [MaxLength(50)]
        public string Email { get; set; } = " ";

        [Required]
        [MaxLength(20)]
        public string PhoneNo { get; set; } = " ";

        [Required]
        [MaxLength(256)]
        public string PasswordHash { get; set; } = " ";

        [Required]
        public Role Role { get; set; } = Role.Patient;

        [Required]
        public AccountStatus AccountStatus { get; set; } = AccountStatus.Active;

        [Required]
        public DateTime CreatedAt { get; set; }

        public string PhotoUrl { get; set; } = " ";
    }
}
