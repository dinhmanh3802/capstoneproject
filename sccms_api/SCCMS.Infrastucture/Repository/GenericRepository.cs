using Microsoft.EntityFrameworkCore;
using SCCMS.Infrastucture.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Infrastucture.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        public GenericRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> filter = null, bool tracked = true, string? includeProperties = null)
        {
            IQueryable<T> query = _context.Set<T>();
            if (!tracked)
            {
                query = query.AsNoTracking();
            }
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return await query.FirstOrDefaultAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }
		public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, string? includeProperties = null)
		{
			IQueryable<T> query = _context.Set<T>().Where(predicate);
			if (!string.IsNullOrWhiteSpace(includeProperties))
			{
				foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(includeProperty.Trim());
				}
			}

			return (await query.ToListAsync()).AsEnumerable().Reverse();
		}


		public async Task<IEnumerable<T>> GetAllAsync(
	Expression<Func<T, bool>>? predicate = null,
	string? includeProperties = null)
		{
			IQueryable<T> query = _context.Set<T>();

			// Kiểm tra và áp dụng các thuộc tính liên quan
			if (includeProperties != null)
			{
				foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(includeProp);
				}
			}

			// Áp dụng bộ lọc nếu có
			if (predicate != null)
			{
				query = query.Where(predicate);
			}

			// Lấy danh sách và đảo ngược kết quả
			return (await query.ToListAsync()).AsEnumerable().Reverse();
		}



		public async Task<T> GetByIdAsync(int id, string? includeProperties = null)
        {
            IQueryable<T> query = _context.Set<T>();
            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            return await _context.Set<T>().FindAsync(id);
        }

        public async Task DeleteAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
        }
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _context.Set<T>().AddRangeAsync(entities);
        }

        public async Task UpdateRangeAsync(IEnumerable<T> entities)
        {
            _context.Set<T>().UpdateRange(entities);
        }

        public async Task DeleteRangeAsync(IEnumerable<T> entitys)
        {
            _context.Set<T>().RemoveRange(entitys);
        }
        public async Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<int> ids)
        {
            return await _context.Set<T>().Where(e => ids.Contains(EF.Property<int>(e, "Id"))).ToListAsync();
        }
        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
        {
            IQueryable<T> query = _context.Set<T>();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query.CountAsync();
        }
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().AnyAsync(predicate);
        }

        // Hau viet de paging cho post
        public async Task<IEnumerable<T>> FindPagedAsync(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize)
        {
            var query = _context.Set<T>()
                                .Where(predicate)
                                .OrderByDescending(e => EF.Property<object>(e, "Id"));

            if (pageSize > 0 && pageNumber > 0)
            {
                query = (IOrderedQueryable<T>) query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            return await query.ToListAsync();
        }
    }
}
