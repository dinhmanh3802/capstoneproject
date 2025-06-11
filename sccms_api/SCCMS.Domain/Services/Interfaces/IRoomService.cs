using SCCMS.Domain.DTOs.RoomDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.Services.Interfaces
{
    public interface IRoomService
    {
        Task<IEnumerable<RoomDto>> GetAllRoomsAsync(int courseId);
        Task<RoomDto> GetRoomByIdAsync(int id);
        Task CreateRoomAsync(RoomCreateDto roomDto);
        Task UpdateRoomAsync(int id, RoomUpdateDto roomDto);
        Task DeleteRoomAsync(int id);
    }
}
