using SCCMS.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.DTOs.FeedbackDtos
{
    public class FeedbackCreateDto
    {
        [Required(ErrorMessage = "Mã học sinh là bắt buộc")]
        [StringLength(9, ErrorMessage = "Mã học sinh phải dài 9 chữ số")]
        public string StudentCode { get; set; }
        [Required]
        public int CourseId { get; set; }
        [Required]
        public string Content { get; set; }
    }
}
