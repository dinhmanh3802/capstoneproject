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
    public class StaffFreeTimeRepository : GenericRepository<StaffFreeTime>, IStaffFreeTimeRepository
    {
        public StaffFreeTimeRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<StaffFreeTime>> GetFreeTimeByDateAsync(DateTime date)
        {
            return await _context.StaffFreeTimes
                .Where(sft => sft.Date == date)
                .ToListAsync();

        }
    }
}
