using SCCMS.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;
using SCCMS.Domain.DTOs.FeedbackDtos;
using SCCMS.Domain.DTOs.StudentCourseDtos;
using SCCMS.Domain.DTOs.NightShiftDtos;
using SCCMS.Domain.DTOs.StudentGroupDtos;
using SCCMS.Domain.DTOs.RoomDtos;

namespace SCCMS.Domain.DTOs.CourseDtos
{
    public class CourseDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string CourseName { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public int ExpectedStudents { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(20)]
        public CourseStatus Status { get; set; }

        public DateTime StudentApplicationStartDate { get; set; }
        public DateTime StudentApplicationEndDate { get; set; }

        public DateTime VolunteerApplicationStartDate { get; set; }
        public DateTime VolunteerApplicationEndDate { get; set; }
        public DateTime? FreeTimeApplicationStartDate { get; set; }
        public DateTime? FreeTimeApplicationEndDate { get; set; }

      //  public ICollection<StudentCourseMiniDto>? StudentCourses { get; set; }

      //  public ICollection<FeedbackMiniDto>? Feedback { get; set; }

       // public ICollection<NightShiftDto>? NightShift { get; set; }
      //  public ICollection<StudentGroupDto>? StudentGroup { get; set; }
     //   public ICollection<RoomDto>? Room { get; set; }
    }

}
