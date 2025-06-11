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
    public class StudentRepository : GenericRepository<Student>, IStudentRepository
    {
		private readonly AppDbContext _context;
		public StudentRepository(AppDbContext context) : base(context)
        {
			_context = context;
		}
		public async Task<Student?> GetByNationalIdAsync(string nationalId)
		{
			return await _context.Students.FirstOrDefaultAsync(s => s.NationalId == nationalId);
		}

	}
}
