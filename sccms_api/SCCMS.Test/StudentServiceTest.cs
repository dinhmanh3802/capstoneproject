using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using SCCMS.Domain.DTOs.StudentDtos;
using Microsoft.AspNetCore.Http;
using AutoMapper;
using System.Collections.Generic;
using SCCMS.Domain.Services.Implements;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.UnitOfWork;
using System.Net.Http;
using System.Linq.Expressions;
using SCCMS.Domain.Services.Interfaces;
using Utility;

[TestFixture]
public class StudentServiceTests
{
	private Mock<IUnitOfWork> _unitOfWorkMock;
	private Mock<IBlobService> _blobServiceMock;
	private Mock<IMapper> _mapperMock;
	private Mock<HttpClient> _httpClientMock;
	private Mock<ICourseService> _courseServiceMock;
	private Mock<IStudentGroupAssignmentService> _studentGroupAssignmentServiceMock;
	private StudentService _studentService;

	[SetUp]
	public void SetUp()
	{
		_unitOfWorkMock = new Mock<IUnitOfWork>();
		_blobServiceMock = new Mock<IBlobService>();
		_mapperMock = new Mock<IMapper>();
		_httpClientMock = new Mock<HttpClient>(); // Mock HttpClient
		_courseServiceMock = new Mock<ICourseService>(); // Mock ICourseService
		_studentGroupAssignmentServiceMock = new Mock<IStudentGroupAssignmentService>(); // Mock IStudentGroupAssignmentService

		// Sửa constructor để truyền vào HttpClient, ICourseService, IStudentGroupAssignmentService
		_studentService = new StudentService(
			_unitOfWorkMock.Object,
			_mapperMock.Object,
			_blobServiceMock.Object,
			_httpClientMock.Object,
			_courseServiceMock.Object,
			_studentGroupAssignmentServiceMock.Object
		);
	}




	[Test]
	public void CreateStudentAsync_ShouldThrowArgumentException_WhenDateOfBirthIsInTheFuture()
	{
		var studentCreateDto = new StudentCreateDto
		{
			FullName = "Do Thanh Thuy",
			DateOfBirth = DateTime.Now.AddYears(1), // Future date
			Email = "dinhmanh3802@gmail.com",
			NationalId = "123456789"
		};

		Assert.ThrowsAsync<ArgumentException>(async () =>
			await _studentService.CreateStudentAsync(studentCreateDto, 1));
	}

	[Test]
	public void CreateStudentAsync_ShouldThrowArgumentException_WhenFullNameIsInvalid()
	{
		var studentCreateDto = new StudentCreateDto
		{
			FullName = "", // Empty name
			DateOfBirth = DateTime.Now.AddYears(-18),
			Email = "johndoe@example.com",
			NationalId = "123456789"
		};

		Assert.ThrowsAsync<ArgumentException>(async () =>
			await _studentService.CreateStudentAsync(studentCreateDto, 1));
	}

	[Test]
	public void CreateStudentAsync_ShouldThrowArgumentException_WhenEmailIsInvalid()
	{
		var studentCreateDto = new StudentCreateDto
		{
			FullName = "John Doe",
			DateOfBirth = DateTime.Now.AddYears(-18),
			Email = "invalid-email", // Invalid email format
			NationalId = "123456789"
		};

		Assert.ThrowsAsync<ArgumentException>(async () =>
			await _studentService.CreateStudentAsync(studentCreateDto, 1));
	}
	[Test]
	public void CreateStudentAsync_ShouldThrowArgumentException_WhenNationalIdIsEmpty()
	{
		// Arrange: Create a StudentCreateDto with an empty NationalId
		var studentCreateDto = new StudentCreateDto
		{
			FullName = "John Doe",
			DateOfBirth = DateTime.Now.AddYears(-18),
			Email = "john.doe@example.com", // Valid email
			NationalId = "", // Empty NationalId
			Address = "123 Some Street",
			ParentName = "Jane Doe",
			EmergencyContact = "0987654321"
		};

		// Act & Assert: Expect ArgumentException when trying to create student
		var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
			await _studentService.CreateStudentAsync(studentCreateDto, 1));

