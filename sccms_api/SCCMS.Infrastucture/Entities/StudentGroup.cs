using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;
using System.Text.Json.Serialization;

namespace SCCMS.Infrastucture.Entities
{
    public class StudentGroup : BaseEntity
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }
        [Required]
        [StringLength(100)]
        public string GroupName { get; set; }
        public Gender Gender { get; set; }
        public int? RoomId { get; set; }
        public Room? Room { get; set; }
        public ICollection<StudentGroupAssignment>? StudentGroupAssignment { get; set; }
        public ICollection<SupervisorStudentGroup>? SupervisorStudentGroup { get; set; }
        public ICollection<Report>? Report { get; set; }
    }
}
