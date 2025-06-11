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
    public class NightShiftAssignmentRepository : GenericRepository<NightShiftAssignment>, INightShiftAssignmentRepository
    {
        public NightShiftAssignmentRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<NightShiftAssignment>> GetAssignmentsByDateAsync(DateTime date)
        {
            return await _context.NightShiftAssignments
                .Where(a => a.Date == date)
                .ToListAsync();
        }

        public async Task<IEnumerable<NightShiftAssignment>> GetAssignmentsByShiftAsync(int shiftId, DateTime date)
        {
            return await _context.NightShiftAssignments
                .Where(a => a.NightShiftId == shiftId && a.Date == date)
                .ToListAsync();
        }
    }
}
