using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Infrastucture.Entities
{
    public class StudentReport : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public int ReportId { get; set; }
        public Report Report { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; }

        public StudentReportStatus Status { get; set; }
        public string? Comment { get; set; }
    }
}
