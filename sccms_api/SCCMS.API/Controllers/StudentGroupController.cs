using Microsoft.AspNetCore.Mvc;
using SCCMS.Domain.DTOs.StudentGroupDtos;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using System.Net;
using Utility;

namespace SCCMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentGroupController : ControllerBase
    {
        private readonly IStudentGroupService _studentGroupService;

        public StudentGroupController(IStudentGroupService studentGroupService)
        {
            _studentGroupService = studentGroupService;
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetStudentGroupByCourseId(int courseId)
        {
            try
            {
                var studentGroups = await _studentGroupService.GetAllStudentGroupByCourseIdAsync(courseId);

                return Ok(new ApiResponse(HttpStatusCode.OK, true, studentGroups));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { "Đã xảy ra lỗi trong quá trình xử lý.", ex.Message }));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentGroupById(int id)
        {
            try
            {
                var studentGroup = await _studentGroupService.GetStudentGroupByIdAsync(id);
                if (studentGroup == null)
                {
                    return NotFound(new ApiResponse(HttpStatusCode.NotFound, false, null));
                }
                return Ok(new ApiResponse(HttpStatusCode.OK, true, studentGroup));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateStudentGroup([FromBody] StudentGroupCreateDto studentGroupDto)
        {
            try
            {
                await _studentGroupService.CreateStudentGroupAsync(studentGroupDto);
                return StatusCode((int)HttpStatusCode.Created, new ApiResponse(HttpStatusCode.Created, true, "Tạo thành công"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { "InternalServerError", ex.Message }));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudentGroup(int id, [FromBody] StudentGroupUpdateDto studentGroupDto)
        {
            try
            {
                await _studentGroupService.UpdateStudentGroupAsync(id, studentGroupDto);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Cập nhật thành công"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { "InternalServerError", ex.Message, ex.InnerException?.ToString() }));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudentGroup(int id)
        {
            try
            {
                await _studentGroupService.DeleteStudentGroupAsync(id);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Xóa thành công"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { "InternalServerError", ex.Message, ex.InnerException?.ToString() }));
            }
        }
        

    }
}
