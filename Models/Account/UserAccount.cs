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

    [Index("Email", IsUnique = true)]
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = " ";

        [Required]
        [MaxLength(50)]
        public string Email { get; set; } = " ";

        [MaxLength(20)]
        public string? PhoneNo { get; set; }

        [Required]
        [MaxLength(256)]
        public string PasswordHash { get; set; } = " ";

        public Role Role { get; set; } = Role.Patient;

        public AccountStatus AccountStatus { get; set; } = AccountStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? PhotoUrl { get; set; }
    }
}
