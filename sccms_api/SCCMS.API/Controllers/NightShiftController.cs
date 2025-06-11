using Microsoft.AspNetCore.Mvc;
using SCCMS.Domain.DTOs.NightShiftDtos;
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
    public class NightShiftController : ControllerBase
    {
        private readonly INightShiftService _nightShiftService;

        public NightShiftController(INightShiftService nightShiftService)
        {
            _nightShiftService = nightShiftService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllNightShifts([FromQuery] int courseId)
        {
            try
            {
                var nightShifts = await _nightShiftService.GetAllNightShiftsAsync(courseId);

                var sortedNightShifts = nightShifts
                    .OrderBy(ns => CalculateSortOrder(ns.StartTime))
                    .ToList();

                return Ok(new ApiResponse(HttpStatusCode.OK, true, sortedNightShifts));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        private int CalculateSortOrder(TimeSpan startTime)
        {
            var threshold = new TimeSpan(12, 0, 0); 

            if (startTime >= threshold)
            {
                return (int)startTime.TotalMinutes;
            }
            else
            {
                return (int)(startTime.TotalMinutes + 1440);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNightShiftById(int id)
        {
            try
            {
                var nightShift = await _nightShiftService.GetNightShiftByIdAsync(id);
                if (nightShift == null)
                {
                    return NotFound(new ApiResponse(HttpStatusCode.NotFound, false, null));
                }
                return Ok(new ApiResponse(HttpStatusCode.OK, true, nightShift));
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
        public async Task<IActionResult> CreateNightShift([FromBody] NightShiftCreateDto nightShiftDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)));
                }
                await _nightShiftService.CreateNightShiftAsync(nightShiftDto);
                return StatusCode((int)HttpStatusCode.Created, new ApiResponse(HttpStatusCode.Created, true, "Tạo ca trực thành công"));
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
        public async Task<IActionResult> UpdateNightShift(int id, [FromBody] NightShiftUpdateDto nightShiftDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)));
                }
                await _nightShiftService.UpdateNightShiftAsync(id, nightShiftDto);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Cập nhật ca trực thành công"));
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
        public async Task<IActionResult> DeleteNightShift(int id)
        {
            try
            {
                await _nightShiftService.DeleteNightShiftAsync(id);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Xóa ca trực thành công"));
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
