using SCCMS.Domain.DTOs.StaffFreeTimeDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.Services.Interfaces
{
    public interface IStaffFreeTimeService
    {
        Task<IEnumerable<StaffFreeTimeDto>> GetAllStaffFreeTimesAsync(int? userId, int? courseId ,DateTime? dateTime);
        Task<StaffFreeTimeDto> GetStaffFreeTimeByIdAsync(int id);
        Task CreateStaffFreeTimeAsync(StaffFreeTimeCreateDto staffFreeTimeDto);
        Task DeleteStaffFreeTimeAsync(int id);
    }
}
