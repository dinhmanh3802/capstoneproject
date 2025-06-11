using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SCCMS.Infrastucture.Context;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.Repository.Interfaces;

namespace SCCMS.Infrastucture.Repository.Implements
{
	public class SupervisorStudentGroupRepository : GenericRepository<SupervisorStudentGroup>, ISupervisorStudentGroupRepository
	{
		public SupervisorStudentGroupRepository(AppDbContext context) : base(context) { }

		public async Task<bool> AnyAsync(Expression<Func<SupervisorStudentGroup, bool>> predicate)
		{
			return await _context.Set<SupervisorStudentGroup>().AnyAsync(predicate);
		}
	}
}
