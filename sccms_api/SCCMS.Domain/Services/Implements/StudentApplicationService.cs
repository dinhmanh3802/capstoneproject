using AutoMapper;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PdfSharp.Pdf;
using SCCMS.API.Services;
using SCCMS.Domain.DTOs.CourseDtos;
using SCCMS.Domain.DTOs.EmailDtos;
using SCCMS.Domain.DTOs.StudentCourseDtos;
using SCCMS.Domain.DTOs.StudentDtos;
using SCCMS.Domain.DTOs.StudentGroupDtos;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.UnitOfWork;
using Utility;

namespace SCCMS.Domain.Services.Implements
{
    public class StudentApplicationService : IStudentApplicationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        public StudentApplicationService(IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        public Task CreateStudentApplicationAsync(StudentCourseDto studentApplicationCreateDto)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteStudentApplicationAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Id phải lớn hơn 0.");
            }
            StudentCourse application = await _unitOfWork.StudentCourse.GetByIdAsync(id);
            if (application == null)
            {
                throw new ArgumentException("Đơn đăng ký không tồn tại.");
            }
            await _unitOfWork.StudentCourse.DeleteAsync(application);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task<StudentCourseDto?> GetByStudentIdAndCourseIdAsync(int studentId, int courseId)
        {
            if (studentId <= 0)
            {
                throw new ArgumentException("StudentId không hợp lệ. Phải lớn hơn 0.");
            }
            if (courseId <= 0)
            {
                throw new ArgumentException("CourseId không hợp lệ. Phải lớn hơn 0.");
            }

            // Thực hiện truy vấn với điều kiện lọc theo studentId và courseId
            var studentApplication = await _unitOfWork.StudentCourse.FindAsync(
                sa => sa.StudentId == studentId && sa.CourseId == courseId && sa.Status != ProgressStatus.Delete,
                includeProperties: "Student,Course,Reviewer,Student.StudentGroupAssignment.StudentGroup");

            var studentCourseEntity = studentApplication.FirstOrDefault();

            if (studentCourseEntity == null)
            {
                return null;
            }

            // Ánh xạ kết quả thành DTO
            var studentCourseDto = new StudentCourseDto
            {
                Id = studentCourseEntity.Id,
                CourseId = studentCourseEntity.CourseId,
                Course = new CourseInforDto
                {
                    Id = studentCourseEntity.CourseId,
                    CourseName = studentCourseEntity.Course.CourseName,
                },
                StudentId = studentCourseEntity.StudentId,
                Student = new DTOs.StudentCourseDtos.StudentInforDto
                {
                    Id = studentCourseEntity.StudentId,
                    FullName = studentCourseEntity.Student.FullName,
                    DateOfBirth = studentCourseEntity.Student.DateOfBirth,
                    Gender = studentCourseEntity.Student.Gender,
                    Email = studentCourseEntity.Student.Email,
                    ParentName = studentCourseEntity.Student.ParentName,
                    EmergencyContact = studentCourseEntity.Student.EmergencyContact,
                    Address = studentCourseEntity.Student.Address,
                    NationalId = studentCourseEntity.Student.NationalId,
                    NationalImageFront = studentCourseEntity.Student.NationalImageFront,
                    NationalImageBack = studentCourseEntity.Student.NationalImageBack,
                    Conduct = studentCourseEntity.Student.Conduct,
                    AcademicPerformance = studentCourseEntity.Student.AcademicPerformance,

                    // Lấy thông tin nhóm liên quan đến khóa học hiện tại
                    StudentGroups = studentCourseEntity.Student.StudentGroupAssignment
                        .Where(sga => sga.StudentGroup.CourseId == studentCourseEntity.CourseId)
                        .Select(sga => new StudentGroupInforDto
                        {
                            Id = sga.StudentGroup.Id,
                            GroupName = sga.StudentGroup.GroupName,
                        }).ToList()
                },
                StudentCode = studentCourseEntity.StudentCode,
                ApplicationDate = studentCourseEntity.ApplicationDate ?? DateTime.MinValue,
                Status = studentCourseEntity.Status,
                Note = studentCourseEntity.Note,
                ReviewerId = studentCourseEntity.ReviewerId,
                Reviewer = studentCourseEntity.Reviewer != null ? new UserInforDto
                {
                    Id = studentCourseEntity.ReviewerId.Value,
                    FullName = studentCourseEntity.Reviewer.FullName
                } : null,
                ReviewDate = studentCourseEntity.ReviewDate
            };

            return studentCourseDto;
        }






