using SCCMS.Domain.DTOs.SupervisorDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.Services.Interfaces
{
    public interface ISupervisorService 
    {
        Task<IEnumerable<SupervisorDto>> GetSupervisorsByCourseIdAsync(int courseId,
          string? name = null, string? email = null, string? phoneNumber = null,
          UserStatus? status = null,
          Gender? gender = null);
        Task<SupervisorDto?> GetSupervisorByIdAsync(int id);
        Task ChangeSupervisorsGroupAsync(List<int> supervisorIds, int newGroupId);
        Task<IEnumerable<SupervisorDto>> GetAvailableSupervisorsForCourseAsync(int courseId);
    }

}
