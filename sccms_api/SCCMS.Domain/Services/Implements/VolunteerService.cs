using AutoMapper;
using SCCMS.Domain.DTOs.VolunteerDtos;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.UnitOfWork;
using Utility;
using ClosedXML.Excel;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Net.Http;
using System.Text;
using SCCMS.Domain.DTOs.StudentDtos;
using SCCMS.Domain.DTOs.TeamDtos;
using System.Linq.Expressions;
using SCCMS.API.Services;

namespace SCCMS.Domain.Services.Implements
{
    public class VolunteerService : IVolunteerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IBlobService _blobService;
        private readonly HttpClient _httpClient;
        private readonly IEmailService _emailService;

        public VolunteerService(IUnitOfWork unitOfWork, IMapper mapper, IBlobService blobService, HttpClient httpClient, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _blobService = blobService;
            _httpClient = httpClient;
            _emailService = emailService;
        }
        public async Task<IEnumerable<VolunteerDto>> GetAllVolunteersAsync(
            string? fullName = null,
            Gender? gender = null,
            ProfileStatus? status = null,
            int? teamId = null,
            int? courseId = null,
            string? nationalId = null,
            string? address = null
        )
        {
            var volunteers = await _unitOfWork.Volunteer.FindAsync(
                v =>
                    (string.IsNullOrEmpty(fullName) || v.FullName.Contains(fullName)) &&
                    (gender == null || v.Gender.Equals(gender)) &&
                    (status == null || v.Status.Equals(status)) &&
                    (teamId == null || v.VolunteerTeam.Any(vt => vt.TeamId == teamId)) &&
                    (courseId == null || v.VolunteerCourse.Any(vc => vc.CourseId == courseId)) &&
                    (string.IsNullOrEmpty(nationalId) || v.NationalId.Contains(nationalId)) &&
                    (string.IsNullOrEmpty(address) || v.Address.Contains(address)),
                includeProperties: "VolunteerTeam.Team,VolunteerCourse.Course"
            );

			if (volunteers == null || !volunteers.Any())
			{
				return new List<VolunteerDto>();
			}
			var volunteerDtos = volunteers.Select(v => new VolunteerDto
			{
				Id = v.Id,
				FullName = v.FullName,
				DateOfBirth = v.DateOfBirth,
				Gender = v.Gender,
				NationalId = v.NationalId,
				NationalImageFront = v.NationalImageFront,
				NationalImageBack = v.NationalImageBack,
				Address = v.Address,
				Status = v.Status,
				Note = v.Note,
				Image = v.Image,
				PhoneNumber = v.PhoneNumber, // Đảm bảo có giá trị này

				Teams = v.VolunteerTeam?.Select(vt => new TeamInfoDto
				{
					TeamId = vt.TeamId,
					TeamName = vt.Team.TeamName
				}).ToList(),

				Courses = v.VolunteerCourse?.Select(vc => new CourseInfoDto
				{
					CourseId = vc.CourseId,
					CourseName = vc.Course.CourseName
				}).ToList()
			}).ToList();

            return volunteerDtos;
        }

