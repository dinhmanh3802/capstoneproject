using SCCMS.Domain.DTOs.StudentGroupDtos;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace SCCMS.Domain.DTOs.RoomDtos
{
    public class RoomCreateDto
    {
        [Required(ErrorMessage = "CourseId không được để trống.")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Tên phòng không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên phòng không được vượt quá 100 ký tự.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Giới tính không được để trống.")]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "Số lượng nhân viên không được để trống.")]
        [Range(1, 10, ErrorMessage = "Số lượng nhân viên phải trong khoảng 1-10.")]
        public int NumberOfStaff { get; set; }
        public int[]? StudentGroupId { get; set; }
    }
}
