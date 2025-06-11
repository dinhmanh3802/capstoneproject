using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SCCMS.Domain.DTOs.UserDtos;
using SCCMS.Infrastucture.Entities;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync(
          string? name = null, string? email = null, string? phoneNumber = null,
          UserStatus? status = null,
          Gender? gender = null,
          int? roleId = null,
          DateTime? startDate = null,
          DateTime? endDate = null);
        Task<UserDto?> GetUserByIdAsync(int id);
        Task CreateUserAsync(UserCreateDto userCreateDto);
        Task UpdateUserAsync(int userId, UserUpdateDto userUpdateDto);
        Task DeleteUserAsync(int id);
        Task ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task ChangeUserStatusAsync(List<int> userIds, UserStatus newStatus);
        Task<UserDto?> GetUserByUsernameAsync(string username);
        Task ResetPasswordAsync(int userId, UserResetPasswordDto resetPasswordDto);
        Task<byte[]> GenerateExcelTemplateAsync();
        Task<BulkCreateUsersResultDto> BulkCreateUsersAsync(IFormFile file);
        Task ChangeUserRoleAsync(List<int> userIds, int newRoleId);
        Task<IEnumerable<UserDto>> GetAvailableSupervisorsAsync();

    }
}
