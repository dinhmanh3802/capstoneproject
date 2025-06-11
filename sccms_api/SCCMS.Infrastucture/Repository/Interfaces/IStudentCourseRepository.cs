using SCCMS.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Infrastucture.Repository.Interfaces
{
    public interface IStudentCourseRepository : IGenericRepository<StudentCourse>
    {
        Task<StudentCourse?> GetByStudentCodeAsync(string studentCode);
    }
}
