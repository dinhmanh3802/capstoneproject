using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SCCMS.Domain.DTOs.EmailDtos;
using SCCMS.Domain.DTOs.StudentCourseDtos;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using System.Net;
using Utility;

namespace SCCMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentApplicationController : ControllerBase
    {
        private readonly IStudentApplicationService _studentApplicationService;

        public StudentApplicationController(IStudentApplicationService studentApplicationService)
        {
            _studentApplicationService = studentApplicationService;
        }

        [HttpGet()]
        public async Task<IActionResult> GetAllStudentApplications(int courseId, [FromQuery] string? name,
                                                    [FromQuery] Gender? gender,
                                                    [FromQuery] string? parentName, [FromQuery] string? phoneNumber,
                                                    [FromQuery] ProgressStatus? status,
                                                    [FromQuery] int? reviewerId, [FromQuery] DateTime? startDob,
                                                    [FromQuery] DateTime? endDob, [FromQuery] string nationalId)
        {
            try
            {
                var studentApplications = await _studentApplicationService.GetAllStudentApplicationAsync(courseId, name, gender, parentName, phoneNumber, status, reviewerId, startDob, endDob, nationalId);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, studentApplications));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpGet("student")]
        public async Task<IActionResult> GetAllStudentCourse(int courseId, [FromQuery] string? name,
                                            [FromQuery] Gender? gender,
                                            [FromQuery] string? studentGroupName, [FromQuery] string? phoneNumber,
                                            [FromQuery] ProgressStatus? status,
                                            [FromQuery] string? studentCode, [FromQuery] DateTime? startDob,
                                            [FromQuery] DateTime? endDob, [FromQuery] int? StudentGroup,
                                            [FromQuery] int? StudentGroupExcept, [FromQuery] bool? isGetStudentDrop)
        {
            try
            {
                var studentApplications = await _studentApplicationService.GetAllStudentCourseAsync(courseId, name, gender, studentGroupName, phoneNumber, status, studentCode, startDob, endDob, StudentGroup, StudentGroupExcept, isGetStudentDrop);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, studentApplications));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentApplicationId(int id)
        {
            try
            {
                var studentApplication = await _studentApplicationService.GetStudentApplicationByIdAsync(id);
                if (studentApplication == null)
                {
                    return NotFound(new ApiResponse(HttpStatusCode.NotFound, false, null));
                }
                return Ok(new ApiResponse(HttpStatusCode.OK, true, studentApplication));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpGet("{studentId:int}/{courseId:int}")]
        public async Task<IActionResult> GetStudentApplicationIdAndCourseId(int studentId, int courseId)
        {
            try
            {
                var studentApplication = await _studentApplicationService.GetByStudentIdAndCourseIdAsync(studentId, courseId);
                if (studentApplication == null)
                {
                    return NotFound(new ApiResponse(HttpStatusCode.NotFound, false, null));
                }
                return Ok(new ApiResponse(HttpStatusCode.OK, true, studentApplication));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateStudentApplicationStatus([FromBody] StudentCourseUpdateDto studentApplicationDto)
        {
            try
            {
                await _studentApplicationService.UpdateStatusStudentApplicationAsync(studentApplicationDto);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Cập nhật thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpPut("detail")]
        public async Task<IActionResult> UpdateStudentApplicationDetail([FromBody] StudentApplicationDetailDto request)
        {
            try
            {
                await _studentApplicationService.UpdateStatusStudentApplicationDetailAsync(request.courseId, request.id, request.note, request.status,  request.studentGroupId);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Cập nhật thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpPut("AutoAssign")]
        public async Task<IActionResult> AutoAssignApplications([FromQuery] int courseId)
        {
            try
            {
                await _studentApplicationService.AutoAssignApplicationsAsync(courseId);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, null));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpPost("SendResult")]
        public async Task<IActionResult> SendApplicationResult([FromBody] StudentApplicationResultRequest studentApplicationResultRequest)
        {
            try
            {
                await _studentApplicationService.SendApplicationResultAsync(studentApplicationResultRequest.ListStudentApplicationId, studentApplicationResultRequest.CourseId, studentApplicationResultRequest.Subject, studentApplicationResultRequest.Message);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, null));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpPost("printCards/{courseId}")]
        public async Task<IActionResult> PrintStudentCards([FromBody] List<int> studentCourseIds, int courseId)
        {
            if (studentCourseIds == null || studentCourseIds.Count == 0)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, "mã học sinh trống"));
            }

            try
            {
                var pdfBytes = await _studentApplicationService.GenerateStudentCardsPdfAsync(studentCourseIds, courseId);
                return File(pdfBytes, "application/pdf", "thẻ khóa sinh.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }
    }
}
