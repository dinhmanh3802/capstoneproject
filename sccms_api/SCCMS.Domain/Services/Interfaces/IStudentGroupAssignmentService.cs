using SCCMS.Domain.DTOs.StudentCourseDtos;
using SCCMS.Domain.DTOs.StudentGroupAssignmentDtos;
using SCCMS.Domain.DTOs.StudentGroupDtos;
using SCCMS.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.Services.Interfaces
{
    public interface IStudentGroupAssignmentService
    {
        Task AddStudentsIntoGroupAsync(StudentGroupAssignmentDto studentGroupAssignmentDto);

        Task RemoveStudentsFromGroupAsync(StudentGroupAssignmentDto studentGroupAssignmentDto);
		Task UpdateStudentGroupAssignmentAsync(int studentId, int studentGroupId, int studentGroupIdUpdate);
        Task DistributeStudentsByGroupAsync(int courseId);
        Task<List<StudentGroupDto>> AutoAssignSupervisorsAsync(int courseId);
    }
}
