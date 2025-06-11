using System.ComponentModel.DataAnnotations;
using Utility;

namespace SCCMS.Domain.DTOs.UserDtos
{
    public class UserUpdateDto
    {
        [Required(ErrorMessage = "UserName là trường bắt buộc")]
        [MaxLength(50, ErrorMessage = "UserName không được vượt quá 50 ký tự")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Email là trường bắt buộc")]
        [EmailAddress(ErrorMessage = "Email sai định dạng")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "FullName là trường bắt buộc")]
        [MaxLength(100, ErrorMessage = "FullName không được vượt quá 100 ký tự")]
        public string? FullName { get; set; }

        [Phone(ErrorMessage = "Số điện thoại sai định dạng")]
        public string? PhoneNumber { get; set; }

        public Gender? Gender { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [MaxLength(200, ErrorMessage = "Address không được vượt quá 200 ký tự")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "NationalId là trường bắt buộc")]
        [MaxLength(20, ErrorMessage = "NationalId không được vượt quá 20 ký tự")]
        public string? NationalId { get; set; }

        [Required(ErrorMessage = "RoleId là trường bắt buộc")]
        public int? RoleId { get; set; }

        [Required(ErrorMessage = "Status là trường bắt buộc")]
        public UserStatus? Status { get; set; }
    }
}
