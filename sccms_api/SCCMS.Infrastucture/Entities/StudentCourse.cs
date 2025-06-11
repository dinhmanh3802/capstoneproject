using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Infrastucture.Entities
{
    public class StudentCourse : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public int CourseId { get; set; }
       
        public Course Course { get; set; }

        public int StudentId { get; set; }
        
        public Student Student { get; set; }

        public string? StudentCode { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ApplicationDate { get; set; }

        public ProgressStatus Status { get; set; } = ProgressStatus.Pending;

		[StringLength(500)]
        public string ? Note { get; set; }

        public int? ReviewerId { get; set; }
        public User? Reviewer { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ReviewDate { get; set; }
    }
}
