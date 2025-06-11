using System.ComponentModel.DataAnnotations;
using Utility;
namespace SCCMS.Infrastucture.Entities
{
    public class NightShiftAssignment: BaseEntity
    {
        public int Id { get; set; }

        public int NightShiftId { get; set; }
        public NightShift? NightShift { get; set; }  

        public int? UserId { get; set; }
        public User? User { get; set; }
        public DateTime Date { get; set; }

        public int? RoomId { get; set; }
        public Room? Room { get; set; }

        [StringLength(50)]
        public NightShiftAssignmentStatus Status { get; set; }

        [StringLength(255)]
        public string? RejectionReason { get; set; }
    }
}
