using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace SCCMS.Infrastucture.Entities
{
    public class User : BaseEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string? UserName { get; set; }

        [Required]
        public string? PasswordHash { get; set; } = null;

        
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [MaxLength(100)]
        public string? FullName { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public Gender? Gender { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }

        [Required]
        [MaxLength(20)]
        public string? NationalId { get; set; }

        public UserStatus? Status { get; set; }

        [ForeignKey("RoleId")]
        [Required]
        public int? RoleId { get; set; }
        public Role? Role { get; set; }

        public ICollection<SupervisorStudentGroup>? SupervisorStudentGroup { get; set; }
        public ICollection<NightShiftAssignment>? NightShiftAssignment { get; set; }
        public ICollection<Post>? Post { get; set; }
        public ICollection<Report>? Report { get; set; }
    }
}