        public async Task<IEnumerable<StudentCourseDto>> GetAllStudentCourseAsync(
    int courseId,
    string? studentName = null,
    Gender? gender = null,
    string? studentGroupName = null,
    string? phoneNumber = null,
    ProgressStatus? status = null,
    string? studentCode = null,
    DateTime? startDob = null,
    DateTime? endDob = null,
    int? studentGroup = null,
    int? studentGroupExcept = null,
    bool? isGetStudentDrop = true)
        {
            // Kiểm tra định dạng số điện thoại
            if (!string.IsNullOrEmpty(phoneNumber) && !System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^\d{10}$"))
            {
                throw new ArgumentException("Định dạng số điện thoại không hợp lệ. Phải có 10 chữ số.");
            }
            if (courseId <= 0)
            {
                throw new ArgumentException("CourseId không hợp lệ. Phải lớn hơn 0.");
            }

            // Loại bỏ dấu cách dư thừa từ các tham số đầu vào
            studentName = studentName?.Trim();
            studentGroupName = studentGroupName?.Trim();
            phoneNumber = phoneNumber?.Trim();
            studentCode = studentCode?.Trim();

            // Thực hiện truy vấn với điều kiện lọc không phân biệt dấu
            var studentApplications = await _unitOfWork.StudentCourse.FindAsync(sa =>
                (string.IsNullOrEmpty(studentName) ||
                    EF.Functions.Collate(sa.Student.FullName, "Latin1_General_CI_AI").Contains(studentName)) &&
                (string.IsNullOrEmpty(studentGroupName) ||
                    sa.Student.StudentGroupAssignment.Any(sga =>
                        EF.Functions.Collate(sga.StudentGroup.GroupName, "Latin1_General_CI_AI").Contains(studentGroupName))) &&
                (string.IsNullOrEmpty(phoneNumber) || sa.Student.EmergencyContact.Contains(phoneNumber)) &&
                (status == null || sa.Status.Equals(status)) &&
                (gender == null || sa.Student.Gender.Equals(gender)) &&
                (string.IsNullOrEmpty(studentCode) ||
                    EF.Functions.Collate(sa.StudentCode, "Latin1_General_CI_AI").Contains(studentCode)) &&
                (sa.CourseId == courseId) &&
                (!startDob.HasValue || sa.Student.DateOfBirth >= startDob.Value) &&
                (!endDob.HasValue || sa.Student.DateOfBirth <= endDob.Value) &&
                sa.Status != ProgressStatus.Delete &&
                (sa.Status != ProgressStatus.Rejected && sa.Status != ProgressStatus.Pending),
                includeProperties: "Student,Course,Reviewer,Student.StudentGroupAssignment.StudentGroup");

            // Xử lý lọc theo StudentGroup nếu có
            if (studentGroup.HasValue)
            {
                studentApplications = studentApplications.Where(sa => sa.Student.StudentGroupAssignment.Any(sga => sga.StudentGroupId == studentGroup));
            }
            if (studentGroupExcept.HasValue)
            {
                studentApplications = studentApplications.Where(sa => !sa.Student.StudentGroupAssignment.Any(sga => sga.StudentGroupId == studentGroupExcept));
            }
            if (isGetStudentDrop.HasValue && isGetStudentDrop.Value == false)
            {
                studentApplications = studentApplications.Where(sa => sa.Status != ProgressStatus.DropOut && sa.Status != ProgressStatus.Pending);
            }

            if (studentApplications == null || !studentApplications.Any())
            {
                return new List<StudentCourseDto>();
            }

            // Ánh xạ kết quả thành DTO
            var studentApplicationDtos = studentApplications.Select(sa => new StudentCourseDto
            {
                Id = sa.Id,
                CourseId = sa.CourseId,
                Course = new CourseInforDto
                {
                    Id = sa.CourseId,
                    CourseName = sa.Course.CourseName,
                },
                StudentId = sa.StudentId,
                Student = new DTOs.StudentCourseDtos.StudentInforDto
                {
                    Id = sa.StudentId,
                    FullName = sa.Student.FullName,
                    DateOfBirth = sa.Student.DateOfBirth,
                    Gender = sa.Student.Gender,
                    Email = sa.Student.Email,
                    ParentName = sa.Student.ParentName,
                    EmergencyContact = sa.Student.EmergencyContact,
                    Address = sa.Student.Address,
                    NationalId = sa.Student.NationalId,
                    NationalImageFront = sa.Student.NationalImageFront,
                    NationalImageBack = sa.Student.NationalImageBack,
                    Conduct = sa.Student.Conduct,
                    AcademicPerformance = sa.Student.AcademicPerformance,

                    // Lấy thông tin nhóm liên quan đến khóa học hiện tại
                    StudentGroups = sa.Student.StudentGroupAssignment
                        .Where(sga => sga.StudentGroup.CourseId == sa.CourseId)
                        .Select(sga => new StudentGroupInforDto
                        {
                            Id = sga.StudentGroup.Id,
                            GroupName = sga.StudentGroup.GroupName,
                        }).ToList()
                },
                StudentCode = sa.StudentCode,
                ApplicationDate = sa.ApplicationDate ?? DateTime.MinValue,
                Status = sa.Status,
                Note = sa.Note,
                ReviewerId = sa.ReviewerId,
                Reviewer = sa.Reviewer != null ? new UserInforDto
                {
                    Id = sa.ReviewerId.Value,
                    FullName = sa.Reviewer.FullName,
                    UserName = sa.Reviewer.UserName,
                    Email = sa.Reviewer.Email,
                    PhoneNumber = sa.Reviewer.PhoneNumber,
                } : null,
                ReviewDate = sa.ReviewDate
            });

            return studentApplicationDtos;
        }

        public async Task<IEnumerable<StudentCourseDto>> GetAllStudentApplicationAsync(
    int? courseId = null,
    string? studentName = null,
    Gender? gender = null,
    string? parentName = null,
    string? phoneNumber = null,
    ProgressStatus? status = null,
    int? reviewerId = null,
    DateTime? startDob = null,
    DateTime? endDob = null,
    string? nationalId = null)
        {
            // Validate phone number format
            if (!string.IsNullOrEmpty(phoneNumber) && !System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^\d{10}$"))
            {
                throw new ArgumentException("Định dạng số điện thoại không hợp lệ. Phải có 10 chữ số.");
            }
            if (courseId.HasValue && courseId <= 0)
            {
                throw new ArgumentException("CourseId không hợp lệ. Phải lớn hơn 0.");
            }
            if (reviewerId.HasValue && reviewerId <= 0)
            {
                throw new ArgumentException("ReviewerId không hợp lệ. Phải lớn hơn 0.");
            }

            // Loại bỏ dấu cách dư thừa từ các tham số đầu vào
            studentName = studentName?.Trim();
            parentName = parentName?.Trim();
            phoneNumber = phoneNumber?.Trim();
            nationalId = nationalId?.Trim();

            // Thực hiện truy vấn với điều kiện lọc không phân biệt dấu
            var studentApplications = await _unitOfWork.StudentCourse.FindAsync(sa =>
                (string.IsNullOrEmpty(studentName) ||
                    EF.Functions.Collate(sa.Student.FullName, "Latin1_General_CI_AI").Contains(studentName)) &&
                (string.IsNullOrEmpty(parentName) ||
                    EF.Functions.Collate(sa.Student.ParentName, "Latin1_General_CI_AI").Contains(parentName)) &&
                (string.IsNullOrEmpty(phoneNumber) || sa.Student.EmergencyContact.Contains(phoneNumber)) &&
                (status == null || sa.Status.Equals(status)) &&
                (gender == null || sa.Student.Gender.Equals(gender)) &&
                (reviewerId == null || sa.ReviewerId == reviewerId) &&
                (courseId == null || sa.CourseId == courseId) &&
                (!startDob.HasValue || sa.Student.DateOfBirth >= startDob.Value) &&
                (!endDob.HasValue || sa.Student.DateOfBirth <= endDob.Value) &&
                sa.Status != ProgressStatus.Delete &&
                (string.IsNullOrEmpty(nationalId) ||
                    EF.Functions.Collate(sa.Student.NationalId, "Latin1_General_CI_AI").Contains(nationalId)),
                includeProperties: "Student,Course,Reviewer,Student.StudentGroupAssignment.StudentGroup");

            if (studentApplications == null || !studentApplications.Any())
            {
                return new List<StudentCourseDto>();
            }

            // Ánh xạ kết quả thành DTO
            var studentApplicationDtos = studentApplications.Select(sa => new StudentCourseDto
            {
                Id = sa.Id,
                CourseId = sa.CourseId,
                Course = new CourseInforDto
                {
                    Id = sa.CourseId,
                    CourseName = sa.Course.CourseName,
                },
                StudentId = sa.StudentId,
                Student = new DTOs.StudentCourseDtos.StudentInforDto
                {
                    Id = sa.StudentId,
                    FullName = sa.Student.FullName,
                    DateOfBirth = sa.Student.DateOfBirth,
                    Gender = sa.Student.Gender,
                    Email = sa.Student.Email,
                    ParentName = sa.Student.ParentName,
                    EmergencyContact = sa.Student.EmergencyContact,
                    Address = sa.Student.Address,
                    NationalId = sa.Student.NationalId,
                    NationalImageFront = sa.Student.NationalImageFront,
                    NationalImageBack = sa.Student.NationalImageBack,
                    Conduct = sa.Student.Conduct,
                    AcademicPerformance = sa.Student.AcademicPerformance,

                    // Lấy thông tin nhóm liên quan đến khóa học hiện tại
                    StudentGroups = sa.Student.StudentGroupAssignment
                        .Where(sga => sga.StudentGroup.CourseId == sa.CourseId)
                        .Select(sga => new StudentGroupInforDto
                        {
                            Id = sga.StudentGroup.Id,
                            GroupName = sga.StudentGroup.GroupName,
                        }).ToList()
                },
                ApplicationDate = sa.ApplicationDate ?? DateTime.MinValue,
                Status = sa.Status,
                Note = sa.Note,
                ReviewerId = sa.ReviewerId,
                Reviewer = sa.Reviewer != null ? new UserInforDto
                {
                    Id = sa.ReviewerId.Value,
                    FullName = sa.Reviewer.FullName,
                    UserName = sa.Reviewer.UserName,
                    Email = sa.Reviewer.Email,
                    PhoneNumber = sa.Reviewer.PhoneNumber,
                } : null,
                ReviewDate = sa.ReviewDate
            });

            return studentApplicationDtos;
        }


        public async Task<StudentCourseDto?> GetStudentApplicationByIdAsync(int id)
        {
            // Kiểm tra điều kiện hợp lệ của id
            if (id <= 0)
            {
                throw new ArgumentException("Id phải lớn hơn 0.");
            }

            // Thực hiện truy vấn lấy StudentCourse theo id với các thuộc tính liên quan được bao gồm
            var studentApplication = await _unitOfWork.StudentCourse.FindAsync(
                sa => sa.Id == id && sa.Status != ProgressStatus.Delete,
                includeProperties: "Student,Course,Reviewer,Student.StudentGroupAssignment.StudentGroup"
            );

            // Nếu không tìm thấy, trả về null
            if (studentApplication == null || !studentApplication.Any())
            {
                return null;
            }

            // Lấy bản ghi đầu tiên phù hợp
            var sa = studentApplication.First();
            var listStudent = await _unitOfWork.StudentCourse.FindAsync(a => a.Status != ProgressStatus.Delete && 
                                                                        a.Student.NationalId == sa.Student.NationalId && 
                                                                        a.Id!= sa.Id&&
                                                                        a.CourseId== sa.CourseId);
            var numberSameNationId = listStudent.Count();

            // Ánh xạ đối tượng StudentCourse sang StudentCourseDto tương tự như trong GetAllStudentCourseAsync
            var studentApplicationDto = new StudentCourseDto
            {
                Id = sa.Id,
                CourseId = sa.CourseId,
                Course = new CourseInforDto
                {
                    Id = sa.CourseId,
                    CourseName = sa.Course.CourseName,
                    Status= sa.Course.Status
                },
                StudentId = sa.StudentId,
                Student = new DTOs.StudentCourseDtos.StudentInforDto
                {
                    Id = sa.StudentId,
                    FullName = sa.Student.FullName,
                    DateOfBirth = sa.Student.DateOfBirth,
                    Gender = sa.Student.Gender,
                    Email = sa.Student.Email,
                    Image= sa.Student.Image,
                    ParentName = sa.Student.ParentName,
                    EmergencyContact = sa.Student.EmergencyContact,
                    Address = sa.Student.Address,
                    NationalId = sa.Student.NationalId,
                    NationalImageFront = sa.Student.NationalImageFront,
                    NationalImageBack = sa.Student.NationalImageBack,
                    Conduct = sa.Student.Conduct,
                    AcademicPerformance = sa.Student.AcademicPerformance,

                    // Lấy thông tin nhóm liên quan đến khóa học hiện tại
                    StudentGroups = sa.Student.StudentGroupAssignment
                        .Where(sga => sga.StudentGroup.CourseId == sa.CourseId)  // Lọc chỉ lấy nhóm liên quan đến khóa học hiện tại
                        .Select(sga => new StudentGroupInforDto
                        {
                            Id = sga.StudentGroup.Id,
                            GroupName = sga.StudentGroup.GroupName,
                        }).ToList()
                },
                StudentCode = sa.StudentCode,
                ApplicationDate = sa.ApplicationDate ?? DateTime.MinValue,
                Status = sa.Status,
                Note = sa.Note,
                ReviewerId = sa.ReviewerId,
                Reviewer = sa.Reviewer != null ? new UserInforDto
                {
                    Id = sa.ReviewerId.Value,
                    FullName = sa.Reviewer.FullName,
                    UserName = sa.Reviewer.UserName,
                    Email = sa.Reviewer.Email,
                    PhoneNumber = sa.Reviewer.PhoneNumber,
                } : null,
                ReviewDate = sa.ReviewDate,
                SameNationId= numberSameNationId
            };

            return studentApplicationDto;
        }

        public async Task UpdateStatusStudentApplicationAsync(StudentCourseUpdateDto entity)
        {
            if (entity.Ids == null || !entity.Ids.Any())
            {
                throw new ArgumentException("Id không được để trống hoặc null.");
            }

            if (entity.ReviewerId.HasValue && entity.ReviewerId > 0)
            {
                var existingReviewers = await _unitOfWork.User.FindAsync(u => u.Id == entity.ReviewerId);
                if (existingReviewers == null || !existingReviewers.Any())
                {
                    throw new ArgumentException($"Người duyệt có ID {entity.ReviewerId} không tồn tại.");
                }
            }
            if (entity.ReviewerId.HasValue && entity.ReviewerId < 0)
            {
                throw new ArgumentException("Reviewer Id phải lớn hơn 0.");
            }

            foreach (var studentApplicationId in entity.Ids)
            {
                if (studentApplicationId <= 0)
                {
                    throw new ArgumentException("Id phải lớn hơn 0.");
                }

                var studentApplication = await _unitOfWork.StudentCourse.GetByIdAsync(studentApplicationId);
                if (studentApplication == null)
                {
                    throw new ArgumentException($"Đơn đăng ký có ID {studentApplicationId} không tồn tại.");
                }

                var course = await _unitOfWork.Course.GetByIdAsync(studentApplication.CourseId);
                if (course == null)
                {
                    throw new ArgumentException($"Khóa học có ID {studentApplication.CourseId} không tồn tại.");
                }

                // Check if the course has already started
                if (course.StartDate <= DateTime.Now &&
                    (entity.Status == ProgressStatus.Rejected))
                {
                    throw new InvalidOperationException("Không thể thay đổi trạng thái 'Rejected' khi khóa học đã bắt đầu.");
                }

                if (entity.Status == ProgressStatus.Approved)
                {
                    var studentCourses = await _unitOfWork.StudentCourse
                        .FindAsync(sc => sc.CourseId == studentApplication.CourseId && sc.StudentCode != null && sc.Status != ProgressStatus.Delete);

                    var maxStudentCode = studentCourses
                        .OrderByDescending(sc => sc.StudentCode)
                        .Select(sc => sc.StudentCode)
                        .FirstOrDefault();

                    int nextStudentNumber;
                    if (string.IsNullOrEmpty(maxStudentCode))
                    {
                        nextStudentNumber = 1;
                    }
                    else
                    {
                        nextStudentNumber = int.Parse(maxStudentCode.Substring(6)) + 1;
                    }

                    string studentCode = $"{course.StartDate:yyMMdd}{nextStudentNumber:D3}";
                    studentApplication.StudentCode = studentCode;
                }
                if (studentApplication.Status == ProgressStatus.Pending)
                {
                    // Chỉ cập nhật ReviewerId khi trạng thái là Pending
                    studentApplication.ReviewerId = entity.ReviewerId > 0 ? entity.ReviewerId : null;
                }
                else
                {
                    // Đảm bảo không cập nhật ReviewerId khi trạng thái là Approved hoặc Rejected
                    studentApplication.ReviewerId = studentApplication.ReviewerId;
                }
                if (entity.Status.HasValue)
                {
                    studentApplication.Status = entity.Status.Value;
                    if (studentApplication.Status == ProgressStatus.DropOut || studentApplication.Status== ProgressStatus.Delete)
                    {
                        var studentGroupAssignment = await _unitOfWork.StudentGroupAssignment.FindAsync(sga => sga.StudentId == studentApplication.StudentId &&
                        sga.StudentGroup.CourseId == studentApplication.CourseId);
                        if (studentGroupAssignment != null)
                        {
                            foreach (var sga in studentGroupAssignment)
                            {
                                await _unitOfWork.StudentGroupAssignment.DeleteAsync(sga);
                            }
                        }
                    }
                }
                if (entity.ReviewerId.HasValue && !entity.Status.HasValue)
                {
                    await _notificationService.NotifyUserAsync(entity.ReviewerId.Value,
                        "Bạn có đơn đăng ký mới cần duyệt. Vui lòng kiểm tra!", $"/student-applications?courseId={studentApplication.CourseId}&reviewerId={entity.ReviewerId.Value}");
                }
                studentApplication.Note = entity.Note;
                studentApplication.DateModified = DateTime.Now;
                studentApplication.ReviewDate = DateTime.Now;
            }

            await _unitOfWork.SaveChangeAsync();
        }

        public async Task UpdateStatusStudentApplicationDetailAsync(int courseId, int applicationId, string note, ProgressStatus status, int studentGroupId)
        {
            if (applicationId <= 0)
            {
                throw new ArgumentException("Id phải lớn hơn 0.");
            }

            var studentApplication = await _unitOfWork.StudentCourse.GetByIdAsync(applicationId);
            if (studentApplication == null || studentApplication.Status== ProgressStatus.Delete)
            {
                throw new ArgumentException($"Đơn đăng ký có ID {applicationId} không tồn tại.");
            }

            var course = await _unitOfWork.Course.GetByIdAsync(courseId);
            if (course == null)
            {
                throw new ArgumentException($"Khóa học có ID {courseId} không tồn tại.");
            }
            
            var studentApplicationDetail = await GetByStudentIdAndCourseIdAsync(studentApplication.StudentId, courseId);
            var exitStudentGroup= studentApplicationDetail?.Student.StudentGroups.FirstOrDefault();
            if(exitStudentGroup?.Id != studentGroupId)
            {
                var studentGroupAssignment= await _unitOfWork.StudentGroupAssignment.GetAsync(s=> s.StudentGroupId== exitStudentGroup.Id && s.StudentId== studentApplication.StudentId);
                await _unitOfWork.StudentGroupAssignment.DeleteAsync(studentGroupAssignment);

                var newAssignment = new StudentGroupAssignment
                {
                    StudentId = studentApplication.StudentId,
                    StudentGroupId = studentGroupId
                };

                await _unitOfWork.StudentGroupAssignment.AddAsync(newAssignment);
            }

            if(status== ProgressStatus.DropOut)
            {
                var studentGroupAssigment= await _unitOfWork.StudentGroupAssignment.FindAsync(sga => sga.StudentId == studentApplication.StudentId && sga.StudentGroupId == studentGroupId);
                if (studentGroupAssigment != null)
                {
                      await _unitOfWork.StudentGroupAssignment.DeleteRangeAsync(studentGroupAssigment);
                }
            }

            studentApplication.Status = status;
            studentApplication.Note = note;
            await _unitOfWork.StudentCourse.UpdateAsync(studentApplication);
            await _unitOfWork.SaveChangeAsync();

        }


        public async Task AutoAssignApplicationsAsync(int courseId)
        {
            // Bước 1: Lấy danh sách thư ký
            var availableUsers = await _unitOfWork.User
                .FindAsync(u => u.RoleId == SD.RoleId_Secretary && u.Status == UserStatus.Active);

            if (availableUsers == null || !availableUsers.Any())
            {
                throw new InvalidOperationException("Không có thư ký nào để phân chia");
            }

            // Bước 2: Lấy danh sách Application chưa được gán
            var unassignedApplications = await _unitOfWork.StudentCourse
                .FindAsync(a => a.ReviewerId == null && a.CourseId == courseId && a.Status != ProgressStatus.Delete);

            if (unassignedApplications == null || !unassignedApplications.Any())
            {
                throw new InvalidOperationException("Tất cả đơn đăng ký đã được phân chia");
            }

            // Bước 3: Lấy số lượng đơn đã được gán cho mỗi thư ký
            var secretaryAssignments = new List<SecretaryAssignment>();
            foreach (var user in availableUsers)
            {
                var assignedCount = await _unitOfWork.StudentCourse.CountAsync(a => a.ReviewerId == user.Id && a.CourseId == courseId && a.Status != ProgressStatus.Delete);
                secretaryAssignments.Add(new SecretaryAssignment
                {
                    Secretary = user,
                    AssignedCount = assignedCount
                });
            }

            // Khởi tạo danh sách để chứa ReviewerIds đã được gán
            var assignedReviewerIds = new List<int>();

            // Chuyển danh sách đơn chưa được gán thành List để dễ xử lý
            var unassignedApps = unassignedApplications.ToList();

            // Bước 4: Phân phối Application cho User dựa trên số lượng đơn đã có
            while (unassignedApps.Any())
            {
                // Tìm thư ký có số lượng đơn ít nhất
                var minAssignedCount = secretaryAssignments.Min(sa => sa.AssignedCount);
                var secretaryToAssign = secretaryAssignments.First(sa => sa.AssignedCount == minAssignedCount);

                // Gán một đơn cho thư ký này
                var application = unassignedApps.First();
                application.ReviewerId = secretaryToAssign.Secretary.Id;
                application.DateModified = DateTime.Now;
                application.ReviewDate = DateTime.Now;

                // Cập nhật số lượng đơn của thư ký
                secretaryToAssign.AssignedCount += 1;

                // Thêm ReviewerId vào danh sách nếu chưa có
                if (!assignedReviewerIds.Contains(secretaryToAssign.Secretary.Id))
                {
                    assignedReviewerIds.Add(secretaryToAssign.Secretary.Id);
                }

                // Xóa đơn khỏi danh sách chưa được gán
                unassignedApps.RemoveAt(0);
            }

            // Bước 5: Lưu các thay đổi
            await _unitOfWork.SaveChangeAsync();

            //Gửi thông báo cho thư ký
            foreach (var reviewerId in assignedReviewerIds)
            {
                await _notificationService.NotifyUserAsync(reviewerId,
                                       "Bạn có đơn đăng ký mới cần duyệt. Vui lòng kiểm tra!", $"/student-applications?courseId={courseId}&reviewerId={reviewerId}");
            }
            
        }

        public async Task SendApplicationResultAsync(int[] listStudentApplicaionId, int courseId, string subject, string body)
        {
            var listStudentId = new List<int>();
            var course = await _unitOfWork.Course.GetByIdAsync(courseId);
            if (course == null)
            {
                throw new ArgumentException("Khóa học không tồn tại.");
            }
            var listStudentAppication = await _unitOfWork.StudentCourse.FindAsync(sc => listStudentApplicaionId.Contains(sc.Id)&& sc.Status != ProgressStatus.Delete);
            if (listStudentAppication == null)
            {
                throw new ArgumentException("Không tìm thấy đơn đăng ký.");
            }
            foreach (var studentApp in listStudentAppication)
            {
                if (studentApp.Status == ProgressStatus.Pending)
                {
                    throw new ArgumentException("Đơn đăng ký chưa được duyệt hết.");
                }
                listStudentId.Add(studentApp.StudentId);
            }
            await _unitOfWork.SaveChangeAsync();


            SendBulkEmailRequestDto sendBulkEmailRequestDto = new SendBulkEmailRequestDto
            {
                CourseId = courseId,
                ListStudentId = listStudentId,
                Subject = subject,
                Message = body
            };
            _emailService.SendBulkEmailAsync(sendBulkEmailRequestDto);
        }

        public async Task<byte[]> GenerateStudentCardsPdfAsync(List<int> studentCourseIds, int courseId)
        {
            using (var ms = new MemoryStream())
            {
                PdfDocument document = new PdfDocument();

                // Lấy danh sách sinh viên theo studentCourseIds và courseId
                var studentCourses = await _unitOfWork.StudentCourse.FindAsync(
                    sc => studentCourseIds.Contains(sc.Id) && sc.CourseId == courseId && sc.Status != ProgressStatus.Delete,
                    includeProperties: "Student,Course,Student.StudentGroupAssignment.StudentGroup"
                );

                if (studentCourses == null || !studentCourses.Any())
                {
                    return null;
                }

                // URL đến hình nền trên cloud
                string backgroundImageUrl = "https://coloan.blob.core.windows.net/coloanimage/studentCard.png";

                byte[] backgroundImageBytes;

                // Tải hình nền từ cloud
                using (var client = new HttpClient())
                {
                    backgroundImageBytes = await client.GetByteArrayAsync(backgroundImageUrl);
                }
                var backgroundTempFilePath = Path.GetTempFileName();
                await File.WriteAllBytesAsync(backgroundTempFilePath, backgroundImageBytes);

                // Tạo thẻ sinh viên cho từng sinh viên
                foreach (var studentCourse in studentCourses)
                {
                    var student = studentCourse.Student; // Lấy thông tin sinh viên từ thực thể StudentCourse

                    // Lấy nhóm sinh viên chỉ thuộc về courseId hiện tại
                    var studentGroupName = student.StudentGroupAssignment
                        .Where(sga => sga.StudentGroup.CourseId == courseId)
                        .Select(sga => sga.StudentGroup.GroupName)
                        .FirstOrDefault() ?? null;  // Nếu không có nhóm, trả về "N/A"
                    if(studentGroupName == null)
                    {
                        throw new InvalidOperationException("Học sinh "+ student.FullName + " chưa được phân vào chánh.");
                    }

                    // Lấy mã sinh viên trong khóa học hiện tại
                    var studentCode = student.StudentCourses
                        .Where(sc => sc.CourseId == courseId)
                        .Select(sc => sc.StudentCode)
                        .FirstOrDefault() ?? "";

                    var page = document.AddPage();
                    page.Size = PdfSharp.PageSize.A5;
                    var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page);

                    // Thêm ảnh nền
                    using (var backgroundImage = PdfSharp.Drawing.XImage.FromFile(backgroundTempFilePath))
                    {
                        gfx.DrawImage(backgroundImage, 0, 0, page.Width, page.Height); // Vẽ ảnh nền toàn trang
                    }

                    // Thiết lập font chữ
                    var titleFont = new PdfSharp.Drawing.XFont("Verdana", 24, PdfSharp.Drawing.XFontStyleEx.Bold);

                    var contentFont = new PdfSharp.Drawing.XFont("Verdana", 22);

                    // Thêm tên khóa học
                    string schoolName = $"{studentCourse.Course.CourseName}";
                    double schoolNameX = (page.Width - gfx.MeasureString(schoolName, titleFont).Width) / 2;
                    gfx.DrawString(schoolName, titleFont, PdfSharp.Drawing.XBrushes.DarkBlue, new PdfSharp.Drawing.XPoint(schoolNameX, 110));

                    // Thêm ảnh sinh viên
                    if (!string.IsNullOrEmpty(student.Image))
                    {
                        try
                        {
                            using (var client = new HttpClient())
                            {
                                var imageBytes = await client.GetByteArrayAsync(student.Image);

                                // Tạo một tệp tạm thời
                                var tempFilePath = Path.GetTempFileName();

                                try
                                {
                                    // Lưu ảnh vào tệp tạm thời
                                    await File.WriteAllBytesAsync(tempFilePath, imageBytes);

                                    // Đọc ảnh từ tệp tạm thời
                                    using (var studentImage = PdfSharp.Drawing.XImage.FromFile(tempFilePath))
                                    {
                                        // Kích thước gốc của hình ảnh
                                        double originalWidth = studentImage.PixelWidth;
                                        double originalHeight = studentImage.PixelHeight;

                                        // Xác định kích thước mong muốn khi in ra (có thể thay đổi tùy theo yêu cầu)
                                        double desiredWidth = 220; // Ví dụ chiều rộng mong muốn
                                        double desiredHeight;

                                        // Tính toán tỷ lệ để đảm bảo không méo hình
                                        double aspectRatio = originalWidth / originalHeight;
                                        desiredHeight = desiredWidth / aspectRatio;

                                        // Vẽ hình ảnh với kích thước đã được tính toán theo tỷ lệ
                                        double imageX = (page.Width - desiredWidth) / 2;
                                        double imageY = (page.Height - desiredHeight) / 2- 40;
                                        gfx.DrawImage(studentImage, imageX, imageY, desiredWidth, desiredHeight);
                                    }
                                }
                                finally
                                {
                                    // Xóa tệp tạm thời sau khi sử dụng
                                    if (File.Exists(tempFilePath))
                                    {
                                        File.Delete(tempFilePath);
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // Xử lý lỗi nếu có khi tải ảnh sinh viên
                        }
                    }

                    // Thêm tên sinh viên
                    string fullName = $"{student.FullName}";
                    double fullNameX = (page.Width - gfx.MeasureString(fullName, contentFont).Width) / 2;
                    gfx.DrawString(fullName, contentFont, PdfSharp.Drawing.XBrushes.Black, new PdfSharp.Drawing.XPoint(fullNameX, 450));

                    // Thêm tên nhóm sinh viên
                    string groupName = $"Chánh: {studentGroupName}";
                    double groupNameX = (page.Width - gfx.MeasureString(groupName, contentFont).Width) / 2;
                    gfx.DrawString(groupName, contentFont, PdfSharp.Drawing.XBrushes.Black, new PdfSharp.Drawing.XPoint(groupNameX, 490));

                    // Căn giữa mã sinh viên
                    string studentId = $"Mã khóa sinh: {studentCode}";
                    double studentIdX = (page.Width - gfx.MeasureString(studentId, contentFont).Width) / 2;
                    gfx.DrawString(studentId, contentFont, PdfSharp.Drawing.XBrushes.Black, new PdfSharp.Drawing.XPoint(studentIdX, 520));
                }

                // Xóa ảnh nền tạm thời
                if (File.Exists(backgroundTempFilePath))
                {
                    File.Delete(backgroundTempFilePath);
                }

                // Lưu tài liệu PDF vào MemoryStream và trả về mảng byte
                document.Save(ms, false);
                return ms.ToArray();
            }
        }



        private class SecretaryAssignment
        {
            public User Secretary { get; set; }
            public int AssignedCount { get; set; }
        }


    }
}
