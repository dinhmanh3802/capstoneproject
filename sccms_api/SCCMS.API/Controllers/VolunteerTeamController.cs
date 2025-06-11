using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SCCMS.Domain.DTOs.VolunteerTeamDtos;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using System.Net;

namespace SCCMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VolunteerTeamController : ControllerBase
    {
        private readonly IVolunteerTeamService _volunteerTeamService;

        public VolunteerTeamController(IVolunteerTeamService volunteerTeamService)
        {
            _volunteerTeamService = volunteerTeamService;
        }

        [HttpPost("add-volunteers")]
        public async Task<IActionResult> AddVolunteersIntoTeam([FromBody] VolunteerTeamDto volunteerTeamDto)
        {
            try
            {
                await _volunteerTeamService.AddVolunteersIntoTeamAsync(volunteerTeamDto);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Đã thêm tình nguyện viên vào ban thành công."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { "Đã xảy ra lỗi khi thêm tình nguyện viên vào ban.", ex.Message }));
            }
        }

        [HttpDelete("remove-volunteers")]
        public async Task<IActionResult> RemoveVolunteersFromTeam([FromBody] VolunteerTeamDto volunteerTeamDto)
        {
            try
            {
                await _volunteerTeamService.RemoveVolunteersFromTeamAsync(volunteerTeamDto);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Đã xóa tình nguyện viên khỏi ban thành công."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { "Đã xảy ra lỗi khi xóa tình nguyện viên khỏi ban.", ex.Message }));
            }
        }
        [HttpPost("auto-assign")]
        public async Task<IActionResult> AutoAssignVolunteers(int courseId)
        {
            try
            {
                await _volunteerTeamService.AutoAssignVolunteersToTeamsAsync(courseId);

                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Phân team tự động hoàn tất"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { "Đã xảy ra lỗi trong quá trình phân team tự động.", ex.Message }));
            }
        }
    }
}
