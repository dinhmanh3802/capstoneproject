using SCCMS.Domain.DTOs.RoomDtos;
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
    public class NightShiftAssignmentUpdateDto
    {
        public int Id { get; set; }

        public int NightShiftId { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }
        public DateTime Date { get; set; }

        public int? RoomId { get; set; }

        public NightShiftAssignmentStatus Status { get; set; }

        [StringLength(255)]
        public string? RejectionReason { get; set; }
    }
}
