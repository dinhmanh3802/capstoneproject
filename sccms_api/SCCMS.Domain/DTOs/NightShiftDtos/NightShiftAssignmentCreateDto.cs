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
    public class NightShiftAssignmentCreateDto
    {
        public int NightShiftId { get; set; }

        public List<int> UserIds { get; set; }
        public int? RoomId { get; set; }


        public DateTime Date { get; set; }
    }
}
