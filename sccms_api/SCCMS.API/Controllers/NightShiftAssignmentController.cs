using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SCCMS.Domain.DTOs.NightShiftDtos;
using SCCMS.Domain.Services.Implements;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using System.Net;
using Utility;

namespace SCCMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NightShiftAssignmentController : ControllerBase
    {
        private readonly INightShiftAssignmentService _nightShiftAssignmentService;

        public NightShiftAssignmentController(INightShiftAssignmentService nightShiftAssignmentService)
        {
            _nightShiftAssignmentService = nightShiftAssignmentService;
        }

        // GET: api/NightShiftAssignment
        [HttpGet]
        public async Task<IActionResult> GetAllAssignments([FromQuery] int courseId, [FromQuery] DateTime? dateTime, [FromQuery] NightShiftAssignmentStatus? status)
        {
            try
            {
                var assignments = await _nightShiftAssignmentService.GetShiftsByCourseAsync(courseId, dateTime, status);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, assignments));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpGet("my-nightShift")]
        public async Task<IActionResult> GetAllAssignments([FromQuery] int? courseId, [FromQuery] int userId)
        {
            try
            {
                var assignments = await _nightShiftAssignmentService.GetShiftsByCourseAndUserAsync(courseId, userId);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, assignments));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        // GET: api/NightShiftAssignment/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAssignmentById(int id)
        {
            try
            {
                var assignment = await _nightShiftAssignmentService.GetNightShiftsByIdAsync(id);
                if (assignment == null)
                {
                    return NotFound(new ApiResponse(HttpStatusCode.NotFound, false, null));
                }
                return Ok(new ApiResponse(HttpStatusCode.OK, true, assignment));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpPost("auto-assign")]
        public async Task<IActionResult> AutoAssignNightShifts([FromQuery] int courseId)
        {
            try
            {
                await _nightShiftAssignmentService.ScheduleShiftsAsync(courseId);

                return StatusCode((int)HttpStatusCode.Created, new ApiResponse(HttpStatusCode.Created, true));
            }
            catch (ArgumentException ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpGet("suggest")]
        public async Task<IActionResult> SuggestStaff([FromQuery] DateTime date, [FromQuery] int shiftId, [FromQuery] int roomId, [FromQuery] int courseId)
        {
            try
            {
                var suggestedStaffs = await _nightShiftAssignmentService.SuggestStaffForShiftAsync(date, shiftId, roomId, courseId);

                return Ok(new ApiResponse(HttpStatusCode.OK, true, suggestedStaffs));
            }
            catch (ArgumentException ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        // POST: api/NightShiftAssignment/AssignStaff
        [HttpPost("AssignStaff")]
        public async Task<IActionResult> AssignStaffToShift([FromBody] NightShiftAssignmentCreateDto assignmentDto)
        {
            try
            {
                await _nightShiftAssignmentService.AssignStaffToShiftAsync(assignmentDto);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Phân công nhân viên thành công"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { "Dữ liệu không hợp lệ", ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { "Đã xảy ra lỗi trong quá trình xử lý.", ex.Message }));
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateNightShiftAssignment(int id, [FromBody] NightShiftAssignmentUpdateDto updateDto)
        {
            // Lấy roleId từ token
            var roleIdString = User.Claims.FirstOrDefault(c => c.Type == "roleId")?.Value;
            if (string.IsNullOrEmpty(roleIdString))
            {
                return Unauthorized("Không thể xác định quyền của người dùng.");
            }

            if (!int.TryParse(roleIdString, out int roleId))
            {
                return Unauthorized("RoleId không hợp lệ.");
            }

            try
            {
                await _nightShiftAssignmentService.UpdateAssignmentAsync(updateDto, roleId);
                return Ok("Cập nhật thành công.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Đã xảy ra lỗi khi cập nhật ca trực.");
            }
        }

        // DELETE: api/NightShiftAssignment/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            try
            {
                await _nightShiftAssignmentService.DeleteAssignmentAsync(id);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Xóa phân công thành công"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { "Dữ liệu không hợp lệ", ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { "Đã xảy ra lỗi trong quá trình xử lý.", ex.Message }));
            }
        }

        [HttpPatch("update-status/{id}")]
        public async Task<IActionResult> UpdateNightShiftAssignmentStatus(int id, [FromBody] NightShiftAssignmentRejectDto updateDto)
        {
            if (id != updateDto.Id)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, "ID trong URL và body không khớp."));
            }

            // Lấy roleId từ token
            var roleIdString = User.Claims.FirstOrDefault(c => c.Type == "roleId")?.Value;
            if (string.IsNullOrEmpty(roleIdString))
            {
                return Unauthorized("Không thể xác định quyền của người dùng.");
            }

            if (!int.TryParse(roleIdString, out int roleId))
            {
                return Unauthorized("RoleId không hợp lệ.");
            }

            try
            {
                await _nightShiftAssignmentService.UpdateAssignmentStatusAsync(updateDto, roleId);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Cập nhật trạng thái ca trực thành công."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
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

        [HttpPatch("reassign")]
        // [Authorize(Roles = "Manager,Secretary")] // Nếu bạn sử dụng authorization
        public async Task<IActionResult> ReassignNightShiftAssignment([FromBody] NightShiftAssignmentReassignDto reassignDto)
        {

            // Lấy roleId từ token
            var roleIdString = User.Claims.FirstOrDefault(c => c.Type == "roleId")?.Value;
            if (string.IsNullOrEmpty(roleIdString))
            {
                return Unauthorized("Không thể xác định quyền của người dùng.");
            }

            if (!int.TryParse(roleIdString, out int roleId))
            {
                return Unauthorized("RoleId không hợp lệ.");
            }

            try
            {
                await _nightShiftAssignmentService.ReassignAssignmentAsync(reassignDto, roleId);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Gán lại ca trực thành công."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return Ok(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { "Đã xảy ra lỗi trong quá trình xử lý.", ex.Message }));
            }
        }


    }
}
