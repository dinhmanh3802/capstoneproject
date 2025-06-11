using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.DTOs.NightShiftDtos
{
    public class NightShiftUpdateDto
    {
        [Required(ErrorMessage = "Id không được để trống.")]
        public int Id { get; set; }
        [Required(ErrorMessage = "CourseId không được để trống.")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Thời gian bắt đầu không được để trống.")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Thời gian kết thúc không được để trống.")]
        public TimeSpan EndTime { get; set; }

        [StringLength(200, ErrorMessage = "Ghi chú không được vượt quá 200 ký tự.")]
        public string? Note { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartTime >= EndTime)
            {
                yield return new ValidationResult(
                    "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc.",
                    new[] { nameof(StartTime), nameof(EndTime) });
            }
        }
    }
}