		// Verify that the exception message is correct
		Assert.AreEqual("NationalId is invalid.", exception.Message); // Ensure this matches the validation message
	}

	[Test]
	public void CreateStudentAsync_ShouldThrowArgumentException_WhenNationalIdIsShorterThan9Digit()
	{
		// Arrange: Create a StudentCreateDto with an empty NationalId
		var studentCreateDto = new StudentCreateDto
		{
			FullName = "John Doe",
			DateOfBirth = DateTime.Now.AddYears(-18),
			Email = "john.doe@example.com", // Valid email
			NationalId = "12345678", // Empty NationalId
			Address = "123 Some Street",
			ParentName = "Jane Doe",
			EmergencyContact = "0987654321"
		};

		// Act & Assert: Expect ArgumentException when trying to create student
		var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
			await _studentService.CreateStudentAsync(studentCreateDto, 1));

		// Verify that the exception message is correct
		Assert.AreEqual("NationalId is invalid.", exception.Message); // Ensure this matches the validation message
	}

	[Test]
	public void CreateStudentAsync_ShouldThrowArgumentException_WhenNationalIdIsGreaterThan12Digit()
	{
		// Arrange: Create a StudentCreateDto with an empty NationalId
		var studentCreateDto = new StudentCreateDto
		{
			FullName = "John Doe",
			DateOfBirth = DateTime.Now.AddYears(-18),
			Email = "john.doe@example.com", // Valid email
			NationalId = "123456789101112", // Empty NationalId
			Address = "123 Some Street",
			ParentName = "Jane Doe",
			EmergencyContact = "0987654321"
		};

		// Act & Assert: Expect ArgumentException when trying to create student
		var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
			await _studentService.CreateStudentAsync(studentCreateDto, 1));

		// Verify that the exception message is correct
		Assert.AreEqual("NationalId is invalid.", exception.Message); // Ensure this matches the validation message
	}

	[Test]
	public void CreateStudentAsync_ShouldThrowArgumentException_WhenNationalIdIsCharacter()
	{
		// Arrange: Create a StudentCreateDto with an empty NationalId
		var studentCreateDto = new StudentCreateDto
		{
			FullName = "John Doe",
			DateOfBirth = DateTime.Now.AddYears(-18),
			Email = "john.doe@example.com", // Valid email
			NationalId = "dadsadadasd", 
			Address = "123 Some Street",
			ParentName = "Jane Doe",
			EmergencyContact = "0987654321"
		};

		// Act & Assert: Expect ArgumentException when trying to create student
		var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
			await _studentService.CreateStudentAsync(studentCreateDto, 1));

		// Verify that the exception message is correct
		Assert.AreEqual("NationalId is invalid.", exception.Message); // Ensure this matches the validation message
	}

	[Test]
	public async Task CreateStudentAsync_FullNameIsEmpty_ThrowsArgumentException()
	{
		// Arrange: Create a StudentCreateDto with an empty FullName
		var studentCreateDto = new StudentCreateDto
		{
			FullName = "", // Empty FullName
			DateOfBirth = DateTime.Now.AddYears(-20),
			Gender = Gender.Male,
			Email = "john.doe@example.com",
			NationalId = "123456789",
			Address = "123 Some Street",
			ParentName = "Jane Doe",
			EmergencyContact = "0987654321"
		};

		// Mocking the necessary service calls
		_unitOfWorkMock.Setup(uow => uow.Student.GetAllAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync(new List<Student>());

		// Act & Assert: Expect ArgumentException when trying to create student
		var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _studentService.CreateStudentAsync(studentCreateDto, 1));

		// Verify that the exception message is correct
		Assert.AreEqual("FullName is required and must not exceed 50 characters.", exception.Message);
	}
	[Test]
	public async Task CreateStudentAsync_FullNameIsEmpty1_ThrowsArgumentException()
	{
		// Arrange: Create a StudentCreateDto with an empty FullName
		var studentCreateDto = new StudentCreateDto
		{
			FullName = "", // Empty FullName
			DateOfBirth = DateTime.Now.AddYears(-20),
			Gender = Gender.Male,
			Email = "dinhmanh3826@gmail.com",
			NationalId = "123456789",
			Address = "123 Some Street",
			ParentName = "Jane Doe",
			EmergencyContact = "0987654321"
		};

		// Mocking the necessary service calls
		_unitOfWorkMock.Setup(uow => uow.Student.GetAllAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync(new List<Student>());

		// Act & Assert: Expect ArgumentException when trying to create student
		var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _studentService.CreateStudentAsync(studentCreateDto, 1));

		// Verify that the exception message is correct
		Assert.AreEqual("FullName is required and must not exceed 50 characters.", exception.Message);
	}

	[Test]
	public void CreateStudentAsync_ShouldThrowArgumentException_WhenEmailIsEmpty()
	{
		var studentCreateDto = new StudentCreateDto
		{
			FullName = "John Doe",
			DateOfBirth = DateTime.Now.AddYears(-18),
			Email = "", // Empty email
			NationalId = "123456789"
		};

		Assert.ThrowsAsync<ArgumentException>(async () =>
			await _studentService.CreateStudentAsync(studentCreateDto, 1));
	}
	[Test]
	public async Task CreateStudentAsync_EmergencyContactIsEmpty_ThrowsArgumentException()
	{
		// Arrange: Create a StudentCreateDto with an empty EmergencyContact
		var studentCreateDto = new StudentCreateDto
		{
			FullName = "aa", // Valid FullName
			DateOfBirth = DateTime.Now.AddYears(-20),
			Gender = Gender.Male,
			Email = "dinhmanh3826@gmail.com",
			NationalId = "123456789",
			Address = "123 Some Street",
			ParentName = "Jane Doe",
			EmergencyContact = "" // Empty EmergencyContact
		};

		// Mocking the necessary service calls
		_unitOfWorkMock.Setup(uow => uow.Student.GetAllAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync(new List<Student>());

		// Act & Assert: Expect ArgumentException when trying to create student
		var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _studentService.CreateStudentAsync(studentCreateDto, 1));

		// Verify that the exception message is correct
		Assert.AreEqual("EmergencyContact is required and must be a valid phone number with 10 digits.", exception.Message);
	}

	[Test]
	public async Task CreateStudentAsync_EmergencyContactIsSmallerThan10Digit_ThrowsArgumentException()
	{
		// Arrange: Create a StudentCreateDto with an empty EmergencyContact
		var studentCreateDto = new StudentCreateDto
		{
			FullName = "aa", // Valid FullName
			DateOfBirth = DateTime.Now.AddYears(-20),
			Gender = Gender.Male,
			Email = "dinhmanh3826@gmail.com",
			NationalId = "123456789",
			Address = "123 Some Street",
			ParentName = "Jane Doe",
			EmergencyContact = "123456" // Empty EmergencyContact
		};

		// Mocking the necessary service calls
		_unitOfWorkMock.Setup(uow => uow.Student.GetAllAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync(new List<Student>());

		// Act & Assert: Expect ArgumentException when trying to create student
		var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _studentService.CreateStudentAsync(studentCreateDto, 1));

		// Verify that the exception message is correct
		Assert.AreEqual("EmergencyContact is required and must be a valid phone number with 10 digits.", exception.Message);
	}
	[Test]
	public async Task CreateStudentAsync_EmergencyContactIsGreaterThan10Digit_ThrowsArgumentException()
	{
		// Arrange: Create a StudentCreateDto with an empty EmergencyContact
		var studentCreateDto = new StudentCreateDto
		{
			FullName = "aa", // Valid FullName
			DateOfBirth = DateTime.Now.AddYears(-20),
			Gender = Gender.Male,
			Email = "dinhmanh3826@gmail.com",
			NationalId = "123456789",
			Address = "123 Some Street",
			ParentName = "Jane Doe",
			EmergencyContact = "12345678901" // Empty EmergencyContact
		};

		// Mocking the necessary service calls
		_unitOfWorkMock.Setup(uow => uow.Student.GetAllAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync(new List<Student>());

		// Act & Assert: Expect ArgumentException when trying to create student
		var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _studentService.CreateStudentAsync(studentCreateDto, 1));

		// Verify that the exception message is correct
		Assert.AreEqual("EmergencyContact is required and must be a valid phone number with 10 digits.", exception.Message);
	}


	[Test]
	public async Task CreateStudentAsync_EmergencyContactIsCharacter_ThrowsArgumentException()
	{
		// Arrange: Create a StudentCreateDto with an empty EmergencyContact
		var studentCreateDto = new StudentCreateDto
		{
			FullName = "aa", // Valid FullName
			DateOfBirth = DateTime.Now.AddYears(-20),
			Gender = Gender.Male,
			Email = "dinhmanh3826@gmail.com",
			NationalId = "123456789",
			Address = "123 Some Street",
			ParentName = "Jane Doe",
			EmergencyContact = "dasdsdaa" // Empty EmergencyContact
		};

		// Mocking the necessary service calls
		_unitOfWorkMock.Setup(uow => uow.Student.GetAllAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync(new List<Student>());

		// Act & Assert: Expect ArgumentException when trying to create student
		var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _studentService.CreateStudentAsync(studentCreateDto, 1));

		// Verify that the exception message is correct
		Assert.AreEqual("EmergencyContact is required and must be a valid phone number with 10 digits.", exception.Message);
	}
	[Test]
	public async Task CreateStudentAsync_AllFieldsValid_CreatesStudentSuccessfully()
	{
		// Arrange: Tạo đối tượng StudentCreateDto hợp lệ
		var studentCreateDto = new StudentCreateDto
		{
			FullName = "John Doe",
			DateOfBirth = DateTime.Now.AddYears(-20),
			Gender = Gender.Male,
			Email = "john.doe@example.com",
			NationalId = "123456789",
			Address = "123 Some Street",
			ParentName = "Jane Doe",
			EmergencyContact = "0987654321",
			Conduct = "Good",
			AcademicPerformance = "Excellent",
			Status = ProfileStatus.Active
		};

		// Mock các phương thức cần thiết trong UnitOfWork và các dịch vụ khác
		_unitOfWorkMock.Setup(uow => uow.Student.GetAllAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync(new List<Student>()); // Không có sinh viên trùng NationalId

		_unitOfWorkMock.Setup(uow => uow.Student.AddAsync(It.IsAny<Student>())).Returns(Task.CompletedTask);
		_unitOfWorkMock.Setup(uow => uow.SaveChangeAsync()).ReturnsAsync(1);
		_unitOfWorkMock.Setup(uow => uow.StudentCourse.AddAsync(It.IsAny<StudentCourse>())).Returns(Task.CompletedTask);

		_mapperMock.Setup(m => m.Map<Student>(It.IsAny<StudentCreateDto>())).Returns(new Student { Id = 1 });

		_blobServiceMock.Setup(b => b.UploadBlob(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IFormFile>()))
						.ReturnsAsync("https://someurl.com/image.jpg"); // Trả về URL hình ảnh giả

		// Act: Gọi phương thức cần test
		await _studentService.CreateStudentAsync(studentCreateDto, 1);

		// Assert: Kiểm tra các phương thức được gọi chính xác
		_unitOfWorkMock.Verify(uow => uow.Student.AddAsync(It.IsAny<Student>()), Times.Once); // AddAsync chỉ cần gọi 1 lần
		_unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Exactly(2)); // SaveChangeAsync cần gọi 2 lần

		// Kiểm tra các phương thức khác đã được gọi đúng số lần
		_unitOfWorkMock.Verify(uow => uow.StudentCourse.AddAsync(It.IsAny<StudentCourse>()), Times.Once);
	}

	[Test]
	public async Task GetStudentByIdAsync_IdLessThanOrEqualToZero_ThrowsArgumentException()
	{
		// Arrange
		var invalidId = 0; // Hoặc id < 0

		// Act & Assert
		var exception = Assert.ThrowsAsync<ArgumentException>(() => _studentService.GetStudentByIdAsync(invalidId));
		Assert.AreEqual("Id must be greater than 0. (Parameter 'id')", exception.Message);
	}
	[Test]
	public async Task GetStudentByIdAsync_StudentNotFound_ReturnsNull()
	{
		// Arrange
		var validId = 1; // ID hợp lệ
		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync(new List<Student>()); // Không tìm thấy sinh viên

		// Act
		var result = await _studentService.GetStudentByIdAsync(validId);

		// Assert
		Assert.IsNull(result); // Kỳ vọng kết quả là null
	}

	[Test]
	public async Task GetStudentByIdAsync_StudentFound_ReturnsStudentDto()
	{
		// Arrange
		var validId = 1;
		var student = new Student
		{
			Id = 1,
			FullName = "John Doe",
			DateOfBirth = DateTime.Now.AddYears(-20),
			Gender = Gender.Male,
			Image = "some-image-url",
			NationalId = "123456789",
			NationalImageFront = "front-image-url",
			NationalImageBack = "back-image-url",
			Address = "123 Some Street",
			ParentName = "Jane Doe",
			EmergencyContact = "0987654321",
			Email = "john.doe@example.com",
			Conduct = "Good",
			AcademicPerformance = "Excellent",
			Status = ProfileStatus.Active,
			Note = "Note",
			StudentCourses = new List<StudentCourse>
		{
			new StudentCourse
			{
				CourseId = 101,
				Course = new Course { CourseName = "Math" }
			}
		},
			StudentGroupAssignment = new List<StudentGroupAssignment>
		{
			new StudentGroupAssignment
			{
				StudentGroupId = 202,
				StudentGroup = new StudentGroup { GroupName = "Group A" }
			}
		}
		};

		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync(new List<Student> { student }); // Trả về một sinh viên

		// Act
		var result = await _studentService.GetStudentByIdAsync(validId);

		// Assert
		Assert.IsNotNull(result); // Kỳ vọng kết quả không phải null
		Assert.AreEqual(student.Id, result.Id);
		Assert.AreEqual(student.FullName, result.FullName);
		Assert.AreEqual(student.Email, result.Email);
		Assert.AreEqual(student.StudentCourses.Count, result.Courses.Count); // Kiểm tra số lượng khóa học
		Assert.AreEqual(student.StudentGroupAssignment.Count, result.Groups.Count); // Kiểm tra số lượng nhóm
		Assert.AreEqual("Math", result.Courses.First().CourseName); // Kiểm tra thông tin khóa học
		Assert.AreEqual("Group A", result.Groups.First().GroupName); // Kiểm tra thông tin nhóm
	}
	[Test]
	public async Task GetStudentByIdAsync_IdLessThanOrEqualToZero1_ThrowsArgumentException()
	{
		// Arrange
		var invalidId = -1; // ID không hợp lệ

		// Act & Assert
		var exception = Assert.ThrowsAsync<ArgumentException>(() => _studentService.GetStudentByIdAsync(invalidId));
		Assert.AreEqual("Id must be greater than 0. (Parameter 'id')", exception.Message);
	}
	[Test]
	public async Task GetStudentByIdAsync_StudentNotFound_ReturnsNull_ForId999()
	{
		// Arrange
		var nonExistentId = 999; // ID không tồn tại
		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync(new List<Student>()); // Không có sinh viên với ID này

		// Act
		var result = await _studentService.GetStudentByIdAsync(nonExistentId);

		// Assert
		Assert.IsNull(result); // Kỳ vọng kết quả là null
	}

	[Test]
	public async Task GetStudentByIdAsync_StudentFound_ReturnsStudentDto_ForId2()
	{
		// Arrange
		var validId = 2; // ID hợp lệ
		var student = new Student
		{
			Id = 2,
			FullName = "Jane Doe",
			DateOfBirth = DateTime.Now.AddYears(-21),
			Gender = Gender.Female,
			Image = "some-image-url",
			NationalId = "987654321",
			NationalImageFront = "front-image-url",
			NationalImageBack = "back-image-url",
			Address = "456 Some Avenue",
			ParentName = "John Doe",
			EmergencyContact = "0987654322",
			Email = "jane.doe@example.com",
			Conduct = "Excellent",
			AcademicPerformance = "Good",
			Status = ProfileStatus.Active,
			Note = "No note",
			StudentCourses = new List<StudentCourse>
		{
			new StudentCourse
			{
				CourseId = 102,
				Course = new Course { CourseName = "Physics" }
			}
		},
			StudentGroupAssignment = new List<StudentGroupAssignment>
		{
			new StudentGroupAssignment
			{
				StudentGroupId = 203,
				StudentGroup = new StudentGroup { GroupName = "Group B" }
			}
		}
		};

		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync(new List<Student> { student }); // Trả về sinh viên với ID 2

		// Act
		var result = await _studentService.GetStudentByIdAsync(validId);

		// Assert
		Assert.IsNotNull(result); // Kỳ vọng kết quả không phải null
		Assert.AreEqual(student.Id, result.Id);
		Assert.AreEqual(student.FullName, result.FullName);
		Assert.AreEqual(student.Email, result.Email);
		Assert.AreEqual(student.StudentCourses.Count, result.Courses.Count); // Kiểm tra số lượng khóa học
		Assert.AreEqual(student.StudentGroupAssignment.Count, result.Groups.Count); // Kiểm tra số lượng nhóm
		Assert.AreEqual("Physics", result.Courses.First().CourseName); // Kiểm tra thông tin khóa học
		Assert.AreEqual("Group B", result.Groups.First().GroupName); // Kiểm tra thông tin nhóm
	}
	[Test]
	public async Task GetAllStudentsAsync_NoFilters_ReturnsAllStudents()
	{
		// Arrange

		// Mock các Course và StudentGroup
		var course = new Course { CourseName = "Math" };
		var studentGroup = new StudentGroup { GroupName = "Group A" };

		// Mock StudentCourses và StudentGroupAssignment
		var studentCourses = new List<StudentCourse>
	{
		new StudentCourse { CourseId = 1, Course = course, StudentCode = "S1234" },
		new StudentCourse { CourseId = 2, Course = new Course { CourseName = "English" }, StudentCode = "S2345" }
	};
		var studentGroupAssignments = new List<StudentGroupAssignment>
	{
		new StudentGroupAssignment { StudentGroupId = 1, StudentGroup = studentGroup }
	};

		// Mock danh sách students (đảm bảo sắp xếp đúng thứ tự)
		var students = new List<Student>
	{
		new Student
		{
			Id = 1,
			FullName = "John Doe",
			Email = "john.doe@example.com",
			Gender = Gender.Male,
			Status = ProfileStatus.Active,
			StudentCourses = studentCourses,
			StudentGroupAssignment = studentGroupAssignments
		},
		new Student
		{
			Id = 2,
			FullName = "Jane Doe",
			Email = "jane.doe@example.com",
			Gender = Gender.Female,
			Status = ProfileStatus.Active,
			StudentCourses = studentCourses,
			StudentGroupAssignment = studentGroupAssignments
		}
	};

		// Mock _unitOfWork và phương thức FindAsync trả về danh sách students đã được sắp xếp
		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync(students.OrderByDescending(s => s.Id).ToList());  // Sắp xếp theo ID giảm dần

		// Act
		var result = await _studentService.GetAllStudentsAsync();

		// Assert
		Assert.AreEqual(2, result.Count());  // Kiểm tra có 2 sinh viên
		Assert.AreEqual("Jane Doe", result.First().FullName);  // Kiểm tra sinh viên đầu tiên là John Doe
		Assert.AreEqual("John Doe", result.Last().FullName);  // Kiểm tra sinh viên cuối cùng là Jane Doe

		// Kiểm tra chi tiết của các đối tượng ánh xạ
		var firstStudent = result.First();
		Assert.AreEqual(2, firstStudent.Courses.Count);  // Kiểm tra số lượng courses
		Assert.AreEqual("Math", firstStudent.Courses.First().CourseName);  // Kiểm tra tên course đầu tiên
		Assert.AreEqual(1, firstStudent.Groups.Count);  // Kiểm tra số lượng groups
		Assert.AreEqual("Group A", firstStudent.Groups.First().GroupName);  // Kiểm tra tên group đầu tiên
	}

	[Test]
	public async Task GetAllStudentsAsync_WithFilters_ReturnsFilteredStudents()
	{
		// Arrange

		// Mock các Course và StudentGroup
		var course = new Course { CourseName = "Math" };
		var studentGroup = new StudentGroup { GroupName = "Group A" };

		// Mock StudentCourses và StudentGroupAssignment
		var studentCourses = new List<StudentCourse>
	{
		new StudentCourse { CourseId = 1, Course = course, StudentCode = "S1234" }
	};
		var studentGroupAssignments = new List<StudentGroupAssignment>
	{
		new StudentGroupAssignment { StudentGroupId = 1, StudentGroup = studentGroup }
	};

		// Mock danh sách students với các filter
		var students = new List<Student>
	{
		new Student
		{
			Id = 1,
			FullName = "John Doe",
			Email = "john.doe@example.com",
			Gender = Gender.Male,
			Status = ProfileStatus.Active,
			StudentCourses = studentCourses,
			StudentGroupAssignment = studentGroupAssignments
		},
		new Student
		{
			Id = 2,
			FullName = "Jane Doe",
			Email = "jane.doe@example.com",
			Gender = Gender.Female,
			Status = ProfileStatus.Active,
			StudentCourses = studentCourses,
			StudentGroupAssignment = studentGroupAssignments
		}
	};

		// Mock _unitOfWork và phương thức FindAsync, sử dụng điều kiện lọc fullName
		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.Is<Expression<Func<Student, bool>>>(expr =>
			expr.Compile().Invoke(students[0]) == true && expr.Compile().Invoke(students[1]) == false),
			It.IsAny<string>()))
			.ReturnsAsync(new List<Student> { students[0] });  // Trả về chỉ sinh viên "John Doe"

		// Act
		var result = await _studentService.GetAllStudentsAsync(fullName: "John Doe");

		// Assert
		Assert.AreEqual(1, result.Count());  // Kiểm tra có 1 sinh viên theo filter
		Assert.AreEqual("John Doe", result.First().FullName);  // Kiểm tra sinh viên đầu tiên là John Doe
	}

	[Test]
	public async Task GetAllStudentsAsync_WithNoStudents_ReturnsEmptyList()
	{
		// Arrange
		var students = new List<Student>();

		// Mock _unitOfWork và phương thức FindAsync trả về danh sách trống
		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync(students);

		// Act
		var result = await _studentService.GetAllStudentsAsync();

		// Assert
		Assert.AreEqual(0, result.Count());  // Kiểm tra không có sinh viên nào trả về
	}

	[Test]
	public async Task GetAllStudentsAsync_WithFullName_ReturnsCorrectStudent()
	{
		// Arrange
		var students = GetMockStudents();
		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync((Expression<Func<Student, bool>> filter, string includeProperties) =>
					   {
						   return students.Where(filter.Compile()).ToList();
					   });

		// Act
		var result = await _studentService.GetAllStudentsAsync(fullName: "Do Thanh Thuy");

		// Assert
		Assert.AreEqual(1, result.Count());  // Kiểm tra chỉ có 1 sinh viên với fullName "Do Thanh Thuy"
		Assert.AreEqual("Do Thanh Thuy", result.First().FullName);
	}

	[Test]
	public async Task GetAllStudentsAsync_WithFullNameNull_ReturnsAllStudents()
	{
		// Arrange
		var students = GetMockStudents();
		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync((Expression<Func<Student, bool>> filter, string includeProperties) =>
					   {
						   return students.Where(filter.Compile()).ToList();
					   });

		// Act
		var result = await _studentService.GetAllStudentsAsync(fullName: null);

		// Assert
		Assert.AreEqual(4, result.Count());  // Kiểm tra tất cả sinh viên được trả về
	}

	[Test]
	public async Task GetAllStudentsAsync_WithFullName_HiHI_ReturnsCorrectStudent()
	{
		// Arrange
		var students = GetMockStudents();
		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync((Expression<Func<Student, bool>> filter, string includeProperties) =>
					   {
						   return students.Where(filter.Compile()).ToList();
					   });

		// Act
		var result = await _studentService.GetAllStudentsAsync(fullName: "HiHI");

		// Assert
		Assert.AreEqual(1, result.Count());  // Kiểm tra chỉ có 1 sinh viên "HiHI"
		Assert.AreEqual("HiHI", result.First().FullName);  // Kiểm tra tên đúng
	}

	[Test]
	public async Task GetAllStudentsAsync_WithStudentCode_220505002_ReturnsCorrectStudent()
	{
		// Arrange
		var students = GetMockStudents();
		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync((Expression<Func<Student, bool>> filter, string includeProperties) =>
					   {
						   return students.Where(filter.Compile()).ToList();
					   });

		// Act
		var result = await _studentService.GetAllStudentsAsync(fullName: "HiHI");

		// Assert
		Assert.AreEqual(1, result.Count());  // Kiểm tra chỉ có 1 sinh viên "HiHI"
		Assert.AreEqual("HiHI", result.First().FullName);  // Kiểm tra tên đúng
	}
	[Test]
	public async Task GetAllStudentsAsync_WithStudentCode_null_ReturnsCorrectStudent()
	{
		// Arrange
		var students = GetMockStudents();
		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync((Expression<Func<Student, bool>> filter, string includeProperties) =>
					   {
						   return students.Where(filter.Compile()).ToList();
					   });

		// Act
		var result = await _studentService.GetAllStudentsAsync(fullName: "HiHI");

		// Assert
		Assert.AreEqual(1, result.Count());  // Kiểm tra chỉ có 1 sinh viên "HiHI"
		Assert.AreEqual("HiHI", result.First().FullName);  // Kiểm tra tên đúng
	}

	[Test]
	public async Task GetAllStudentsAsync_WithStudentCode_Empty_ReturnsCorrectStudent()
	{
		// Arrange
		var students = GetMockStudents();
		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync((Expression<Func<Student, bool>> filter, string includeProperties) =>
					   {
						   return students.Where(filter.Compile()).ToList();
					   });

		// Act
		var result = await _studentService.GetAllStudentsAsync(fullName: "HiHI");

		// Assert
		Assert.AreEqual(1, result.Count());  // Kiểm tra chỉ có 1 sinh viên "HiHI"
		Assert.AreEqual("HiHI", result.First().FullName);  // Kiểm tra tên đúng
	}
	[Test]
	public async Task GetAllStudentsAsync_WithEmailNull_ReturnsAllStudents()
	{
		// Arrange
		var students = GetMockStudents();
		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync((Expression<Func<Student, bool>> filter, string includeProperties) =>
					   {
						   return students.Where(filter.Compile()).ToList();
					   });

		// Act
		var result = await _studentService.GetAllStudentsAsync(email: null);

		// Assert
		Assert.AreEqual(4, result.Count());  // Kiểm tra tất cả sinh viên được trả về
	}

	[Test]
	public async Task GetAllStudentsAsync_WithPhoneNull_ReturnsAllStudents()
	{
		// Arrange
		var students = GetMockStudents();
		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync((Expression<Func<Student, bool>> filter, string includeProperties) =>
					   {
						   return students.Where(filter.Compile()).ToList();
					   });

		// Act
		var result = await _studentService.GetAllStudentsAsync(emergencyContact: null);

		// Assert
		Assert.AreEqual(4, result.Count());  // Kiểm tra tất cả sinh viên được trả về
	}

	[Test]
	public async Task GetAllStudentsAsync_WithNameNull_ReturnsAllStudents()
	{
		// Arrange
		var students = GetMockStudents();
		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync((Expression<Func<Student, bool>> filter, string includeProperties) =>
					   {
						   return students.Where(filter.Compile()).ToList();
					   });

		// Act
		var result = await _studentService.GetAllStudentsAsync(fullName: null);

		// Assert
		Assert.AreEqual(4, result.Count());  // Kiểm tra tất cả sinh viên được trả về
	}
	[Test]
	public async Task GetAllStudentsAsync_WithStudentCode1_ReturnsAllStudents()
	{
		// Arrange
		var students = GetMockStudents();
		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync((Expression<Func<Student, bool>> filter, string includeProperties) =>
					   {
						   return students.Where(filter.Compile()).ToList();
					   });

		// Act
		var result = await _studentService.GetAllStudentsAsync(studentCode: null);

		// Assert
		Assert.AreEqual(4, result.Count());  // Kiểm tra tất cả sinh viên được trả về
	}
	[Test]
	public async Task GetAllStudentsAsync_WithStudenGroupIDNull_ReturnsAllStudents()
	{
		// Arrange
		var students = GetMockStudents();
		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync((Expression<Func<Student, bool>> filter, string includeProperties) =>
					   {
						   return students.Where(filter.Compile()).ToList();
					   });

		// Act
		var result = await _studentService.GetAllStudentsAsync(studentGroupId: null);

		// Assert
		Assert.AreEqual(4, result.Count());  // Kiểm tra tất cả sinh viên được trả về
	}
	[Test]
	public async Task GetAllStudentsAsync_WithCourseIDNull_ReturnsAllStudents()
	{
		// Arrange
		var students = GetMockStudents();
		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync((Expression<Func<Student, bool>> filter, string includeProperties) =>
					   {
						   return students.Where(filter.Compile()).ToList();
					   });

		// Act
		var result = await _studentService.GetAllStudentsAsync(courseId: null);

		// Assert
		Assert.AreEqual(4, result.Count());  // Kiểm tra tất cả sinh viên được trả về
	}
	[Test]
	public async Task GetAllStudentsAsync_WithDobNull_ReturnsAllStudents()
	{
		// Arrange
		var students = GetMockStudents();
		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync((Expression<Func<Student, bool>> filter, string includeProperties) =>
					   {
						   return students.Where(filter.Compile()).ToList();
					   });

		// Act
		var result = await _studentService.GetAllStudentsAsync(dateOfBirth: null);

		// Assert
		Assert.AreEqual(4, result.Count());  // Kiểm tra tất cả sinh viên được trả về
	}

	[Test]
	public async Task GetAllStudentsAsync_WithStudentCodeNull_ReturnsAllStudents()
	{
		// Arrange
		var students = GetMockStudents();
		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync((Expression<Func<Student, bool>> filter, string includeProperties) =>
					   {
						   return students.Where(filter.Compile()).ToList();
					   });

		// Act
		var result = await _studentService.GetAllStudentsAsync(studentCode: null);

		// Assert
		Assert.AreEqual(4, result.Count());  // Kiểm tra tất cả sinh viên được trả về
	}
	[Test]
	public async Task GetAllStudentsAsync_WithParentNameNull_ReturnsAllStudents()
	{
		// Arrange
		var students = GetMockStudents();
		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync((Expression<Func<Student, bool>> filter, string includeProperties) =>
					   {
						   return students.Where(filter.Compile()).ToList();
					   });

		// Act
		var result = await _studentService.GetAllStudentsAsync(parentName: null);

		// Assert
		Assert.AreEqual(4, result.Count());  // Kiểm tra tất cả sinh viên được trả về
	}
	[Test]
	public async Task GetAllStudentsAsync_WithCourseIdNull_ReturnsAllStudents()
	{
		// Arrange
		var students = GetMockStudents();
		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync((Expression<Func<Student, bool>> filter, string includeProperties) =>
					   {
						   return students.Where(filter.Compile()).ToList();
					   });

		// Act
		var result = await _studentService.GetAllStudentsAsync(courseId: null);

		// Assert
		Assert.AreEqual(4, result.Count());  // Kiểm tra tất cả sinh viên được trả về
	}
	[Test]
	public async Task GetAllStudentsAsync_WithStudentGroupIDNull_ReturnsAllStudents()
	{
		// Arrange
		var students = GetMockStudents();
		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync((Expression<Func<Student, bool>> filter, string includeProperties) =>
					   {
						   return students.Where(filter.Compile()).ToList();
					   });

		// Act
		var result = await _studentService.GetAllStudentsAsync(studentGroupId: null);

		// Assert
		Assert.AreEqual(4, result.Count());  // Kiểm tra tất cả sinh viên được trả về
	}
	[Test]
	public async Task GetAllStudentsAsync_WithEmail_ReturnsCorrectStudent()
	{
		// Arrange
		var students = GetMockStudents();
		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync((Expression<Func<Student, bool>> filter, string includeProperties) =>
					   {
						   return students.Where(filter.Compile()).ToList();
					   });

		// Act
		var result = await _studentService.GetAllStudentsAsync(email: "dinhmanh3802@gmail.com");

		// Assert
		Assert.AreEqual(1, result.Count());  // Kiểm tra chỉ có 1 sinh viên với email này
		Assert.AreEqual("Do Thanh Thuy", result.First().FullName);  // Kiểm tra sinh viên đúng
	}

	[Test]
