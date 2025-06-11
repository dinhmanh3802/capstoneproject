using AutoMapper;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using SCCMS.Domain.DTOs.RoomDtos;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.Services.Implements
{
    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RoomService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RoomDto>> GetAllRoomsAsync(int courseId)
        {
            var rooms = await _unitOfWork.Room.FindAsync(r => r.CourseId == courseId, includeProperties: "StudentGroups,StudentGroups.StudentGroupAssignment.Student");
            return _mapper.Map<IEnumerable<RoomDto>>(rooms);
        }

        public async Task<RoomDto> GetRoomByIdAsync(int id)
        {
            var room = await _unitOfWork.Room.GetAsync(r => r.Id == id, includeProperties: "StudentGroups,StudentGroups.StudentGroupAssignment.Student");
            if (room == null)
            {
                throw new ArgumentException("Phòng không tồn tại.");
            }
            return _mapper.Map<RoomDto>(room);
        }

        public async Task CreateRoomAsync(RoomCreateDto roomDto)
        {
            var course = await _unitOfWork.Course.GetByIdAsync(roomDto.CourseId);
            if (course.Status == CourseStatus.closed || course.Status == CourseStatus.deleted)
            {
                throw new InvalidOperationException("Không thể thêm phòng vào khóa tu đã kết thúc.");
            }

            var existingRoom = await _unitOfWork.Room.GetAsync(r => r.Name == roomDto.Name && r.CourseId == roomDto.CourseId);
            if (existingRoom != null)
            {
                throw new InvalidOperationException("Tên phòng đã tồn tại. Vui lòng chọn tên khác.");
            }

            var room = _mapper.Map<Room>(roomDto);
            await _unitOfWork.Room.AddAsync(room);
            await _unitOfWork.SaveChangeAsync();

            // Gán RoomId vào StudentGroup
            if (roomDto.StudentGroupId != null && roomDto.StudentGroupId.Any())
            {
                var studentGroups = await _unitOfWork.StudentGroup.GetByIdsAsync(roomDto.StudentGroupId);
                foreach (var group in studentGroups)
                {
                    group.RoomId = room.Id;
                }

                await _unitOfWork.StudentGroup.UpdateRangeAsync(studentGroups);
                await _unitOfWork.SaveChangeAsync();
            }
        }

        public async Task UpdateRoomAsync(int id, RoomUpdateDto roomDto)
        {
            // Kiểm tra trạng thái của khóa tu
            var course = await _unitOfWork.Course.GetByIdAsync(roomDto.CourseId);
            if (course.Status == CourseStatus.closed || course.Status == CourseStatus.deleted)
            {
                throw new InvalidOperationException("Không thể sửa thông tin khóa tu đã kết thúc.");
            }

            // Kiểm tra sự tồn tại của phòng
            var room = await _unitOfWork.Room.GetByIdAsync(id);
            if (room == null)
            {
                throw new ArgumentException("Phòng không tồn tại.");
            }

            // Kiểm tra trùng tên phòng
            var duplicateRoom = await _unitOfWork.Room.GetAsync(r => r.Name == roomDto.Name && r.CourseId == roomDto.CourseId && r.Id != id);
            if (duplicateRoom != null)
            {
                throw new InvalidOperationException("Tên phòng đã tồn tại. Vui lòng chọn tên khác.");
            }

            // Cập nhật thông tin phòng
            _mapper.Map(roomDto, room);
            await _unitOfWork.Room.UpdateAsync(room);

            // Lấy danh sách StudentGroup hiện tại đang thuộc phòng này
            var currentStudentGroups = await _unitOfWork.StudentGroup.FindAsync(sg => sg.RoomId == room.Id);

            // Gán RoomId = null cho các StudentGroup cũ
            if (currentStudentGroups != null && currentStudentGroups.Any())
            {
                foreach (var group in currentStudentGroups)
                {
                    group.RoomId = null;
                }

                await _unitOfWork.StudentGroup.UpdateRangeAsync(currentStudentGroups);
            }

            // Gán RoomId cho các StudentGroup mới
            if (roomDto.StudentGroupId != null && roomDto.StudentGroupId.Any())
            {
                var newStudentGroups = await _unitOfWork.StudentGroup.GetByIdsAsync(roomDto.StudentGroupId);
                foreach (var group in newStudentGroups)
                {
                    group.RoomId = room.Id;
                }

                await _unitOfWork.StudentGroup.UpdateRangeAsync(newStudentGroups);
            }

            // Lưu các thay đổi
            await _unitOfWork.SaveChangeAsync();
        }


        public async Task DeleteRoomAsync(int id)
        {
            var room = await _unitOfWork.Room.GetByIdAsync(id,includeProperties: "StudentGroups");
            if (room == null)
            {
                throw new ArgumentException("Phòng không tồn tại.");
            }

            var studentGroups = await _unitOfWork.StudentGroup.FindAsync(sg => sg.RoomId == id);
            foreach (var group in studentGroups)
            {
                group.RoomId = null;
                await _unitOfWork.StudentGroup.UpdateAsync(group);
            }

            var relatedAssignments = await _unitOfWork.NightShiftAssignment.FindAsync(a => a.RoomId == id);
            await _unitOfWork.NightShiftAssignment.DeleteRangeAsync(relatedAssignments);
            await _unitOfWork.Room.DeleteAsync(room);
            await _unitOfWork.SaveChangeAsync();
        }
    }
}
