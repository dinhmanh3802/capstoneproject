using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SCCMS.Domain.DTOs.SupervisorDtos;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using System.Net;
using Utility;

namespace SCCMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupervisorController : ControllerBase
    {
        private readonly ISupervisorService _supervisorService;
        protected ApiResponse _response;

        public SupervisorController(ISupervisorService supervisorService)
        {
            _supervisorService = supervisorService;
            _response = new ApiResponse();
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetSupervisorsByCourseId(
            [FromQuery] int courseId, 
           [FromQuery] string? name = null,
           [FromQuery] string? email = null,
           [FromQuery] string? phoneNumber = null,
           [FromQuery] UserStatus? status = null,
           [FromQuery] Gender? gender = null)
        {
            try
            {
                var supervisors = await _supervisorService.GetSupervisorsByCourseIdAsync(courseId, name, email, phoneNumber, status, gender);

                _response.Result = supervisors;
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;

                return Ok(_response);
            }
            catch (ArgumentException ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);

                return BadRequest(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Đã xảy ra lỗi trong quá trình xử lý.");
                _response.ErrorMessages.Add(ex.Message);
                _response.ErrorMessages.Add(ex.InnerException?.ToString() ?? string.Empty);

                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetSupervisorById(int id)
        {
            try
            {
                var supervisor = await _supervisorService.GetSupervisorByIdAsync(id);

                if (supervisor == null)
                {
                    return NotFound(new ApiResponse
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.NotFound,
                        ErrorMessages = new List<string> { "Không tìm thấy huynh trưởng." }
                    });
                }

                _response.Result = supervisor;
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;

                return Ok(_response);
            }
            catch (ArgumentException ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { "Có lỗi xảy ra.", ex.Message };
                _response.StatusCode = HttpStatusCode.InternalServerError;
                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }

        [HttpPut("change-group")]
        [Authorize]
        public async Task<IActionResult> ChangeSupervisorGroup([FromBody] List<int> supervisorIds, [FromQuery] int newGroupId)
        {
            try
            {
                if (newGroupId < 1)
                {
                    throw new ArgumentException("Chánh không hợp lệ.");
                }

                await _supervisorService.ChangeSupervisorsGroupAsync(supervisorIds, newGroupId);

                _response.Result = "Thay đổi chánh thành công.";
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;

                return Ok(_response);
            }
            catch (ArgumentException ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { "Lỗi nội bộ.", ex.Message };
                _response.StatusCode = HttpStatusCode.InternalServerError;
                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }

        [HttpGet("available")]
        [Authorize]
        public async Task<IActionResult> GetAvailableSupervisorsForCourse([FromQuery] int courseId)
        {
            try
            {
                var supervisors = await _supervisorService.GetAvailableSupervisorsForCourseAsync(courseId);

                _response.Result = supervisors;
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;

                return Ok(_response);
            }
            catch (ArgumentException ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);

                return BadRequest(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Đã xảy ra lỗi trong quá trình xử lý.");
                _response.ErrorMessages.Add(ex.Message);

                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }



    }
}
