using AutoMapper;
using SCCMS.Domain.DTOs.StudentGroupDtos;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.Services.Implements
{
    public class StudentGroupService : IStudentGroupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public StudentGroupService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task CreateStudentGroupAsync(StudentGroupCreateDto entity)
        {
            var errorMessages = new List<string>();

            // Kiểm tra khóa học
            var course = await _unitOfWork.Course.GetByIdAsync(entity.CourseId);
            if (course == null)
                errorMessages.Add("CourseId không tham chiếu đến khóa tu hợp lệ.");

            // Kiểm tra tên nhóm
            if (string.IsNullOrWhiteSpace(entity.GroupName))
            {
                errorMessages.Add("Tên nhóm không được để trống.");
            }
            else
            {
                if (entity.GroupName.Length > 100)
                {
                    errorMessages.Add("Tên nhóm không được vượt quá 100 ký tự.");
                }

                if (ContainsSpecialCharacters(entity.GroupName))
                {
                    errorMessages.Add("Tên nhóm không được chứa ký tự đặc biệt.");
                }

                // Kiểm tra tên nhóm đã tồn tại trong khóa tu chưa (giống Update)
                var normalizedGroupName = entity.GroupName.ToLower();
                var existingGroupWithSameName = await _unitOfWork.StudentGroup.GetAsync(
                    sg => sg.CourseId == entity.CourseId &&
                          sg.GroupName.ToLower() == normalizedGroupName
                );

                if (existingGroupWithSameName != null)
                {
                    errorMessages.Add("Tên nhóm đã tồn tại trong khóa tu này. Vui lòng chọn tên khác.");
                }
            }

            // Nếu có lỗi, ném ngoại lệ
            if (errorMessages.Any())
            {
                throw new ArgumentException(string.Join(" ", errorMessages));
            }

            // Tạo StudentGroup
            var studentGroup = _mapper.Map<StudentGroup>(entity);
            await _unitOfWork.StudentGroup.AddAsync(studentGroup);
            await _unitOfWork.SaveChangeAsync();

            // Gán Supervisor cho nhóm nếu có
            if (entity.SupervisorIds != null && entity.SupervisorIds.Any())
            {
                foreach (var supervisorId in entity.SupervisorIds)
                {
                    var supervisorStudentGroup = new SupervisorStudentGroup
                    {
                        StudentGroupId = studentGroup.Id,
                        SupervisorId = supervisorId
                    };
                    await _unitOfWork.SupervisorStudentGroup.AddAsync(supervisorStudentGroup);

                    // Tạo thông báo cho Supervisor
                    string message = $"Bạn đã được thêm vào nhóm '{entity.GroupName}' của khóa tu '{course.CourseName}'.";
                    string link = "student-groups";
                    await _notificationService.NotifyUserAsync(supervisorId, message, link);
                }
                await _unitOfWork.SaveChangeAsync();
            }

            // Gọi phương thức để tạo báo cáo điểm danh cho StudentGroup
            await CreateAttendanceReportsForStudentGroupAsync(course, studentGroup.Id);
        }



        private async Task CreateAttendanceReportsForStudentGroupAsync(Course course, int studentGroupId)
        {
            // Tìm supervisor đầu tiên trong nhóm
            var supervisorAssignment = await _unitOfWork.SupervisorStudentGroup.GetAsync(ssg => ssg.StudentGroupId == studentGroupId);

            DateTime beginDate = DateTime.Now;
            if (beginDate > course.StartDate)
            {
                var attendanceReport = new Report
                {
                    CourseId = course.Id,
                    StudentGroupId = studentGroupId,
                    ReportDate = beginDate,
                    ReportContent = $"Báo cáo hằng ngày, ngày {beginDate:yyyy-MM-dd}",
                    ReportType = ReportType.DailyReport,
                    SubmissionDate = null,
                    Status = ReportStatus.NotYet
                };
                await _unitOfWork.Report.AddAsync(attendanceReport);
            }

            await _unitOfWork.SaveChangeAsync();
        }

        public async Task<IEnumerable<StudentGroupDto>> GetAllStudentGroupByCourseIdAsync(int courseId)
        {
            if (courseId < 1)
                throw new ArgumentException("CourseId không hợp lệ, phải lớn hơn 0.");

            var studentGroups = await _unitOfWork.StudentGroup.GetAllAsync(
                sg => sg.CourseId == courseId,
                includeProperties: "Course,StudentGroupAssignment.Student.StudentCourses,SupervisorStudentGroup.Supervisor,Report"
            );

            if (studentGroups == null || !studentGroups.Any())
                return new List<StudentGroupDto>();

            var studentGroupDtos = _mapper.Map<List<StudentGroupDto>>(studentGroups);
            return studentGroupDtos;
        }

        public async Task<StudentGroupDto> GetStudentGroupByIdAsync(int id)
        {
            if (id < 1)
                throw new ArgumentException("Id không hợp lệ, phải lớn hơn 0.");

            var studentGroup = await _unitOfWork.StudentGroup.GetAsync(
                sg => sg.Id == id,
                includeProperties: "Course,StudentGroupAssignment.Student.StudentCourses,SupervisorStudentGroup.Supervisor,Report"
            );

            if (studentGroup == null)
                throw new ArgumentException($"Không tìm thấy chánh với ID {id}.");

            var studentGroupDto = _mapper.Map<StudentGroupDto>(studentGroup);
            return studentGroupDto;
        }

        public async Task UpdateStudentGroupAsync(int studentGroupId, StudentGroupUpdateDto entity)
        {
            var existingStudentGroup = await _unitOfWork.StudentGroup.GetAsync(
                x => x.Id == studentGroupId,
                includeProperties: "SupervisorStudentGroup",
                tracked: false
            );

            if (existingStudentGroup == null)
                throw new ArgumentException("Không tìm thấy chánh.");

            var course = await _unitOfWork.Course.GetByIdAsync(entity.CourseId);
            if (course == null)
                throw new ArgumentException("CourseId không tham chiếu đến khóa tu hợp lệ.");

            if (string.IsNullOrEmpty(entity.GroupName))
                throw new ArgumentException("Tên chánh không được để trống.");

            if (entity.GroupName.Length > 100)
                throw new ArgumentException("Tên chánh không được vượt quá 100 ký tự.");

            // Kiểm tra tên nhóm có chứa ký tự đặc biệt không
            if (ContainsSpecialCharacters(entity.GroupName))
            {
                throw new ArgumentException("Tên nhóm không được chứa ký tự đặc biệt.");
            }

            // Kiểm tra tên chánh có bị trùng trong khóa tu không (ngoại trừ chính nó)
            var normalizedGroupName = entity.GroupName.ToLower();

            var existingGroupWithSameName = await _unitOfWork.StudentGroup.GetAsync(
                sg => sg.CourseId == entity.CourseId &&
                      sg.GroupName.ToLower() == normalizedGroupName &&
                      sg.Id != studentGroupId
            );

            if (existingGroupWithSameName != null)
                throw new ArgumentException("Tên chánh đã tồn tại trong khóa tu này. Vui lòng chọn tên khác.");

            // Kiểm tra các SupervisorIds có hợp lệ không
            if (entity.SupervisorIds != null)
            {
                foreach (var supervisorId in entity.SupervisorIds)
                {
                    var supervisor = await _unitOfWork.User.GetByIdAsync(supervisorId);
                    if (supervisor == null)
                        throw new ArgumentException($"Không tìm thấy huynh trưởng với ID {supervisorId}.");
                }
            }

            // Ánh xạ các thuộc tính từ DTO vào Entity
            _mapper.Map(entity, existingStudentGroup);

            // Cập nhật Supervisor assignments
            var currentSupervisors = existingStudentGroup.SupervisorStudentGroup?.Select(s => s.SupervisorId).ToList() ?? new List<int>();
            var newSupervisors = entity.SupervisorIds?.Except(currentSupervisors).ToList() ?? new List<int>();
            var supervisorsToRemove = currentSupervisors.Except(entity.SupervisorIds ?? new List<int>()).ToList();

            foreach (var supervisorId in supervisorsToRemove)
            {
                var supervisorToRemove = existingStudentGroup.SupervisorStudentGroup.FirstOrDefault(s => s.SupervisorId == supervisorId);
                if (supervisorToRemove != null)
                {
                    await _unitOfWork.SupervisorStudentGroup.DeleteAsync(supervisorToRemove);

                    // Tạo thông báo cho Supervisor
                    string message = $"Bạn bị xóa khỏi chánh '{entity.GroupName}' của khóa tu '{course.CourseName}'.";
                    string link = "student-groups";
                    await _notificationService.NotifyUserAsync(supervisorId, message, link);
                }
            }

            foreach (var supervisorId in newSupervisors)
            {
                var newSupervisorStudentGroup = new SupervisorStudentGroup
                {
                    StudentGroupId = studentGroupId,
                    SupervisorId = supervisorId
                };
                await _unitOfWork.SupervisorStudentGroup.AddAsync(newSupervisorStudentGroup);

                // Tạo thông báo cho Supervisor mới
                string message = $"Bạn đã được thêm vào chánh '{entity.GroupName}' của khóa tu '{course.CourseName}'.";
                string link = "student-groups";
                await _notificationService.NotifyUserAsync(supervisorId, message, link);
            }

            await _unitOfWork.StudentGroup.UpdateAsync(existingStudentGroup);
            await _unitOfWork.SaveChangeAsync();
        }


        public async Task DeleteStudentGroupAsync(int id)
        {
            var studentGroup = await _unitOfWork.StudentGroup.GetByIdAsync(id);
            if (studentGroup == null)
                throw new ArgumentException("Không tìm thấy chánh.");


            // Xóa các bản ghi trong bảng Reports liên quan đến StudentGroup
            var reports = await _unitOfWork.Report.GetAllAsync(r => r.StudentGroupId == id);
            if (reports.Any())
            {
                foreach (var report in reports)
                {
                    await _unitOfWork.Report.DeleteAsync(report);
                }
            }
            await _unitOfWork.StudentGroup.DeleteAsync(studentGroup);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task<List<StudentGroupDto>> AutoAssignSupervisorsAsync(int courseId)
        {
            if (courseId < 1)
                throw new ArgumentException("CourseId không hợp lệ.");

            var course = await _unitOfWork.Course.GetByIdAsync(courseId);
            if (course == null)
                throw new ArgumentException("Không tìm thấy khóa tu.");

            // Kiểm tra khóa tu đang diễn ra
            if (DateTime.Now > course.EndDate)
                throw new ArgumentException("Chỉ có thể phân Huynh trưởng khi khóa tu đang diễn ra.");

            // Lấy danh sách tất cả các chánh trong khóa tu
            var allGroups = await _unitOfWork.StudentGroup.GetAllAsync(
                sg => sg.CourseId == courseId,
                includeProperties: "SupervisorStudentGroup"
            );

            if (allGroups == null || !allGroups.Any())
                throw new ArgumentException("Không có chánh nào trong khóa tu này.");

            // Lấy danh sách tất cả các Supervisor đã được phân trong khóa tu này
            var existingSupervisorIds = allGroups
                .SelectMany(g => g.SupervisorStudentGroup.Select(ssg => ssg.SupervisorId))
                .Distinct()
                .ToList();

            // Lấy danh sách Huynh trưởng khả dụng (chưa được phân vào chánh nào trong khóa tu này)
            var availableSupervisors = await _unitOfWork.User.FindAsync(
                u => u.RoleId == SD.RoleId_Supervisor &&
                     u.Status == UserStatus.Active &&
                     !existingSupervisorIds.Contains(u.Id)
            );

            var availableSupervisorsList = availableSupervisors.ToList();

            if (availableSupervisorsList == null || !availableSupervisorsList.Any())
                throw new ArgumentException("Không còn Huynh trưởng nào để phân Chánh trong khóa tu này.");

            // Phân loại supervisors theo giới tính
            var maleSupervisors = availableSupervisorsList.Where(u => u.Gender == Gender.Male).ToList();
            var femaleSupervisors = availableSupervisorsList.Where(u => u.Gender == Gender.Female).ToList();

            // Phân loại groups theo giới tính
            var maleGroups = allGroups.Where(g => g.Gender == Gender.Male).ToList();
            var femaleGroups = allGroups.Where(g => g.Gender == Gender.Female).ToList();

            // Xử lý từng giới tính riêng biệt
            var unassignedGroups = new List<StudentGroup>();

            // Phân bổ supervisors cho nhóm Nam
            var unassignedMaleGroups = await AssignSupervisorsByGenderAsync(maleSupervisors, maleGroups, course);
            unassignedGroups.AddRange(unassignedMaleGroups);

            // Phân bổ supervisors cho nhóm Nữ
            var unassignedFemaleGroups = await AssignSupervisorsByGenderAsync(femaleSupervisors, femaleGroups, course);
            unassignedGroups.AddRange(unassignedFemaleGroups);

            await _unitOfWork.SaveChangeAsync();

            if (unassignedGroups.Any())
                return _mapper.Map<List<StudentGroupDto>>(unassignedGroups);

            // Nếu tất cả chánh đã có ít nhất một Huynh trưởng
            return new List<StudentGroupDto>();
        }

        private async Task<List<StudentGroup>> AssignSupervisorsByGenderAsync(List<User> supervisors, List<StudentGroup> groups, Course course)
        {
            var unassignedGroups = new List<StudentGroup>();
            int supervisorIndex = 0;
            int totalSupervisors = supervisors.Count;
            int totalGroups = groups.Count;

            if (totalGroups == 0)
                return unassignedGroups;

            foreach (var group in groups)
            {
                if (supervisorIndex >= totalSupervisors)
                    break;

                var supervisor = supervisors[supervisorIndex];
                supervisorIndex++;

                var supervisorStudentGroup = new SupervisorStudentGroup
                {
                    SupervisorId = supervisor.Id,
                    StudentGroupId = group.Id
                };
                await _unitOfWork.SupervisorStudentGroup.AddAsync(supervisorStudentGroup);

                // Tạo thông báo cho Supervisor
                string message = $"Bạn đã được thêm vào chánh '{group.GroupName}' của khóa tu '{course.CourseName}'.";
                string link = "student-groups";
                await _notificationService.NotifyUserAsync(supervisor.Id, message, link);
            }

            // Kiểm tra xem có nhóm nào chưa được phân Supervisor không
            var remainingSupervisors = supervisors.Skip(supervisorIndex).ToList();
            if (remainingSupervisors.Any())
            {
                foreach (var group in groups.Skip(supervisorIndex))
                {
                    unassignedGroups.Add(group);
                }
            }

            return unassignedGroups;
        }

        public async Task DistributeStudentsByGroupAsync(int courseId)
        {
            // Lấy tất cả học sinh thuộc khóa học với CourseId
            var studentsInCourse = await _unitOfWork.StudentCourse.FindAsync(
                sc => sc.CourseId == courseId,
                includeProperties: "Student"
            );

            if (studentsInCourse == null || !studentsInCourse.Any())
                throw new ArgumentException($"Không tìm thấy học sinh nào thuộc khóa học với CourseId: {courseId}");

            // Kiểm tra nếu có bất kỳ studentCourse nào có Status = Pending
            if (studentsInCourse.Any(sc => sc.Status == ProgressStatus.Pending))
                throw new InvalidOperationException("Cần phải duyệt hết đơn đăng ký trước khi xếp chánh.");

            // Chỉ chọn những studentCourse có Status là Approved
            var validStudentsInCourse = studentsInCourse.Where(sc => sc.Status == ProgressStatus.Approved).ToList();

            if (!validStudentsInCourse.Any())
                throw new InvalidOperationException("Không có học sinh hợp lệ để xếp nhóm.");

            var students = validStudentsInCourse.Select(sc => sc.Student).ToList();

            // Lấy tất cả các nhóm thuộc khóa học với courseId
            var studentGroupsInCourse = await _unitOfWork.StudentGroup.FindAsync(
                s => s.CourseId == courseId,
                includeProperties: "StudentGroupAssignment"
            );
            var studentGroupIdsInCourse = studentGroupsInCourse.Select(g => g.Id).ToList();

            // Lọc những học sinh đã được xếp nhóm trong khóa học hiện tại
            var studentGroupAssignments = await _unitOfWork.StudentGroupAssignment.FindAsync(
                sga => students.Select(s => s.Id).Contains(sga.StudentId) && studentGroupIdsInCourse.Contains(sga.StudentGroupId)
            );

            // Lấy danh sách các StudentId đã được xếp nhóm
            var assignedStudentIds = studentGroupAssignments.Select(sga => sga.StudentId).ToHashSet();

            // Lọc ra những học sinh chưa được phân nhóm trong khóa học này
            var unassignedStudents = students.Where(s => !assignedStudentIds.Contains(s.Id)).ToList();

            if (!unassignedStudents.Any())
                throw new InvalidOperationException("Tất cả học sinh đã được xếp nhóm trong khóa học này.");

            // Tính độ tuổi cho mỗi học sinh chưa được xếp nhóm
            var currentDate = DateTime.Now;
            var studentsWithAge = unassignedStudents.Select(student => new
            {
                Student = student,
                Age = (int)((currentDate - student.DateOfBirth).TotalDays / 365.25) // Tính tuổi
            }).ToList();

            // Lấy các nhóm học sinh trong courseId theo giới tính
            var studentGroups = await _unitOfWork.StudentGroup.FindAsync(
                g => g.CourseId == courseId,
                includeProperties: "StudentGroupAssignment"
            );

            if (studentGroups == null || !studentGroups.Any())
                throw new ArgumentException($"Không tìm thấy nhóm nào cho khóa học với CourseId: {courseId}");

            foreach (var genderGroup in studentGroups.GroupBy(g => g.Gender))
            {
                // Lọc học sinh theo giới tính của nhóm
                var studentsByGender = studentsWithAge
                    .Where(s => s.Student.Gender == genderGroup.Key)
                    .OrderBy(s => s.Age) // Sắp xếp học sinh theo độ tuổi
                    .ToList();

                // Nếu không có học sinh nào thuộc giới tính này, bỏ qua nhóm đó
                if (!studentsByGender.Any())
                    continue;

                // Chia học sinh đều vào các nhóm
                var genderGroups = genderGroup.ToList();
                int totalGroups = genderGroups.Count();
                int studentIndex = 0;

                // Lặp qua từng học sinh và phân vào nhóm
                foreach (var studentWithAge in studentsByGender)
                {
                    var targetGroup = genderGroups[studentIndex % totalGroups];
                    studentIndex++;

                    // Kiểm tra xem sinh viên đã được xếp vào nhóm này chưa
                    var existingAssignment = await _unitOfWork.StudentGroupAssignment.GetAsync(
                        sga => sga.StudentId == studentWithAge.Student.Id && sga.StudentGroupId == targetGroup.Id
                    );

                    if (existingAssignment == null)
                    {
                        var studentGroupAssignment = new StudentGroupAssignment
                        {
                            StudentId = studentWithAge.Student.Id,
                            StudentGroupId = targetGroup.Id
                        };
                        await _unitOfWork.StudentGroupAssignment.AddAsync(studentGroupAssignment);
                    }
                }
            }

            await _unitOfWork.SaveChangeAsync();
        }
        private bool ContainsSpecialCharacters(string input)
        {
            // Cho phép các chữ cái Unicode (bao gồm tiếng Việt), số, khoảng trắng và một số ký tự đặc biệt như dấu gạch ngang và dấu nháy
            return !Regex.IsMatch(input, @"^[\p{L}\p{N}\s\-']+$");
        }
    }
}
