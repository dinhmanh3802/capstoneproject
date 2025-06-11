using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Configuration;
using SCCMS.API.Services;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Domain.DTOs.EmailDtos;
using SCCMS.Domain.DTOs.StudentDtos;
using log4net;
using Microsoft.Extensions.DependencyInjection;

public class EmailService : IEmailService
{
    private readonly SmtpClient _smtpClient;
    private readonly string _fromEmail;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly ILoggerService<EmailService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    //TODO: truyền thêm VolunteerService

    public EmailService(IEmailTemplateService emailTemplateService, IConfiguration configuration, ILoggerService<EmailService> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _fromEmail = configuration["EmailSettings:FromEmail"];
        var emailPassword = configuration["EmailSettings:Password"];
        _smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(_fromEmail, emailPassword),
            EnableSsl = true
        };
        _emailTemplateService = emailTemplateService;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    //TODO: Template code thay vi ID, chu y chen them anh (OPTINAL)
    public async Task SendEmailAsync(string toEmail, string templateName, Dictionary<string, string> parameters)
    {
        // Lấy template từ database
        var template = await _emailTemplateService.GetTemplateByNameAsync(templateName);
        if (template == null)
        {
            throw new Exception($"Không tìm thấy template với tên: {templateName}");
        }

        // Thay thế các biến trong nội dung email bằng giá trị thực tế
        string subject = template.Subject;
        string body = template.Body;

        foreach (var param in parameters)
        {
            subject = subject.Replace($"{{{{{param.Key}}}}}", param.Value);
            body = body.Replace($"{{{{{param.Key}}}}}", param.Value);
        }

        var mailMessage = new MailMessage(_fromEmail, toEmail, subject, body)
        {
            IsBodyHtml = true
        };

        await _smtpClient.SendMailAsync(mailMessage);
    }

	public void SendBulkEmailAsync(SendBulkEmailRequestDto emailRequest)
	{
		// Chạy background task
		Task.Run(async () =>
		{
			using (var scope = _serviceScopeFactory.CreateScope())
			{
				try
				{
					var courseService = scope.ServiceProvider.GetRequiredService<ICourseService>();
					var studentService = scope.ServiceProvider.GetRequiredService<IStudentService>();
					var studentApplicationService = scope.ServiceProvider.GetRequiredService<IStudentApplicationService>();
					var volunteerCourseService = scope.ServiceProvider.GetRequiredService<IVolunteerCourseService>(); // Thêm dòng này

					// Lấy thông tin khóa học
					var courseDto = await courseService.GetCourseByIdAsync(emailRequest.CourseId);
					if (courseDto == null)
					{
						var errorMsg = $"Không tìm thấy khóa học với ID: {emailRequest.CourseId}";
						throw new Exception(errorMsg);
					}

					// Gửi email cho danh sách sinh viên
					if (emailRequest.ListStudentId != null && emailRequest.ListStudentId.Any())
					{
						foreach (var studentId in emailRequest.ListStudentId)
						{
							var studentApplication = await studentApplicationService.GetByStudentIdAndCourseIdAsync(studentId, emailRequest.CourseId);

							// Tạo dictionary tham số
							var parameters = new Dictionary<string, string>
							{
								{ "ten_nguoi_nhan", studentApplication.Student.FullName },
								{ "ten_khoa_tu", courseDto.CourseName },
								{ "ngay_bat_dau", courseDto.StartDate.ToString("dd/MM/yyyy") },
								{ "ngay_ket_thuc", courseDto.EndDate.ToString("dd/MM/yyyy") },
							};
							var group = studentApplication.Student.StudentGroups.FirstOrDefault();
							if (group != null)
							{
								parameters.Add("ten_chanh", group.GroupName);
							}
							var studentCode = studentApplication.StudentCode;
							if (studentCode != null)
							{
								parameters.Add("ma_khoa_sinh", studentCode);
							}
							var node = studentApplication.Note;
							if (node != null)
							{
								parameters.Add("ly_do_tu_choi", node);
							}


							// Gửi email
							await SendEmail(studentApplication.Student.Email, emailRequest.Subject, emailRequest.Message, parameters);

						}
					}

					// Gửi email cho tình nguyện viên
					else if (emailRequest.ListVolunteerId != null && emailRequest.ListVolunteerId.Any())
					{
						foreach (var volunteerId in emailRequest.ListVolunteerId)
						{
							var volunteerApplication = await volunteerCourseService.GetByVolunteerIdAndCourseIdAsync(volunteerId, emailRequest.CourseId);

							// Tạo dictionary tham số
							var parameters = new Dictionary<string, string>
							{
								{ "ten_nguoi_nhan", volunteerApplication.Volunteer.FullName },
								{ "ten_khoa_tu", courseDto.CourseName },
								{ "ngay_bat_dau", courseDto.StartDate.ToString("dd/MM/yyyy") },
								{ "ngay_ket_thuc", courseDto.EndDate.ToString("dd/MM/yyyy") },
							};
							var team = volunteerApplication.Volunteer.Teams.FirstOrDefault();
							if (team != null)
							{
								parameters.Add("ban", team.TeamName);
							}
							var volunteerCode = volunteerApplication.VolunteerCode;
							if (volunteerCode != null)
							{
								parameters.Add("ma_tnv", volunteerCode);
							}
							var node = volunteerApplication.Note;
							if (node != null)
							{
								parameters.Add("ly_do_tu_choi", node);
							}


							// Gửi email
							await SendEmail(volunteerApplication.Volunteer.Email, emailRequest.Subject, emailRequest.Message, parameters);

						}
					}
					else
					{
						_logger.LogError("Danh sách sinh viên và tình nguyện viên đều rỗng.");
					}
				}
				catch (Exception ex)
				{
					_logger.LogError($"Lỗi nghiêm trọng trong quá trình gửi email hàng loạt: {ex.Message}");
				}
			}
		});
	}

	public async Task SendEmail(string toEmail, string emailSubject, string emailContent, Dictionary<string, string> parameters)
    {
        try
        {
            string subject = emailSubject;
            string body = emailContent;

            // Thay thế các tham số trong tiêu đề và nội dung email
            foreach (var param in parameters)
            {
                subject = subject.Replace($"{{{{{param.Key}}}}}", param.Value);
                body = body.Replace($"{{{{{param.Key}}}}}", param.Value);
            }

            var mailMessage = new MailMessage(_fromEmail, toEmail, subject, body)
            {
                IsBodyHtml = true
            };

            await _smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception smtpEx)
        {
            _logger.LogError($"Lỗi SMTP khi gửi email đến {toEmail}");
            throw new Exception($"Lỗi SMTP khi gửi email đến {toEmail}");
        }
    }
}
