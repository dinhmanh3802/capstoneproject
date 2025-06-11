using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.DTOs.ReportDtos
{
    public class StudentReportViewDto
    {
        public DateTime? Date { get; set; }
        public StudentReportStatus? Status { get; set; }
        public ReportType? ReportType { get; set; }
        public string? Content { get; set; }
    }
}