        public async Task<VolunteerDto> GetVolunteerByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Id không hợp lệ, phải lớn hơn 0.");
            }

            var volunteer = await _unitOfWork.Volunteer
                .GetAsync(v => v.Id == id, includeProperties: "VolunteerTeam.Team,VolunteerCourse.Course");

            if (volunteer == null)
            {
                throw new ArgumentException($"Không tìm thấy tình nguyện viên với ID {id}.");
            }

            var volunteerDto = _mapper.Map<VolunteerDto>(volunteer);

            return volunteerDto;
        }

        public async Task UpdateVolunteerAsync(int volunteerId, VolunteerUpdateDto volunteerUpdateDto)
        {
            var existingVolunteer = await _unitOfWork.Volunteer.GetByIdAsync(volunteerId);
            if (existingVolunteer == null)
            {
                throw new ArgumentException("Tình nguyện viên không tồn tại.");
            }

            // Kiểm tra xem email có trùng với tình nguyện viên khác hay không
            var volunteerWithSameEmail = await _unitOfWork.Volunteer.GetAsync(v => v.Email == volunteerUpdateDto.Email && v.Id != volunteerId);
            if (volunteerWithSameEmail != null)
            {
                throw new ArgumentException("Email đã tồn tại.");
            }

            // Kiểm tra xem số điện thoại có trùng với tình nguyện viên khác hay không
            var volunteerWithSamePhoneNumber = await _unitOfWork.Volunteer.GetAsync(v => v.PhoneNumber == volunteerUpdateDto.PhoneNumber && v.Id != volunteerId);
            if (volunteerWithSamePhoneNumber != null)
            {
                throw new ArgumentException("Số điện thoại đã tồn tại.");
            }

            // Kiểm tra xem National ID có trùng với tình nguyện viên khác hay không
            var volunteerWithSameNationalId = await _unitOfWork.Volunteer.GetAsync(v => v.NationalId == volunteerUpdateDto.NationalId && v.Id != volunteerId);
            if (volunteerWithSameNationalId != null)
            {
                throw new ArgumentException("Mã định danh đã tồn tại.");
            }

            string newImage = existingVolunteer.Image;
            string newNationalImageFront = existingVolunteer.NationalImageFront;
            string newNationalImageBack = existingVolunteer.NationalImageBack;

            // Upload hình ảnh lên cloud nếu có và xóa ảnh cũ
            if (volunteerUpdateDto.Image != null)
            {
                string fileNameImage = $"{Guid.NewGuid()}{Path.GetExtension(volunteerUpdateDto.Image.FileName)}";
                if (!string.IsNullOrEmpty(existingVolunteer.Image))
                {
                    //     await _blobService.DeleteBlob(existingVolunteer.Image.Split('/').Last(), SD.Storage_Container);
                }
                newImage = await _blobService.UploadBlob(fileNameImage, SD.Storage_Container, volunteerUpdateDto.Image);
            }

            if (volunteerUpdateDto.NationalImageFront != null)
            {
                string fileNameNationalImageFront = $"{Guid.NewGuid()}{Path.GetExtension(volunteerUpdateDto.NationalImageFront.FileName)}";
                if (!string.IsNullOrEmpty(existingVolunteer.NationalImageFront))
                {
                    //    await _blobService.DeleteBlob(existingVolunteer.NationalImageFront.Split('/').Last(), SD.Storage_Container);
                }
                newNationalImageFront = await _blobService.UploadBlob(fileNameNationalImageFront, SD.Storage_Container, volunteerUpdateDto.NationalImageFront);
            }

            if (volunteerUpdateDto.NationalImageBack != null)
            {
                string fileNameNationalImageBack = $"{Guid.NewGuid()}{Path.GetExtension(volunteerUpdateDto.NationalImageBack.FileName)}";
                if (!string.IsNullOrEmpty(existingVolunteer.NationalImageBack))
                {
                    //    await _blobService.DeleteBlob(existingVolunteer.NationalImageBack.Split('/').Last(), SD.Storage_Container);
                }
                newNationalImageBack = await _blobService.UploadBlob(fileNameNationalImageBack, SD.Storage_Container, volunteerUpdateDto.NationalImageBack);
            }

            // Ánh xạ dữ liệu từ DTO sang entity
            _mapper.Map(volunteerUpdateDto, existingVolunteer);
            existingVolunteer.Image = newImage;
            existingVolunteer.NationalImageFront = newNationalImageFront;
            existingVolunteer.NationalImageBack = newNationalImageBack;

            // Cập nhật thông tin tình nguyện viên
            await _unitOfWork.Volunteer.UpdateAsync(existingVolunteer);
            await _unitOfWork.SaveChangeAsync();
        }


        public async Task CreateVolunteerAsync(VolunteerCreateDto volunteerCreateDto, int courseId)
        {
            if (volunteerCreateDto.DateOfBirth >= DateTime.Now)
            {
                throw new ArgumentException("Ngày sinh phải là thời gian trong quá khứ.");
            }

            if (string.IsNullOrWhiteSpace(volunteerCreateDto.FullName) || volunteerCreateDto.FullName.Length > 50)
            {
                throw new ArgumentException("Họ và tên là bắt buộc và không được vượt quá 50 ký tự.");
            }

            if (string.IsNullOrWhiteSpace(volunteerCreateDto.Email))
            {
                throw new ArgumentException("Email là bắt buộc.");
            }
            else
            {
                try
                {
                    var addr = new System.Net.Mail.MailAddress(volunteerCreateDto.Email);
                    if (addr.Address != volunteerCreateDto.Email)
                    {
                        throw new ArgumentException("Định dạng email không hợp lệ.");
                    }
                }
                catch
                {
                    throw new ArgumentException("Định dạng email không hợp lệ.");
                }
            }

            if (string.IsNullOrWhiteSpace(volunteerCreateDto.PhoneNumber))
            {
                throw new ArgumentException("Số điện thoại là bắt buộc.");
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(volunteerCreateDto.PhoneNumber, @"^\d{10}$"))
            {
                throw new ArgumentException("Số điện thoại phải có đúng 10 chữ số.");
            }

            var volunteer = _mapper.Map<Volunteer>(volunteerCreateDto);

            if (volunteerCreateDto.Image != null)
            {
                string fileNameImage = $"{Guid.NewGuid()}{Path.GetExtension(volunteerCreateDto.Image.FileName)}";
                volunteer.Image = await _blobService.UploadBlob(fileNameImage, SD.Storage_Container, volunteerCreateDto.Image);
            }

            if (volunteerCreateDto.NationalImageFront != null)
            {
                string fileNameNationalImageFront = $"{Guid.NewGuid()}{Path.GetExtension(volunteerCreateDto.NationalImageFront.FileName)}";
                volunteer.NationalImageFront = await _blobService.UploadBlob(fileNameNationalImageFront, SD.Storage_Container, volunteerCreateDto.NationalImageFront);
            }

            if (volunteerCreateDto.NationalImageBack != null)
            {
                string fileNameNationalImageBack = $"{Guid.NewGuid()}{Path.GetExtension(volunteerCreateDto.NationalImageBack.FileName)}";
                volunteer.NationalImageBack = await _blobService.UploadBlob(fileNameNationalImageBack, SD.Storage_Container, volunteerCreateDto.NationalImageBack);
            }

            await _unitOfWork.Volunteer.AddAsync(volunteer);
            await _unitOfWork.SaveChangeAsync();

            var newEnrollment = new VolunteerCourse
            {
                VolunteerId = volunteer.Id,
                CourseId = courseId,
                ApplicationDate = DateTime.Now,
                Status = ProgressStatus.Pending,
            };

            await _unitOfWork.VolunteerApplication.AddAsync(newEnrollment);
            await _unitOfWork.SaveChangeAsync();

        }

        public async Task<IEnumerable<VolunteerDto>> GetVolunteersByCourseIdAsync(int courseId)
        {
            if (courseId <= 0)
            {
                throw new ArgumentException("courseId phải lớn hơn 0.", nameof(courseId));
            }
            var volunteerCourses = await _unitOfWork.VolunteerApplication.FindAsync(
                vc => vc.CourseId == courseId,
                includeProperties: "Volunteer.VolunteerTeam.Team,Course"
            );

            if (volunteerCourses == null || !volunteerCourses.Any())
            {
                return new List<VolunteerDto>();
            }

            var volunteerDtos = _mapper.Map<IEnumerable<VolunteerDto>>(volunteerCourses);
            return volunteerDtos;
        }
        public async Task<byte[]> ExportVolunteersByCourseAsync(int courseId)
        {
            // Lấy danh sách tình nguyện viên của khóa học có trạng thái Approved
            var volunteerCourses = await _unitOfWork.VolunteerApplication.FindAsync(
                vc => vc.CourseId == courseId && vc.Status == ProgressStatus.Approved,
                includeProperties: "Volunteer,Volunteer.VolunteerTeam.Team"
            );

            if (volunteerCourses == null || !volunteerCourses.Any())
            {
                return null;
            }

            var course = await _unitOfWork.Course.GetByIdAsync(courseId);
            if (course == null)
            {
                throw new ArgumentException("Không tìm thấy khóa học.");
            }

            var templatePath = Path.Combine(AppContext.BaseDirectory, "TemplateExcel", "VolunteerList.xlsx");
            templatePath = Path.GetFullPath(templatePath);

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Không tìm thấy file template tại đường dẫn: {templatePath}");
            }

            using (var workbook = new XLWorkbook(templatePath))
            {
                var worksheet = workbook.Worksheet(1);

                // Cập nhật tiêu đề khóa học
                string courseName = $"KHOÁ TU CHÙA CỔ LOAN";
                worksheet.Cell("A1").Value = courseName;
                string courseTime = $"Danh sách tình nguyện viên\n Thời gian: {course.StartDate:dd/MM/yyyy} - {course.EndDate:dd/MM/yyyy}";
                worksheet.Cell("A3").Value = courseTime;

                int row = 6;
                int serialNumber = 1;
                worksheet.Cell(5, 7).Value = "Ban";
                var sampleRow = worksheet.Row(6);

                foreach (var volunteerCourse in volunteerCourses)
                {
                    var volunteer = volunteerCourse.Volunteer;
                    var teamName = volunteer.VolunteerTeam?
                        .Where(vt => vt.Team.CourseId == courseId)
                        .Select(vt => vt.Team.TeamName)
                        .FirstOrDefault() ?? "N/A"; // Nếu không có team phù hợp, trả về "N/A"

                    worksheet.Cell(row, 1).Value = serialNumber;
                    worksheet.Cell(row, 2).Value = volunteerCourse.VolunteerCode;
                    worksheet.Cell(row, 3).Value = volunteer.FullName;
                    worksheet.Cell(row, 4).Value = volunteer.PhoneNumber ?? "N/A";
                    worksheet.Cell(row, 5).Value = volunteer.DateOfBirth?.ToString("dd/MM/yyyy") ?? "N/A";
                    worksheet.Cell(row, 6).Value = volunteer.Gender.GetDisplayGender();
                    worksheet.Cell(row, 7).Value = teamName;
                    worksheet.Cell(row, 8).Value = volunteer.Address ?? "N/A";  // Địa chỉ
                    worksheet.Cell(row, 9).Value = volunteer.Email ?? "N/A";    // Email
                    worksheet.Cell(row, 10).Value = volunteer.NationalId ?? "N/A"; // CCCD
                    worksheet.Row(row).Style = sampleRow.Style;

                    serialNumber++;
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
        public async Task<byte[]> ExportVolunteersByTeamAsync(int teamId)
        {
            // Lấy danh sách tình nguyện viên thuộc team cụ thể
            var volunteerTeams = await _unitOfWork.VolunteerTeam.FindAsync(
                vt => vt.TeamId == teamId,
                includeProperties: "Volunteer,Volunteer.VolunteerCourse,Team"
            );

            if (volunteerTeams == null || !volunteerTeams.Any())
            {
                return null;
            }

            var team = await _unitOfWork.Team.GetByIdAsync(teamId);
            if (team == null)
            {
                throw new ArgumentException("Không tìm thấy đội ngũ.");
            }

            //var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "SCCMS.Infrastucture", "TemplateExcel", "VolunteerList.xlsx");
            var templatePath = Path.Combine(AppContext.BaseDirectory, "TemplateExcel", "VolunteerList.xlsx");

            templatePath = Path.GetFullPath(templatePath);

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Không tìm thấy file template tại đường dẫn: {templatePath}");
            }

            using (var workbook = new XLWorkbook(templatePath))
            {
                var worksheet = workbook.Worksheet(1);

                // Cập nhật tiêu đề khóa học
                string courseName = $"KHOÁ TU CHÙA CỔ LOAN";
                worksheet.Cell("A1").Value = courseName;
                string courseTime = $"Danh sách tình nguyện viên theo ban\n";
                worksheet.Cell("A3").Value = courseTime;

                int row = 6;
                int serialNumber = 1;
                worksheet.Cell(5, 7).Value = "Ban";
                var sampleRow = worksheet.Row(6);

                foreach (var volunteerTeam in volunteerTeams)
                {
                    var volunteer = volunteerTeam.Volunteer;
                    var volunteerCode = volunteer.VolunteerCourse?
                        .FirstOrDefault(vc => vc.CourseId == volunteerTeam.Team.CourseId)?.VolunteerCode ?? "N/A";

                    worksheet.Cell(row, 1).Value = serialNumber;
                    worksheet.Cell(row, 2).Value = volunteerCode;
                    worksheet.Cell(row, 3).Value = volunteer.FullName;
                    worksheet.Cell(row, 4).Value = volunteer.PhoneNumber ?? "N/A";
                    worksheet.Cell(row, 5).Value = volunteer.DateOfBirth?.ToString("dd/MM/yyyy") ?? "N/A";
                    worksheet.Cell(row, 6).Value = volunteer.Gender.GetDisplayGender();
                    worksheet.Cell(row, 7).Value = volunteerTeam.Team.TeamName ?? "N/A";
                    worksheet.Cell(row, 8).Value = volunteer.Address ?? "N/A";
                    worksheet.Cell(row, 9).Value = volunteer.Email ?? "N/A";
                    worksheet.Cell(row, 10).Value = volunteer.NationalId ?? "N/A";
                    worksheet.Row(row).Style = sampleRow.Style;

                    serialNumber++;
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
        public async Task SendEmailsToVolunteersAsync(int? courseId, string subject, string templateName, Dictionary<string, string> additionalParameters = null)
        {
            IEnumerable<Volunteer> volunteers;

            if (courseId.HasValue)
            {
                var volunteerCourses = await _unitOfWork.VolunteerApplication.FindAsync(
                    vc => vc.CourseId == courseId.Value,
                    includeProperties: "Volunteer"
                );

                volunteers = volunteerCourses.Select(vc => vc.Volunteer).Distinct();
            }
            else
            {
                volunteers = await _unitOfWork.Volunteer.GetAllAsync();
            }

            if (!volunteers.Any())
            {
                throw new ArgumentException("Không có tình nguyện viên nào để gửi email.");
            }

            foreach (var volunteer in volunteers)
            {
                var parameters = new Dictionary<string, string>
                {
                    { "FullName", volunteer.FullName },
                    { "Email", volunteer.Email }
                };

                if (additionalParameters != null)
                {
                    foreach (var param in additionalParameters)
                    {
                        parameters[param.Key] = param.Value;
                    }
                }

                await _emailService.SendEmailAsync(volunteer.Email, templateName, parameters);
            }
        }
    }
}
