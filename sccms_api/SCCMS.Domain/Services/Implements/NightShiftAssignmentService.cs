using AutoMapper;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using FirebaseAdmin.Messaging;
using SCCMS.Domain.DTOs.NightShiftDtos;
using SCCMS.Domain.DTOs.RoomDtos;
using SCCMS.Domain.DTOs.UserDtos;
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
    public class NightShiftAssignmentService : INightShiftAssignmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;

        public NightShiftAssignmentService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task ScheduleShiftsAsync(int courseId)
        {
            var course = await _unitOfWork.Course.GetByIdAsync(courseId);
            DateTime startDate = course.StartDate;
            DateTime endDate = course.EndDate;

            var assignments = new List<NightShiftAssignment>();
            var shifts = (await _unitOfWork.NightShift.FindAsync(ns => ns.CourseId == courseId)).ToList();
            var rooms = (await _unitOfWork.Room.FindAsync(r => r.CourseId == courseId)).ToList();

            // Lưu trữ ngày cuối cùng mà mỗi nhân viên làm việc
            var staffLastWorkedDay = new Dictionary<int, DateTime>();

            // Lấy tất cả nhân viên
            var allStaffs = (await _unitOfWork.User.GetAllAsync()).ToList();
            // Lấy tất cả thời gian rảnh của nhân viên trong khoảng thời gian khóa học
            var allStaffFreeTimes = (await _unitOfWork.StaffFreeTime.FindAsync(sft => sft.Date >= startDate && sft.Date <= endDate && sft.isCancel != true)).ToList();
            // Lấy tất cả các phân công trong khoảng thời gian khóa học
            var allAssignments = (await _unitOfWork.NightShiftAssignment.FindAsync(a => a.Date >= startDate && a.Date <= endDate)).ToList();

            // Khởi tạo staffLastWorkedDay với dữ liệu lịch sử
            var historicalAssignments = (await _unitOfWork.NightShiftAssignment.GetAllAsync()).ToList();
            staffLastWorkedDay = historicalAssignments
                .Where(a => a.UserId.HasValue)
                .GroupBy(a => a.UserId.Value)
                .ToDictionary(g => g.Key, g => g.Max(a => a.Date));

            for (DateTime day = startDate.Date; day <= endDate.Date; day = day.AddDays(1))
            {
                foreach (var shift in shifts)
                {
                    foreach (var room in rooms)
                    {
                        int requiredStaff = room.NumberOfStaff;

                        // Lấy danh sách phân công hiện có cho phòng và ngày này
                        var existingAssignmentsForRoomAndDay = allAssignments
                            .Where(a => a.RoomId == room.Id && a.Date == day && a.NightShiftId == shift.Id)
                            .ToList();

                        int existingStaffCount = existingAssignmentsForRoomAndDay.Count;

                        // Nếu phòng đã đủ số lượng nhân viên, bỏ qua việc xếp thêm
                        if (existingStaffCount >= requiredStaff)
                        {
                            // Nếu đủ nhân viên rồi thì bỏ qua, duyệt tiếp sang phòng khác
                            continue;
                        }

                        // Số lượng nhân viên cần xếp thêm
                        int staffNeeded = requiredStaff - existingStaffCount;

                        for (int i = 0; i < staffNeeded; i++)
                        {
                            var availableStaff = FindAvailableStaff(
                                day, shift, room, staffLastWorkedDay, allStaffs, allStaffFreeTimes, allAssignments);

                            if (availableStaff != null)
                            {
                                var assignment = new NightShiftAssignment
                                {
                                    NightShiftId = shift.Id,
                                    RoomId = room.Id,
                                    Status = NightShiftAssignmentStatus.notStarted,
                                    Date = day,
                                    UserId = availableStaff.Id
                                };

                                staffLastWorkedDay[availableStaff.Id] = day;
                                await _unitOfWork.NightShiftAssignment.AddAsync(assignment);
                                if (assignment.UserId != null)
                                {
                                    int id = assignment.UserId.Value;

                                    await _notificationService.NotifyUserAsync(id,
                                        $"Bạn được phân công ca trực mới vào ngày {assignment.Date.ToString("dd/MM/yyyy")}. Truy cập vào lịch trực để xem chi tiết", "my-night-shift");
                                }
                                assignments.Add(assignment);
                                // Thêm vào allAssignments để cập nhật danh sách phân công
                                allAssignments.Add(assignment);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }

            await _unitOfWork.SaveChangeAsync();
        }

        private User FindAvailableStaff(
            DateTime day,
            NightShift shift,
            Room room,
            Dictionary<int, DateTime> staffLastWorkedDay,
            IEnumerable<User> allStaffs,
            IEnumerable<StaffFreeTime> allStaffFreeTimes,
            IEnumerable<NightShiftAssignment> allAssignments)
        {
            // Lấy danh sách nhân viên phù hợp theo giới tính
            var suitableStaffs = allStaffs.Where(u => u.Gender == room.Gender).ToList();

            // Danh sách nhân viên rảnh vào ngày đó
            var freeUserIds = allStaffFreeTimes
                .Where(sft => sft.Date == day)
                .Select(sft => sft.UserId)
                .Distinct()
                .ToList();

            suitableStaffs = suitableStaffs.Where(u => freeUserIds.Contains(u.Id)).ToList();

            // Loại bỏ những nhân viên đã được phân công vào bất kỳ ca nào trong ngày này
            var assignedUserIds = allAssignments
                .Where(a => a.Date == day)
                .Select(a => a.UserId)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .Distinct()
                .ToList();

            suitableStaffs = suitableStaffs.Where(u => !assignedUserIds.Contains(u.Id)).ToList();

            if (!suitableStaffs.Any())
            {
                // Nếu không có nhân viên rảnh, trả về null
                return null;
            }

            // Sắp xếp nhân viên theo ngày làm việc gần nhất xa nhất
            suitableStaffs = suitableStaffs
                .OrderBy(u => staffLastWorkedDay.ContainsKey(u.Id) ? staffLastWorkedDay[u.Id] : DateTime.MinValue)
                .ThenBy(u => u.Id)
                .ToList();

            return suitableStaffs.FirstOrDefault();
        }

        public async Task<IEnumerable<UserDto>> SuggestStaffForShiftAsync(DateTime date, int shiftId, int roomId, int courseId)
        {
            var shift = await _unitOfWork.NightShift.GetByIdAsync(shiftId);
            var room = await _unitOfWork.Room.GetByIdAsync(roomId);

            if (shift == null || room == null)
            {
                throw new ArgumentException("Shift or Room not found.");
            }

            // Lấy danh sách tất cả các ca trực, sắp xếp theo thời gian bắt đầu
            var shifts = (await _unitOfWork.NightShift.FindAsync(s => s.CourseId == courseId))
                .OrderBy(s => s.StartTime)
                .ToList();

            // Xác định chỉ số của ca trực hiện tại
            int currentShiftIndex = shifts.FindIndex(s => s.Id == shift.Id);

            // Lấy danh sách nhân viên có giới tính phù hợp
            var suitableStaffs = await _unitOfWork.User.FindAsync(u => u.Gender == room.Gender);

            // Danh sách nhân viên rảnh vào ngày đó
            var freeUserIds = (await _unitOfWork.StaffFreeTime.FindAsync(sft => sft.Date == date && sft.isCancel!= true))
                .Select(sft => sft.UserId)
                .Distinct()
                .ToList();

            // Lọc nhân viên phù hợp và rảnh
            var availableStaffs = suitableStaffs.Where(u => freeUserIds.Contains(u.Id)).ToList();

            // Lấy tất cả các phân công trong ngày
            var assignmentsOnDate = await _unitOfWork.NightShiftAssignment.FindAsync(a => a.Date == date);

            // Nhóm các phân công theo nhân viên
            var staffAssignmentsOnDate = assignmentsOnDate
                .Where(a => a.UserId.HasValue)
                .GroupBy(a => a.UserId.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToList()
                );

            // Sắp xếp nhân viên theo thứ tự ưu tiên
            var prioritizedStaffs = availableStaffs
                .Select(u =>
                {
                    var hasAssignments = staffAssignmentsOnDate.ContainsKey(u.Id);
                    var assignments = hasAssignments ? staffAssignmentsOnDate[u.Id] : new List<NightShiftAssignment>();

                    // Kiểm tra nhân viên đã làm việc trong phòng này vào ngày đó chưa
                    bool hasWorkedInSameRoom = assignments.Any(a => a.RoomId == roomId);

                    // Kiểm tra nhân viên đã làm việc trong ngày đó nhưng ở phòng khác
                    bool hasWorkedInOtherRoom = hasAssignments && !hasWorkedInSameRoom;

                    // Tính khoảng cách ca trực xa nhất với ca hiện tại
                    int maxShiftDistance = -1;
                    if (hasAssignments)
                    {
                        var workedShiftIds = assignments.Select(a => a.NightShiftId).Distinct();
                        foreach (var workedShiftId in workedShiftIds)
                        {
                            int shiftIndex = shifts.FindIndex(s => s.Id == workedShiftId);
                            int distance = Math.Abs(shiftIndex - currentShiftIndex);
                            if (distance > maxShiftDistance)
                            {
                                maxShiftDistance = distance;
                            }
                        }
                    }
                    else
                    {
                        // Nhân viên chưa làm việc trong ngày đó
                        maxShiftDistance = shifts.Count;
                    }

                    return new
                    {
                        User = u,
                        HasWorkedInSameRoom = hasWorkedInSameRoom,
                        HasWorkedInOtherRoom = hasWorkedInOtherRoom,
                        MaxShiftDistance = maxShiftDistance,
                        HasWorkedOnDate = hasAssignments
                    };
                })
                .OrderByDescending(u => u.HasWorkedInSameRoom) // Ưu tiên nhân viên đã làm việc trong phòng này
                .ThenBy(u => u.MaxShiftDistance)     // ưu tiên khoảng cách ca trực xa nhất
                .ThenByDescending(u => u.HasWorkedInOtherRoom) // nhân viên làm ở phòng khác
                .ThenBy(u => u.HasWorkedOnDate)                // nhân viên chưa trực
                .ThenBy(u => u.User.Id)
                .Select(u => u.User)
                .ToList();

            var userDtos = prioritizedStaffs.Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                FullName = u.FullName,
                PhoneNumber = u.PhoneNumber,
                Gender = u.Gender,
                DateOfBirth = u.DateOfBirth,
                Address = u.Address,
                NationalId = u.NationalId,
                Status = u.Status,
                RoleId = u.RoleId
            });

            return userDtos;
        }


        //admin assign staff to shift
        public async Task AssignStaffToShiftAsync(NightShiftAssignmentCreateDto assignStaffDto)
        {
            // Lấy tất cả các bản ghi cho phòng, ca, ngày cụ thể
            var existingAssignments = await _unitOfWork.NightShiftAssignment.GetAllAsync(
                a => a.Date == assignStaffDto.Date &&
                     a.NightShiftId == assignStaffDto.NightShiftId &&
                     a.RoomId == assignStaffDto.RoomId);

            var room = await _unitOfWork.Room.GetByIdAsync((int)assignStaffDto.RoomId);
            if (room == null)
            {
                throw new ArgumentException("Room does not exist.");
            }
            var shift = await _unitOfWork.NightShift.GetByIdAsync(assignStaffDto.NightShiftId);
            if (shift == null)
            {
                throw new ArgumentException("Night shift does not exist.");
            }
            foreach (var userId in assignStaffDto.UserIds)
            {
                var user = await _unitOfWork.User.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ArgumentException("User does not exist.");
                }
            }


            // Duyệt từng UserId trong UserIds và tạo bản ghi tương ứng
            foreach (var userId in assignStaffDto.UserIds)
            {
                var existingAssignment = existingAssignments.FirstOrDefault(a => a.UserId == userId);

                if (existingAssignment == null)
                {
                    // Tạo bản ghi mới nếu chưa tồn tại
                    var newAssignment = new NightShiftAssignment
                    {
                        NightShiftId = assignStaffDto.NightShiftId,
                        UserId = userId,
                        RoomId = assignStaffDto.RoomId,
                        Date = assignStaffDto.Date,
                        Status = NightShiftAssignmentStatus.notStarted
                    };

                    await _unitOfWork.NightShiftAssignment.AddAsync(newAssignment);
                    if (newAssignment.UserId != null)
                    {
                        int id = newAssignment.UserId.Value;

                        await _notificationService.NotifyUserAsync(id,
                            $"Bạn được phân công ca trực mới vào ngày {newAssignment.Date.ToString("dd/MM/yyyy")}. Truy cập vào lịch trực để xem chi tiết", "my-night-shift");
                    }
                }
                else
                {
                    // Nếu bản ghi đã tồn tại, cập nhật trạng thái nếu cần
                    existingAssignment.Status = NightShiftAssignmentStatus.notStarted;
                    await _unitOfWork.NightShiftAssignment.UpdateAsync(existingAssignment);
                }
            }
            await _unitOfWork.SaveChangeAsync();
        }



        public async Task UpdateAssignmentAsync(NightShiftAssignmentUpdateDto updateDto, int userRole)
        {
            var assignment = await _unitOfWork.NightShiftAssignment.GetByIdAsync(updateDto.Id);

            var room = await _unitOfWork.Room.GetByIdAsync((int)updateDto.RoomId);
            if (room == null)
            {
                throw new ArgumentException("Room does not exist.");
            }
            var shift = await _unitOfWork.NightShift.GetByIdAsync(updateDto.NightShiftId);
            if (shift == null)
            {
                throw new ArgumentException("Night shift does not exist.");
            }
            var user = await _unitOfWork.User.GetByIdAsync((int)updateDto.UserId);
            if (user == null)
            {
                throw new ArgumentException("User does not exist.");
            }


            // Lấy ngày hôm nay
            var today = DateTime.Now.Date;

            // Kiểm tra nếu assignment diễn ra hôm nay
            if (assignment.Date.Date == today)
            {
                // Nếu role không phải manager hoặc secretary thì throw exception
                if (userRole != SD.RoleId_Manager && userRole != SD.RoleId_Secretary)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền cập nhật ca trực.");
                }
            }

            if (assignment == null)
            {
                throw new ArgumentException("Ca trực không tồn tại.");
            }

            _mapper.Map(updateDto, assignment);

            await _unitOfWork.NightShiftAssignment.UpdateAsync(assignment);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task DeleteAssignmentAsync(int assignmentId)
        {
            var assignment = await _unitOfWork.NightShiftAssignment.GetByIdAsync(assignmentId);

            if (assignment == null)
            {
                throw new ArgumentException("Không tìm thấy ca trực");
            }

            await _unitOfWork.NightShiftAssignment.DeleteAsync(assignment);
            await _unitOfWork.SaveChangeAsync();
            if (assignment.UserId != null)
            {
                int id = assignment.UserId.Value;

                await _notificationService.NotifyUserAsync(id,
                    $"Bạn được hủy ca trực vào ngày {assignment.Date.ToString("dd/MM/yyyy")}. Truy cập vào lịch trực để xem chi tiết", "my-night-shift");
            }

        }

        public async Task<IEnumerable<MyShiftAssignmentDto>> GetShiftsByCourseAndUserAsync(int? courseId, int userId)
        {
            if (courseId == null)
            {
                var assignments = await _unitOfWork.NightShiftAssignment.GetAllAsync(sa => sa.UserId == userId, includeProperties: "NightShift,Room,User");

                // Chuyển đổi sang DTO
                var shiftAssignmentDtos = _mapper.Map<IEnumerable<MyShiftAssignmentDto>>(assignments);

                return shiftAssignmentDtos;
            }
            else
            {
                // Lấy các ShiftAssignment thỏa mãn điều kiện courseId và userId
                var assignments = await _unitOfWork.NightShiftAssignment.GetAllAsync(
                    sa => sa.UserId == userId && sa.NightShift.CourseId == courseId,
                    includeProperties: "NightShift,Room");

                // Chuyển đổi sang DTO
                var shiftAssignmentDtos = _mapper.Map<IEnumerable<MyShiftAssignmentDto>>(assignments);

                return shiftAssignmentDtos;
            }

        }

        public async Task<MyShiftAssignmentDto> GetNightShiftsByIdAsync(int shiftId)
        {
            var nightShift = await _unitOfWork.NightShiftAssignment.GetAsync(ns => ns.Id == shiftId, includeProperties: "NightShift,Room,User");
            if (nightShift == null)
            {
                throw new ArgumentException("Ca trực không tồn tại.");
            }
            return _mapper.Map<MyShiftAssignmentDto>(nightShift);
        }

        public async Task<IEnumerable<MyShiftAssignmentDto>> GetShiftsByCourseAsync(int courseId, DateTime? dateTime = null, NightShiftAssignmentStatus? status = null)
        {
            if (status != null)
            {
                var assignments = await _unitOfWork.NightShiftAssignment.GetAllAsync(
                                       sa => sa.NightShift.CourseId == courseId &&
                                                                (dateTime == null || sa.Date.Date == dateTime.Value.Date) &&
                                                                                         (status == null || sa.Status == status),
                                                          includeProperties: "NightShift,Room,User");

                // Chuyển đổi sang DTO
                var shiftAssignmentDtos = _mapper.Map<IEnumerable<MyShiftAssignmentDto>>(assignments);
                return shiftAssignmentDtos;
            }
            else
            {
                var assignments = await _unitOfWork.NightShiftAssignment.GetAllAsync(
                                       sa => sa.NightShift.CourseId == courseId &&
                                                                (dateTime == null || sa.Date.Date == dateTime.Value.Date) &&
                                                                sa.Status != NightShiftAssignmentStatus.cancelled,
                                                          includeProperties: "NightShift,Room,User");

                // Chuyển đổi sang DTO
                var shiftAssignmentDtos = _mapper.Map<IEnumerable<MyShiftAssignmentDto>>(assignments);

                return shiftAssignmentDtos;
            }

        }



        public async Task UpdateAssignmentStatusAsync(NightShiftAssignmentRejectDto updateDto, int userRole)
        {
            var assignment = await _unitOfWork.NightShiftAssignment.GetByIdAsync(updateDto.Id);

            if (assignment == null)
            {
                throw new ArgumentException("Ca trực không tồn tại.");
            }

            // Kiểm tra nếu assignment diễn ra hôm nay
            var today = DateTime.Now.Date;
            if (assignment.Date.Date == today)
            {
                // Nếu role không phải manager hoặc secretary thì throw exception
                if (userRole != SD.RoleId_Manager && userRole != SD.RoleId_Secretary)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền cập nhật trạng thái ca trực này.");
                }
            }

            // Xử lý từ chối
            if (updateDto.Status == NightShiftAssignmentStatus.rejected)
            {
                if (string.IsNullOrEmpty(updateDto.RejectionReason))
                {
                    throw new ArgumentException("Lý do từ chối là bắt buộc khi từ chối ca trực.");
                }

                assignment.Status = NightShiftAssignmentStatus.rejected;
                assignment.RejectionReason = updateDto.RejectionReason;
                var freeTime = await _unitOfWork.StaffFreeTime.FindAsync(s => s.UserId == assignment.UserId && s.Date == assignment.Date);
                foreach (var item in freeTime)
                {
                    item.isCancel = true;
                    await _unitOfWork.StaffFreeTime.UpdateAsync(item);
                }

            }
            // Xử lý hủy từ chối
            else if (updateDto.Status == NightShiftAssignmentStatus.notStarted)
            {
                if (assignment.Status != NightShiftAssignmentStatus.rejected)
                {
                    throw new ArgumentException("Chỉ các ca trực đang ở trạng thái từ chối mới có thể hủy từ chối.");
                }

                assignment.Status = NightShiftAssignmentStatus.notStarted;
                assignment.RejectionReason = null;
                var freeTime = await _unitOfWork.StaffFreeTime.FindAsync(s => s.UserId == assignment.UserId && s.Date == assignment.Date);
                foreach (var item in freeTime)
                {
                    item.isCancel = false;
                    await _unitOfWork.StaffFreeTime.UpdateAsync(item);
                }
            }
            else
            {
                // Xử lý các trạng thái khác nếu cần
                assignment.Status = updateDto.Status;
                // Có thể thêm logic cho các trạng thái khác
            }
            string message = "";

            if (assignment.Status == NightShiftAssignmentStatus.rejected)
            {
                message += $"{assignment.User.UserName} đã hủy ca trực ở ngày {assignment.Date.ToString("dd/MM/yyyy")}";
            }
            else if (assignment.Status == NightShiftAssignmentStatus.notStarted)
            {
                message += $"{assignment.User.UserName} đã hủy từ chối ca trực ở ngày {assignment.Date.ToString("dd/MM/yyyy")}";
            }

            string link = $"reject-night-shift?id={assignment.Id}";

            var sendNotification = await _unitOfWork.User.FindAsync(u => u.RoleId == SD.RoleId_Manager || u.RoleId == SD.RoleId_Secretary);

            foreach (var user in sendNotification)
            {
                await _notificationService.NotifyUserAsync(user.Id, message, link);
            }

            await _unitOfWork.NightShiftAssignment.UpdateAsync(assignment);
            await _unitOfWork.SaveChangeAsync();
        }


        public async Task ReassignAssignmentAsync(NightShiftAssignmentReassignDto reassignDto, int userRole)
        {
            // Lấy ca trực cũ
            var assignment = await _unitOfWork.NightShiftAssignment.GetByIdAsync(reassignDto.Id);
            if (assignment == null)
            {
                throw new ArgumentException("Ca trực không tồn tại.");
            }

            var freeTime= await _unitOfWork.StaffFreeTime.FindAsync(s=> s.UserId== assignment.UserId && s.Date== assignment.Date && s.isCancel != true);
            foreach(var item in freeTime)
            {
                item.isCancel = true;
                await _unitOfWork.StaffFreeTime.UpdateAsync(item);
            }

            // Lấy ngày hôm nay
            var today = DateTime.Now.Date;

            // Kiểm tra nếu assignment diễn ra hôm nay
            if (assignment.Date.Date >= today.AddDays(3))
            {
                // Nếu role không phải manager hoặc secretary thì throw exception
                if (userRole != SD.RoleId_Manager && userRole != SD.RoleId_Secretary)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền gán lại ca trực.");
                }
            }

            // Kiểm tra người dùng mới có tồn tại không
            var newUser = await _unitOfWork.User.GetByIdAsync(reassignDto.NewUserId);
            if (newUser == null)
            {
                throw new ArgumentException("Người dùng mới không tồn tại.");
            }

            // Kiểm tra người dùng mới có sẵn sàng cho ca trực này không
            bool isAvailable = await IsUserAvailableForShiftAsync(reassignDto.NewUserId, assignment);
            if (!isAvailable)
            {
                throw new ArgumentException("Người dùng mới không khả dụng cho ca trực này.");
            }

            // Cập nhật trạng thái của ca trực cũ
            assignment.Status = NightShiftAssignmentStatus.cancelled;
            await _unitOfWork.NightShiftAssignment.UpdateAsync(assignment);

            // Tạo bản ghi mới cho người dùng mới
            var newAssignment = new NightShiftAssignment
            {
                Date = assignment.Date,
                UserId = reassignDto.NewUserId,
                NightShiftId = assignment.NightShiftId,
                RoomId = assignment.RoomId,
                Status = NightShiftAssignmentStatus.notStarted,
            };

            await _unitOfWork.NightShiftAssignment.AddAsync(newAssignment);

            // Gửi thông báo cho người dùng mới
            string message = $"Bạn mới được thêm vào ca trực ở ngày {newAssignment.Date.ToString("dd/MM/yyyy")}. Truy cập và lịch để xem chi tiết.";
            await _notificationService.NotifyUserAsync(reassignDto.NewUserId, message, "my-night-shift");

            // Gửi thông báo cho người dùng cũ
            message = $"Yêu cầu hủy ca trực ở ngày {newAssignment.Date.ToString("dd/MM/yyyy")} đã được duyệt. Truy cập và lịch để xem chi tiết.";
            await _notificationService.NotifyUserAsync(assignment.UserId.Value, message, "my-night-shift");

            // Lưu thay đổi
            await _unitOfWork.SaveChangeAsync();
        }


        private async Task<bool> IsUserAvailableForShiftAsync(int userId, NightShiftAssignment assignment)
        {
            // Kiểm tra xem người dùng có bị từ chối hoặc không có trong ngày đó không
            var existingAssignments = await _unitOfWork.NightShiftAssignment.FindAsync(a =>
                a.UserId == userId &&
                a.Date == assignment.Date &&
                a.NightShiftId == assignment.NightShiftId &&
                a.RoomId == assignment.RoomId);

            return !existingAssignments.Any();
        }



    }
}
