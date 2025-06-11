using Moq;
using NUnit.Framework;
using AutoMapper;
using System;
using System.Threading.Tasks;
using SCCMS.Domain.DTOs.VolunteerDtos;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.UnitOfWork;
using SCCMS.Domain.Services.Implements;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.API.Services;
using System.Linq.Expressions;
using Utility;

[TestFixture]
public class VolunteerServiceTests
{
	private Mock<IUnitOfWork> _unitOfWorkMock;
	private Mock<IMapper> _mapperMock;
	private Mock<IBlobService> _blobServiceMock;
	private Mock<HttpClient> _httpClientMock;
	private Mock<IEmailService> _emailServiceMock;
	private VolunteerService _volunteerService;

	[SetUp]
	public void SetUp()
	{
		// Mock các phụ thuộc
		_unitOfWorkMock = new Mock<IUnitOfWork>();
		_mapperMock = new Mock<IMapper>();
		_blobServiceMock = new Mock<IBlobService>();
		_httpClientMock = new Mock<HttpClient>();
		_emailServiceMock = new Mock<IEmailService>();

		// Khởi tạo dịch vụ với các mock đã tạo
		_volunteerService = new VolunteerService(
			_unitOfWorkMock.Object,
			_mapperMock.Object,
			_blobServiceMock.Object,
			_httpClientMock.Object,
			_emailServiceMock.Object
		);
	}

	[Test]
	public void GetVolunteerByIdAsync_InvalidId_ThrowsArgumentException()
	{
		// Arrange
		var invalidId = 0; // ID không hợp lệ

		// Act & Assert
		var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
			await _volunteerService.GetVolunteerByIdAsync(invalidId)
		);
		Assert.AreEqual("Id không hợp lệ, phải lớn hơn 0.", ex.Message);
	}

	[Test]
	public void GetVolunteerByIdAsync_NonExistingId_ThrowsArgumentException()
	{
		// Arrange
		var nonExistingId = 999;

		// Mock với Expression<Func<Volunteer, bool>> cho filter
		_unitOfWorkMock.Setup(uow => uow.Volunteer.GetAsync(
				It.IsAny<Expression<Func<Volunteer, bool>>>(),  // filter
				It.IsAny<bool>(),                               // tracked (thực ra có thể để true hoặc false, mặc định là true)
				It.IsAny<string>()                              // includeProperties
			))
			.ReturnsAsync((Volunteer)null);  // Không tìm thấy tình nguyện viên với ID này

		// Act & Assert
		var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
			await _volunteerService.GetVolunteerByIdAsync(nonExistingId)
		);
		Assert.AreEqual($"Không tìm thấy tình nguyện viên với ID {nonExistingId}.", ex.Message);
	}
	[Test]
	public async Task GetVolunteerByIdAsync_ValidId_ReturnsVolunteerDto()
	{
		// Arrange
		var validId = 1;
		var volunteer = new Volunteer
		{
			Id = validId,
			FullName = "John Doe",  // Tên tình nguyện viên
			Gender = Gender.Male,
			NationalId = "123456789",
			Address = "123 Street",
			PhoneNumber = "123456789",
			Email = "john.doe@example.com",
			Status = ProfileStatus.Active
		};

		var volunteerDto = new VolunteerDto
		{
			Id = validId,
			FullName = "John Doe",
			Gender = Gender.Male,
			NationalId = "123456789",
			Address = "123 Street",
			PhoneNumber = "123456789",
			Email = "john.doe@example.com",
			Status = ProfileStatus.Active
		};

		// Mock trả về volunteer khi gọi GetAsync
		_unitOfWorkMock.Setup(uow => uow.Volunteer.GetAsync(It.IsAny<Expression<Func<Volunteer, bool>>>(), It.IsAny<bool>(), It.IsAny<string>()))
			.ReturnsAsync(volunteer);

		// Mock ánh xạ từ Volunteer thành VolunteerDto
		_mapperMock.Setup(m => m.Map<VolunteerDto>(It.IsAny<Volunteer>()))
			.Returns(volunteerDto);

		// Act
		var result = await _volunteerService.GetVolunteerByIdAsync(validId);

		// Assert
		Assert.IsInstanceOf<VolunteerDto>(result);
		Assert.AreEqual(validId, result.Id);
		Assert.AreEqual("John Doe", result.FullName);  // Kiểm tra tên
		Assert.AreEqual(Gender.Male, result.Gender);   // Kiểm tra giới tính
		Assert.AreEqual("john.doe@example.com", result.Email); // Kiểm tra email
	}
	[Test]
	public async Task GetVolunteersByCourseIdAsync_ValidCourseId_ReturnsVolunteerDtos()
	{
		// Arrange
		var validCourseId = 2;

		// Tạo ra các tình nguyện viên giả
		var volunteerCourse = new VolunteerCourse
		{
			Id = 1,
			CourseId = validCourseId,
			Volunteer = new Volunteer
			{
				Id = 1,
				FullName = "John Doe",
				Gender = Gender.Male,
				NationalId = "123456789",
				Address = "123 Street",
				PhoneNumber = "123456789",
				Email = "john.doe@example.com",
				Status = ProfileStatus.Active
			},
			VolunteerCode = "VC001"
		};

		var volunteerDtos = new List<VolunteerDto>
	{
		new VolunteerDto
		{
			Id = 1,
			FullName = "John Doe",
			Gender = Gender.Male,
			NationalId = "123456789",
			Address = "123 Street",
			PhoneNumber = "123456789",
			Email = "john.doe@example.com",
			Status = ProfileStatus.Active
		}
	};

		// Mock trả về một danh sách tình nguyện viên
		_unitOfWorkMock.Setup(uow => uow.VolunteerApplication.FindAsync(
				It.IsAny<Expression<Func<VolunteerCourse, bool>>>(),
				It.IsAny<string>())
			)
			.ReturnsAsync(new List<VolunteerCourse> { volunteerCourse });

		// Mock ánh xạ từ VolunteerCourse thành VolunteerDto
		_mapperMock.Setup(m => m.Map<IEnumerable<VolunteerDto>>(It.IsAny<IEnumerable<VolunteerCourse>>()))
			.Returns(volunteerDtos);

		// Act
		var result = await _volunteerService.GetVolunteersByCourseIdAsync(validCourseId);

		// Assert
		Assert.IsInstanceOf<IEnumerable<VolunteerDto>>(result);
		Assert.AreEqual(1, result.Count());  // Kiểm tra có 1 tình nguyện viên trong danh sách
		Assert.AreEqual("John Doe", result.First().FullName);  // Kiểm tra tên
		Assert.AreEqual("john.doe@example.com", result.First().Email); // Kiểm tra email
	}

	[Test]
	public void GetVolunteersByCourseIdAsync_InvalidCourseId_ThrowsArgumentException()
	{
		// Arrange
		var invalidCourseId = -1;  // CourseId không hợp lệ

		// Act & Assert
		var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
			await _volunteerService.GetVolunteersByCourseIdAsync(invalidCourseId)
		);
		Assert.AreEqual("courseId phải lớn hơn 0. (Parameter 'courseId')", ex.Message);  // Cập nhật thông báo lỗi
	}


}