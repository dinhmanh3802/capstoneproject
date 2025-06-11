
using SCCMS.Domain.DTOs.StudentGroupDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.Services.Interfaces
{
    public interface IStudentGroupService
    {

        Task<IEnumerable<StudentGroupDto>> GetAllStudentGroupByCourseIdAsync(int id);
        Task<StudentGroupDto?> GetStudentGroupByIdAsync(int id);
        Task CreateStudentGroupAsync(StudentGroupCreateDto entity);
        Task UpdateStudentGroupAsync(int courseId, StudentGroupUpdateDto entity);
        Task DeleteStudentGroupAsync(int id);
        
    }
}
