using AutoMapper;
using SCCMS.Domain.DTOs.StaffFreeTimeDtos;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.Services.Implements
{
    public class StaffFreeTimeService : IStaffFreeTimeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StaffFreeTimeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<StaffFreeTimeDto>> GetAllStaffFreeTimesAsync(int? userId, int? courseId, DateTime? dateTime)
        {
            if(userId.HasValue && courseId.HasValue && dateTime.HasValue)
            {
                var freeTimes = await _unitOfWork.StaffFreeTime.GetAllAsync(
                                       sft => sft.UserId == userId && sft.CourseId == courseId && sft.Date.Date == dateTime.Value.Date,
                                                          includeProperties: "User,Course");
                return _mapper.Map<IEnumerable<StaffFreeTimeDto>>(freeTimes);
            } else if(userId.HasValue && courseId.HasValue)
            {
                var freeTimes = await _unitOfWork.StaffFreeTime.GetAllAsync(
                                                          sft => sft.UserId == userId && sft.CourseId == courseId, includeProperties: "User,Course");
                return _mapper.Map<IEnumerable<StaffFreeTimeDto>>(freeTimes);
            } else if(dateTime.HasValue)
            {
                var freeTimes = await _unitOfWork.StaffFreeTime.GetAllAsync(
                                                          sft => sft.Date.Date == dateTime.Value.Date, includeProperties: "User,Course");
                return _mapper.Map<IEnumerable<StaffFreeTimeDto>>(freeTimes);
            }
            else if(userId.HasValue)
            {
                var freeTimes = await _unitOfWork.StaffFreeTime.GetAllAsync( sft => sft.UserId == userId,includeProperties: "User,Course");
                return _mapper.Map<IEnumerable<StaffFreeTimeDto>>(freeTimes);
            } else if(courseId.HasValue)
            {
                var freeTimes = await _unitOfWork.StaffFreeTime.GetAllAsync(sft => sft.CourseId == courseId, includeProperties: "User,Course");
                return _mapper.Map<IEnumerable<StaffFreeTimeDto>>(freeTimes);
            }
            else
            {
                var freeTimes = await _unitOfWork.StaffFreeTime.GetAllAsync(includeProperties: "User,Course");
                return _mapper.Map<IEnumerable<StaffFreeTimeDto>>(freeTimes);
            }
        }

        public async Task<StaffFreeTimeDto> GetStaffFreeTimeByIdAsync(int id)
        {
            var freeTime = await _unitOfWork.StaffFreeTime.GetAsync(sft => sft.Id == id, includeProperties: "User,Course");
            if (freeTime == null)
            {
                throw new ArgumentException("Thời gian rảnh không tồn tại.");
            }
            return _mapper.Map<StaffFreeTimeDto>(freeTime);
        }

        public async Task CreateStaffFreeTimeAsync(StaffFreeTimeCreateDto staffFreeTimeDto)
        {
            var existingFreeTimes = await _unitOfWork.StaffFreeTime.GetAllAsync(
                sft => sft.UserId == staffFreeTimeDto.UserId && sft.CourseId == staffFreeTimeDto.CourseId);

            await _unitOfWork.StaffFreeTime.DeleteRangeAsync(existingFreeTimes);

            // Thêm các thời gian rảnh mới
            var freeTimeEntities = staffFreeTimeDto.FreeDates.Select(date => new StaffFreeTime
            {
                UserId = staffFreeTimeDto.UserId,
                CourseId = staffFreeTimeDto.CourseId,
                Date = date
            }).ToList();

            await _unitOfWork.StaffFreeTime.AddRangeAsync(freeTimeEntities);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task DeleteStaffFreeTimeAsync(int id)
        {
            var freeTime = await _unitOfWork.StaffFreeTime.GetByIdAsync(id);
            if (freeTime == null)
            {
                throw new ArgumentException("Thời gian rảnh không tồn tại.");
            }

            await _unitOfWork.StaffFreeTime.DeleteAsync(freeTime);
            await _unitOfWork.SaveChangeAsync();
        }
    }
}
