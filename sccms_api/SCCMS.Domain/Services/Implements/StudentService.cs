using AutoMapper;
using SCCMS.Domain.DTOs.StudentDtos;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.UnitOfWork;
using Utility;
using ClosedXML.Excel;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Net.Http;
using System.Text;
using System.Linq.Expressions;
using DocumentFormat.OpenXml.Office2010.Excel;
using SCCMS.Domain.DTOs.CourseDtos;
using Microsoft.AspNetCore.Cors.Infrastructure;
using DocumentFormat.OpenXml.Spreadsheet;
using SCCMS.Domain.DTOs.StudentCourseDtos;


namespace SCCMS.Domain.Services.Implements
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IBlobService _blobService;
        private readonly HttpClient _httpClient;
        private readonly ICourseService _courseService;
        private readonly IStudentGroupAssignmentService _studentGroupAssignmentService;


        public StudentService(IUnitOfWork unitOfWork, IMapper mapper, IBlobService blobService, HttpClient httpClient, ICourseService courseService, IStudentGroupAssignmentService studentGroupAssignmentService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _blobService = blobService;
            _httpClient = httpClient;
            _courseService = courseService;
            _studentGroupAssignmentService = studentGroupAssignmentService;
        }

        public async Task<IEnumerable<StudentDto>> GetAllStudentsAsync(
            string? fullName = null,
            string? email = null,
            Gender? genders = null,
            ProfileStatus? status = null,
            int? courseId = null,
            int? studentGroupId = null,
            string? parentName = null,
            string? emergencyContact = null,
            string? studentCode = null,
            DateTime? dateOfBirth = null
)
        {
            // Thực hiện truy vấn với FindAsync và điều kiện lọc
            var students = await _unitOfWork.Student.FindAsync(
                s =>
                    (string.IsNullOrEmpty(fullName) || s.FullName.Contains(fullName)) &&
                    (string.IsNullOrEmpty(email) || s.Email.Contains(email)) &&
                    (genders == null || s.Gender.Equals(genders)) &&
                    (status == null || s.Status.Equals(status)) &&
                    (courseId == null || s.StudentCourses.Any(sc => sc.CourseId == courseId)) &&
                    (studentGroupId == null || s.StudentGroupAssignment.Any(sga => sga.StudentGroupId == studentGroupId)) &&
                    (string.IsNullOrEmpty(parentName) || s.ParentName.Contains(parentName)) &&
                    (string.IsNullOrEmpty(emergencyContact) || s.EmergencyContact.Contains(emergencyContact)) &&
                    (string.IsNullOrEmpty(studentCode) || s.StudentCourses.Any(sc => sc.StudentCode != null && sc.StudentCode == studentCode)) &&
                    (!dateOfBirth.HasValue || s.DateOfBirth == dateOfBirth)
                    && s.Status != ProfileStatus.Delete &&
                    !s.StudentCourses.Any(sc => sc.Status == ProgressStatus.Rejected),
                includeProperties: "StudentCourses.Course,StudentGroupAssignment.StudentGroup"
            );

            // Sắp xếp theo ID giảm dần
            var sortedStudents = students.OrderByDescending(s => s.Id);

            // Ánh xạ kết quả thành StudentDto
            var studentDtos = sortedStudents.Select(s => new StudentDto
            {
                Id = s.Id,
                FullName = s.FullName,
                DateOfBirth = s.DateOfBirth,
                Gender = s.Gender,
                Image = s.Image,
                NationalId = s.NationalId,
                NationalImageFront = s.NationalImageFront,
                NationalImageBack = s.NationalImageBack,
                Address = s.Address,
                ParentName = s.ParentName,
                EmergencyContact = s.EmergencyContact,
                Email = s.Email,
                Conduct = s.Conduct,
                AcademicPerformance = s.AcademicPerformance,
                Status = s.Status,
                Note = s.Note,
                Courses = s.StudentCourses.Select(sc => new CourseInfoDto
                {
                    CourseId = sc.CourseId,
                    CourseName = sc.Course.CourseName
                }).ToList(),
                Groups = s.StudentGroupAssignment.Select(sga => new GroupInfoDto
                {
                    GroupId = sga.StudentGroupId,
                    GroupName = sga.StudentGroup.GroupName
                }).ToList()
            });

            return studentDtos;
        }


        public async Task<StudentDto?> GetStudentByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Id must be greater than 0.", nameof(id));
            }

            // Predicate để lọc sinh viên theo id
            Expression<Func<Student, bool>> predicate = s => s.Id == id && s.Status != ProfileStatus.Delete;

            // Bao gồm các thực thể liên quan
            var students = await _unitOfWork.Student.FindAsync(
                predicate,
                includeProperties: "StudentCourses.Course,StudentGroupAssignment.StudentGroup"
            );

            // Kiểm tra xem sinh viên có tồn tại không
            if (students == null || !students.Any())
            {
                return null;
            }

            var student = students.FirstOrDefault();

            if (student == null)
            {
                return null;
            }

            // Thực hiện map thủ công
            var studentDto = new StudentDto
            {
                Id = student.Id,
                FullName = student.FullName,
                DateOfBirth = student.DateOfBirth,
                Gender = student.Gender,
                Image = student.Image,
                NationalId = student.NationalId,
                NationalImageFront = student.NationalImageFront,
                NationalImageBack = student.NationalImageBack,
                Address = student.Address,
                ParentName = student.ParentName,
                EmergencyContact = student.EmergencyContact,
                Email = student.Email,
                Conduct = student.Conduct,
                AcademicPerformance = student.AcademicPerformance,
                Status = student.Status,
                Note = student.Note,
                Courses = student.StudentCourses.Select(sc => new CourseInfoDto
                {
                    CourseId = sc.CourseId,
                    CourseName = sc.Course.CourseName
                }).ToList(),
                Groups = student.StudentGroupAssignment.Select(sga => new GroupInfoDto
                {
                    GroupId = sga.StudentGroupId,
                    GroupName = sga.StudentGroup.GroupName
                }).ToList()
            };

            return studentDto;
        }



        public async Task CreateStudentAsync(StudentCreateDto studentCreateDto, int courseId)
        {
            // **Bước 1: Xác thực các thuộc tính của sinh viên**

            // Kiểm tra Ngày sinh
            if (studentCreateDto.DateOfBirth >= DateTime.Now)
            {
                throw new ArgumentException("Date of birth must be in the past.");
            }

            // Kiểm tra Họ tên
            if (string.IsNullOrWhiteSpace(studentCreateDto.FullName) || studentCreateDto.FullName.Length > 50)
            {
                throw new ArgumentException("FullName is required and must not exceed 50 characters.");
            }
			if (string.IsNullOrWhiteSpace(studentCreateDto.NationalId) || studentCreateDto.NationalId.Length < 9 || studentCreateDto.NationalId.Length > 12 || !studentCreateDto.NationalId.All(char.IsDigit))
			{
				throw new ArgumentException("NationalId is invalid.");
			}
			if (string.IsNullOrWhiteSpace(studentCreateDto.EmergencyContact) || studentCreateDto.EmergencyContact.Length != 10 || !studentCreateDto.EmergencyContact.All(char.IsDigit))
			{
				throw new ArgumentException("EmergencyContact is required and must be a valid phone number with 10 digits.");
			}


			// Kiểm tra Email
			if (string.IsNullOrWhiteSpace(studentCreateDto.Email))
            {
                throw new ArgumentException("Email is required.");
            }
            else
            {
                // Kiểm tra định dạng Email
                try
                {
                    var addr = new System.Net.Mail.MailAddress(studentCreateDto.Email);
                    if (addr.Address != studentCreateDto.Email)
                    {
                        throw new ArgumentException("Invalid email format.");
                    }
                }
                catch
                {
                    throw new ArgumentException("Invalid email format.");
                }
            }


            // Tạo mới sinh viên
            var student = _mapper.Map<Student>(studentCreateDto);

            // Lưu hình ảnh lên cloud
            if (studentCreateDto.Image != null)
            {
                string fileNameImage = $"{Guid.NewGuid()}{Path.GetExtension(studentCreateDto.Image.FileName)}";
                student.Image = await _blobService.UploadBlob(fileNameImage, SD.Storage_Container, studentCreateDto.Image);
            }

            if (studentCreateDto.NationalImageFront != null)
            {
                string fileNameNationalImageFront = $"{Guid.NewGuid()}{Path.GetExtension(studentCreateDto.NationalImageFront.FileName)}";
                student.NationalImageFront = await _blobService.UploadBlob(fileNameNationalImageFront, SD.Storage_Container, studentCreateDto.NationalImageFront);
            }

            if (studentCreateDto.NationalImageBack != null)
            {
                string fileNameNationalImageBack = $"{Guid.NewGuid()}{Path.GetExtension(studentCreateDto.NationalImageBack.FileName)}";
                student.NationalImageBack = await _blobService.UploadBlob(fileNameNationalImageBack, SD.Storage_Container, studentCreateDto.NationalImageBack);
            }

            await _unitOfWork.Student.AddAsync(student);
            await _unitOfWork.SaveChangeAsync(); // Lưu để có được student.Id

            // **Thêm sinh viên vào khóa học**

            var newEnrollment = new StudentCourse
            {
                StudentId = student.Id,
                CourseId = courseId,
                ApplicationDate = DateTime.Now,
                Status = ProgressStatus.Pending,
            };

            await _unitOfWork.StudentCourse.AddAsync(newEnrollment);
            await _unitOfWork.SaveChangeAsync();

        }
        public async Task UpdateStudentAsync(int studentId, StudentUpdateDto studentUpdateDto)
        {
            // Lấy thông tin sinh viên từ cơ sở dữ liệu
            Student existingStudent = await _unitOfWork.Student.GetAsync(s => s.Id == studentId && s.Status != ProfileStatus.Delete);
            if (existingStudent == null)
            {
                throw new ArgumentException("Không tìm thấy sinh viên.");
            }

            // Kiểm tra dữ liệu đầu vào
            if (studentUpdateDto.DateOfBirth >= DateTime.Now)
            {
                throw new ArgumentException("Ngày sinh phải trong quá khứ.");
            }

            if (string.IsNullOrWhiteSpace(studentUpdateDto.FullName) || studentUpdateDto.FullName.Length > 50)
            {
                throw new ArgumentException("Tên đầy đủ là bắt buộc và không được vượt quá 50 ký tự.");
            }

            if (string.IsNullOrWhiteSpace(studentUpdateDto.Email))
            {
                throw new ArgumentException("Email là bắt buộc.");
            }

            string Image = existingStudent.Image;
            string NationalImageFront = existingStudent.NationalImageFront;
            string NationalImageBack = existingStudent.NationalImageBack;

            // Upload hình ảnh lên cloud nếu có
            if (studentUpdateDto.Image != null && studentUpdateDto.Image.Length > 0)
            {
                string fileNameImage = $"{Guid.NewGuid()}{Path.GetExtension(studentUpdateDto.Image.FileName)}";
                // await _blobService.DeleteBlob(existingStudent.Image.Split('/').Last(), SD.Storage_Container);
                Image = await _blobService.UploadBlob(fileNameImage, SD.Storage_Container, studentUpdateDto.Image);
            }

            if (studentUpdateDto.NationalImageFront != null && studentUpdateDto.NationalImageFront.Length > 0)
            {
                string fileNameNationalImageFront = $"{Guid.NewGuid()}{Path.GetExtension(studentUpdateDto.NationalImageFront.FileName)}";
                //  await _blobService.DeleteBlob(existingStudent.NationalImageFront.Split('/').Last(), SD.Storage_Container);
                NationalImageFront = await _blobService.UploadBlob(fileNameNationalImageFront, SD.Storage_Container, studentUpdateDto.NationalImageFront);
            }

            if (studentUpdateDto.NationalImageBack != null && studentUpdateDto.NationalImageBack.Length > 0)
            {
                string fileNameNationalImageBack = $"{Guid.NewGuid()}{Path.GetExtension(studentUpdateDto.NationalImageBack.FileName)}";
                //  await _blobService.DeleteBlob(existingStudent.NationalImageBack.Split('/').Last(), SD.Storage_Container);
                NationalImageBack = await _blobService.UploadBlob(fileNameNationalImageBack, SD.Storage_Container, studentUpdateDto.NationalImageBack);
            }

            // Cập nhật thông tin sinh viên
            _mapper.Map(studentUpdateDto, existingStudent);
            existingStudent.Image = Image;
            existingStudent.NationalImageFront = NationalImageFront;
            existingStudent.NationalImageBack = NationalImageBack;

            await _unitOfWork.Student.UpdateAsync(existingStudent);
            await _unitOfWork.SaveChangeAsync();
        }


        public async Task<IEnumerable<StudentDto>> GetStudentsByCourseIdAsync(int courseId)
        {
            if (courseId <= 0)
            {
                throw new ArgumentException("courseId must be greater than 0.", nameof(courseId));
            }

            // Tìm tất cả StudentCourse với courseId được chỉ định, bao gồm thông tin Student và StudentGroup thông qua StudentGroupAssignment
            var studentCourses = await _unitOfWork.StudentCourse.FindAsync(
                sc => sc.CourseId == courseId,
                includeProperties: "Student.StudentCourses.Course,Student.StudentGroupAssignment.StudentGroup"
            );

            // Kiểm tra nếu không có sinh viên nào trong khóa học này
            if (studentCourses == null || !studentCourses.Any())
            {
                return new List<StudentDto>();
            }

            // Ánh xạ dữ liệu sang StudentDto
            var studentDtos = studentCourses.Select(sc => new StudentDto
            {
                Id = sc.Student.Id,
                FullName = sc.Student.FullName,
                DateOfBirth = sc.Student.DateOfBirth,
                Gender = sc.Student.Gender,
                Image = sc.Student.Image,
                NationalId = sc.Student.NationalId,
                NationalImageFront = sc.Student.NationalImageFront,
                NationalImageBack = sc.Student.NationalImageBack,
                Address = sc.Student.Address,
                ParentName = sc.Student.ParentName,
                EmergencyContact = sc.Student.EmergencyContact,
                Email = sc.Student.Email,
                Conduct = sc.Student.Conduct,
                AcademicPerformance = sc.Student.AcademicPerformance,
                Status = sc.Student.Status,
                Note = sc.Student.Note,

                // Chỉ lấy thông tin Course hiện tại
                Courses = new List<CourseInfoDto>
        {
            new CourseInfoDto
            {
                CourseId = sc.CourseId,
                CourseName = sc.Course?.CourseName ?? "Unknown Course"
            }
        },

                // Lấy nhóm (StudentGroup) thuộc courseId hiện tại
                Groups = sc.Student.StudentGroupAssignment
                    .Where(sga => sga.StudentGroup != null && sga.StudentGroup.CourseId == courseId)
                    .Select(sga => new GroupInfoDto
                    {
                        GroupId = sga.StudentGroupId,
                        GroupName = sga.StudentGroup.GroupName
                    }).ToList()
            }).ToList();

            return studentDtos;
        }


        public async Task<byte[]> ExportStudentsByCourseAsync(int courseId)
        {
            // Thêm điều kiện để chỉ lấy sinh viên có trạng thái Approved
            var studentCourses = await _unitOfWork.StudentCourse.FindAsync(
                sc => sc.CourseId == courseId && sc.Status == ProgressStatus.Approved,
                includeProperties: "Student,Student.StudentGroupAssignment.StudentGroup"
            );

            if (studentCourses == null || !studentCourses.Any())
            {
                return null;
            }

            var course = await _unitOfWork.Course.GetByIdAsync(courseId);
            if (course == null)
            {
                throw new ArgumentException("Không tìm thấy khóa học.");
            }

            //    var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "SCCMS.Infrastucture", "TemplateExcel", "StudentList.xlsx");
            var templatePath = Path.Combine(AppContext.BaseDirectory, "TemplateExcel", "StudentList.xlsx");

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
                string courseTime = $"Danh sách khóa sinh\n Thời gian: {course.StartDate:dd/MM/yyyy} - {course.EndDate:dd/MM/yyyy}";
                worksheet.Cell("A3").Value = courseTime;

                int row = 6;
                int serialNumber = 1;
                var sampleRow = worksheet.Row(6);

                foreach (var studentCourse in studentCourses)
                {
                    var student = studentCourse.Student;
                    var studentGroupName = student.StudentGroupAssignment
                      .Where(sga => sga.StudentGroup.CourseId == courseId)  // Chỉ lấy nhóm liên quan đến courseId
                      .Select(sga => sga.StudentGroup.GroupName)
                      .FirstOrDefault() ?? "N/A";  // Nếu không có nhóm nào phù hợp, trả về "N/A"
                    worksheet.Cell(row, 1).Value = serialNumber;
                    worksheet.Cell(row, 2).Value = studentCourse.StudentCode;
                    worksheet.Cell(row, 3).Value = student.FullName;
                    worksheet.Cell(row, 4).Value = student.ParentName;
                    worksheet.Cell(row, 5).Value = student.EmergencyContact;
                    worksheet.Cell(row, 6).Value = student.DateOfBirth.ToString("dd/MM/yyyy");
                    worksheet.Cell(row, 7).Value = student.Gender.GetDisplayGender();
                    worksheet.Cell(row, 8).Value = studentGroupName;
                    worksheet.Cell(row, 9).Value = student.Address;  // Thêm địa chỉ
                    worksheet.Cell(row, 10).Value = student.Email;    // Thêm email
                    worksheet.Cell(row, 11).Value = student.NationalId; // Thêm CCCD
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
        public async Task<byte[]> ExportStudentsByGroupAsync(int groupId)
        {
            // Lấy danh sách sinh viên thuộc nhóm cụ thể
            var studentGroupAssignments = await _unitOfWork.StudentGroupAssignment.FindAsync(
                sga => sga.StudentGroupId == groupId,
                includeProperties: "Student,StudentGroup,Student.StudentCourses"
            );

            if (studentGroupAssignments == null || !studentGroupAssignments.Any())
            {
                return null;
            }

            var studentGroup = await _unitOfWork.StudentGroup.GetByIdAsync(groupId);
            if (studentGroup == null)
            {
                throw new ArgumentException("Không tìm thấy nhóm sinh viên.");
            }

            var templatePath = Path.Combine(AppContext.BaseDirectory, "TemplateExcel", "StudentList.xlsx");
            templatePath = Path.GetFullPath(templatePath);

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Không tìm thấy file template tại đường dẫn: {templatePath}");
            }

            using (var workbook = new XLWorkbook(templatePath))
            {
                var worksheet = workbook.Worksheet(1);

                // Cập nhật tiêu đề nhóm
                // Cập nhật tiêu đề khóa học
                string courseName = $"KHOÁ TU CHÙA CỔ LOAN";
                worksheet.Cell("A1").Value = courseName;
                string groupName = $"Danh Sách Sinh Viên Nhóm {studentGroup.GroupName}";
                worksheet.Cell("A3").Value = groupName;


                int row = 6;
                int serialNumber = 1;
                var sampleRow = worksheet.Row(6);

                foreach (var assignment in studentGroupAssignments)
                {
                    var student = assignment.Student;
                    var studentCode = student.StudentCourses?
                        .FirstOrDefault(vc => vc.CourseId == assignment.StudentGroup.CourseId)?.StudentCode ?? "N/A";


                    worksheet.Cell(row, 1).Value = serialNumber;
                    worksheet.Cell(row, 2).Value = studentCode;
                    worksheet.Cell(row, 3).Value = student.FullName;
                    worksheet.Cell(row, 4).Value = student.ParentName ?? "N/A";
                    worksheet.Cell(row, 5).Value = student.EmergencyContact ?? "N/A";
                    worksheet.Cell(row, 6).Value = student.DateOfBirth.ToString("dd/MM/yyyy");
                    worksheet.Cell(row, 7).Value = student.Gender.GetDisplayGender();
                    worksheet.Cell(row, 8).Value = student.Address ?? "N/A";
                    worksheet.Cell(row, 9).Value = student.Email ?? "N/A";
                    worksheet.Cell(row, 10).Value = student.NationalId ?? "N/A";
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




    }


}
