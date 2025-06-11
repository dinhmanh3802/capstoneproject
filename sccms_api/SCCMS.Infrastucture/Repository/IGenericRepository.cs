using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Infrastucture.Repository
{
    public interface IGenericRepository<T> where T : class
    {

        Task<T> GetByIdAsync(int id, string? includeProperties = null);
        Task<T> GetAsync(Expression<Func<T, bool>> filter = null, bool tracked = true, string? includeProperties = null);
        Task<IEnumerable<T>> GetAllAsync(
   Expression<Func<T, bool>>? predicate = null,
   string? includeProperties = null);

		Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, string? includeProperties = null);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        Task UpdateRangeAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task DeleteRangeAsync(IEnumerable<T> entitys);
        Task SaveAsync();
        Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<int> ids);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

        Task<IEnumerable<T>> FindPagedAsync(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize);
    }
}
