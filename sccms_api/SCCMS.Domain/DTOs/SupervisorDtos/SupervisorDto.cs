using SCCMS.Domain.DTOs.StudentCourseDtos;
using SCCMS.Domain.DTOs.StudentDtos;
using SCCMS.Domain.DTOs.StudentGroupDtos;
using SCCMS.Domain.DTOs.UserDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.DTOs.SupervisorDtos
{
    public class SupervisorDto
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public Gender? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? NationalId { get; set; }
        public UserStatus? Status { get; set; }
        public GroupInfoDto? Group { get; set; }
    }
}
