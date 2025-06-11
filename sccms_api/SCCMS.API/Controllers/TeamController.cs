using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SCCMS.Domain.DTOs.TeamDtos;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using System.Net;
using Utility;

namespace SCCMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _teamService;

        public TeamController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        [HttpGet("{teamId}/volunteers")]
        public async Task<IActionResult> GetVolunteersInTeam(int teamId, [FromQuery] string? volunteerCode, [FromQuery] string? fullName, [FromQuery] string? phoneNumber, [FromQuery] Gender? gender, [FromQuery] ProgressStatus? status)
        {
            try
            {
                var volunteers = await _teamService.GetVolunteersInTeamAsync(teamId, volunteerCode, fullName, phoneNumber, gender, status);

                if (volunteers == null || !volunteers.Any())
                {
                    // Return an empty list if no volunteers are found
                    return Ok(new ApiResponse(HttpStatusCode.OK, true, new List<object>()));
                }

                return Ok(new ApiResponse(HttpStatusCode.OK, true, volunteers));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetTeamsByCourseId(int courseId)
        {
            try
            {
                var teams = await _teamService.GetAllTeamsByCourseIdAsync(courseId);

                if (teams == null || !teams.Any())
                {
                    return Ok(new ApiResponse(HttpStatusCode.OK, true, new List<object>()));
                }

                return Ok(new ApiResponse(HttpStatusCode.OK, true, teams));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTeamById(int id)
        {
            try
            {
                var team = await _teamService.GetTeamByIdAsync(id);

                if (team == null)
                {
                    return NotFound(new ApiResponse(HttpStatusCode.NotFound, false, "Không tìm thấy ban với ID này."));
                }

                return Ok(new ApiResponse(HttpStatusCode.OK, true, team));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTeam([FromBody] TeamCreateDto teamDto)
        {
            try
            {
                await _teamService.CreateTeamAsync(teamDto);
                return StatusCode((int)HttpStatusCode.Created, new ApiResponse(HttpStatusCode.Created, true, "Tạo ban thành công"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTeam(int id, [FromBody] TeamUpdateDto teamDto)
        {
            try
            {
                await _teamService.UpdateTeamAsync(id, teamDto);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Cập nhật thành công"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeam(int id)
        {
            try
            {
                await _teamService.DeleteTeamAsync(id);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Xóa thành công"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }
    }
}
