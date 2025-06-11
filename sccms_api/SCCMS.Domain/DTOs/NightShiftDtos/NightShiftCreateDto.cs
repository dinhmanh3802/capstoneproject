using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.DTOs.NightShiftDtos
{
    public class NightShiftCreateDto
    {
        [Required(ErrorMessage = "CourseId không được để trống.")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Thời gian bắt đầu không được để trống.")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Thời gian kết thúc không được để trống.")]
        public TimeSpan EndTime { get; set; }

        [StringLength(200, ErrorMessage = "Ghi chú không được vượt quá 200 ký tự.")]
        public string Note { get; set; }
    }
}
