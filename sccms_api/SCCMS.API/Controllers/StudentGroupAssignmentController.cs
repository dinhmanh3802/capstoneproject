using Microsoft.AspNetCore.Mvc;
using SCCMS.Domain.DTOs.StudentGroupAssignmentDtos;
using SCCMS.Domain.Services.Implements;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using System.Net;

namespace SCCMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentGroupAssignmentController : ControllerBase
    {
        private readonly IStudentGroupAssignmentService _studentGroupAssignmentService;

        public StudentGroupAssignmentController(IStudentGroupAssignmentService studentGroupAssignmentService)
        {
            _studentGroupAssignmentService = studentGroupAssignmentService;
        }

        [HttpPost]
        public async Task<IActionResult> AddStudentsIntoGroup([FromBody] StudentGroupAssignmentDto studentGroupAssignment)
        {
            try
            {
                await _studentGroupAssignmentService.AddStudentsIntoGroupAsync(studentGroupAssignment);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Đã thêm khóa sinh vào chánh thành công."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { "Đã xảy ra lỗi khi thêm khóa sinh vào chánh.", ex.Message }));
            }
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveStudentsFromGroup([FromBody] StudentGroupAssignmentDto studentGroupAssignment)
        {
            try
            {
                await _studentGroupAssignmentService.RemoveStudentsFromGroupAsync(studentGroupAssignment);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Đã xóa khóa sinh khỏi chánh thành công."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { "Đã xảy ra lỗi khi xóa khóa sinh khỏi chánh.", ex.Message }));
            }
        }

        [HttpPut("auto-assign")]
        public async Task<IActionResult> AutoAssignStudentToStudentGroup([FromQuery] int courseId)
        {
            try
            {
                await _studentGroupAssignmentService.DistributeStudentsByGroupAsync(courseId);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Auto-assignment completed successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpPost("auto-assign-supervisors")]
        public async Task<IActionResult> AutoAssignSupervisors([FromQuery] int courseId)
        {
            try
            {
                var unassignedGroups = await _studentGroupAssignmentService.AutoAssignSupervisorsAsync(courseId);

                if (unassignedGroups != null && unassignedGroups.Any())
                {
                    return Ok(new ApiResponse(HttpStatusCode.OK, true, unassignedGroups));
                }

                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Phân Huynh trưởng tự động thành công. Tất cả chánh đã có Huynh trưởng."));
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
    }
}
