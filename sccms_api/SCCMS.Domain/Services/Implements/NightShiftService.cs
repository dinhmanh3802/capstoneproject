using AutoMapper;
using SCCMS.Domain.DTOs.NightShiftDtos;
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
    public class NightShiftService : INightShiftService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public NightShiftService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<NightShiftDto>> GetAllNightShiftsAsync(int courseId)
        {
            var nightShifts = await _unitOfWork.NightShift.FindAsync(ns => ns.CourseId == courseId);
            return _mapper.Map<IEnumerable<NightShiftDto>>(nightShifts);
        }

        public async Task<NightShiftDto> GetNightShiftByIdAsync(int id)
        {
            var nightShift = await _unitOfWork.NightShift.GetAsync(ns => ns.Id == id);
            if (nightShift == null)
            {
                throw new ArgumentException("Ca trực không tồn tại.");
            }
            return _mapper.Map<NightShiftDto>(nightShift);
        }

        public async Task CreateNightShiftAsync(NightShiftCreateDto nightShiftDto)
        {
            var course = await _unitOfWork.Course.GetByIdAsync(nightShiftDto.CourseId);
            if (course.Status == CourseStatus.closed || course.Status == CourseStatus.deleted)
            {
                throw new InvalidOperationException("không thể sửa thông tin khóa tu đã kết thúc");
            }

            // Lấy tất cả các ca trực hiện có trong cùng courseId
            var existingShifts = await _unitOfWork.NightShift
                .FindAsync(shift => shift.CourseId == nightShiftDto.CourseId);

            // Lấy các khoảng thời gian của ca trực mới
            var newShiftIntervals = GetIntervals(nightShiftDto.StartTime, nightShiftDto.EndTime);

            // Kiểm tra chồng chéo với từng ca trực hiện có
            foreach (var shift in existingShifts)
            {
                var existingShiftIntervals = GetIntervals(shift.StartTime, shift.EndTime);

                foreach (var newInterval in newShiftIntervals)
                {
                    foreach (var existingInterval in existingShiftIntervals)
                    {
                        if (IsOverlapping(newInterval.Start, newInterval.End, existingInterval.Start, existingInterval.End))
                        {
                            throw new ArgumentException("Ca trực mới chồng chéo với một ca trực hiện có. Vui lòng chọn thời gian khác.");
                        }
                    }
                }
            }

            // Nếu không có chồng chéo, tiến hành thêm ca trực
            var nightShift = _mapper.Map<NightShift>(nightShiftDto);
            await _unitOfWork.NightShift.AddAsync(nightShift);
            await _unitOfWork.SaveChangeAsync();
        }

        // phân chia khoảng thời gian
        private List<(TimeSpan Start, TimeSpan End)> GetIntervals(TimeSpan start, TimeSpan end)
        {
            if (start <= end)
            {
                // Ca trực không qua đêm
                return new List<(TimeSpan, TimeSpan)> { (start, end) };
            }
            else
            {
                // Ca trực qua đêm, chia thành hai khoảng
                return new List<(TimeSpan, TimeSpan)>
            {
                (start, TimeSpan.FromHours(24)),
                (TimeSpan.Zero, end)
            };
            }
        }

        // Kiểm tra chồng chéo giữa hai khoảng thời gian
        private bool IsOverlapping(TimeSpan start1, TimeSpan end1, TimeSpan start2, TimeSpan end2)
        {
            return start1 < end2 && start2 < end1;
        }


        public async Task UpdateNightShiftAsync(int id, NightShiftUpdateDto nightShiftDto)
        {
            var course = await _unitOfWork.Course.GetByIdAsync(nightShiftDto.CourseId);
            if (course.Status == CourseStatus.closed || course.Status == CourseStatus.deleted)
            {
                throw new InvalidOperationException("không thể sửa thông tin khóa tu đã kết thúc");
            }

            // Tìm ca trực hiện tại theo Id
            var existingShift = await _unitOfWork.NightShift.GetByIdAsync(id);
            if (existingShift == null)
            {
                throw new ArgumentException("Ca trực không tồn tại.");
            }

            // Lấy tất cả các ca trực hiện có trong cùng courseId, ngoại trừ ca trực đang cập nhật
            var existingShifts = await _unitOfWork.NightShift.FindAsync(shift => shift.CourseId == nightShiftDto.CourseId && shift.Id != id);

            // Lấy các khoảng thời gian của ca trực mới
            var newShiftIntervals = GetIntervals(nightShiftDto.StartTime, nightShiftDto.EndTime);

            // Kiểm tra chồng chéo với từng ca trực hiện có
            foreach (var shift in existingShifts)
            {
                var existingShiftIntervals = GetIntervals(shift.StartTime, shift.EndTime);

                foreach (var newInterval in newShiftIntervals)
                {
                    foreach (var existingInterval in existingShiftIntervals)
                    {
                        if (IsOverlapping(newInterval.Start, newInterval.End, existingInterval.Start, existingInterval.End))
                        {
                            throw new ArgumentException("Ca trực mới chồng chéo với một ca trực hiện có(từ "+ existingInterval.Start.ToString(@"hh\:mm") + " đến "+ existingInterval.End.ToString(@"hh\:mm") + "). Vui lòng chọn thời gian khác.");
                        }
                    }
                }
            }

            // Nếu không có chồng chéo, tiến hành cập nhật ca trực
            existingShift.CourseId = nightShiftDto.CourseId;
            existingShift.StartTime = nightShiftDto.StartTime;
            existingShift.EndTime = nightShiftDto.EndTime;
            existingShift.Note = nightShiftDto.Note;

            _unitOfWork.NightShift.UpdateAsync(existingShift);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task DeleteNightShiftAsync(int id)
        {
            var nightShift = await _unitOfWork.NightShift.GetByIdAsync(id);
            if (nightShift == null)
            {
                throw new ArgumentException("Ca trực không tồn tại.");
            }

            await _unitOfWork.NightShift.DeleteAsync(nightShift);
            await _unitOfWork.SaveChangeAsync();
        }
    }
}
