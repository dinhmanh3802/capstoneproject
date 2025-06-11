using Microsoft.EntityFrameworkCore;


namespace SCCMS.Infrastucture.Entities
{
    [PrimaryKey(nameof(SupervisorId), nameof(StudentGroupId))]
    public class SupervisorStudentGroup : BaseEntity
    {
        public int SupervisorId { get; set; }
        public User Supervisor { get; set; }

        public int StudentGroupId { get; set; }
        public StudentGroup StudentGroup { get; set; }
    }
}
