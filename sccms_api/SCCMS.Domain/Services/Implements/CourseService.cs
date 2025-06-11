using AutoMapper;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using SCCMS.Domain.DTOs.CourseDtos;
using SCCMS.Domain.DTOs.DashboardDtos;
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
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CourseService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task CreateCourseAsync(CourseCreateDto courseCreateDto)
        {
            var courseAll = await _unitOfWork.Course.GetAllAsync();
            foreach (var item in courseAll)
            {
                if (item.Status != CourseStatus.closed && item.Status != CourseStatus.deleted &&
                    item.EndDate.Date >= courseCreateDto.StartDate.Date && item.StartDate.Date <= courseCreateDto.EndDate.Date)
                {
                    throw new ArgumentException("Thời gian bắt đầu khóa tu đang trùng với khóa tu: " + item.CourseName);
                }
                if (item.Status != CourseStatus.closed && item.Status != CourseStatus.deleted &&
                                       item.CourseName == courseCreateDto.CourseName)
                {
                    throw new ArgumentException("Tên khóa tu đã tồn tại: ");
                }
            }


            if (courseCreateDto.StudentApplicationStartDate.Date < DateTime.Now.Date)
            {
                throw new ArgumentException("Ngày nộp đơn phải lớn hơn ngày hiện tại.");
            }

            if (courseCreateDto.VolunteerApplicationStartDate.Date < DateTime.Now.Date)
            {
                throw new ArgumentException("Ngày nộp đơn phải lớn hơn ngày hiện tại.");
            }

            if (courseCreateDto.StudentApplicationEndDate.Date < courseCreateDto.StudentApplicationStartDate.Date)
            {
                throw new ArgumentException("Ngày kết thúc phải lớn hơn ngày bắt đầu.");
            }

            if (courseCreateDto.VolunteerApplicationEndDate.Date < courseCreateDto.VolunteerApplicationStartDate.Date)
            {
                throw new ArgumentException("Ngày kết thúc phải lớn hơn ngày bắt đầu.");
            }

            if (courseCreateDto.StartDate.Date < courseCreateDto.StudentApplicationEndDate.Date || courseCreateDto.StartDate.Date < courseCreateDto.VolunteerApplicationEndDate.Date)
            {
                throw new ArgumentException("Ngày bắt đầu khóa tu phải sau ngày kết thúc thuyển sinh");
            }

            if (courseCreateDto.EndDate.Date < courseCreateDto.StartDate.Date)
            {
                throw new ArgumentException("Ngày kết thúc phải lớn hơn ngày bắt đầu");
            }
            if (courseCreateDto.CourseName == null || courseCreateDto.CourseName == "")
            {
                throw new ArgumentException("Tên không hợp lệ");
            }
            if (courseCreateDto.ExpectedStudents <= 0 || courseCreateDto.ExpectedStudents >= 1000)
            {
                throw new ArgumentException("Số lượng học sinh dự kiến không hợp lệ");
            }

            Course course = _mapper.Map<Course>(courseCreateDto);
            course.Status = CourseStatus.notStarted;
            await _unitOfWork.Course.AddAsync(course);
            await _unitOfWork.SaveChangeAsync();
            await _unitOfWork.NightShift.AddAsync(
                new NightShift(course.Id, new TimeSpan(22, 0, 0), new TimeSpan(1, 0, 0)));
            await _unitOfWork.NightShift.AddAsync(
                new NightShift(course.Id, new TimeSpan(1, 0, 0), new TimeSpan(3, 0, 0)));
            await _unitOfWork.NightShift.AddAsync(
                new NightShift(course.Id, new TimeSpan(3, 0, 0), new TimeSpan(6, 0, 0)));
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task DeleteCourseAsync(int id)
        {
            Course course = await _unitOfWork.Course.GetByIdAsync(id);
            if (course == null)
            {
                throw new ArgumentException("Khóa tu không tồn tại");
            }
            await _unitOfWork.Course.DeleteAsync(course);
            await _unitOfWork.SaveChangeAsync();
        }


        public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync(
    string? courseName = null,
    CourseStatus? status = null,
    DateTime? startDate = null,
    DateTime? endDate = null)
        {
            // Loại bỏ dấu cách dư thừa từ các tham số đầu vào
            courseName = courseName?.Trim();

            var courses = await _unitOfWork.Course.FindAsync(c =>
                (string.IsNullOrEmpty(courseName) || EF.Functions.Collate(c.CourseName, "Latin1_General_CI_AI").Contains(courseName)) &&
                (status == null || c.Status.Equals(status)) &&
                (!startDate.HasValue || c.StartDate.Date >= startDate.Value.Date) &&
                (!endDate.HasValue || c.StartDate.Date <= endDate.Value.Date) &&
                (c.Status != CourseStatus.deleted));

            if (courses == null || !courses.Any())
            {
                return new List<CourseDto>();
            }

            var courseDtos = _mapper.Map<IEnumerable<CourseDto>>(courses);

            return courseDtos;
        }



        public async Task<IEnumerable<CourseDto>> GetAvaiableFeedBackCourseAsync()
        {
            var currentDate = DateTime.Now.Date;
            var threeMonthsAgo = currentDate.AddMonths(-3);

            var courses = await _unitOfWork.Course.FindAsync(
                c => c.EndDate.Date >= threeMonthsAgo && c.EndDate.Date <= currentDate && c.Status == CourseStatus.closed
            );

            var orderedCourses = courses.OrderBy(c => c.EndDate);

            var courseDtos = _mapper.Map<IEnumerable<CourseDto>>(orderedCourses);

            return courseDtos;
        }




        public async Task<CourseDto?> GetCourseByIdAsync(int id)
        {
            try
            {
                var course = (await _unitOfWork.Course.FindAsync(c => c.Id == id && c.Status != CourseStatus.deleted, includeProperties: "StudentGroup,StudentCourses"))
                     .FirstOrDefault();

                if (course == null)
                {
                    return null;
                }

                CourseDto courseDto = _mapper.Map<CourseDto>(course);
                return courseDto;
            }
            catch (Exception ex)
            {
                throw new Exception($"Không tìm thấy khóa tu hợp lệ");
            }
        }

        public async Task<CourseDto?> GetCurrentCourseAsync()
        {
            // Tìm khóa học đang diễn ra
            var currentCourse = await _unitOfWork.Course.GetAsync(
                c => c.Status == CourseStatus.inProgress
            );
            if (currentCourse == null)
            {
                currentCourse = await _unitOfWork.Course.GetAsync(
                                       c => c.Status == CourseStatus.recruiting);
            }

            if (currentCourse == null)
            {
                // Tìm tất cả các khóa học sắp diễn ra trong tương lai
                var upcomingCourses = await _unitOfWork.Course.FindAsync(
                    c => c.StartDate >= DateTime.Now
                );

                // Chọn khóa học có StartDate gần nhất
                currentCourse = upcomingCourses
                    .OrderBy(c => c.StartDate)
                    .FirstOrDefault();

                // Nếu không có khóa học sắp diễn ra, tìm khóa học có EndDate gần nhất
                if (currentCourse == null)
                {
                    var pastCourses = await _unitOfWork.Course.GetAllAsync();
                    currentCourse = pastCourses
                        .OrderByDescending(c => c.EndDate) // Sắp xếp theo EndDate tăng dần
                        .FirstOrDefault();
                }
            }

            if (currentCourse == null)
            {
                return null;
            }

            var courseDto = _mapper.Map<CourseDto>(currentCourse);
            return courseDto;
        }

        public async Task UpdateCourseAsync(int courseId, CourseUpdateDto courseUpdateDto)
        {
            var existingCourse = await _unitOfWork.Course.GetByIdAsync(courseId);
            if (existingCourse == null)
            {
                throw new ArgumentException("Course not found.");
            }
            if (existingCourse.Status == CourseStatus.inProgress)
            {
                if (courseUpdateDto.StudentApplicationEndDate.HasValue ||
                    courseUpdateDto.VolunteerApplicationEndDate.HasValue ||
                    courseUpdateDto.StudentApplicationStartDate.HasValue ||
                    courseUpdateDto.VolunteerApplicationStartDate.HasValue ||
                    courseUpdateDto.StartDate.HasValue)
                {
                    throw new ArgumentException("Không thể thay đổi ngày khi khóa tu đang diễn ra");
                }
            }
            if (existingCourse.Status == CourseStatus.closed || existingCourse.Status == CourseStatus.deleted)
            {
                if (courseUpdateDto.StudentApplicationEndDate.HasValue ||
                    courseUpdateDto.VolunteerApplicationEndDate.HasValue ||
                    courseUpdateDto.StudentApplicationStartDate.HasValue ||
                    courseUpdateDto.VolunteerApplicationStartDate.HasValue ||
                    courseUpdateDto.StartDate.HasValue ||
                    courseUpdateDto.EndDate.HasValue)
                {
                    throw new ArgumentException("Không thể thay đổi ngày khi khóa tu đã kết thúc");
                }
            }
            if (existingCourse.Status == CourseStatus.recruiting)
            {
                if (courseUpdateDto.StudentApplicationStartDate.HasValue || courseUpdateDto.VolunteerApplicationStartDate.HasValue)
                {
                    throw new ArgumentException("Không thể thay đổi ngày khi khóa tu đang tuyển sinh");
                }
            }

            // Initialize variables with existing values or updated ones
            var updatedStartDate = courseUpdateDto.StartDate ?? existingCourse.StartDate;
            var updatedEndDate = courseUpdateDto.EndDate ?? existingCourse.EndDate;
            var updatedCourseName = courseUpdateDto.CourseName ?? existingCourse.CourseName;

            // Check for overlapping dates with other courses only if dates have changed
            if ((courseUpdateDto.StartDate.HasValue && courseUpdateDto.StartDate.Value.Date != existingCourse.StartDate.Date) ||
                (courseUpdateDto.EndDate.HasValue && courseUpdateDto.EndDate.Value.Date != existingCourse.EndDate.Date))
            {
                var courseAll = await _unitOfWork.Course.GetAllAsync();
                foreach (var item in courseAll)
                {
                    if (item.Status != CourseStatus.closed && item.Status != CourseStatus.deleted && item.Id != courseId &&
                        item.EndDate.Date >= updatedStartDate.Date &&
                        item.StartDate.Date <= updatedEndDate.Date)
                    {
                        throw new ArgumentException("Thời gian khóa tu đang trùng với khóa tu: " + item.CourseName);
                    }
                }
            }

            // **Thêm Kiểm Tra Tên Khóa Tu Trùng Lặp Ở Đây**
            if (!string.IsNullOrEmpty(courseUpdateDto.CourseName) && courseUpdateDto.CourseName != existingCourse.CourseName)
            {
                // Sử dụng EF.Functions.Collate để so sánh không phân biệt chữ hoa chữ thường và dấu
                bool isDuplicateName = await _unitOfWork.Course.AnyAsync(c =>
                    c.Status != CourseStatus.closed && c.Status != CourseStatus.deleted &&
                    EF.Functions.Collate(c.CourseName, "Latin1_General_CI_AI") == courseUpdateDto.CourseName &&
                    c.Id != courseId
                );

                if (isDuplicateName)
                {
                    throw new ArgumentException($"Tên khóa tu đã tồn tại: {courseUpdateDto.CourseName}");
                }
            }

            //check have course is InProgress
            if (courseUpdateDto.Status.HasValue && courseUpdateDto.Status.Value == CourseStatus.inProgress)
            {
                var inProgressCourses = await _unitOfWork.Course.FindAsync(
                    c => c.Status == CourseStatus.inProgress && c.Id != courseId
                );

                if (inProgressCourses.Any())
                {
                    throw new ArgumentException("Không thể mở khóa tu mới khi đã có khóa tu đang diễn ra");
                }
            }

            if (courseUpdateDto.CourseName == "")
            {
                throw new ArgumentException("Tên không hợp lệ");
            }
            if (courseUpdateDto.ExpectedStudents.HasValue && (courseUpdateDto.ExpectedStudents <= 0 || courseUpdateDto.ExpectedStudents >= 1000))
            {
                throw new ArgumentException("Số lượng học sinh dự kiến không hợp lệ");
            }

            // The duplicate name check đã được thực hiện ở trên, không cần kiểm tra lại

            // Validate Student Application Start Date only if changed
            if (courseUpdateDto.StudentApplicationStartDate.HasValue &&
                courseUpdateDto.StudentApplicationStartDate.Value.Date != existingCourse.StudentApplicationStartDate.Date &&
                courseUpdateDto.StudentApplicationStartDate.Value.Date < DateTime.Now.Date)
            {
                throw new ArgumentException("Ngày bắt đầu đăng ký khóa sinh phải lớn hơn hoặc bằng ngày hiện tại.");
            }

            // Validate Volunteer Application Start Date only if changed
            if (courseUpdateDto.VolunteerApplicationStartDate.HasValue &&
                courseUpdateDto.VolunteerApplicationStartDate.Value.Date != existingCourse.VolunteerApplicationStartDate.Date &&
                courseUpdateDto.VolunteerApplicationStartDate.Value.Date < DateTime.Now.Date)
            {
                throw new ArgumentException("Ngày bắt đầu đăng ký tình nguyện viên phải lớn hơn hoặc bằng ngày hiện tại.");
            }

            // Validate Student Application End Date only if changed
            if (courseUpdateDto.StudentApplicationEndDate.HasValue &&
                (courseUpdateDto.StudentApplicationStartDate ?? existingCourse.StudentApplicationStartDate) > courseUpdateDto.StudentApplicationEndDate.Value)
            {
                throw new ArgumentException("Ngày kết thúc đăng ký khóa sinh phải lớn hơn ngày bắt đầu.");
            }

            // Validate Volunteer Application End Date only if changed
            if (courseUpdateDto.VolunteerApplicationEndDate.HasValue &&
                (courseUpdateDto.VolunteerApplicationStartDate ?? existingCourse.VolunteerApplicationStartDate) > courseUpdateDto.VolunteerApplicationEndDate.Value)
            {
                throw new ArgumentException("Ngày kết thúc đăng ký tình nguyện viên phải lớn hơn ngày bắt đầu.");
            }

            // Validate Course Start Date after application end dates only if Start Date changed
            if (courseUpdateDto.StartDate.HasValue &&
                (courseUpdateDto.StartDate.Value.Date < (courseUpdateDto.StudentApplicationEndDate ?? existingCourse.StudentApplicationEndDate).Date ||
                 courseUpdateDto.StartDate.Value.Date < (courseUpdateDto.VolunteerApplicationEndDate ?? existingCourse.VolunteerApplicationEndDate).Date))
            {
                throw new ArgumentException("Ngày bắt đầu khóa tu phải sau ngày kết thúc tuyển sinh.");
            }

            // Validate Course End Date after Start Date only if End Date changed
            if (courseUpdateDto.EndDate.HasValue &&
                updatedStartDate.Date > courseUpdateDto.EndDate.Value.Date)
            {
                throw new ArgumentException("Ngày kết thúc khóa tu phải lớn hơn ngày bắt đầu.");
            }

            // Update fields if provided and different
            if (!string.IsNullOrEmpty(courseUpdateDto.CourseName) && courseUpdateDto.CourseName != existingCourse.CourseName)
                existingCourse.CourseName = courseUpdateDto.CourseName;

            if (courseUpdateDto.StartDate.HasValue && courseUpdateDto.StartDate.Value.Date != existingCourse.StartDate.Date)
                existingCourse.StartDate = courseUpdateDto.StartDate.Value;

            if (courseUpdateDto.EndDate.HasValue && courseUpdateDto.EndDate.Value.Date != existingCourse.EndDate.Date)
                existingCourse.EndDate = courseUpdateDto.EndDate.Value;

            if (courseUpdateDto.Status.HasValue && courseUpdateDto.Status.Value != existingCourse.Status)
            {
                if (existingCourse.Status == CourseStatus.notStarted)
                {
                    if (courseUpdateDto.Status.Value == CourseStatus.recruiting)
                    {
                        var studentGroup = await _unitOfWork.StudentGroup.FindAsync(s => s.CourseId == courseId);
                        var team = await _unitOfWork.Team.FindAsync(t => t.CourseId == courseId);
                        if (!studentGroup.Any() && !team.Any())
                        {
                            throw new ArgumentException("Không thể tuyển sinh khi chưa tạo ban hoặc chánh");
                        }
                        existingCourse.StudentApplicationStartDate = DateTime.Now;
                        existingCourse.VolunteerApplicationStartDate = DateTime.Now;
                    }
                    else if (courseUpdateDto.Status.Value == CourseStatus.closed)
                    {
                        existingCourse.Status = courseUpdateDto.Status.Value;
                        existingCourse.EndDate = DateTime.Now;
                    }
                }
                else if (existingCourse.Status == CourseStatus.recruiting)
                {
                    if (courseUpdateDto.Status.Value == CourseStatus.inProgress)
                    {
                        if (existingCourse.StudentApplicationEndDate > DateTime.Now)
                        {
                            existingCourse.StudentApplicationEndDate = DateTime.Now;
                        }
                        if (existingCourse.VolunteerApplicationEndDate > DateTime.Now)
                        {
                            existingCourse.VolunteerApplicationEndDate = DateTime.Now;
                        }
                        existingCourse.StartDate = DateTime.Now;
                    }
                    else if (courseUpdateDto.Status.Value == CourseStatus.closed)
                    {
                        existingCourse.Status = courseUpdateDto.Status.Value;
                        existingCourse.EndDate = DateTime.Now;
                    }
                }
                else if (existingCourse.Status == CourseStatus.inProgress)
                {
                    if (courseUpdateDto.Status.Value == CourseStatus.closed)
                    {
                        existingCourse.Status = courseUpdateDto.Status.Value;
                        existingCourse.EndDate = DateTime.Now;
                    }
                }
                existingCourse.Status = courseUpdateDto.Status.Value;
            }


            if (courseUpdateDto.ExpectedStudents.HasValue && courseUpdateDto.ExpectedStudents.Value != existingCourse.ExpectedStudents)
                existingCourse.ExpectedStudents = courseUpdateDto.ExpectedStudents.Value;

            if (!string.IsNullOrEmpty(courseUpdateDto.Description) && courseUpdateDto.Description != existingCourse.Description)
                existingCourse.Description = courseUpdateDto.Description;

            if (courseUpdateDto.StudentApplicationStartDate.HasValue && courseUpdateDto.StudentApplicationStartDate.Value.Date != existingCourse.StudentApplicationStartDate.Date)
                existingCourse.StudentApplicationStartDate = courseUpdateDto.StudentApplicationStartDate.Value;

            if (courseUpdateDto.StudentApplicationEndDate.HasValue && courseUpdateDto.StudentApplicationEndDate.Value.Date != existingCourse.StudentApplicationEndDate.Date)
                existingCourse.StudentApplicationEndDate = courseUpdateDto.StudentApplicationEndDate.Value;

            if (courseUpdateDto.VolunteerApplicationStartDate.HasValue && courseUpdateDto.VolunteerApplicationStartDate.Value.Date != existingCourse.VolunteerApplicationStartDate.Date)
                existingCourse.VolunteerApplicationStartDate = courseUpdateDto.VolunteerApplicationStartDate.Value;

            if (courseUpdateDto.VolunteerApplicationEndDate.HasValue && courseUpdateDto.VolunteerApplicationEndDate.Value.Date != existingCourse.VolunteerApplicationEndDate.Date)
                existingCourse.VolunteerApplicationEndDate = courseUpdateDto.VolunteerApplicationEndDate.Value;

            if (courseUpdateDto.FreeTimeApplicationStartDate.HasValue)
                existingCourse.FreeTimeApplicationStartDate = courseUpdateDto.FreeTimeApplicationStartDate.Value;

            if (courseUpdateDto.FreeTimeApplicationEndDate.HasValue)
                existingCourse.FreeTimeApplicationEndDate = courseUpdateDto.FreeTimeApplicationEndDate.Value;

            await _unitOfWork.Course.UpdateAsync(existingCourse);
            await _unitOfWork.SaveChangeAsync();
        }


        public async Task<DashboardDto> GetCourseDashboardDataAsync(int courseId)
        {
            var dashboard = new DashboardDto();

            // Retrieve the course with necessary related entities
            var course = (await _unitOfWork.Course.FindAsync(
                c => c.Id == courseId && c.Status != CourseStatus.deleted,
                includeProperties: "StudentGroup,StudentCourses,StudentCourses.Student,Feedback,NightShift,Room"))
                .FirstOrDefault();

            if (course == null)
            {
                // Return a meaningful error response or handle as per your application's error handling strategy
                return null;
            }

            // Calculate total students
            var studentCount = course.StudentCourses?.Count(sc =>
                sc.Status == ProgressStatus.Approved ||
                sc.Status == ProgressStatus.Enrolled ||
                sc.Status == ProgressStatus.Graduated ||
                sc.Status == ProgressStatus.DropOut) ?? 0;
            dashboard.TotalStudents = studentCount;

            // Calculate total male students
            dashboard.TotalMaleStudents = course.StudentCourses?.Count(sc =>
                sc.Student.Gender == Gender.Male &&
                (sc.Status == ProgressStatus.Approved ||
                 sc.Status == ProgressStatus.Enrolled ||
                 sc.Status == ProgressStatus.Graduated ||
                 sc.Status == ProgressStatus.DropOut)) ?? 0;

            // Calculate total registered students
            dashboard.TotalRegisteredStudents = course.StudentCourses?.Count() ?? 0;

            // Calculate attendance rate safely
            var totalReports = await _unitOfWork.Report.FindAsync(
                r => r.CourseId == courseId,
                includeProperties: "StudentReports");
            var totalStudentPresent = totalReports?.Sum(report =>
                report.StudentReports?.Count(sr => sr.Status == StudentReportStatus.Present) ?? 0) ?? 0;
            var totalStudentReport = totalReports?.Sum(report =>
                report.StudentReports?.Count() ?? 0) ?? 0;

            if (studentCount > 0 && totalStudentReport > 0)
            {
                dashboard.AttendanceRate = ((double)totalStudentPresent / totalStudentReport) * 100;
            }
            else
            {
                dashboard.AttendanceRate = 0;
            }

            // Calculate graduation rate safely
            var totalGraduatedStudents = course.StudentCourses?.Count(sc => sc.Status == ProgressStatus.Graduated) ?? 0;
            if (studentCount > 0)
            {
                dashboard.GraduationRate = ((double)totalGraduatedStudents / studentCount) * 100;
            }
            else
            {
                dashboard.GraduationRate = 0;
            }

            // Calculate total feedback
            dashboard.TotalFeedbacks = await _unitOfWork.Feedback.CountAsync(f => f.CourseId == courseId);

            // Calculate total volunteers
            dashboard.TotalVolunteers = await _unitOfWork.VolunteerCourse.CountAsync(v =>
                v.CourseId == courseId &&
                (v.Status == ProgressStatus.Enrolled ||
                 v.Status == ProgressStatus.Approved ||
                 v.Status == ProgressStatus.Graduated ||
                 v.Status == ProgressStatus.DropOut));

            // Calculate total male volunteers
            dashboard.TotalMaleVolunteers = await _unitOfWork.VolunteerCourse.CountAsync(v =>
                v.CourseId == courseId &&
                v.Volunteer.Gender == Gender.Male &&
                (v.Status == ProgressStatus.Enrolled ||
                 v.Status == ProgressStatus.Approved ||
                 v.Status == ProgressStatus.Graduated ||
                 v.Status == ProgressStatus.DropOut));

            // Calculate total registered volunteers
            dashboard.TotalRegisteredVolunteers = await _unitOfWork.VolunteerApplication.CountAsync(va => va.CourseId == courseId);



            return dashboard;
        }

        public async Task<List<RegistrationOverTimeDto>> GetStudentRegistrationsOverTimeAsync(int years)
        {
            DateTime fromDate = DateTime.Now.AddYears(-years);
            var registrations = await _unitOfWork.StudentCourse
                .FindAsync(sc => sc.ApplicationDate >= fromDate && sc.ApplicationDate <= DateTime.Now);

            var grouped = registrations
                .GroupBy(sc => new { sc.ApplicationDate.Value.Year, sc.ApplicationDate.Value.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new RegistrationOverTimeDto
                {
                    Period = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Count = g.Count()
                })
                .ToList();

            return grouped;
        }

        public async Task<List<RegistrationOverTimeDto>> GetVolunteerRegistrationsOverTimeAsync(int years)
        {
            DateTime fromDate = DateTime.Now.AddYears(-years);
            var registrations = await _unitOfWork.VolunteerCourse
                .FindAsync(vc => vc.ApplicationDate >= fromDate && vc.ApplicationDate <= DateTime.Now);

            var grouped = registrations
                .GroupBy(vc => new { vc.ApplicationDate.Year, vc.ApplicationDate.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new RegistrationOverTimeDto
                {
                    Period = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Count = g.Count()
                })
                .ToList();

            return grouped;
        }

        public async Task<List<RegistrationPerCourseDto>> GetStudentRegistrationsPerCourseAsync(int years)
        {
            DateTime fromDate = DateTime.Now.AddYears(-years);
            var registrations = await _unitOfWork.StudentCourse.FindAsync(sc => sc.ApplicationDate >= fromDate && sc.ApplicationDate <= DateTime.Now, includeProperties: "Course");

            var grouped = registrations
                .GroupBy(sc => new { sc.CourseId, sc.Course.CourseName })
                .Select(g => new RegistrationPerCourseDto
                {
                    CourseId = g.Key.CourseId,
                    CourseName = g.Key.CourseName,
                    RegistrationCount = g.Count()
                })
                .ToList();

            return grouped;
        }

        public async Task<List<RegistrationPerCourseDto>> GetVolunteerRegistrationsPerCourseAsync(int years)
        {
            DateTime fromDate = DateTime.Now.AddYears(-years);
            var registrations = await _unitOfWork.VolunteerCourse.FindAsync(vc => vc.ApplicationDate >= fromDate && vc.ApplicationDate <= DateTime.Now, includeProperties: "Course");

            var grouped = registrations
                .GroupBy(vc => new { vc.CourseId, vc.Course.CourseName })
                .Select(g => new RegistrationPerCourseDto
                {
                    CourseId = g.Key.CourseId,
                    CourseName = g.Key.CourseName,
                    RegistrationCount = g.Count()
                })
                .ToList();

            return grouped;
        }

    }
}
