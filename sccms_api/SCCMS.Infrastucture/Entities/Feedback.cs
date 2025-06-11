using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Infrastucture.Entities
{
    public class Feedback
    {
        public int Id { get; set; }
        public string StudentCode { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }  

        [Required]
        public string Content { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime SubmissionDate { get; set; }
    }
}
