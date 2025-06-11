using Microsoft.AspNetCore.Mvc;
using SCCMS.Domain.DTOs.VolunteerDtos;
using SCCMS.Domain.Services.Interfaces;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using SCCMS.Infrastucture.Entities;
using Utility;
using SCCMS.Domain.DTOs.EmailDtos;
using SCCMS.Domain.Services.Implements;

namespace SCCMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VolunteerController : ControllerBase
    {
        private readonly IVolunteerService _volunteerService;
        private readonly ICourseService _courseService;

        public VolunteerController(IVolunteerService volunteerService, ICourseService courseService)
        {
            _volunteerService = volunteerService;
            _courseService = courseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllVolunteers(
            [FromQuery] string? fullName,
            [FromQuery] Gender? gender,
            [FromQuery] ProfileStatus? status,
            [FromQuery] int? teamId,
            [FromQuery] int? courseId,
            [FromQuery] string? nationalId,
            [FromQuery] string? address)
        {
            try
            {
                var volunteers = await _volunteerService.GetAllVolunteersAsync(fullName, gender, status, teamId, courseId, nationalId, address);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, volunteers));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpGet("{volunteerId}")]
        public async Task<IActionResult> GetVolunteerById(int volunteerId)
        {
            try
            {
                var volunteer = await _volunteerService.GetVolunteerByIdAsync(volunteerId);

                if (volunteer == null)
                {
                    return NotFound();
                }

                return Ok(new ApiResponse(HttpStatusCode.OK, true, volunteer));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateVolunteer([FromForm] VolunteerCreateDto volunteerCreateDto, [FromQuery] int courseId)
        {
            try
            {
                await _volunteerService.CreateVolunteerAsync(volunteerCreateDto, courseId);
                return StatusCode((int)HttpStatusCode.Created,
                    new ApiResponse(HttpStatusCode.Created, true, "Tạo tình nguyện viên thành công."));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpPut("{volunteerId}")]
        public async Task<IActionResult> UpdateVolunteer(int volunteerId, [FromForm] VolunteerUpdateDto volunteerUpdateDto)
        {
            try
            {
                await _volunteerService.UpdateVolunteerAsync(volunteerId, volunteerUpdateDto);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Cập nhật tình nguyện viên thành công."));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message, ex.InnerException?.ToString() }));
            }
        }

        [HttpGet("ByCourse/{courseId}")]
        public async Task<IActionResult> GetVolunteersByCourseId(int courseId)
        {
            try
            {
                var volunteers = await _volunteerService.GetVolunteersByCourseIdAsync(courseId);

                if (volunteers == null || !volunteers.Any())
                {
                    return NotFound();
                }

                return Ok(new ApiResponse(HttpStatusCode.OK, true, volunteers));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }
        [HttpGet("ExportVolunteersByCourse/{courseId}")]
        public async Task<IActionResult> ExportVolunteersByCourse(int courseId)
        {
            try
            {
                // Bước 1: Gọi service để xuất dữ liệu tình nguyện viên ra file Excel
                var fileContent = await _volunteerService.ExportVolunteersByCourseAsync(courseId);

                // Bước 2: Kiểm tra nếu không có dữ liệu
                if (fileContent == null || fileContent.Length == 0)
                {
                    return NotFound(new ApiResponse(HttpStatusCode.NotFound, false, "Không tìm thấy dữ liệu tình nguyện viên cho khóa học này."));
                }
                var course = await _courseService.GetCourseByIdAsync(courseId);
                if (course == null)
                {
                    return NotFound(new ApiResponse(HttpStatusCode.NotFound, false));
                }

                // Bước 3: Tạo tên file với định dạng "Danh_sach_tinh_nguyen_vien_Khoa_{courseId}.xlsx"
                string fileName = $"Danh_sach_tinh_nguyen_vien_Khoa_{course.CourseName}.xlsx";

                // Bước 4: Trả về file với nội dung Excel, type và tên file
                return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                // Bước 5: Xử lý lỗi
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message, ex.InnerException?.Message }));
            }
        }
        [HttpGet("ExportVolunteersByTeam/{teamId}")]
        public async Task<IActionResult> ExportVolunteersByTeam(int teamId)
        {
            try
            {
                // Gọi service để xuất dữ liệu tình nguyện viên ra file Excel
                var fileContent = await _volunteerService.ExportVolunteersByTeamAsync(teamId);

                // Kiểm tra nếu không có dữ liệu
                if (fileContent == null || fileContent.Length == 0)
                {
                    return NotFound(new ApiResponse(HttpStatusCode.NotFound, false, "Không tìm thấy dữ liệu tình nguyện viên cho đội ngũ này."));
                }


                // Tạo tên file với định dạng "Danh_sach_tinh_nguyen_vien_Team_{teamId}.xlsx"
                string fileName = $"Danh_sach_tinh_nguyen_vien.xlsx";

                // Trả về file với nội dung Excel, type và tên file
                return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message, ex.InnerException?.Message }));
            }
        }

    }
}
