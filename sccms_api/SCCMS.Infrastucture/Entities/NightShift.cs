using System.ComponentModel.DataAnnotations;

namespace SCCMS.Infrastucture.Entities
{
    public class NightShift: BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }  

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public string? Note { get; set; }

        public ICollection<User>? User { get; set; }
        public ICollection<NightShiftAssignment>? NightShiftAssignment { get; set; }

        public NightShift(int courseId, TimeSpan startTime, TimeSpan endTime)
        {
            CourseId = courseId;
            StartTime = startTime;
            EndTime = endTime;
        }

    }
}
