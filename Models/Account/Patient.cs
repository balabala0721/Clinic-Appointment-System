using Microsoft.EntityFrameworkCore;
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

    public enum Nationality
    {
        Malaysian,
        Singaporean,
        Indonesian,
        Thai,
        Vietnamese,
        Filipino,
        Chinese,
        Combodian,
        Other
    }

    [Index("IdNumber", IsUnique = true)]
    [Index("EmergencyContactPhone", IsUnique = true)]   
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int? UserId { get; set; }

        public User User { get; set; } = null!;

        public IdType? IdType { get; set; }

        [MaxLength(20)]
        public string? IdNumber { get; set; }

        public Gender? Gender { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        [MaxLength(100)]
        public string? EmergencyContactName { get; set; }

        [MaxLength(20)]
        public string? EmergencyContactPhone { get; set; }

        public Nationality? Nationality { get; set; }

        public string? Address { get; set; }
    }
}
