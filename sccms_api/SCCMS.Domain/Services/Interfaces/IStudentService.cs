using SCCMS.Domain.DTOs.CourseDtos;
using SCCMS.Domain.DTOs.StudentDtos;
using SCCMS.Infrastucture.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.Services.Interfaces
{
	public interface IStudentService
	{
		Task<IEnumerable<StudentDto>> GetAllStudentsAsync(
		string? fullName = null,
		string? email = null,
		Gender? genders = null,
		ProfileStatus? status = null,
		int? courseId = null,
		int? studentGroupId = null,
		string? parentName = null,
		string? emergencyContact = null,
		string? studentCode = null,  // New parameter
		DateTime? dateOfBirth = null  // New parameter
	);

		Task<StudentDto?> GetStudentByIdAsync(int id);
		Task CreateStudentAsync(StudentCreateDto studentCreateDto, int courseId);
		Task UpdateStudentAsync(int studentId, StudentUpdateDto studentUpdateDto);
		Task<byte[]> ExportStudentsByCourseAsync(int courseId);
		Task<IEnumerable<StudentDto>> GetStudentsByCourseIdAsync(int courseId);
		Task<byte[]> ExportStudentsByGroupAsync(int groupId);


	}
}