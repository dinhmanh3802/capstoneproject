using SCCMS.Domain.DTOs.StudentGroupDtos;
using SCCMS.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.DTOs.RoomDtos
{
    public class RoomDto
    {
        public int Id { get; set; }
        public int? CourseId { get; set; }
        public string? CourseName { get; set; }
        public string Name { get; set; }
        public Gender Gender { get; set; }
        public int? NumberOfStaff { get; set; }
        public ICollection<StudentGroupDto>? StudentGroups { get; set; }
    }
}
