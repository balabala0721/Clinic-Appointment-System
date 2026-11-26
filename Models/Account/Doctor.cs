using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicSystem.Models.Account
{
    public class Specialisation
    {
        [Key]
        public int SpecialisationId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = " ";
    }

    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }

        [Required]
        public int? UserId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Qualifications { get; set; } = " ";

        [Required]
        [Range(0, 99)]
        public int YearsOfExperience { get; set; }

        [Required]
        [Precision(4, 2)]
        public decimal ConsultationFee { get; set; }

        public string? Bio { get; set; }

        [Required]
        [MaxLength(11)]
        public int? SpecialisationId { get; set; }

        public Specialisation? Specialisation { get; set; }

        public User User { get; set; } = null!;
    }
}
