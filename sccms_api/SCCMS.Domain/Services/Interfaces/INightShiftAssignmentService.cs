using SCCMS.Domain.DTOs.NightShiftDtos;
using SCCMS.Domain.DTOs.UserDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.Services.Interfaces
{
    public interface INightShiftAssignmentService
    {
        Task<IEnumerable<MyShiftAssignmentDto>> GetShiftsByCourseAndUserAsync(int? courseId, int userId);
        Task<IEnumerable<MyShiftAssignmentDto>> GetShiftsByCourseAsync(int courseId, DateTime? dateTime = null, NightShiftAssignmentStatus? status = null);

        Task<MyShiftAssignmentDto> GetNightShiftsByIdAsync(int shiftId);

        Task ScheduleShiftsAsync(int courseId);
        Task<IEnumerable<UserDto>> SuggestStaffForShiftAsync(DateTime date, int shiftId, int roomId, int courseId);
        Task AssignStaffToShiftAsync(NightShiftAssignmentCreateDto assignStaffDto);
        Task UpdateAssignmentAsync(NightShiftAssignmentUpdateDto updateDto, int userRole);
        Task UpdateAssignmentStatusAsync(NightShiftAssignmentRejectDto updateDto, int userRole);
        Task ReassignAssignmentAsync(NightShiftAssignmentReassignDto reassignDto, int userRole);
        Task DeleteAssignmentAsync(int assignmentId);
    }
}
