using Microsoft.AspNetCore.Mvc;
using SCCMS.Domain.DTOs.StudentDtos;
using SCCMS.Domain.Services.Implements;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using System.Net;
using Utility;

namespace SCCMS.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class StudentController : ControllerBase
	{
		private readonly IStudentService _studentService;
		private readonly ICourseService _courseService;

		protected ApiResponse _response;

		public StudentController(IStudentService studentService, ICourseService courseService )
		{
			_studentService = studentService;
			_response = new ApiResponse();
			_courseService = courseService;
		}
		[HttpGet]
		public async Task<IActionResult> GetAllStudents(
	[FromQuery] string? fullName = null,
	[FromQuery] string? email = null,
	[FromQuery] Gender? genders = null,
	[FromQuery] ProfileStatus? status = null,
	[FromQuery] int? courseId = null,
	[FromQuery] int? studentGroupId = null,
	[FromQuery] string? parentName = null,
	[FromQuery] string? emergencyContact = null,
	[FromQuery] string? studentCode = null,         
	[FromQuery] DateTime? dateOfBirth = null       
)
		{
			var students = await _studentService.GetAllStudentsAsync(
				fullName,
				email,
				genders,
				status,
				courseId,
				studentGroupId,
				parentName,
				emergencyContact,
				studentCode,
				dateOfBirth
			);

			_response.Result = students;
			_response.IsSuccess = true;
			_response.StatusCode = HttpStatusCode.OK;

			return Ok(_response);
		}

		// Route: /student/{id}
		[HttpGet("{id}")]
		public async Task<IActionResult> GetStudentById(int id)
		{
			var student = await _studentService.GetStudentByIdAsync(id);
			if (student == null)
			{
				return NotFound();
			}
			_response.Result = student;
			_response.IsSuccess = true;
			_response.StatusCode = HttpStatusCode.OK;
			return Ok(_response);
		}


		[HttpPost("{courseId}")]
		public async Task<IActionResult> CreateStudent(int courseId, [FromForm] StudentCreateDto studentCreateDto)
		{
			try
			{
				await _studentService.CreateStudentAsync(studentCreateDto, courseId);

				_response.StatusCode = HttpStatusCode.Created;
				_response.IsSuccess = true;
				_response.Result = "create success";

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
				_response.ErrorMessages.Add(ex.InnerException?.ToString());

				return StatusCode((int)HttpStatusCode.InternalServerError, _response);
			}
		}


		[HttpPatch("{studentId}")]
		public async Task<IActionResult> UpdateStudent(int studentId, [FromForm] StudentUpdateDto studentUpdateDto)
		{
			try
			{
				await _studentService.UpdateStudentAsync(studentId, studentUpdateDto);

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = "update success";

				return StatusCode((int)HttpStatusCode.OK, _response);
			}
			catch (ArgumentException ex)
			{
				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add("invalid data");
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

		[HttpGet("{courseId}/export")]
		public async Task<IActionResult> ExportStudents(int courseId)
		{
			try
			{
				// Bước 1: Gọi hàm service để xuất dữ liệu sinh viên ra file Excel
				var fileContent = await _studentService.ExportStudentsByCourseAsync(courseId);

				// Bước 2: Kiểm tra xem file có dữ liệu hay không
				if (fileContent == null)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Không tìm thấy sinh viên nào cho khóa học này.");
					return NotFound(_response);
				}

				// Bước 3: Lấy thông tin khóa học để tạo tên file động
				var course = await _courseService.GetCourseByIdAsync(courseId);
				if (course == null)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Không tìm thấy khóa học.");
					return NotFound(_response);
				}

				// Bước 4: Tạo tên file với định dạng "Danh_sach_khoa_sinh_{course.CourseName}.xlsx"
				string fileName = $"Danh_sach_khoa_sinh_{course.CourseName}.xlsx";

				// Bước 5: Thiết lập phản hồi thành công
				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = "File generated successfully.";

				// Bước 6: Trả về file với nội dung Excel, type và tên file
				return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
			}
			catch (Exception ex)
			{
				// Bước 7: Xử lý lỗi
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add("InternalServerError");
				_response.ErrorMessages.Add(ex.Message);
				_response.ErrorMessages.Add(ex.InnerException?.ToString());

				return StatusCode((int)HttpStatusCode.InternalServerError, _response);
			}
		}

		// Endpoint: /api/students/by-course/{courseId}
		[HttpGet("by-course/{courseId}")]
		public async Task<IActionResult> GetStudentsByCourseId(int courseId)
		{
			if (courseId <= 0)
			{
				_response.IsSuccess = false;
				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.ErrorMessages.Add("courseId must be greater than 0.");
				return BadRequest(_response);
			}

			try
			{
				// Gọi service để lấy danh sách sinh viên theo courseId
				var students = await _studentService.GetStudentsByCourseIdAsync(courseId);

				if (students == null || !students.Any())
				{
					_response.IsSuccess = false;
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.ErrorMessages.Add("No students found for this course.");
					return NotFound(_response);
				}

				_response.IsSuccess = true;
				_response.StatusCode = HttpStatusCode.OK;
				_response.Result = students;
				return Ok(_response);
			}
			catch (Exception ex)
			{
				// Xử lý lỗi và trả về phản hồi lỗi
				_response.IsSuccess = false;
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.ErrorMessages.Add("InternalServerError");
				_response.ErrorMessages.Add(ex.Message);
				if (ex.InnerException != null)
				{
					_response.ErrorMessages.Add(ex.InnerException.ToString());
				}
				return StatusCode((int)HttpStatusCode.InternalServerError, _response);
			}
		}

		[HttpGet("ExportStudentsByGroup/{groupId}")]
		public async Task<IActionResult> ExportStudentsByGroup(int groupId)
		{
			try
			{
				// Gọi service để xuất dữ liệu sinh viên ra file Excel
				var fileContent = await _studentService.ExportStudentsByGroupAsync(groupId);

				// Kiểm tra nếu không có dữ liệu
				if (fileContent == null || fileContent.Length == 0)
				{
					return NotFound(new ApiResponse(HttpStatusCode.NotFound, false, "Không tìm thấy dữ liệu sinh viên cho nhóm này."));
				}

				// Tạo tên file với định dạng "Danh_sach_sinh_vien_Nhom_{groupId}.xlsx"
				string fileName = $"Danh_sach_khoa_sinh.xlsx";

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

