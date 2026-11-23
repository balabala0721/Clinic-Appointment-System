using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicSystem.Models.Account
{
    public enum IdType
    {
        NRIC,
        Passport
    }

    public enum Gender
    {
        Male,
        Female,
        Other
    }

    public class Patient
    {
        [Required]
        [Key]
        [MaxLength(11)]
        public int PatientId { get; set; }

        [Required]
        [MaxLength(11)]
        public int? UserId { get; set; }

        [Required]
        public IdType IdType { get; set; } = IdType.NRIC;

        [MaxLength(20)]
        [Required]
        public string IdNumber { get; set; } = " ";

        [Required]
        public Gender Gender { get; set; }

        [Required]
        public DateOnly DateOfBirth { get; set; }

        [MaxLength(100)]
        public string EmergencyContactName { get; set; } = " ";

        [MaxLength(20)]
        public string EmergencyContactPhone { get; set; } = " ";

        public string? Address { get; set; }

        public User User { get; set; } = new User();
    }
}
