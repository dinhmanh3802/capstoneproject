using SCCMS.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Infrastucture.Repository.Interfaces
{
    public interface INightShiftAssignmentRepository : IGenericRepository<NightShiftAssignment>
    {
        Task<IEnumerable<NightShiftAssignment>> GetAssignmentsByDateAsync(DateTime date);
        Task<IEnumerable<NightShiftAssignment>> GetAssignmentsByShiftAsync(int shiftId, DateTime date);
    }
}
