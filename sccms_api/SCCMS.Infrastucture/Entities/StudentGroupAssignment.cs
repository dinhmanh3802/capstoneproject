using Microsoft.EntityFrameworkCore;

namespace SCCMS.Infrastucture.Entities
{
    [PrimaryKey(nameof(StudentId), nameof(StudentGroupId))]
    public class StudentGroupAssignment : BaseEntity
    {
        public int StudentId { get; set; }
   
        public Student Student { get; set; }

        public int StudentGroupId { get; set; }
       
        public StudentGroup StudentGroup { get; set; }
    }
}
