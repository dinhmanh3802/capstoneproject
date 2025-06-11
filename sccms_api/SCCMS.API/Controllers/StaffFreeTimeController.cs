using Microsoft.AspNetCore.Mvc;
using SCCMS.Domain.DTOs.StaffFreeTimeDtos;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffFreeTimeController : ControllerBase
    {
        private readonly IStaffFreeTimeService _staffFreeTimeService;

        public StaffFreeTimeController(IStaffFreeTimeService staffFreeTimeService)
        {
            _staffFreeTimeService = staffFreeTimeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStaffFreeTimes([FromQuery]int? userId, [FromQuery] int? courseId, [FromQuery] DateTime? dateTime)
        {
            try
            {
                var freeTimes = await _staffFreeTimeService.GetAllStaffFreeTimesAsync(userId, courseId, dateTime);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, freeTimes));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetStaffFreeTimeById(int id)
        {
            try
            {
                var freeTime = await _staffFreeTimeService.GetStaffFreeTimeByIdAsync(id);
                if (freeTime == null)
                {
                    return NotFound(new ApiResponse(HttpStatusCode.NotFound, false, null));
                }
                return Ok(new ApiResponse(HttpStatusCode.OK, true, freeTime));
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

        [HttpPost]
        public async Task<IActionResult> CreateStaffFreeTime([FromBody] StaffFreeTimeCreateDto staffFreeTimeDto)
        {
            try
            {
                await _staffFreeTimeService.CreateStaffFreeTimeAsync(staffFreeTimeDto);
                return StatusCode((int)HttpStatusCode.Created, new ApiResponse(HttpStatusCode.Created, true, "Tạo thời gian rảnh thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStaffFreeTime(int id)
        {
            try
            {
                await _staffFreeTimeService.DeleteStaffFreeTimeAsync(id);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Xóa thời gian rảnh thành công"));
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
