using System.ComponentModel.DataAnnotations;

namespace SCCMS.Domain.DTOs.UserDtos
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Mật khẩu cũ là trường bắt buộc")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Mật khẩu mới là trường bắt buộc")]
        [MinLength(8, ErrorMessage = "Mật khẩu mới phải có ít nhất 8 ký tự")]
        public string NewPassword { get; set; }
    }
}
