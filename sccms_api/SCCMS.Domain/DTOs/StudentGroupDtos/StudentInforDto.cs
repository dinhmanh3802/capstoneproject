using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.DTOs.StudentGroupDtos
{
    public class StudentInforDto
    {
        public string StudentCode { get; set; }
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public Gender Gender { get; set; }
    }
}
