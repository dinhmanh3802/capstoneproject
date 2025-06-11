using Utility;

namespace SCCMS.Domain.DTOs.UserDtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public Gender? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? NationalId { get; set; }
        public UserStatus? Status { get; set; }
        public int? RoleId { get; set; }
    }
}
