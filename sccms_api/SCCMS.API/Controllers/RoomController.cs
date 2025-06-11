using Microsoft.AspNetCore.Mvc;
using SCCMS.Domain.DTOs.RoomDtos;
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
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRooms([FromQuery] int courseId)
        {
            try
            {
                var rooms = await _roomService.GetAllRoomsAsync(courseId);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, rooms));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoomById(int id)
        {
            try
            {
                var room = await _roomService.GetRoomByIdAsync(id);
                if (room == null)
                {
                    return NotFound(new ApiResponse(HttpStatusCode.NotFound, false, null));
                }
                return Ok(new ApiResponse(HttpStatusCode.OK, true, room));
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
        public async Task<IActionResult> CreateRoom([FromBody] RoomCreateDto roomDto)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)));
                }
                await _roomService.CreateRoomAsync(roomDto);
                return StatusCode((int)HttpStatusCode.Created, new ApiResponse(HttpStatusCode.Created, true, "Tạo phòng thành công"));
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
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] RoomUpdateDto roomDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)));
                }
                await _roomService.UpdateRoomAsync(id, roomDto);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Cập nhật phòng thành công"));
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
        public async Task<IActionResult> DeleteRoom(int id)
        {
            try
            {
                await _roomService.DeleteRoomAsync(id);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Xóa phòng thành công"));
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
