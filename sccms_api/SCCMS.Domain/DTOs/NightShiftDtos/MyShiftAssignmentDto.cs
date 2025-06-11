using SCCMS.Domain.DTOs.RoomDtos;
using SCCMS.Domain.DTOs.UserDtos;
using SCCMS.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.DTOs.NightShiftDtos
{
    public class MyShiftAssignmentDto
    {
        public int Id { get; set; }

        public int NightShiftId { get; set; }
        public NightShiftDto? NightShift { get; set; }

        public int? UserId { get; set; }
        public UserDto? User { get; set; }
        public DateTime Date { get; set; }

        public int? RoomId { get; set; }
        public RoomDto Room { get; set; }

        [StringLength(50)]
        public NightShiftAssignmentStatus Status { get; set; }

        [StringLength(255)]
        public string? RejectionReason { get; set; }
        public DateTime DateModified { get; set; }
        public string UpdatedBy { get; set; }
    }
}
