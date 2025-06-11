using SCCMS.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Infrastucture.Repository.Interfaces
{
    public interface IStudentGroupRepository : IGenericRepository<StudentGroup>
    {

        Task<IEnumerable<StudentGroup>> GetAllAsync(Expression<Func<StudentGroup, bool>>? filter = null, string? includeProperties = null);
    }
}