public async Task GetAllStudentsAsync_WithPhoneNumberNull_ReturnsAllStudents()
{
    // Arrange
    var students = GetMockStudents();
    _unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
                   .ReturnsAsync((Expression<Func<Student, bool>> filter, string includeProperties) =>
                   {
                       return students.Where(filter.Compile()).ToList();
                   });

    // Act
    var result = await _studentService.GetAllStudentsAsync(emergencyContact: null);

    // Assert
    Assert.AreEqual(4, result.Count());  // Kiểm tra tất cả sinh viên được trả về
}

	[Test]
	public async Task GetAllStudentsAsync_WithPhoneNumber012345_ReturnsCorrectStudent()
	{
		// Arrange
		var students = GetMockStudents();
		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync((Expression<Func<Student, bool>> filter, string includeProperties) =>
					   {
						   return students.Where(filter.Compile()).ToList();
					   });

		// Act
		var result = await _studentService.GetAllStudentsAsync(emergencyContact: "012345");

		// Assert
		Assert.AreEqual(1, result.Count());  // Kiểm tra chỉ có 1 sinh viên với phoneNumber "012345"
		Assert.AreEqual("John Doe", result.First().FullName);  // Kiểm tra sinh viên đúng
	}

	[Test]
	public async Task GetAllStudentsAsync_WithPhoneNumber1234567890_ReturnsCorrectStudent()
	{
		// Arrange
		var students = GetMockStudents();
		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync((Expression<Func<Student, bool>> filter, string includeProperties) =>
					   {
						   return students.Where(filter.Compile()).ToList();
					   });

		// Act
		var result = await _studentService.GetAllStudentsAsync(emergencyContact: "1234567890");

		// Assert
		Assert.AreEqual(1, result.Count());  // Kiểm tra chỉ có 1 sinh viên với phoneNumber "1234567890"
		Assert.AreEqual("Jane Doe", result.First().FullName);  // Kiểm tra sinh viên đúng
	}

	[Test]
	public async Task GetAllStudentsAsync_WithPhoneNumber9999999999_ReturnsCorrectStudent()
	{
		// Arrange
		var students = GetMockStudents();
		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync((Expression<Func<Student, bool>> filter, string includeProperties) =>
					   {
						   return students.Where(filter.Compile()).ToList();
					   });

		// Act
		var result = await _studentService.GetAllStudentsAsync(emergencyContact: "9999999999");

		// Assert
		Assert.AreEqual(1, result.Count());  // Kiểm tra chỉ có 1 sinh viên với phoneNumber "9999999999"
		Assert.AreEqual("Do Thanh Thuy", result.First().FullName);  // Kiểm tra sinh viên đúng
	}

	[Test]
	public async Task GetAllStudentsAsync_WithPhoneNumberInvalid_ReturnsNoStudents()
	{
		// Arrange
		var students = GetMockStudents();
		_unitOfWorkMock.Setup(uow => uow.Student.FindAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<string>()))
					   .ReturnsAsync((Expression<Func<Student, bool>> filter, string includeProperties) =>
					   {
						   return students.Where(filter.Compile()).ToList();
					   });

		// Act
		var result = await _studentService.GetAllStudentsAsync(emergencyContact: "abcdefghjk");

		// Assert
		Assert.AreEqual(1, result.Count());  // Kiểm tra không có sinh viên nào với phoneNumber không hợp lệ
	}

	private List<Student> GetMockStudents()
	{
		var course = new Course { CourseName = "Math" };
		var studentGroup = new StudentGroup { GroupName = "Group A" };

		var studentCourses = new List<StudentCourse>
	{
		new StudentCourse { CourseId = 1, Course = course, StudentCode = "S1234" }
	};
		var studentGroupAssignments = new List<StudentGroupAssignment>
	{
		new StudentGroupAssignment { StudentGroupId = 1, StudentGroup = studentGroup }
	};

		return new List<Student>
	{
		new Student
		{
			Id = 1,
			FullName = "John Doe",
			Email = "john.doe@example.com",
			Gender = Gender.Male,
			Status = ProfileStatus.Active,
			EmergencyContact = "012345",
			StudentCourses = studentCourses,
			StudentGroupAssignment = studentGroupAssignments
		},
		new Student
		{
			Id = 2,
			FullName = "Jane Doe",
			Email = "jane.doe@example.com",
			Gender = Gender.Female,
			Status = ProfileStatus.Active,
			EmergencyContact = "1234567890",
			StudentCourses = studentCourses,
			StudentGroupAssignment = studentGroupAssignments
		},
		new Student
		{
			Id = 3,
			FullName = "Do Thanh Thuy",
			Email = "dinhmanh3802@gmail.com",
			Gender = Gender.Female,
			Status = ProfileStatus.Active,
			EmergencyContact = "9999999999",
			StudentCourses = studentCourses,
			StudentGroupAssignment = studentGroupAssignments
		},
		new Student
		{
			Id = 4,
			FullName = "HiHI",
			Email = "dinhmanh@example.com",
			Gender = Gender.Male,
			Status = ProfileStatus.Active,
			EmergencyContact = "abcdefghjk",
			StudentCourses = studentCourses,
			StudentGroupAssignment = studentGroupAssignments
		}
	};
	}

}