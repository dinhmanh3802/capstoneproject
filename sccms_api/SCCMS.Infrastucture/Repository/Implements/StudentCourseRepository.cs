using Microsoft.EntityFrameworkCore;
using SCCMS.Infrastucture.Context;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Infrastucture.Repository.Implements
{
    public class StudentCourseRepository : GenericRepository<StudentCourse>, IStudentCourseRepository
    {
        private readonly AppDbContext _context;
        public StudentCourseRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<StudentCourse?> GetByStudentCodeAsync(string studentCode)
        {
            return await _context.StudentCourses.FirstOrDefaultAsync(s => s.StudentCode == studentCode);
        }

    }
}
