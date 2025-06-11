using SCCMS.Domain.DTOs.RoomDtos;
using SCCMS.Domain.DTOs.UserDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.DTOs.NightShiftDtos
{
    public class NightShiftAssignmentDto
    {
        public int Id { get; set; }
        public int NightShiftId { get; set; }
        public NightShiftDto NightShift { get; set; }
        public int? UserId { get; set; }  
        public UserDto? User { get; set; }
        public int? RoomId { get; set; }
        public RoomDto Room { get; set; }
        public NightShiftAssignmentStatus Status { get; set; }
        public DateTime Date { get; set; }
    }
}
