using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SCCMS.Domain.DTOs.StudentCourseDtos;
using SCCMS.Domain.DTOs.VolunteerApplicationDtos;
using SCCMS.Domain.Services.Interfaces;
using System.Net;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using SCCMS.Infrastucture.Entities;
using Utility;
using SCCMS.Domain.DTOs.EmailDtos;
using SCCMS.Domain.DTOs.VolunteerCourseDtos;
using SCCMS.Domain.Services.Implements;

namespace SCCMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VolunteerCourseController : ControllerBase
    {
        private readonly IVolunteerCourseService _volunteerCourseService;

        public VolunteerCourseController(IVolunteerCourseService volunteerCourseService)
        {
            _volunteerCourseService = volunteerCourseService;
        }

        [HttpGet("GetVolunteerApplication/{courseId}")]
        public async Task<IActionResult> GetAllVolunteerApplication(int courseId, [FromQuery] string? name,
                                                                    [FromQuery] Gender? gender,
                                                                    [FromQuery] string? phoneNumber,
                                                                    [FromQuery] ProgressStatus? status,
                                                                    [FromQuery] int? reviewerId,
                                                                    [FromQuery] DateTime? startDob,
                                                                    [FromQuery] DateTime? endDob,
                                                                    [FromQuery] int? teamId,
                                                                    [FromQuery] string? volunteerCode,
                                                                    [FromQuery] string nationalId)
        {
            try
            {
                var studentApplications = await _volunteerCourseService.GetVolunteerApplicationAsync(courseId, name, gender, phoneNumber, status, reviewerId, startDob, endDob, teamId,volunteerCode, nationalId);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, studentApplications));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpGet("GetVolunteerCourse/{courseId}")]
        public async Task<IActionResult> GetAllVolunteerCourses(int courseId, [FromQuery] string? name,
                                                                [FromQuery] Gender? gender,
                                                                [FromQuery] string? teamName,
                                                                [FromQuery] string? phoneNumber,
                                                                [FromQuery] ProgressStatus? status,
                                                                [FromQuery] string? volunteerCode,
                                                                [FromQuery] DateTime? startDob,
                                                                [FromQuery] DateTime? endDob)
        {
            try
            {
                var studentApplications = await _volunteerCourseService.GetAllVolunteerCourseAsync(courseId, name, gender, teamName, phoneNumber, status, volunteerCode, startDob, endDob);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, studentApplications));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpGet("volunteer/{volunteerId}/course/{courseId}")]
        public async Task<IActionResult> GetVolunteerCourseByVolunteerIdAndCourseId(int volunteerId, int courseId)
        {
            try
            {
                var volunteerCourse = await _volunteerCourseService.GetByVolunteerIdAndCourseIdAsync(volunteerId, courseId);
                if (volunteerCourse == null)
                {
                    return NotFound();
                }
                return Ok(new ApiResponse(HttpStatusCode.OK, true, volunteerCourse));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVolunteerCourseById(int id)
        {
            try
            {
                var volunteerCourse = await _volunteerCourseService.GetVolunteerCourseByIdAsync(id);
                if (volunteerCourse == null)
                {
                    return NotFound();
                }
                return Ok(new ApiResponse(HttpStatusCode.OK, true, volunteerCourse));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateVolunteerCourseStatus([FromBody] VolunteerCourseUpdateDto volunteerCourseUpdateDto)
        {
            try
            {
                await _volunteerCourseService.UpdateVolunteerCourseAsync(volunteerCourseUpdateDto);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Cập nhật thành công."));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }


        [HttpPut("UpdateVolunteerInformation")]
        public async Task<IActionResult> UpdateVolunteerInformationInACourse([FromBody] VolunteerInformationInACourseDto volunteerInfoDto)
        {
            try
            {
                await _volunteerCourseService.UpdateVolunteerInformationInACourseAsync(volunteerInfoDto);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Cập nhật thông tin tình nguyện viên thành công."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse(HttpStatusCode.NotFound, false, new List<string> { ex.Message }));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                // Bạn nên log exception ở đây để theo dõi lỗi
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { "Đã xảy ra lỗi trong quá trình cập nhật." }));
            }
        }

        [HttpPut("AutoAssign")]
        public async Task<IActionResult> AutoAssignVolunteerApplications([FromQuery] int courseId)
        {
            try
            {
                await _volunteerCourseService.AutoAssignApplicationsAsync(courseId);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Tự động phân công thành công."));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }
		[HttpPost("SendResult")]
		public async Task<IActionResult> SendVolunteerEmail([FromBody] VolunteerApplicationResultRequest request)
		{
			try
			{
				await _volunteerCourseService.SendVolunteerApplicationResultAsync(
					listVolunteerApplicationId: request.ListVolunteerApplicationId,
					courseId: request.CourseId,
					subject: request.Subject,
					body: request.Message
				);
				return Ok(new ApiResponse(HttpStatusCode.OK, true, null));
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
			}
			catch (Exception ex)
			{
				// Bạn nên log exception ở đây để theo dõi lỗi
				return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { "Đã xảy ra lỗi khi gửi email." }));
			}
		}

        [HttpPost("printCards/{courseId}")]
        public async Task<IActionResult> PrintVolunteerCards([FromBody] List<int> volunteerCourseIds, int courseId)
        {
            if (volunteerCourseIds == null || volunteerCourseIds.Count == 0)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, "mã học sinh trống"));
            }

            try
            {
                var pdfBytes = await _volunteerCourseService.GenerateVolunteerCardsPdfAsync(volunteerCourseIds, courseId);
                return File(pdfBytes, "application/pdf", "thẻ tình nguyện viên.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse(HttpStatusCode.InternalServerError, false));
            }
        }

        [HttpPost("printCertificate/{courseId}")]
        public async Task<IActionResult> PrintVolunteerCertificate([FromBody] List<int> volunteerCourseIds, int courseId)
        {
            if (volunteerCourseIds == null || volunteerCourseIds.Count == 0)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, "mã học sinh trống"));
            }

            try
            {
                var pdfBytes = await _volunteerCourseService.GenerateVolunteerCertificatePdfAsync(volunteerCourseIds, courseId);
                return File(pdfBytes, "application/pdf", "Chứng nhận tình nguyện viên.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse(HttpStatusCode.InternalServerError, false));
            }
        }

    }
}
