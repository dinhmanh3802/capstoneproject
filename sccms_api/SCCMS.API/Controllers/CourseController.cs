using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCCMS.Domain.DTOs.CourseDtos;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using System.Net;
using Utility;

namespace SCCMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;
        protected ApiResponse _response;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
            _response = new ApiResponse();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCourses([FromQuery] string? name, [FromQuery] CourseStatus? status, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var courses = await _courseService.GetAllCoursesAsync(name, status, startDate, endDate);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, courses));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, 
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message}));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourseById(int id)
        {
            CourseDto course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            _response.Result = course;
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }

        [HttpGet("current-course")]
        public async Task<IActionResult> GetCurrentCourses()
        {
            var course = await _courseService.GetCurrentCourseAsync();
            if (course == null)
            {
                return NotFound(new ApiResponse(HttpStatusCode.NotFound, false));
            }
            _response.Result = course;
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse([FromBody] CourseCreateDto courseCreateDto)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();

                    return StatusCode((int)HttpStatusCode.BadRequest, _response);
                }
                await _courseService.CreateCourseAsync(courseCreateDto);

                _response.StatusCode = HttpStatusCode.Created;
                _response.IsSuccess = true;
                _response.Result = "create sussess";

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
                _response.ErrorMessages.Add("InternalServerError");
                _response.ErrorMessages.Add(ex.Message);
                _response.ErrorMessages.Add(ex.InnerException.ToString());

                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }

        [HttpPatch("{courseId}")]
        public async Task<IActionResult> UpdateCourse(int courseId, [FromBody] CourseUpdateDto courseUpdateDto)
        {
            try
            {
                if (courseUpdateDto == null || IsDtoEmpty(courseUpdateDto))
                {
                    return StatusCode((int)HttpStatusCode.OK, new ApiResponse(HttpStatusCode.OK, true));
                }

                if (!ModelState.IsValid)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();

                    return StatusCode((int)HttpStatusCode.BadRequest, _response);
                }
                await _courseService.UpdateCourseAsync(courseId, courseUpdateDto);

                return StatusCode((int)HttpStatusCode.OK, new ApiResponse(HttpStatusCode.OK, true));
            }
            catch (ArgumentException ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
        }

        private bool IsDtoEmpty(CourseUpdateDto dto)
        {
            return dto.CourseName == null &&
                   dto.StartDate == null &&
                   dto.EndDate == null &&
                   dto.Status == null &&
                   dto.ExpectedStudents == null &&
                   dto.Description == null &&
                   dto.StudentApplicationStartDate == null &&
                   dto.StudentApplicationEndDate == null &&
                   dto.VolunteerApplicationStartDate == null &&
                   dto.VolunteerApplicationEndDate == null &&
                   dto.FreeTimeApplicationStartDate == null &&
                   dto.FreeTimeApplicationEndDate == null;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            try
            {
                await _courseService.DeleteCourseAsync(id);

                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                _response.Result = "delete success";

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
                _response.ErrorMessages.Add("InternalServerError");
                _response.ErrorMessages.Add(ex.Message);
                _response.ErrorMessages.Add(ex.InnerException?.ToString());

                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }

        [HttpGet("available-feedback-courses")]
        public async Task<IActionResult> GetAvailableFeedbackCourses()
        {
            try
            {
                var responseData = await _courseService.GetAvaiableFeedBackCourseAsync();
                return Ok(new ApiResponse(HttpStatusCode.OK, true, responseData));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse(
                    HttpStatusCode.InternalServerError,
                    false,
                    new List<string> { "Đã xảy ra lỗi.", ex.Message }
                ));
            }
        }

        [HttpGet("dashboard/{courseId}")]
        public async Task<IActionResult> GetCourseDashboard(int courseId)
        {
            try
            {
                var dashboardData = await _courseService.GetCourseDashboardDataAsync(courseId);

                if (dashboardData == null)
                {
                    return NotFound(new ApiResponse(HttpStatusCode.NotFound, false, new List<string> { "Khóa học không tồn tại" }));
                }

                return Ok(new ApiResponse(HttpStatusCode.OK, true, dashboardData));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpGet("registrations/students-per-course")]
        public async Task<IActionResult> GetStudentRegistrationsPerCourse([FromQuery] int years = 3)
        {
            try
            {
                var data = await _courseService.GetStudentRegistrationsPerCourseAsync(years);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, data));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpGet("registrations/volunteers-per-course")]
        public async Task<IActionResult> GetVolunteerRegistrationsPerCourse([FromQuery] int years = 3)
        {
            try
            {
                var data = await _courseService.GetVolunteerRegistrationsPerCourseAsync(years);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, data));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }
    }
}
