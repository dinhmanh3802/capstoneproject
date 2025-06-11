using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.DTOs.ReportDtos
{
    public class AttendanceSummaryDto
    {
        public DateTime Date { get; set; }
        public int TotalStudents { get; set; }
        public int TotalPresent { get; set; }
    }

}
