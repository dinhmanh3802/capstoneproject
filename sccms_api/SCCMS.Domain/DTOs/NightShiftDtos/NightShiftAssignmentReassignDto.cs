﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.DTOs.NightShiftDtos
{
    public class NightShiftAssignmentReassignDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int NewUserId { get; set; }
    }

}
