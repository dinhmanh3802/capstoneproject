using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PdfSharp.Pdf;
using SCCMS.API.Services;
using SCCMS.Domain.DTOs.EmailDtos;
using SCCMS.Domain.DTOs.StudentCourseDtos;
using SCCMS.Domain.DTOs.TeamDtos;
using SCCMS.Domain.DTOs.VolunteerApplicationDtos;
using SCCMS.Domain.DTOs.VolunteerCourseDtos;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.Services.Implements
{
    public class VolunteerCourseService : IVolunteerCourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        public VolunteerCourseService(IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<VolunteerCourseDto?>> GetVolunteerApplicationAsync(
    int courseId,
    string? name = null,
    Gender? gender = null,
    string? phoneNumber = null,
    ProgressStatus? status = null,
    int? reviewerId = null,
    DateTime? startDob = null,
    DateTime? endDob = null,
    int? teamId = null,
    string? volunteerCode = null,
    string? nationalId = null)
        {
            // Kiểm tra định dạng số điện thoại
            if (courseId <= 0)
            {
                throw new ArgumentException("CourseId không hợp lệ. Phải lớn hơn 0.");
            }
            if (reviewerId.HasValue && reviewerId <= 0)
            {
                throw new ArgumentException("ReviewerId không hợp lệ. Phải lớn hơn 0.");
            }

            // Loại bỏ dấu cách dư thừa từ các tham số đầu vào
            name = name?.Trim();
            phoneNumber = phoneNumber?.Trim();
            volunteerCode = volunteerCode?.Trim();
            nationalId = nationalId?.Trim();

            // Thực hiện truy vấn với điều kiện lọc không phân biệt dấu
            var volunteerCourses = await _unitOfWork.VolunteerApplication.FindAsync(vc =>
                (string.IsNullOrEmpty(name) || EF.Functions.Collate(vc.Volunteer.FullName, "Latin1_General_CI_AI").Contains(name)) &&
                (string.IsNullOrEmpty(phoneNumber) || vc.Volunteer.PhoneNumber.Contains(phoneNumber)) &&
                (string.IsNullOrEmpty(volunteerCode) || vc.VolunteerCode.Contains(volunteerCode)) &&
                (status == null || vc.Status.Equals(status)) &&
                (gender == null || vc.Volunteer.Gender.Equals(gender)) &&
                (reviewerId == null || vc.ReviewerId == reviewerId) &&
                (vc.CourseId == courseId) &&
                (!startDob.HasValue || vc.Volunteer.DateOfBirth >= startDob.Value) &&
                vc.Status != ProgressStatus.Delete &&
                (string.IsNullOrEmpty(nationalId) || EF.Functions.Collate(vc.Volunteer.NationalId, "Latin1_General_CI_AI").Contains(nationalId)) &&
                (!endDob.HasValue || vc.Volunteer.DateOfBirth <= endDob.Value),
                includeProperties: "Volunteer,Course,Reviewer,Volunteer.VolunteerTeam.Team");

            // Xử lý lọc theo teamId nếu có
            if (teamId != null && teamId != 0)
            {
                volunteerCourses = volunteerCourses.Where(vc => vc.Volunteer.VolunteerTeam.Any(vt => vt.TeamId == teamId && vt.Team.CourseId == vc.CourseId));
            }
            else if (teamId == 0)
            {
                volunteerCourses = volunteerCourses.Where(vc => vc.Volunteer.VolunteerTeam == null || vc.Volunteer.VolunteerTeam.Count == 0 || !vc.Volunteer.VolunteerTeam.Any(vt => vt.Team.CourseId == courseId));
            }

            if (volunteerCourses == null || !volunteerCourses.Any())
            {
                return new List<VolunteerCourseDto>();
            }

            // Ánh xạ danh sách VolunteerCourse sang VolunteerCourseDto
            var volunteerCourseDtos = volunteerCourses.Select(vc => new VolunteerCourseDto
            {
                Id = vc.Id,
                CourseId = vc.CourseId,
                Course = new DTOs.VolunteerCourseDtos.CourseInforDto
                {
                    Id = vc.CourseId,
                    CourseName = vc.Course.CourseName,
                },
                VolunteerId = vc.VolunteerId,
                Volunteer = new VolunteerInforDto
                {
                    Id = vc.Volunteer.Id,
                    FullName = vc.Volunteer.FullName,
                    DateOfBirth = vc.Volunteer.DateOfBirth,
                    Gender = vc.Volunteer.Gender,
                    Email = vc.Volunteer.Email,
                    PhoneNumber = vc.Volunteer.PhoneNumber,
                    Address = vc.Volunteer.Address,
                    NationalId = vc.Volunteer.NationalId,
                    Image = vc.Volunteer.Image,
                    NationalImageFront = vc.Volunteer.NationalImageFront,
                    NationalImageBack = vc.Volunteer.NationalImageBack,
                    Note = vc.Volunteer.Note,

                    // Lọc các Team thuộc về CourseId hiện tại
                    Teams = vc.Volunteer.VolunteerTeam
                        .Where(vt => vt.Team.CourseId == vc.CourseId)
                        .Select(vt => new TeamInforDto
                        {
                            Id = vt.Team.Id,
                            TeamName = vt.Team.TeamName
                        }).ToList()
                },
                VolunteerCode = vc.VolunteerCode,
                ApplicationDate = vc.ApplicationDate,
                Status = vc.Status,
                Note = vc.Note,
                ReviewerId = vc.ReviewerId,
                Reviewer = vc.Reviewer != null ? new ReviewerInforDto
                {
                    Id = vc.ReviewerId.Value,
                    FullName = vc.Reviewer.FullName,
                    UserName = vc.Reviewer.UserName,
                    Email = vc.Reviewer.Email,
                    PhoneNumber = vc.Reviewer.PhoneNumber,
                } : null,
                ReviewDate = vc.ReviewDate,
                TeamId = vc.Volunteer.VolunteerTeam.Where(vt => vt.Team.CourseId == vc.CourseId).FirstOrDefault()?.TeamId
            });

            return volunteerCourseDtos;
        }




        public async Task<VolunteerCourseDto> GetVolunteerCourseByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Id phải lớn hơn 0.");
            }

            var volunteerCourse = await _unitOfWork.VolunteerApplication
                .GetAsync(vc => vc.Id == id && vc.Status != ProgressStatus.Delete, includeProperties: "Volunteer,Course,Reviewer,Volunteer.VolunteerTeam.Team");

            if (volunteerCourse == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy tình nguyện viên với ID {id}.");
            }


            var listVolunteer = await _unitOfWork.VolunteerApplication.FindAsync(a => a.Status != ProgressStatus.Delete &&
                                                                       a.Volunteer.NationalId == volunteerCourse.Volunteer.NationalId &&
                                                                        a.Id != volunteerCourse.Id &&
                                                                        a.CourseId == volunteerCourse.CourseId);
            var numberSameNationId = listVolunteer.Count();

            var volunteerCourseDto = _mapper.Map<VolunteerCourseDto>(volunteerCourse);
            volunteerCourseDto.SameNationId = numberSameNationId;
            return volunteerCourseDto;
        }

        public async Task UpdateVolunteerCourseAsync(VolunteerCourseUpdateDto entity)
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

            foreach (var volunteerCourseId in entity.Ids)
            {
                if (volunteerCourseId <= 0)
                {
                    throw new ArgumentException("Id phải lớn hơn 0.");
                }

                var volunteerCourse = await _unitOfWork.VolunteerApplication.GetByIdAsync(volunteerCourseId);
                if (volunteerCourse == null)
                {
                    throw new ArgumentException($"Đơn đăng ký tình nguyện viên có ID {volunteerCourseId} không tồn tại.");
                }

                var course = await _unitOfWork.Course.GetByIdAsync(volunteerCourse.CourseId);
                if (course == null)
                {
                    throw new ArgumentException($"Khóa học có ID {volunteerCourse.CourseId} không tồn tại.");
                }

                // Kiểm tra nếu khóa học đã bắt đầu, không thể chuyển trạng thái thành Approved hoặc Rejected
                if (course.StartDate <= DateTime.Now &&
                    (entity.Status == ProgressStatus.Rejected))
                {
                    throw new InvalidOperationException("Không thể thay đổi trạng thái thành 'Rejected' sau khi khóa học đã bắt đầu.");
                }

                // Nếu trạng thái là Approved, tạo mã tình nguyện viên (VolunteerCode)
                if (entity.Status == ProgressStatus.Approved)
                {
                    var volunteerCourses = await _unitOfWork.VolunteerApplication
                        .FindAsync(vc => vc.CourseId == volunteerCourse.CourseId&& vc.Status!= ProgressStatus.Delete && vc.VolunteerCode != null);

                    var maxVolunteerCode = volunteerCourses
                        .OrderByDescending(vc => vc.VolunteerCode)
                        .Select(vc => vc.VolunteerCode)
                        .FirstOrDefault();

                    int nextVolunteerNumber;
                    if (string.IsNullOrEmpty(maxVolunteerCode))
                    {
                        nextVolunteerNumber = 1;
                    }
                    else
                    {
                        nextVolunteerNumber = int.Parse(maxVolunteerCode.Substring(6)) + 1;
                    }

                    string volunteerCode = $"{course.StartDate:yyMMdd}{nextVolunteerNumber:D3}";
                    volunteerCourse.VolunteerCode = volunteerCode;
                }

                if(entity.Status == ProgressStatus.Rejected)
                {
                    var volunteerTeam = await _unitOfWork.VolunteerTeam
                        .FindAsync(vt => vt.VolunteerId == volunteerCourse.VolunteerId && vt.Team.CourseId == volunteerCourse.CourseId);
                        VolunteerTeam? vt = volunteerTeam.FirstOrDefault();
                    if(vt != null) { 
                        await _unitOfWork.VolunteerTeam.DeleteAsync(vt);
                    }
                }


                if (volunteerCourse.Status == ProgressStatus.Pending || (volunteerCourse.Status != entity.Status && entity.Status != null))
                {
                    volunteerCourse.ReviewerId = entity.ReviewerId > 0 ? entity.ReviewerId : null;
                }
                else
                {
                    volunteerCourse.ReviewerId = volunteerCourse.ReviewerId;
                }
                volunteerCourse.Status = entity.Status ?? volunteerCourse.Status;
                volunteerCourse.ReviewDate = DateTime.Now;
                volunteerCourse.Note = entity.Note;
                volunteerCourse.ReviewDate = DateTime.Now;
                await _unitOfWork.VolunteerApplication.UpdateAsync(volunteerCourse);
                if (entity.ReviewerId.HasValue && !entity.Status.HasValue)
                {
                    await _notificationService.NotifyUserAsync(entity.ReviewerId.Value,
                        "Bạn có đơn đăng ký mới cần duyệt. Vui lòng kiểm tra!", $"/volunteer-applications?courseId={volunteerCourse.CourseId}&reviewerId={entity.ReviewerId.Value}");
                }
            }

            await _unitOfWork.SaveChangeAsync();
        }

        // Hàm AutoAssignApplicationsAsync để tự động gán đơn đăng ký cho các reviewer
        public async Task AutoAssignApplicationsAsync(int courseId)
        {
            // Bước 1: Lấy danh sách các thư ký (reviewer) có thể gán
            var availableUsers = await _unitOfWork.User
                .FindAsync(u => u.RoleId == SD.RoleId_Secretary && u.Status == UserStatus.Active);

            if (availableUsers == null || !availableUsers.Any())
            {
                throw new InvalidOperationException("Không có thư ký nào để phân chia.");
            }

            // Bước 2: Lấy danh sách Application chưa được gán reviewer
            var unassignedApplications = await _unitOfWork.VolunteerApplication
                .FindAsync(a => a.ReviewerId == null&& a.Status != ProgressStatus.Delete && a.CourseId == courseId);

            if (unassignedApplications == null || !unassignedApplications.Any())
            {
                throw new InvalidOperationException("Tất cả đơn đăng ký đã được phân chia.");
            }

            // Bước 3: Lấy số lượng đơn đã được gán cho mỗi thư ký
            var secretaryAssignments = new List<SecretaryAssignment>();
            foreach (var user in availableUsers)
            {
                var assignedCount = await _unitOfWork.VolunteerApplication.CountAsync(a => a.ReviewerId == user.Id && a.CourseId == courseId&& a.Status != ProgressStatus.Delete);
                secretaryAssignments.Add(new SecretaryAssignment
                {
                    Secretary = user,
                    AssignedCount = assignedCount
                });
            }

            // Bước 4: Phân phối đơn đăng ký cho reviewer
            var unassignedApps = unassignedApplications.ToList();
            while (unassignedApps.Any())
            {
                var minAssignedCount = secretaryAssignments.Min(sa => sa.AssignedCount);
                var secretaryToAssign = secretaryAssignments.First(sa => sa.AssignedCount == minAssignedCount);

                var application = unassignedApps.First();
                application.ReviewerId = secretaryToAssign.Secretary.Id;
                application.ReviewDate = DateTime.Now;

                secretaryToAssign.AssignedCount += 1;
                unassignedApps.RemoveAt(0);
            }

            await _unitOfWork.SaveChangeAsync();
        }

        // Hàm SendApplicationResultAsync để gửi kết quả của các đơn đăng ký
        public async Task SendApplicationResultAsync(int[] listVolunteerApplicationId, int courseId, string subject, string body)
        {
            var listVolunteerId = new List<int>();
            var course = await _unitOfWork.Course.GetByIdAsync(courseId);
            if (course == null)
            {
                throw new ArgumentException("Khóa học không tồn tại.");
            }

            var listVolunteerApplication = await _unitOfWork.VolunteerApplication.FindAsync(va => listVolunteerApplicationId.Contains(va.Id)&& va.Status != ProgressStatus.Delete);
            if (listVolunteerApplication == null || !listVolunteerApplication.Any())
            {
                throw new ArgumentException("Không tìm thấy đơn đăng ký.");
            }

            foreach (var volunteerApp in listVolunteerApplication)
            {
                if (volunteerApp.Status == ProgressStatus.Pending)
                {
                    throw new ArgumentException("Đơn đăng ký chưa được duyệt hết.");
                }
                listVolunteerId.Add(volunteerApp.VolunteerId);
            }

            SendBulkEmailRequestDto sendBulkEmailRequestDto = new SendBulkEmailRequestDto
            {
                CourseId = courseId,
                ListVolunteerId = listVolunteerId,
                Subject = subject,
                Message = body
            };
            _emailService.SendBulkEmailAsync(sendBulkEmailRequestDto);
        }

        public async Task<IEnumerable<VolunteerCourseDto?>> GetAllVolunteerCourseAsync(
    int courseId,
    string? name = null,
    Gender? gender = null,
    string? teamName = null,
    string? phoneNumber = null,
    ProgressStatus? status = null,
    string? volunteerCode = null,
    DateTime? startDob = null,
    DateTime? endDob = null)
        {
            // Validate phone number format
            if (!string.IsNullOrEmpty(phoneNumber) && !System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^\d{10}$"))
            {
                throw new ArgumentException("Định dạng số điện thoại không hợp lệ. Phải có 10 chữ số.");
            }
            if (courseId <= 0)
            {
                throw new ArgumentException("CourseId không hợp lệ. Phải lớn hơn 0.");
            }

            // Perform query with filtering conditions, excluding "rejected" and "pending" statuses
            var volunteerCourses = await _unitOfWork.VolunteerApplication.FindAsync(vc =>
                vc.CourseId == courseId &&
                (string.IsNullOrEmpty(name) || EF.Functions.Collate(vc.Volunteer.FullName, "Latin1_General_CI_AI").Contains(name)) &&
                (string.IsNullOrEmpty(phoneNumber) || vc.Volunteer.PhoneNumber.Contains(phoneNumber)) &&
                (status == null || vc.Status.Equals(status)) &&
                (vc.Status != ProgressStatus.Rejected && vc.Status != ProgressStatus.Pending) &&
                (gender == null || vc.Volunteer.Gender.Equals(gender)) &&
                (string.IsNullOrEmpty(volunteerCode) || EF.Functions.Collate(vc.VolunteerCode, "Latin1_General_CI_AI").Contains(volunteerCode)) &&
                (!startDob.HasValue || vc.Volunteer.DateOfBirth >= startDob.Value) &&
                (!endDob.HasValue || vc.Volunteer.DateOfBirth <= endDob.Value) &&
                vc.Status != ProgressStatus.Delete &&
                (string.IsNullOrEmpty(teamName) || vc.Volunteer.VolunteerTeam.Any(vt => EF.Functions.Collate(vt.Team.TeamName, "Latin1_General_CI_AI").Contains(teamName))),
                includeProperties: "Volunteer,Course,Reviewer,Volunteer.VolunteerTeam.Team");

            // Kiểm tra nếu không có kết quả nào
            if (volunteerCourses == null || !volunteerCourses.Any())
            {
                return new List<VolunteerCourseDto>();
            }

            // Ánh xạ kết quả thành DTO
            var volunteerCourseDtos = volunteerCourses.Select(vc => new VolunteerCourseDto
            {
                Id = vc.Id,
                CourseId = vc.CourseId,
                Course = new DTOs.VolunteerCourseDtos.CourseInforDto
                {
                    Id = vc.CourseId,
                    CourseName = vc.Course.CourseName,
                },
                VolunteerId = vc.VolunteerId,
                Volunteer = new VolunteerInforDto
                {
                    Id = vc.Volunteer.Id,
                    FullName = vc.Volunteer.FullName,
                    DateOfBirth = vc.Volunteer.DateOfBirth,
                    Gender = vc.Volunteer.Gender,
                    Email = vc.Volunteer.Email,
                    PhoneNumber = vc.Volunteer.PhoneNumber,
                    Address = vc.Volunteer.Address,
                    NationalId = vc.Volunteer.NationalId,
                    Image = vc.Volunteer.Image,
                    NationalImageFront = vc.Volunteer.NationalImageFront,
                    NationalImageBack = vc.Volunteer.NationalImageBack,
                    Note = vc.Volunteer.Note,

                    // Lấy thông tin nhóm liên quan đến khóa học hiện tại
                    Teams = vc.Volunteer.VolunteerTeam
                        .Where(vt => vt.Team.CourseId == vc.CourseId)
                        .Select(vt => new TeamInforDto
                        {
                            Id = vt.Team.Id,
                            TeamName = vt.Team.TeamName
                        }).ToList()
                },
                VolunteerCode = vc.VolunteerCode,
                ApplicationDate = vc.ApplicationDate,
                Status = vc.Status,
                Note = vc.Note,
                ReviewerId = vc.ReviewerId,
                Reviewer = vc.Reviewer != null ? new ReviewerInforDto
                {
                    Id = vc.ReviewerId.Value,
                    FullName = vc.Reviewer.FullName,
                    UserName = vc.Reviewer.UserName,
                    Email = vc.Reviewer.Email,
                    PhoneNumber = vc.Reviewer.PhoneNumber,
                } : null,
                ReviewDate = vc.ReviewDate
            });

            return volunteerCourseDtos;
        }
    



        public async Task<VolunteerCourseDto?> GetByVolunteerIdAndCourseIdAsync(int volunteerId, int courseId)
        {
            if (volunteerId <= 0)
            {
                throw new ArgumentException("VolunteerId không hợp lệ. Phải lớn hơn 0.");
            }
            if (courseId <= 0)
            {
                throw new ArgumentException("CourseId không hợp lệ. Phải lớn hơn 0.");
            }

            // Thực hiện truy vấn với điều kiện lọc theo volunteerId và courseId
            var volunteerApplication = await _unitOfWork.VolunteerApplication.FindAsync(
                vc => vc.VolunteerId == volunteerId && vc.CourseId == courseId&& vc.Status != ProgressStatus.Delete,
                includeProperties: "Volunteer,Course,Reviewer,Volunteer.VolunteerTeam.Team");

            var volunteerCourseEntity = volunteerApplication.FirstOrDefault();

            if (volunteerCourseEntity == null)
            {
                return null;
            }

            // Ánh xạ kết quả thành DTO
            var volunteerCourseDto = new VolunteerCourseDto
            {
                Id = volunteerCourseEntity.Id,
                CourseId = volunteerCourseEntity.CourseId,
                Course = new DTOs.VolunteerCourseDtos.CourseInforDto
                {
                    Id = volunteerCourseEntity.CourseId,
                    CourseName = volunteerCourseEntity.Course.CourseName,
                },
                VolunteerId = volunteerCourseEntity.VolunteerId,
                Volunteer = new VolunteerInforDto
                {
                    Id = volunteerCourseEntity.VolunteerId,
                    FullName = volunteerCourseEntity.Volunteer.FullName,
                    DateOfBirth = volunteerCourseEntity.Volunteer.DateOfBirth,
                    Gender = volunteerCourseEntity.Volunteer.Gender,
                    Email = volunteerCourseEntity.Volunteer.Email,
                    PhoneNumber = volunteerCourseEntity.Volunteer.PhoneNumber,
                    Address = volunteerCourseEntity.Volunteer.Address,
                    NationalId = volunteerCourseEntity.Volunteer.NationalId,
                    Image = volunteerCourseEntity.Volunteer.Image,
                    NationalImageFront = volunteerCourseEntity.Volunteer.NationalImageFront,
                    NationalImageBack = volunteerCourseEntity.Volunteer.NationalImageBack,
                    Note = volunteerCourseEntity.Volunteer.Note,
                    Status = volunteerCourseEntity.Volunteer.Status,

                    // Lấy thông tin nhóm tình nguyện liên quan đến khóa học hiện tại
                    Teams = volunteerCourseEntity.Volunteer.VolunteerTeam
                        .Where(vt => vt.Team.CourseId == volunteerCourseEntity.CourseId)
                        .Select(vt => new TeamInforDto
                        {
                            Id = vt.Team.Id,
                            TeamName = vt.Team.TeamName,
                        }).ToList()
                },
                VolunteerCode = volunteerCourseEntity.VolunteerCode,
                ApplicationDate = volunteerCourseEntity.ApplicationDate,
                Status = volunteerCourseEntity.Status,
                Note = volunteerCourseEntity.Note,
                ReviewerId = volunteerCourseEntity.ReviewerId,
                Reviewer = volunteerCourseEntity.Reviewer != null ? new ReviewerInforDto
                {
                    Id = volunteerCourseEntity.ReviewerId.Value,
                    FullName = volunteerCourseEntity.Reviewer.FullName,
                    UserName = volunteerCourseEntity.Reviewer.UserName,
                    Email = volunteerCourseEntity.Reviewer.Email,
                    PhoneNumber = volunteerCourseEntity.Reviewer.PhoneNumber,

                } : null,
                ReviewDate = volunteerCourseEntity.ReviewDate
            };

            return volunteerCourseDto;
        }
		public async Task SendVolunteerApplicationResultAsync(int[] listVolunteerApplicationId, int courseId, string subject, string body)
		{
			// Danh sách để lưu trữ các ID của Volunteer được duyệt
			var listVolunteerId = new List<int>();

			// Lấy thông tin khóa học dựa trên courseId
			var course = await _unitOfWork.Course.GetByIdAsync(courseId);
			if (course == null)
			{
				throw new ArgumentException("Khóa học không tồn tại.");
			}

			// Lấy danh sách các đơn đăng ký Volunteer dựa trên listVolunteerApplicationId
			var listVolunteerApplication = await _unitOfWork.VolunteerApplication.FindAsync(vc => listVolunteerApplicationId.Contains(vc.Id) && vc.Status != ProgressStatus.Delete);
			if (listVolunteerApplication == null || !listVolunteerApplication.Any())
			{
				throw new ArgumentException("Không tìm thấy đơn đăng ký Volunteer.");
			}

			// Kiểm tra trạng thái của từng đơn đăng ký và thu thập các VolunteerId
			foreach (var volunteerApp in listVolunteerApplication)
			{
				if (volunteerApp.Status == ProgressStatus.Pending)
				{
					throw new ArgumentException("Có đơn đăng ký Volunteer chưa được duyệt.");
				}
				listVolunteerId.Add(volunteerApp.VolunteerId);
			}

			// Lưu các thay đổi nếu có
			await _unitOfWork.SaveChangeAsync();

			// Tạo đối tượng yêu cầu gửi email
			SendBulkEmailRequestDto sendBulkEmailRequestDto = new SendBulkEmailRequestDto
			{
				CourseId = courseId,
				ListVolunteerId = listVolunteerId,
				Subject = subject,
				Message = body
			};

			// Gửi email
			_emailService.SendBulkEmailAsync(sendBulkEmailRequestDto);
		}

        public async Task<byte[]> GenerateVolunteerCardsPdfAsync(List<int> volunteerCourseIds, int courseId)
        {
            using (var ms = new MemoryStream())
            {
                PdfDocument document = new PdfDocument();

                // Lấy danh sách sinh viên theo volunteerCourseIds và courseId
                var volunteerCourses = await _unitOfWork.VolunteerApplication.FindAsync(
                    sc => volunteerCourseIds.Contains(sc.Id) && sc.CourseId == courseId && sc.Status != ProgressStatus.Delete,
                    includeProperties: "Volunteer,Course,Volunteer.VolunteerTeam.Team"
                );

                if (volunteerCourses == null || !volunteerCourses.Any())
                {
                    return null;
                }

                // URL đến hình nền trên cloud
                string backgroundImageUrl = "https://coloan.blob.core.windows.net/coloanimage/VolunteerCard.png";

                byte[] backgroundImageBytes;

                // Tải hình nền từ cloud
                using (var client = new HttpClient())
                {
                    backgroundImageBytes = await client.GetByteArrayAsync(backgroundImageUrl);
                }
                var backgroundTempFilePath = Path.GetTempFileName();
                await File.WriteAllBytesAsync(backgroundTempFilePath, backgroundImageBytes);

                // Tạo thẻ sinh viên cho từng sinh viên
                foreach (var volunteerCourse in volunteerCourses)
                {
                    var volunteer = volunteerCourse.Volunteer; // Lấy thông tin sinh viên từ thực thể VolunteerCourse

                    // Lấy nhóm sinh viên chỉ thuộc về courseId hiện tại
                    var teamName = volunteer.VolunteerTeam
                        .Where(sga => sga.Team.CourseId == courseId)
                        .Select(sga => sga.Team.TeamName)
                        .FirstOrDefault() ?? "";

                    // Lấy mã sinh viên trong khóa học hiện tại
                    var volunteerCode = volunteer.VolunteerCourse
                        .Where(sc => sc.CourseId == courseId)
                        .Select(sc => sc.VolunteerCode)
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
                    string schoolName = $"{volunteerCourse.Course.CourseName}";
                    double schoolNameX = (page.Width - gfx.MeasureString(schoolName, titleFont).Width) / 2;
                    gfx.DrawString(schoolName, titleFont, PdfSharp.Drawing.XBrushes.YellowGreen, new PdfSharp.Drawing.XPoint(schoolNameX, 110));

                    // Thêm ảnh sinh viên
                    if (!string.IsNullOrEmpty(volunteer.Image))
                    {
                        try
                        {
                            using (var client = new HttpClient())
                            {
                                var imageBytes = await client.GetByteArrayAsync(volunteer.Image);

                                // Tạo một tệp tạm thời
                                var tempFilePath = Path.GetTempFileName();

                                try
                                {
                                    // Lưu ảnh vào tệp tạm thời
                                    await File.WriteAllBytesAsync(tempFilePath, imageBytes);

                                    // Đọc ảnh từ tệp tạm thời
                                    using (var volunteerImage = PdfSharp.Drawing.XImage.FromFile(tempFilePath))
                                    {
                                        // Kích thước gốc của hình ảnh
                                        double originalWidth = volunteerImage.PixelWidth;
                                        double originalHeight = volunteerImage.PixelHeight;

                                        // Xác định kích thước mong muốn khi in ra (có thể thay đổi tùy theo yêu cầu)
                                        double desiredWidth = 220; // Ví dụ chiều rộng mong muốn
                                        double desiredHeight;

                                        // Tính toán tỷ lệ để đảm bảo không méo hình
                                        double aspectRatio = originalWidth / originalHeight;
                                        desiredHeight = desiredWidth / aspectRatio;

                                        // Vẽ hình ảnh với kích thước đã được tính toán theo tỷ lệ
                                        double imageX = (page.Width - desiredWidth) / 2;
                                        double imageY = (page.Height - desiredHeight) / 2- 40;
                                        gfx.DrawImage(volunteerImage, imageX, imageY, desiredWidth, desiredHeight);
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
                    string fullName = $"{volunteer.FullName}";
                    double fullNameX = (page.Width - gfx.MeasureString(fullName, contentFont).Width) / 2;
                    gfx.DrawString(fullName, contentFont, PdfSharp.Drawing.XBrushes.Black, new PdfSharp.Drawing.XPoint(fullNameX, 450));

                    // Thêm tên nhóm sinh viên
                    string groupName = $"Ban: {teamName}";
                    double groupNameX = (page.Width - gfx.MeasureString(groupName, contentFont).Width) / 2;
                    gfx.DrawString(groupName, contentFont, PdfSharp.Drawing.XBrushes.Black, new PdfSharp.Drawing.XPoint(groupNameX, 490));

                    // Căn giữa mã sinh viên
                    string volunteerId = $"Mã TNV: {volunteerCode}";
                    double volunteerIdX = (page.Width - gfx.MeasureString(volunteerId, contentFont).Width) / 2;
                    gfx.DrawString(volunteerId, contentFont, PdfSharp.Drawing.XBrushes.Black, new PdfSharp.Drawing.XPoint(volunteerIdX, 520));
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

        public async Task<byte[]> GenerateVolunteerCertificatePdfAsync(List<int> volunteerCourseIds, int courseId)
        {
            using (var ms = new MemoryStream())
            {
                PdfDocument document = new PdfDocument();

                // Lấy danh sách sinh viên theo volunteerCourseIds và courseId
                var volunteerCourses = await _unitOfWork.VolunteerApplication.FindAsync(
                    sc => volunteerCourseIds.Contains(sc.Id) && sc.CourseId == courseId && sc.Status != ProgressStatus.Delete,
                    includeProperties: "Volunteer,Course,Volunteer.VolunteerTeam.Team"
                );

                if (volunteerCourses == null || !volunteerCourses.Any())
                {
                    return null;
                }

                // URL đến hình nền trên cloud
                string backgroundImageUrl = "https://coloan.blob.core.windows.net/coloanimage/certificate.png";

                byte[] backgroundImageBytes;

                // Tải hình nền từ cloud
                using (var client = new HttpClient())
                {
                    backgroundImageBytes = await client.GetByteArrayAsync(backgroundImageUrl);
                }
                var backgroundTempFilePath = Path.GetTempFileName();
                await File.WriteAllBytesAsync(backgroundTempFilePath, backgroundImageBytes);

                // Tạo thẻ sinh viên cho từng sinh viên
                foreach (var volunteerCourse in volunteerCourses)
                {
                    var volunteer = volunteerCourse.Volunteer; // Lấy thông tin sinh viên từ thực thể VolunteerCourse

                    // Lấy nhóm sinh viên chỉ thuộc về courseId hiện tại
                    var teamName = volunteer.VolunteerTeam
                        .Where(sga => sga.Team.CourseId == courseId)
                        .Select(sga => sga.Team.TeamName)
                        .FirstOrDefault() ?? "";  // Nếu không có nhóm, trả về "N/A"

                    // Lấy mã sinh viên trong khóa học hiện tại
                    var volunteerCode = volunteer.VolunteerCourse
                        .Where(sc => sc.CourseId == courseId)
                        .Select(sc => sc.VolunteerCode)
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
                    var nameFont = new PdfSharp.Drawing.XFont("Verdana", 30, PdfSharp.Drawing.XFontStyleEx.BoldItalic);

                    var contentFont = new PdfSharp.Drawing.XFont("Verdana", 13);

                    // Thêm tên khóa học
                    string schoolName = $"{volunteerCourse.Course.CourseName}";
                    double schoolNameX = (page.Width - gfx.MeasureString(schoolName, titleFont).Width) / 2;
                    gfx.DrawString(schoolName, titleFont, PdfSharp.Drawing.XBrushes.DarkRed, new PdfSharp.Drawing.XPoint(schoolNameX, 180));

                    // Thêm tên sinh viên
                    string fullName = $"{volunteer.FullName}";
                    double fullNameX = (page.Width - gfx.MeasureString(fullName, nameFont).Width) / 2;
                    gfx.DrawString(fullName, nameFont, PdfSharp.Drawing.XBrushes.Black, new PdfSharp.Drawing.XPoint(fullNameX, 290));

                    // Thêm text sinh viên
                    string text = $"Là tình nguyên viên có đóng góp tích cực";
                    double groupNameX = (page.Width - gfx.MeasureString(text, contentFont).Width) / 2;
                    gfx.DrawString(text, contentFont, PdfSharp.Drawing.XBrushes.Black, new PdfSharp.Drawing.XPoint(groupNameX, 340));
                    // Thêm text sinh viên
                    string text2 = $"Trong khóa tu {volunteerCourse.Course.CourseName}";
                    double groupNameX2 = (page.Width - gfx.MeasureString(text2, contentFont).Width) / 2;
                    gfx.DrawString(text2, contentFont, PdfSharp.Drawing.XBrushes.Black, new PdfSharp.Drawing.XPoint(groupNameX2, 360));
                    // Thêm text sinh viên
                    string text3 = $"Tại chùa Cổ Loan- Ninh Tiến- Ninh Bình";
                    double groupNameX3 = (page.Width - gfx.MeasureString(text3, contentFont).Width) / 2;
                    gfx.DrawString(text3, contentFont, PdfSharp.Drawing.XBrushes.Black, new PdfSharp.Drawing.XPoint(groupNameX3, 380));


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


        public async Task UpdateVolunteerInformationInACourseAsync(VolunteerInformationInACourseDto volunteerInfoDto)
        {
            // 1. Validate inputs
            if (volunteerInfoDto.VolunteerId <= 0)
                throw new ArgumentException("Invalid VolunteerId.");

            if (volunteerInfoDto.CourseId <= 0)
                throw new ArgumentException("Invalid CourseId.");

            // 2. Retrieve the VolunteerCourse entity
            var volunteerCourses = await _unitOfWork.VolunteerApplication.FindAsync(vc =>
                vc.VolunteerId == volunteerInfoDto.VolunteerId && vc.CourseId == volunteerInfoDto.CourseId && volunteerInfoDto.Status != ProgressStatus.Delete,
                includeProperties: "Volunteer,Course,Volunteer.VolunteerTeam.Team");

            var volunteerCourse = volunteerCourses.FirstOrDefault();
            if (volunteerCourse == null)
                throw new KeyNotFoundException("VolunteerCourse not found.");

            // 3. Update Status and Note
            volunteerCourse.Status = volunteerInfoDto.Status;
            volunteerCourse.Note = volunteerInfoDto.Note;

            // 4. Handle TeamId
            if (volunteerInfoDto.TeamId > 0)
            {
                // a. Kiểm tra xem Team có tồn tại và thuộc về Course không
                var team = await _unitOfWork.Team.GetByIdAsync(volunteerInfoDto.TeamId);
                if (team == null || team.CourseId != volunteerInfoDto.CourseId)
                    throw new ArgumentException("Invalid TeamId. Team does not exist or does not belong to the Course.");

                // b. Kiểm tra xem Volunteer đã thuộc Team nào cho Course này chưa
                var existingVolunteerTeam = volunteerCourse.Volunteer.VolunteerTeam
                    .FirstOrDefault(vt => vt.Team.CourseId == volunteerInfoDto.CourseId);

                if (existingVolunteerTeam != null)
                {
                    if (existingVolunteerTeam.TeamId != volunteerInfoDto.TeamId)
                    {

                        // Cập nhật TeamId
                        await _unitOfWork.VolunteerTeam.DeleteAsync(existingVolunteerTeam);
                        var newVolunteerTeam = new VolunteerTeam
                        {
                            VolunteerId = volunteerInfoDto.VolunteerId,
                            TeamId = volunteerInfoDto.TeamId
                        };

                        await _unitOfWork.VolunteerTeam.AddAsync(newVolunteerTeam);
                    }
                }
                else
                {
                    // Thêm mới VolunteerTeam association
                    var newVolunteerTeam = new VolunteerTeam
                    {
                        VolunteerId = volunteerInfoDto.VolunteerId,
                        TeamId = volunteerInfoDto.TeamId
                    };
                    await _unitOfWork.VolunteerTeam.AddAsync(newVolunteerTeam);
                }
            }
            else if (volunteerInfoDto.TeamId == 0)
            {
                // Nếu TeamId = 0, loại bỏ mọi association với Team cho Course này
                var volunteerTeamsToRemove = volunteerCourse.Volunteer.VolunteerTeam
                    .Where(vt => vt.Team.CourseId == volunteerInfoDto.CourseId)
                    .ToList();

                foreach (var vt in volunteerTeamsToRemove)
                {
                    await _unitOfWork.VolunteerTeam.DeleteAsync(vt);
                }
            }

            // 5. Cập nhật VolunteerCourse entity
            await _unitOfWork.VolunteerApplication.UpdateAsync(volunteerCourse);

            // 6. Lưu thay đổi vào database
            await _unitOfWork.SaveChangeAsync();
        }

        private class SecretaryAssignment
        {
            public User Secretary { get; set; }
            public int AssignedCount { get; set; }
        }
    }

}

