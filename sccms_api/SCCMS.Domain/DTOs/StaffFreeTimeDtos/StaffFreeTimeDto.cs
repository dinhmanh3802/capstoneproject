using SCCMS.Domain.DTOs.UserDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.DTOs.StaffFreeTimeDtos
{
    public class StaffFreeTimeDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public Gender Gender { get; set; }
        public int CourseId { get; set; }
        public DateTime Date { get; set; }
        public bool isCancel { get; set; }
    }
}
