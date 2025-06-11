using SCCMS.Domain.DTOs.NightShiftDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.Services.Interfaces
{
    public interface INightShiftService
    {
        Task<IEnumerable<NightShiftDto>> GetAllNightShiftsAsync(int courseId);
        Task<NightShiftDto> GetNightShiftByIdAsync(int id);
        Task CreateNightShiftAsync(NightShiftCreateDto nightShiftDto);
        Task UpdateNightShiftAsync(int id, NightShiftUpdateDto nightShiftDto);
        Task DeleteNightShiftAsync(int id);
    }
}
