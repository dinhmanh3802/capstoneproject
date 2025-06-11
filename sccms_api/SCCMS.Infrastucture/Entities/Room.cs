using System.ComponentModel.DataAnnotations;
using Utility;

namespace SCCMS.Infrastucture.Entities
{
    public class Room : BaseEntity
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; } 

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public Gender Gender { get; set; }  

        public int NumberOfStaff { get; set; }
        public ICollection<NightShiftAssignment> NightShiftAssignment { get; set; }
        public ICollection<StudentGroup>? StudentGroups { get; set; }
    }
}
