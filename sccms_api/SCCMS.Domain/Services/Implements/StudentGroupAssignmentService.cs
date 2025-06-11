using AutoMapper;
using DocumentFormat.OpenXml.Office2016.Excel;
using SCCMS.Domain.DTOs.StudentGroupAssignmentDtos;
using SCCMS.Domain.DTOs.StudentGroupDtos;
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
    public class StudentGroupAssignmentService : IStudentGroupAssignmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public StudentGroupAssignmentService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task AddStudentsIntoGroupAsync(StudentGroupAssignmentDto dto)
        {
            // Kiểm tra xem danh sách StudentIds có hợp lệ không
            if (dto.StudentIds == null || !dto.StudentIds.Any())
            {
                throw new ArgumentException("Danh sách sinh viên không được để trống.");
            }

            // Lấy tất cả các StudentCourse liên quan đến danh sách StudentIds trong cùng khóa học và có trạng thái Approved
            var approvedStudentCourses = await _unitOfWork.StudentCourse
                .GetAllAsync(sc => dto.StudentIds.Contains(sc.StudentId) && sc.CourseId == dto.CourseId && sc.Status == ProgressStatus.Approved || sc.Status== ProgressStatus.Enrolled);

            // Lấy danh sách StudentIds có trạng thái không phải Approved
            var unapprovedStudentIds = dto.StudentIds.Except(approvedStudentCourses.Select(sc => sc.StudentId)).ToList();

            // Nếu có bất kỳ sinh viên nào không có trạng thái Approved, ném ra lỗi
            if (unapprovedStudentIds.Any())
            {
                throw new ArgumentException($"Khóa sinh đã tốt nghiệp hoặc không trong khóa tu thì không thể thay đổi chánh");
            }

            // Lấy tất cả các phân nhóm sinh viên hiện tại trong cùng khóa học (CourseId)
            var existingAssignments = await _unitOfWork.StudentGroupAssignment
                .GetAllAsync(s => dto.StudentIds.Contains(s.StudentId) && s.StudentGroup.CourseId == dto.CourseId);

            var newAssignments = new List<StudentGroupAssignment>();

            foreach (var studentId in dto.StudentIds)
            {
                // Tìm xem sinh viên này đã có trong bất kỳ nhóm nào của khóa học chưa
                var existingAssignment = existingAssignments.FirstOrDefault(s => s.StudentId == studentId);

                if (existingAssignment != null)
                {
                    // Xóa bản ghi cũ nếu sinh viên đã ở trong nhóm khác của khóa học này
                    await _unitOfWork.StudentGroupAssignment.DeleteAsync(existingAssignment);
                }

                // Tạo bản ghi mới với StudentGroupId đã cập nhật
                var studentGroupAssignment = new StudentGroupAssignment
                {
                    StudentId = studentId,
                    StudentGroupId = dto.StudentGroupId
                };

                newAssignments.Add(studentGroupAssignment);
            }

            // Thêm tất cả các sinh viên vào nhóm
            if (newAssignments.Any())
            {
                await _unitOfWork.StudentGroupAssignment.AddRangeAsync(newAssignments);
            }

            // Lưu các thay đổi
            await _unitOfWork.SaveChangeAsync();
        }


        public async Task RemoveStudentsFromGroupAsync(StudentGroupAssignmentDto dto)
        {
            foreach (var studentId in dto.StudentIds)
            {
                var existingAssignment = await _unitOfWork.StudentGroupAssignment
                    .GetAsync(s => s.StudentId == studentId && s.StudentGroupId == dto.StudentGroupId);

                if (existingAssignment == null)
                {
                    throw new ArgumentException($"Khóa sinh {studentId} không có trong chánh {dto.StudentGroupId}.");
                }

                _unitOfWork.StudentGroupAssignment.DeleteAsync(existingAssignment);
            }

            await _unitOfWork.SaveChangeAsync();
        }

        public async Task UpdateStudentGroupAssignmentAsync(int studentId, int studentGroupId, int studentGroupIdUpdate)
        {
            // Tìm StudentGroupAssignment hiện tại từ database
            var existingAssignment = await _unitOfWork.StudentGroupAssignment.GetAsync(sga => sga.StudentId == studentId && sga.StudentGroupId == studentGroupId);

            if (existingAssignment == null)
            {
                throw new ArgumentException("Không tìm thấy StudentGroupAssignment với thông tin đã cung cấp.");
            }

            // Xóa bản ghi hiện tại
            await _unitOfWork.StudentGroupAssignment.DeleteAsync(existingAssignment);
            await _unitOfWork.SaveChangeAsync();

            // Tạo bản ghi mới với StudentGroupIdUpdate
            var newAssignment = new StudentGroupAssignment
            {
                StudentId = studentId,
                StudentGroupId = studentGroupIdUpdate
            };

            await _unitOfWork.StudentGroupAssignment.AddAsync(newAssignment);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task DistributeStudentsByGroupAsync(int courseId)
        {
            // Lấy tất cả học sinh thuộc khóa học với CourseId
            var studentsInCourse = await _unitOfWork.StudentCourse.FindAsync(sc => sc.CourseId == courseId, "Student");

            if (studentsInCourse == null || !studentsInCourse.Any())
            {
                throw new ArgumentException($"Không tìm thấy học sinh nào thuộc khóa học với CourseId: {courseId}");
            }

            // Kiểm tra nếu có bất kỳ studentCourse nào có Status = Pending
            if (studentsInCourse.Any(sc => sc.Status == ProgressStatus.Pending))
            {
                throw new InvalidOperationException("Cần phải duyệt hết đơn đăng ký trước khi xếp chánh.");
            }

            // Chỉ chọn những studentCourse có Status là Approved
            var validStudentsInCourse = studentsInCourse.Where(sc => sc.Status == ProgressStatus.Approved).ToList();

            if (!validStudentsInCourse.Any())
            {
                throw new InvalidOperationException("Không có học sinh hợp lệ để xếp nhóm.");
            }

            var students = validStudentsInCourse.Select(sc => sc.Student).ToList();

            // Lấy tất cả các nhóm thuộc khóa học với courseId
            var studentGroupsInCourse = await _unitOfWork.StudentGroup
                .FindAsync(g => g.CourseId == courseId);
            var studentGroupIdsInCourse = studentGroupsInCourse.Select(g => g.Id).ToList();

            // Lọc những học sinh đã được xếp nhóm trong khóa học hiện tại dựa trên StudentGroupId
            var studentGroupAssignments = await _unitOfWork.StudentGroupAssignment
                .FindAsync(sga => students.Select(s => s.Id).Contains(sga.StudentId) && studentGroupIdsInCourse.Contains(sga.StudentGroupId));

            // Lấy danh sách các StudentId đã được xếp nhóm
            var assignedStudentIds = studentGroupAssignments.Select(sga => sga.StudentId).ToHashSet();

            // Lọc ra những học sinh chưa được phân nhóm trong khóa học này
            var unassignedStudents = students.Where(s => !assignedStudentIds.Contains(s.Id)).ToList();

            if (!unassignedStudents.Any())
            {
                throw new InvalidOperationException("Tất cả học sinh đã được xếp nhóm trong khóa học này.");
            }

            // Tính độ tuổi cho mỗi học sinh chưa được xếp nhóm
            var currentDate = DateTime.Now;
            var studentsWithAge = unassignedStudents.Select(student => new
            {
                Student = student,
                Age = (int)((currentDate - student.DateOfBirth).TotalDays / 365.25) // Tính tuổi
            }).ToList();

            // Lấy các nhóm học sinh trong courseId theo giới tính
            var studentGroups = await _unitOfWork.StudentGroup.FindAsync(g => g.CourseId == courseId);

            if (studentGroups == null || !studentGroups.Any())
            {
                throw new ArgumentException($"Không tìm thấy nhóm nào cho khóa học với CourseId: {courseId}");
            }

            foreach (var genderGroup in studentGroups.GroupBy(g => g.Gender))
            {
                // Lọc học sinh theo giới tính của nhóm
                var studentsByGender = studentsWithAge
                    .Where(s => s.Student.Gender == genderGroup.Key)
                    .OrderBy(s => s.Age) // Sắp xếp học sinh theo độ tuổi
                    .ToList();

                // Nếu không có học sinh nào thuộc giới tính này, bỏ qua nhóm đó
                if (!studentsByGender.Any())
                {
                    continue;
                }

                // Chia học sinh đều vào các nhóm
                var genderGroups = genderGroup.ToList();
                int totalGroups = genderGroups.Count();
                int studentIndex = 0;

                // Lặp qua từng học sinh và phân vào nhóm
                foreach (var studentWithAge in studentsByGender)
                {
                    var targetGroup = genderGroups[studentIndex % totalGroups];
                    if (targetGroup.StudentGroupAssignment == null)
                    {
                        targetGroup.StudentGroupAssignment = new List<StudentGroupAssignment>();
                    }

                    targetGroup.StudentGroupAssignment.Add(new StudentGroupAssignment
                    {
                        StudentId = studentWithAge.Student.Id,
                        StudentGroupId = targetGroup.Id
                    });

                    studentIndex++;
                }
            }
            await _unitOfWork.SaveChangeAsync();
        }
        public async Task<List<StudentGroupDto>> AutoAssignSupervisorsAsync(int courseId)
        {
            if (courseId < 1)
            {
                throw new ArgumentException("CourseId không hợp lệ.");
            }

            var course = await _unitOfWork.Course.GetByIdAsync(courseId);
            if (course == null)
            {
                throw new ArgumentException("Không tìm thấy khóa tu.");
            }

            // Kiểm tra khóa tu đang diễn ra
            if (DateTime.Now > course.EndDate)
            {
                throw new ArgumentException("Chỉ có thể phân Huynh trưởng khi khóa tu đang diễn ra.");
            }

            // Lấy danh sách tất cả các chánh trong khóa tu
            var allGroups = await _unitOfWork.StudentGroup
                .GetAllAsync(
                    sg => sg.CourseId == courseId,
                    includeProperties: "SupervisorStudentGroup"
                );

            if (allGroups == null || !allGroups.Any())
            {
                throw new ArgumentException("Không có chánh nào trong khóa tu này.");
            }

            // Lấy danh sách tất cả các Supervisor đã được phân trong khóa tu này
            var existingSupervisorIds = allGroups
                .SelectMany(g => g.SupervisorStudentGroup.Select(ssg => ssg.SupervisorId))
                .Distinct()
                .ToList();

            // Lấy danh sách Huynh trưởng khả dụng (chưa được phân vào chánh nào trong khóa tu này)
            var availableSupervisors = await _unitOfWork.User
                .FindAsync(
                    u => u.RoleId == SD.RoleId_Supervisor &&
                         u.Status == UserStatus.Active &&
                         !existingSupervisorIds.Contains(u.Id)
                );

            var availableSupervisorsList = availableSupervisors.ToList();

            if (availableSupervisorsList == null || !availableSupervisorsList.Any())
            {
                throw new ArgumentException("Không còn Huynh trưởng nào để phân Chánh trong khóa tu này.");
            }

            // Phân loại supervisors theo giới tính
            var maleSupervisors = availableSupervisorsList
                .Where(u => u.Gender == Gender.Male)
                .ToList();

            var femaleSupervisors = availableSupervisorsList
                .Where(u => u.Gender == Gender.Female)
                .ToList();

            // Phân loại groups theo giới tính
            var maleGroups = allGroups
                .Where(g => g.Gender == Gender.Male)
                .ToList();

            var femaleGroups = allGroups
                .Where(g => g.Gender == Gender.Female)
                .ToList();

            // Xử lý từng giới tính riêng biệt
            var unassignedGroups = new List<StudentGroup>();

            // Phân bổ supervisors cho nhóm Nam
            var unassignedMaleGroups = await AssignSupervisorsByGender(maleSupervisors, maleGroups);
            unassignedGroups.AddRange(unassignedMaleGroups);

            // Phân bổ supervisors cho nhóm Nữ
            var unassignedFemaleGroups = await AssignSupervisorsByGender(femaleSupervisors, femaleGroups);
            unassignedGroups.AddRange(unassignedFemaleGroups);

            await _unitOfWork.SaveChangeAsync();

            if (unassignedGroups.Any())
            {
                return _mapper.Map<List<StudentGroupDto>>(unassignedGroups);
            }

            // Nếu tất cả chánh đã có ít nhất một Huynh trưởng
            return new List<StudentGroupDto>();
        }

        private async Task<List<StudentGroup>> AssignSupervisorsByGender(List<User> supervisors, List<StudentGroup> groups)
        {
            // Lấy danh sách supervisors đã được phân trong các groups
            var existingSupervisorIds = groups
                .SelectMany(g => g.SupervisorStudentGroup.Select(ssg => ssg.SupervisorId))
                .Distinct()
                .ToList();

            // Loại bỏ supervisors đã được phân
            var availableSupervisors = supervisors
                .Where(s => !existingSupervisorIds.Contains(s.Id))
                .ToList();

            // Tổng số Huynh trưởng
            var totalSupervisors = existingSupervisorIds.Count + availableSupervisors.Count;
            var totalGroups = groups.Count;

            if (totalGroups == 0)
            {
                // Không có chánh nào, trả về danh sách trống
                return new List<StudentGroup>();
            }

            // Tính số Huynh trưởng tối thiểu mỗi chánh nhận được
            int supervisorsPerGroup = totalSupervisors / totalGroups;
            int extraSupervisors = totalSupervisors % totalGroups;

            // Sắp xếp các chánh theo số lượng Huynh trưởng hiện có (tăng dần)
            var sortedGroups = groups
                .OrderBy(g => g.SupervisorStudentGroup?.Count() ?? 0)
                .ToList();

            // Khởi tạo chỉ số cho Huynh trưởng khả dụng
            int supervisorIndex = 0;
            int totalAvailableSupervisors = availableSupervisors.Count;

            // Phân bổ Huynh trưởng tối thiểu cho mỗi chánh
            foreach (var group in sortedGroups)
            {
                int currentSupervisors = group.SupervisorStudentGroup?.Count() ?? 0;
                int supervisorsToAdd = supervisorsPerGroup - currentSupervisors;

                // Phân bổ Huynh trưởng tối thiểu
                while (supervisorsToAdd > 0 && supervisorIndex < totalAvailableSupervisors)
                {
                    var supervisor = availableSupervisors[supervisorIndex];
                    supervisorIndex++;

                    var supervisorStudentGroup = new SupervisorStudentGroup
                    {
                        SupervisorId = supervisor.Id,
                        StudentGroupId = group.Id
                    };

                    await _unitOfWork.SupervisorStudentGroup.AddAsync(supervisorStudentGroup);
                    //Tạo thông báo: Lấy thông tin khóa tu và nhóm để gửi thông báo
                    var groupName = group.GroupName;
                    string message = $"Bạn đã được thêm vào chánh '{groupName}' của khóa tu '{group.Course.CourseName}'.";
                    string link = "student-groups/" + group.Id;
                    await _notificationService.NotifyUserAsync(supervisor.Id, message, link);
                    supervisorsToAdd--;
                }
            }

            // Phân bổ Huynh trưởng dư cho các chánh
            foreach (var group in sortedGroups)
            {
                if (extraSupervisors > 0 && supervisorIndex < totalAvailableSupervisors)
                {
                    var supervisor = availableSupervisors[supervisorIndex];
                    supervisorIndex++;

                    var supervisorStudentGroup = new SupervisorStudentGroup
                    {
                        SupervisorId = supervisor.Id,
                        StudentGroupId = group.Id
                    };

                    await _unitOfWork.SupervisorStudentGroup.AddAsync(supervisorStudentGroup);
                    //Tạo thông báo: Lấy thông tin khóa tu và nhóm để gửi thông báo
                    var groupName = group.GroupName;
                    string message = $"Bạn đã được thêm vào chánh '{groupName}' của khóa tu '{group.Course.CourseName}'.";
                    string link = "student-groups/" + group.Id;
                    await _notificationService.NotifyUserAsync(supervisor.Id, message, link);
                    extraSupervisors--;
                }
                else
                {
                    break;
                }
            }

            // Danh sách chánh chưa có Huynh trưởng
            var unassignedGroups = sortedGroups
                .Where(g => g.SupervisorStudentGroup == null || !g.SupervisorStudentGroup.Any())
                .ToList();

            return unassignedGroups;
        }


    }

}
