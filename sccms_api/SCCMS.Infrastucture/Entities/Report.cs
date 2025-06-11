using System.ComponentModel.DataAnnotations;
using Utility;

namespace SCCMS.Infrastucture.Entities
{
    public class Report : BaseEntity
    {
        public int Id { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }
        public int? StudentGroupId { get; set; }
        public StudentGroup? StudentGroup { get; set; }
        public int? RoomId { get; set; }
        public Room? Room { get; set; }
        public int? NightShiftId { get; set; }
        public NightShift? NightShift { get; set; }
        [DataType(DataType.Date)]
        public DateTime ReportDate { get; set; }

        public string? ReportContent { get; set; }

        [StringLength(50)]
        public ReportType ReportType { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? SubmissionDate { get; set; }
        public int? SubmissionBy { get; set; }
        public User? SubmittedByUser { get; set; } 

        public ReportStatus Status { get; set; } = ReportStatus.NotYet;

        public ICollection<StudentReport>? StudentReports { get; set; }
    }
}
