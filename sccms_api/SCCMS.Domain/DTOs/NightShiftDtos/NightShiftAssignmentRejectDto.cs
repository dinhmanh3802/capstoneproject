using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.DTOs.NightShiftDtos
{
    public class NightShiftAssignmentRejectDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public NightShiftAssignmentStatus Status { get; set; }

        [StringLength(255)]
        public string? RejectionReason { get; set; }

    }
}
