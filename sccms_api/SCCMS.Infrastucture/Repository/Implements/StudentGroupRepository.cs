using Microsoft.EntityFrameworkCore;
using SCCMS.Infrastucture.Context;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Infrastucture.Repository.Implements
{
    public class StudentGroupRepository : GenericRepository<StudentGroup>, IStudentGroupRepository
    {
        public StudentGroupRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<StudentGroup>> GetAllAsync(Expression<Func<StudentGroup, bool>>? filter = null, string? includeProperties = null)
        {
            IQueryable<StudentGroup> query = _context.Set<StudentGroup>();

            // Apply filter if provided
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Apply include properties for eager loading
            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            // Order by Id in descending order
            query = query.OrderByDescending(e => EF.Property<object>(e, "Id"));

            return await query.ToListAsync();
        }
    }
}
