using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCCMS.API.Services;
using SCCMS.Domain.DTOs.UserDtos;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using System.Net;
using System.Security.Claims;
using Utility;

namespace SCCMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IUserService _userService;
        protected ApiResponse _response;

        public UserController(IUserService userService)
        {
            _userService = userService;
            _response = new ApiResponse();
        }

        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllUsersAsync(
       [FromQuery] string? name = null,
       [FromQuery] string? email = null,
       [FromQuery] string? phoneNumber = null,
       [FromQuery] UserStatus? status = null,
       [FromQuery] Gender? gender = null,
       [FromQuery] int? roleId = null,
       [FromQuery] DateTime? startDate = null,
       [FromQuery] DateTime? endDate = null)
        {
            var users = await _userService.GetAllUsersAsync(name, email, phoneNumber, status, gender, roleId, startDate, endDate);
            _response.Result = users;
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }


        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            _response.Result = user;
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDto userCreateDto)
        {
            try
            {
                await _userService.CreateUserAsync(userCreateDto);

                _response.StatusCode = HttpStatusCode.Created;
                _response.IsSuccess = true;
                _response.Result = "Tạo người dùng và gửi email thành công";

                return StatusCode((int)HttpStatusCode.Created, _response);
            }
            catch (ArgumentException ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);

                return StatusCode((int)HttpStatusCode.BadRequest, _response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Lỗi nội bộ");
                _response.ErrorMessages.Add(ex.Message);
                _response.ErrorMessages.Add(ex.InnerException?.ToString());

                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }


        [HttpPut("{userId}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UserUpdateDto userUpdateDto)
        {
            try
            {
                await _userService.UpdateUserAsync(userId, userUpdateDto);

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = "Cập nhật người dùng thành công";

                return StatusCode((int)HttpStatusCode.OK, _response);
            }
            catch (ArgumentException ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Dữ liệu không hợp lệ: " + ex.Message);


                return StatusCode((int)HttpStatusCode.BadRequest, _response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Lỗi nội bộ");
                _response.ErrorMessages.Add(ex.Message);
                _response.ErrorMessages.Add(ex.InnerException?.ToString());

                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                await _userService.DeleteUserAsync(id);

                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                _response.Result = "Xóa người dùng thành công";

                return StatusCode((int)HttpStatusCode.NoContent, _response);
            }
            catch (ArgumentException ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);

                return StatusCode((int)HttpStatusCode.BadRequest, _response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Lỗi nội bộ");
                _response.ErrorMessages.Add(ex.Message);
                _response.ErrorMessages.Add(ex.InnerException?.ToString());

                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }

        }
        [HttpPut("change-password/{userId}")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(int userId, [FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                await _userService.ChangePasswordAsync(userId, changePasswordDto);
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = "Thay đổi mật khẩu thành công.";

                return StatusCode((int)HttpStatusCode.OK, _response);
            }
            catch (ArgumentException ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);

                return StatusCode((int)HttpStatusCode.BadRequest, _response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Lỗi nội bộ");
                _response.ErrorMessages.Add(ex.Message);
                _response.ErrorMessages.Add(ex.InnerException?.ToString());

                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }

        [HttpPut("change-status")]
        [Authorize]
        public async Task<IActionResult> ChangeStatus([FromBody] List<int> userIds, [FromQuery] UserStatus newStatus)
        {
            try
            {
                await _userService.ChangeUserStatusAsync(userIds, newStatus);
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = "Thay đổi trạng thái cho người dùng thành công.";
                return StatusCode((int)HttpStatusCode.OK, _response);
            }
            catch (ArgumentException ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.BadRequest, _response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Lỗi nội bộ");
                _response.ErrorMessages.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }

        [HttpPut("reset-password/{userId}")]
        [Authorize]
        public async Task<IActionResult> ResetPassword(int userId, [FromBody] UserResetPasswordDto resetPasswordDto)
        {
            try
            {
                await _userService.ResetPasswordAsync(userId, resetPasswordDto);
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = "Đặt lại mật khẩu thành công.";

                return StatusCode((int)HttpStatusCode.OK, _response);
            }
            catch (ArgumentException ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);

                return StatusCode((int)HttpStatusCode.BadRequest, _response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Lỗi nội bộ");
                _response.ErrorMessages.Add(ex.Message);
                _response.ErrorMessages.Add(ex.InnerException?.ToString());

                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }

        [HttpGet("download-template")]
        [Authorize]
        public async Task<IActionResult> DownloadExcelTemplate()
        {
            var templateBytes = await _userService.GenerateExcelTemplateAsync();
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = "UserTemplate.xlsx";

            return File(templateBytes, contentType, fileName);
        }


        [HttpPost("bulk-create")]
        [Authorize]
        public async Task<IActionResult> BulkCreateUsers([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Không có file nào được tải lên.");
                return BadRequest(_response);
            }

            try
            {
                var result = await _userService.BulkCreateUsersAsync(file);

                if (result.HasErrors)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = result.Errors;
                    return BadRequest(_response);
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = "Tạo người dùng hàng loạt thành công.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Lỗi nội bộ.");
                _response.ErrorMessages.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }

        [HttpPut("change-role")]
        [Authorize]
        public async Task<IActionResult> ChangeUserRole([FromBody] BulkChangeRoleDto changeUserRoleDto)
        {
            try
            {
                await _userService.ChangeUserRoleAsync(changeUserRoleDto.UserIds, changeUserRoleDto.NewRoleId);
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = "Thay đổi vai trò cho người dùng thành công.";
                return StatusCode((int)HttpStatusCode.OK, _response);
            }
            catch (ArgumentException ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.BadRequest, _response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Lỗi nội bộ");
                _response.ErrorMessages.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }
        [HttpGet("available-supervisors")]
        [Authorize]
        public async Task<IActionResult> GetAvailableSupervisors()
        {
            try
            {
                var supervisors = await _userService.GetAvailableSupervisorsAsync();
                _response.Result = supervisors;
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Lỗi nội bộ");
                _response.ErrorMessages.Add(ex.Message);
                _response.ErrorMessages.Add(ex.InnerException?.ToString());

                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }


    }
}
