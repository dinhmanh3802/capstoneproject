using SCCMS.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Infrastucture.Repository.Interfaces
{
    public interface ISupervisorStudentGroupRepository : IGenericRepository<SupervisorStudentGroup>
    {
		Task<bool> AnyAsync(Expression<Func<SupervisorStudentGroup, bool>> predicate);
	}
}
