using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Infrastucture.Entities
{
     public class VolunteerCourse
    {
        [Key]
        public int Id { get; set; }

        public int CourseId { get; set; }
        public Course? Course { get; set; }

        public int VolunteerId { get; set; }
        public Volunteer Volunteer { get; set; }

        public string? VolunteerCode { get; set; }

        [StringLength(20)]
        public ProgressStatus? Status { get; set; }

        [StringLength(255)]
        public string? Note { get; set; }

        public int? ReviewerId { get; set; }
        public User? Reviewer { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ReviewDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime ApplicationDate { get; set; }
    }
}
