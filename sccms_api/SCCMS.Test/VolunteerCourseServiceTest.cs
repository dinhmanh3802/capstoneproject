using AutoMapper;
using Moq;
using NUnit.Framework;
using SCCMS.API.Services;
using SCCMS.Domain.DTOs.VolunteerApplicationDtos;
using SCCMS.Domain.DTOs.VolunteerCourseDtos;  // Đảm bảo có DTO cho tình nguyện viên
using SCCMS.Domain.DTOs.VolunteerTeamDtos;
using SCCMS.Domain.Services.Implements;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Test
{
	[TestFixture]
	public class VolunteerApplicationServiceTests
	{
		private Mock<IUnitOfWork> _unitOfWorkMock;
		private Mock<IMapper> _mapperMock;
		private Mock<IEmailService> _emailServiceMock;
		private Mock<INotificationService> _notificationServiceMock;
		private VolunteerCourseService _service;
		private List<VolunteerCourse> _volunteerCourses;  // Giả sử có lớp VolunteerCourse cho tình nguyện viên
		private List<User> _users;

		[SetUp]
		public void SetUp()
		{
			_unitOfWorkMock = new Mock<IUnitOfWork>();
			_mapperMock = new Mock<IMapper>();
			_emailServiceMock = new Mock<IEmailService>();
			_notificationServiceMock = new Mock<INotificationService>();

			// Mock Volunteer và Course
			_volunteerCourses = new List<VolunteerCourse>
			{
				new VolunteerCourse
				{
					Id = 1,
					VolunteerId = 1,
					CourseId = 1,
					Volunteer = new Volunteer
					{
						Id = 1,
						FullName = "Nguyen Thi Lan",
						DateOfBirth = new DateTime(1995, 1, 1),
						Gender = Gender.Female,
						Email = "lanvolunteer@example.com",
						PhoneNumber = "1234567890",
						Address = "123 Street",
						NationalId = "ID123456",
						NationalImageFront = "front.png",
						NationalImageBack = "back.png",
						VolunteerTeam = new List<VolunteerTeam>  // Thêm VolunteerTeam giả
                        {
							new VolunteerTeam
							{
								TeamId = 1,
								Team = new Team
								{
									CourseId = 1,
									TeamName = "Team 1"
								}
							}
						}
					},
					Course = new Course
					{
						Id = 1,
						CourseName = "Volunteer Training 101",
						StartDate = DateTime.Now.AddDays(-10)
					},
					Status = ProgressStatus.Approved,
					VolunteerCode = "V001",
					ApplicationDate = DateTime.Now,
					ReviewerId = 10,
					Reviewer = new User
					{
						Id = 10,
						FullName = "Reviewer One",
						Email = "reviewer1@example.com",
						Gender = Gender.Male,  // Kiểm tra Gender trong Reviewer
					},
					ReviewDate = DateTime.Now
				},
				new VolunteerCourse
				{
					Id = 1,
					VolunteerId = 2, // VolunteerId = 2
                    CourseId = 1,
					Volunteer = new Volunteer
					{
						Id = 2, // VolunteerId = 2
                        FullName = "Nguyen Thi Lan",
						DateOfBirth = new DateTime(1995, 1, 1),
						Gender = Gender.Female,
						Email = "lanvolunteer@example.com",
						PhoneNumber = "1234567890",
						Address = "123 Street",
						NationalId = "ID123456",
						NationalImageFront = "front.png",
						NationalImageBack = "back.png",
					},
					Course = new Course
					{
						Id = 1,
						CourseName = "Volunteer Training 101",
						StartDate = DateTime.Now.AddDays(-10)
					},
					Status = ProgressStatus.Pending,
					VolunteerCode = null,
					ApplicationDate = DateTime.Now,
					ReviewerId = 10,
					Reviewer = new User
					{
						Id = 10,
						FullName = "Reviewer One",
						Email = "reviewer1@example.com",
						Gender = Gender.Male
					},
					ReviewDate = DateTime.Now
				}

			};


			// Mock phương thức FindAsync

			// Mock phương thức GetByIdAsync (sửa lại để trả về dữ liệu mock đúng)
			_unitOfWorkMock.Setup(uow => uow.VolunteerApplication.GetByIdAsync(It.IsAny<int>(), It.IsAny<string>()))
				.ReturnsAsync((int id, string includes) => _volunteerCourses.FirstOrDefault(vc => vc.Id == id)); // Trả về VolunteerCourse nếu id trùng khớp
																												 // Mock phương thức GetAsync để trả về tình nguyện viên với Id = 1
																												 // Mock phương thức GetAsync để trả về tình nguyện viên với Id = 1
			_unitOfWorkMock.Setup(uow => uow.VolunteerApplication.GetAsync(It.IsAny<Expression<Func<VolunteerCourse, bool>>>(), It.IsAny<bool>(), It.IsAny<string>()))
				.ReturnsAsync((Expression<Func<VolunteerCourse, bool>> filter, bool tracked, string includeProperties) =>
				{
					return _volunteerCourses.FirstOrDefault(filter.Compile()); // Trả về đối tượng đầu tiên tìm thấy theo filter
				});


			// Khởi tạo _service với các mock dependencies
			_service = new VolunteerCourseService(
				_unitOfWorkMock.Object,
				_mapperMock.Object,
				_emailServiceMock.Object,
				_notificationServiceMock.Object);
		}

		[Test]
		public async Task GetAllVolunteerApplicationAsync_WhenNoFilters_ReturnsAllVolunteerApplications()
		{
			_unitOfWorkMock.Setup(uow => uow.VolunteerApplication.FindAsync(
				It.IsAny<Expression<Func<VolunteerCourse, bool>>>(),
				It.IsAny<string>()))
				.ReturnsAsync(new List<VolunteerCourse> { new VolunteerCourse
				{
					Id = 1,
					VolunteerId = 1,
					CourseId = 1,
					Volunteer = new Volunteer
					{
						Id = 1,
						FullName = "Nguyen Thi Lan",
						DateOfBirth = new DateTime(1995, 1, 1),
						Gender = Gender.Female,
						Email = "lanvolunteer@example.com",
						PhoneNumber = "1234567890",
						Address = "123 Street",
						NationalId = "ID123456",
						NationalImageFront = "front.png",
						NationalImageBack = "back.png",
						VolunteerTeam = new List<VolunteerTeam>  // Thêm VolunteerTeam giả
                        {
							new VolunteerTeam
							{
								TeamId = 1,
								Team = new Team
								{
									CourseId = 1,
									TeamName = "Team 1"
								}
							}
						}
					},
					Course = new Course
					{
						Id = 1,
						CourseName = "Volunteer Training 101",
						StartDate = DateTime.Now.AddDays(-10)
					},
					Status = ProgressStatus.Approved,
					VolunteerCode = "V001",
					ApplicationDate = DateTime.Now,
					ReviewerId = 10,
					Reviewer = new User
					{
						Id = 10,
						FullName = "Reviewer One",
						Email = "reviewer1@example.com",
						Gender = Gender.Male,  // Kiểm tra Gender trong Reviewer
					},
					ReviewDate = DateTime.Now
				}});
			// Act
			var result = await _service.GetVolunteerApplicationAsync(
				courseId: 1, // Example: Provide a valid courseId
				name: null,  // Correct parameter name is 'name' (not 'volunteerName')
				gender: null,
				phoneNumber: null,
				status: null,
				reviewerId: null,
				startDob: null,
				endDob: null,
				teamId: null,
				volunteerCode: null
			);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count()); // Since we only have one volunteer in this example

			var resultList = result.ToList();

			// Kiểm tra các thuộc tính cụ thể
			Assert.AreEqual("Nguyen Thi Lan", resultList[0].Volunteer.FullName);
			Assert.AreEqual("Volunteer Training 101", resultList[0].Course.CourseName);
		}
		[Test]
		public async Task GetAllVolunteerApplicationAsync_WhenNameIsNguyenThiLan_ReturnsCorrectVolunteer()
		{
			_unitOfWorkMock.Setup(uow => uow.VolunteerApplication.FindAsync(
				It.IsAny<Expression<Func<VolunteerCourse, bool>>>(),
				It.IsAny<string>()))
				.ReturnsAsync(new List<VolunteerCourse> { new VolunteerCourse
				{
					Id = 1,
					VolunteerId = 1,
					CourseId = 1,
					Volunteer = new Volunteer
					{
						Id = 1,
						FullName = "Nguyen Thi Lan",
						DateOfBirth = new DateTime(1995, 1, 1),
						Gender = Gender.Female,
						Email = "lanvolunteer@example.com",
						PhoneNumber = "1234567890",
						Address = "123 Street",
						NationalId = "ID123456",
						NationalImageFront = "front.png",
						NationalImageBack = "back.png",
						VolunteerTeam = new List<VolunteerTeam>  // Thêm VolunteerTeam giả
                        {
							new VolunteerTeam
							{
								TeamId = 1,
								Team = new Team
								{
									CourseId = 1,
									TeamName = "Team 1"
								}
							}
						}
					},
					Course = new Course
					{
						Id = 1,
						CourseName = "Volunteer Training 101",
						StartDate = DateTime.Now.AddDays(-10)
					},
					Status = ProgressStatus.Approved,
					VolunteerCode = "V001",
					ApplicationDate = DateTime.Now,
					ReviewerId = 10,
					Reviewer = new User
					{
						Id = 10,
						FullName = "Reviewer One",
						Email = "reviewer1@example.com",
						Gender = Gender.Male,  // Kiểm tra Gender trong Reviewer
					},
					ReviewDate = DateTime.Now
				}});
			// Act
			var result = await _service.GetVolunteerApplicationAsync(
				courseId: 1,
				name: "Nguyen Thi Lan", // Name filter
				gender: null,
				phoneNumber: null,
				status: null,
				reviewerId: null,
				startDob: null,
				endDob: null,
				teamId: null,
				volunteerCode: null
			);

			// Assert
			Assert.IsNotNull(result);
			var resultList = result.ToList();
			Assert.AreEqual(1, resultList.Count()); // Should return exactly one result

			Assert.AreEqual("Nguyen Thi Lan", resultList[0].Volunteer.FullName);
		}

		[Test]
		public async Task GetAllVolunteerApplicationAsync_WhenNameIsNonExistent_ReturnsNoResults()
		{
			// Act
			var result = await _service.GetVolunteerApplicationAsync(
				courseId: 1,
				name: "NonExistentName", // Non-existent name
				gender: null,
				phoneNumber: null,
				status: null,
				reviewerId: null,
				startDob: null,
				endDob: null,
				teamId: null,
				volunteerCode: null
			);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(0, result.Count()); // Should return no results
		}
		[Test]
		public async Task GetAllVolunteerApplicationAsync_WithMultipleFilters_ReturnsFilteredResults()
		{

			_unitOfWorkMock.Setup(uow => uow.VolunteerApplication.FindAsync(
				It.IsAny<Expression<Func<VolunteerCourse, bool>>>(),
				It.IsAny<string>()))
				.ReturnsAsync(new List<VolunteerCourse> { new VolunteerCourse
				{
					Id = 1,
					VolunteerId = 1,
					CourseId = 1,
					Volunteer = new Volunteer
					{
						Id = 1,
						FullName = "Nguyen Thi Lan",
						DateOfBirth = new DateTime(1995, 1, 1),
						Gender = Gender.Female,
						Email = "lanvolunteer@example.com",
						PhoneNumber = "1234567890",
						Address = "123 Street",
						NationalId = "ID123456",
						NationalImageFront = "front.png",
						NationalImageBack = "back.png",
						VolunteerTeam = new List<VolunteerTeam>  // Thêm VolunteerTeam giả
                        {
							new VolunteerTeam
							{
								TeamId = 1,
								Team = new Team
								{
									CourseId = 1,
									TeamName = "Team 1"
								}
							}
						}
					},
					Course = new Course
					{
						Id = 1,
						CourseName = "Volunteer Training 101",
						StartDate = DateTime.Now.AddDays(-10)
					},
					Status = ProgressStatus.Approved,
					VolunteerCode = "V001",
					ApplicationDate = DateTime.Now,
					ReviewerId = 10,
					Reviewer = new User
					{
						Id = 10,
						FullName = "Reviewer One",
						Email = "reviewer1@example.com",
						Gender = Gender.Male,  // Kiểm tra Gender trong Reviewer
					},
					ReviewDate = DateTime.Now
				}});

			// Act
			var result = await _service.GetVolunteerApplicationAsync(
				courseId: 1,
				name: "Nguyen Thi Lan", // Filter by name
				gender: Gender.Female, // Filter by gender
				phoneNumber: "1234567890", // Filter by phone number
				status: null,
				reviewerId: null,
				startDob: null,
				endDob: null,
				teamId: null,
				volunteerCode: null
			);

			// Assert
			Assert.IsNotNull(result);
			var resultList = result.ToList();
			Assert.AreEqual(1, resultList.Count()); // Only one volunteer matches the filters

			Assert.AreEqual("Nguyen Thi Lan", resultList[0].Volunteer.FullName);
			Assert.AreEqual(Gender.Female, resultList[0].Volunteer.Gender);
			Assert.AreEqual("1234567890", resultList[0].Volunteer.PhoneNumber);
		}
		[Test]
		public async Task GetAllVolunteerApplicationAsync_WhenPhoneNumberIsInvalid_ReturnsNoResults()
		{
			// Act
			var result = await _service.GetVolunteerApplicationAsync(
				courseId: 1,
				name: null,
				gender: null,
				phoneNumber: "abcdefghjk", // Invalid phone number
				status: null,
				reviewerId: null,
				startDob: null,
				endDob: null,
				teamId: null,
				volunteerCode: null
			);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(0, result.Count()); // Expect no results due to invalid phone number
		}
		[Test]
		public async Task GetAllVolunteerApplicationAsync_WhenGenderIsMale_ReturnsCorrectVolunteer()
		{
			// Act
			var result = await _service.GetVolunteerApplicationAsync(
				courseId: 1,
				name: null,
				gender: Gender.Male, // Filter by Male gender
				phoneNumber: null,
				status: null,
				reviewerId: null,
				startDob: null,
				endDob: null,
				teamId: null,
				volunteerCode: null
			);

			// Assert
			Assert.IsNotNull(result);
			var resultList = result.ToList();
			Assert.AreEqual(0, resultList.Count()); // No male volunteers in the mock data
		}
		[Test]
		public async Task GetAllVolunteerApplicationAsync_WhenGenderIsFemale_ReturnsCorrectVolunteer()
		{
			_unitOfWorkMock.Setup(uow => uow.VolunteerApplication.FindAsync(
				It.IsAny<Expression<Func<VolunteerCourse, bool>>>(),
				It.IsAny<string>()))
				.ReturnsAsync(new List<VolunteerCourse> { new VolunteerCourse
				{
					Id = 1,
					VolunteerId = 1,
					CourseId = 1,
					Volunteer = new Volunteer
					{
						Id = 1,
						FullName = "Nguyen Thi Lan",
						DateOfBirth = new DateTime(1995, 1, 1),
						Gender = Gender.Female,
						Email = "lanvolunteer@example.com",
						PhoneNumber = "1234567890",
						Address = "123 Street",
						NationalId = "ID123456",
						NationalImageFront = "front.png",
						NationalImageBack = "back.png",
						VolunteerTeam = new List<VolunteerTeam>  // Thêm VolunteerTeam giả
                        {
							new VolunteerTeam
							{
								TeamId = 1,
								Team = new Team
								{
									CourseId = 1,
									TeamName = "Team 1"
								}
							}
						}
					},
					Course = new Course
					{
						Id = 1,
						CourseName = "Volunteer Training 101",
						StartDate = DateTime.Now.AddDays(-10)
					},
					Status = ProgressStatus.Approved,
					VolunteerCode = "V001",
					ApplicationDate = DateTime.Now,
					ReviewerId = 10,
					Reviewer = new User
					{
						Id = 10,
						FullName = "Reviewer One",
						Email = "reviewer1@example.com",
						Gender = Gender.Male,  // Kiểm tra Gender trong Reviewer
					},
					ReviewDate = DateTime.Now
				}});
			// Act
			var result = await _service.GetVolunteerApplicationAsync(
				courseId: 1,
				name: null,
				gender: Gender.Female, // Filter by Female gender
				phoneNumber: null,
				status: null,
				reviewerId: null,
				startDob: null,
				endDob: null,
				teamId: null,
				volunteerCode: null
			);

			// Assert
			Assert.IsNotNull(result);
			var resultList = result.ToList();
			Assert.AreEqual(1, resultList.Count()); // Expected 1, because we have one Female volunteer in mock data
		}

		[Test]
		public async Task GetAllVolunteerApplicationAsync_WhenCourseIdIsZero_ThrowsArgumentException()
		{
			// Act & Assert
			var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
				await _service.GetVolunteerApplicationAsync(
					courseId: 0,  // Invalid courseId
					name: null,
					gender: null,
					phoneNumber: null,
					status: null,
					reviewerId: null,
					startDob: null,
					endDob: null,
					teamId: null,
					volunteerCode: null
				)
			);

			// Assert
			Assert.AreEqual("CourseId không hợp lệ. Phải lớn hơn 0.", ex.Message);  // Kiểm tra thông báo lỗi
		}
		[Test]
		public async Task GetAllVolunteerApplicationAsync_WhenCourseIdIsNegative_ThrowsArgumentException()
		{
			// Act & Assert
			var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
				await _service.GetVolunteerApplicationAsync(
					courseId: -1,  // Invalid courseId = -1
					name: null,
					gender: null,
					phoneNumber: null,
					status: null,
					reviewerId: null,
					startDob: null,
					endDob: null,
					teamId: null,
					volunteerCode: null
				)
			);

			// Assert
			Assert.AreEqual("CourseId không hợp lệ. Phải lớn hơn 0.", ex.Message);  // Kiểm tra thông báo lỗi
		}
		[Test]
		public async Task GetAllVolunteerApplicationAsync_WhenPhoneNumberIsShorterThanh10Digit_ReturnsNoResults()
		{
			// Act
			var result = await _service.GetVolunteerApplicationAsync(
				courseId: 1,
				name: null,
				gender: null,
				phoneNumber: "012345",  // Số điện thoại không hợp lệ
				status: null,
				reviewerId: null,
				startDob: null,
				endDob: null,
				teamId: null,
				volunteerCode: null
			);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(0, result.Count()); // Kết quả nên là 0 nếu không có số điện thoại phù hợp
		}


		[Test]
		public void GetVolunteerApplicationAsync_ThrowsArgumentException_WhenCourseIdIsInvalid()
		{
			// Arrange: Thiết lập CourseId không hợp lệ
			var invalidCourseId = -1;

			// Act & Assert: Kiểm tra xem hàm có ném ra ArgumentException hay không
			Assert.ThrowsAsync<ArgumentException>(() => _service.GetVolunteerApplicationAsync(invalidCourseId));
		}

		[Test]
		public async Task GetVolunteerApplicationAsync_HandlesNoResults()
		{
			// Arrange: Không có dữ liệu nào thỏa mãn
			var courseId = 1;
			_unitOfWorkMock.Setup(u => u.VolunteerApplication.FindAsync(It.IsAny<Expression<Func<VolunteerCourse, bool>>>(), It.IsAny<string>()))
				.ReturnsAsync(Enumerable.Empty<VolunteerCourse>().AsQueryable());

			// Act: Gọi hàm
			var result = await _service.GetVolunteerApplicationAsync(courseId);

			// Assert: Kiểm tra kết quả trả về là một danh sách rỗng
			Assert.IsEmpty(result);
		}
		[Test]
		public void GetVolunteerApplicationAsync_ThrowsArgumentException_WhenReviewerIdIsInvalid()
		{
			// Arrange: Thiết lập ReviewerId không hợp lệ
			var invalidReviewerId = -1;
			var courseId = 1;

			// Act & Assert: Kiểm tra xem hàm có ném ra ArgumentException khi ReviewerId không hợp lệ hay không
			Assert.ThrowsAsync<ArgumentException>(() => _service.GetVolunteerApplicationAsync(courseId, reviewerId: invalidReviewerId));
		}
		[Test]
		public async Task GetVolunteerApplicationAsync_ReturnsEmptyList_WhenNoResultsMatchFilters()
		{
			// Arrange: Thiết lập khóa học và lọc không có kết quả
			var courseId = 1;
			var volunteerNameFilter = "Nguyen Thi Mai";

			_unitOfWorkMock.Setup(u => u.VolunteerApplication.FindAsync(It.IsAny<Expression<Func<VolunteerCourse, bool>>>(), It.IsAny<string>()))
				.ReturnsAsync(Enumerable.Empty<VolunteerCourse>().AsQueryable());

			// Act: Gọi hàm
			var result = await _service.GetVolunteerApplicationAsync(courseId, name: volunteerNameFilter);

			// Assert: Kiểm tra kết quả trả về là một danh sách rỗng
			Assert.IsEmpty(result);
		}
		[Test]
		public async Task GetAllVolunteerApplicationAsync_WhenDobIsValid_ReturnsCorrectResults()
		{
			_unitOfWorkMock.Setup(uow => uow.VolunteerApplication.FindAsync(
				It.IsAny<Expression<Func<VolunteerCourse, bool>>>(),
				It.IsAny<string>()))
				.ReturnsAsync(new List<VolunteerCourse> { new VolunteerCourse
				{
					Id = 1,
					VolunteerId = 1,
					CourseId = 1,
					Volunteer = new Volunteer
					{
						Id = 1,
						FullName = "Nguyen Thi Lan",
						DateOfBirth = new DateTime(1995, 1, 1),
						Gender = Gender.Female,
						Email = "lanvolunteer@example.com",
						PhoneNumber = "1234567890",
						Address = "123 Street",
						NationalId = "ID123456",
						NationalImageFront = "front.png",
						NationalImageBack = "back.png",
						VolunteerTeam = new List<VolunteerTeam>  // Thêm VolunteerTeam giả
                        {
							new VolunteerTeam
							{
								TeamId = 1,
								Team = new Team
								{
									CourseId = 1,
									TeamName = "Team 1"
								}
							}
						}
					},
					Course = new Course
					{
						Id = 1,
						CourseName = "Volunteer Training 101",
						StartDate = DateTime.Now.AddDays(-10)
					},
					Status = ProgressStatus.Approved,
					VolunteerCode = "V001",
					ApplicationDate = DateTime.Now,
					ReviewerId = 10,
					Reviewer = new User
					{
						Id = 10,
						FullName = "Reviewer One",
						Email = "reviewer1@example.com",
						Gender = Gender.Male,  // Kiểm tra Gender trong Reviewer
					},
					ReviewDate = DateTime.Now
				}});
			// Act
			var result = await _service.GetVolunteerApplicationAsync(
				courseId: 1,
				name: null,
				gender: null,
				phoneNumber: null,
				status: null,
				reviewerId: null,
				startDob: new DateTime(1995, 1, 1),  // Tìm kiếm với ngày sinh là 01/01/1995
				endDob: new DateTime(1995, 1, 1),    // Ngày sinh chính xác
				teamId: null,
				volunteerCode: null
			);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count()); // Dự kiến có 1 tình nguyện viên có ngày sinh là 01/01/1995

			var resultList = result.ToList();

			// Kiểm tra ngày sinh của tình nguyện viên trong kết quả
			Assert.AreEqual(new DateTime(1995, 1, 1), resultList[0].Volunteer.DateOfBirth);
		}
		[Test]
		public async Task GetAllVolunteerApplicationAsync_WhenDobIsOutOfRange_ReturnsNoResults()
		{
			// Act
			var result = await _service.GetVolunteerApplicationAsync(
				courseId: 1,
				name: null,
				gender: null,
				phoneNumber: null,
				status: null,
				reviewerId: null,
				startDob: new DateTime(2000, 1, 1),  // Ngày sinh sau 01/01/2000
				endDob: null,
				teamId: null,
				volunteerCode: null
			);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(0, result.Count()); // Không có tình nguyện viên nào có ngày sinh sau 01/01/2000
		}

		[Test]
		public async Task GetAllVolunteerApplicationAsync_WhenDobIsInRange_ReturnsCorrectResults()
		{
			_unitOfWorkMock.Setup(uow => uow.VolunteerApplication.FindAsync(
				It.IsAny<Expression<Func<VolunteerCourse, bool>>>(),
				It.IsAny<string>()))
				.ReturnsAsync(new List<VolunteerCourse> { new VolunteerCourse
				{
					Id = 1,
					VolunteerId = 1,
					CourseId = 1,
					Volunteer = new Volunteer
					{
						Id = 1,
						FullName = "Nguyen Thi Lan",
						DateOfBirth = new DateTime(1995, 1, 1),
						Gender = Gender.Female,
						Email = "lanvolunteer@example.com",
						PhoneNumber = "1234567890",
						Address = "123 Street",
						NationalId = "ID123456",
						NationalImageFront = "front.png",
						NationalImageBack = "back.png",
						VolunteerTeam = new List<VolunteerTeam>  // Thêm VolunteerTeam giả
                        {
							new VolunteerTeam
							{
								TeamId = 1,
								Team = new Team
								{
									CourseId = 1,
									TeamName = "Team 1"
								}
							}
						}
					},
					Course = new Course
					{
						Id = 1,
						CourseName = "Volunteer Training 101",
						StartDate = DateTime.Now.AddDays(-10)
					},
					Status = ProgressStatus.Approved,
					VolunteerCode = "V001",
					ApplicationDate = DateTime.Now,
					ReviewerId = 10,
					Reviewer = new User
					{
						Id = 10,
						FullName = "Reviewer One",
						Email = "reviewer1@example.com",
						Gender = Gender.Male,  // Kiểm tra Gender trong Reviewer
					},
					ReviewDate = DateTime.Now
				}});
			// Act
			var result = await _service.GetVolunteerApplicationAsync(
				courseId: 1,
				name: null,
				gender: null,
				phoneNumber: null,
				status: null,
				reviewerId: null,
				startDob: new DateTime(1990, 1, 1),  // Tìm kiếm với ngày sinh từ 01/01/1990
				endDob: new DateTime(2000, 12, 31),  // Đến 31/12/2000
				teamId: null,
				volunteerCode: null
			);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count()); // Dự kiến có 1 tình nguyện viên có ngày sinh trong khoảng thời gian này

			var resultList = result.ToList();

			// Kiểm tra ngày sinh của tình nguyện viên trong kết quả
			Assert.IsTrue(resultList[0].Volunteer.DateOfBirth >= new DateTime(1990, 1, 1) && resultList[0].Volunteer.DateOfBirth <= new DateTime(2000, 12, 31));
		}
		[Test]
		public async Task GetAllVolunteerApplicationAsync_WhenReviewerIdIsValid_ReturnsCorrectResults()
		{
			_unitOfWorkMock.Setup(uow => uow.VolunteerApplication.FindAsync(
				It.IsAny<Expression<Func<VolunteerCourse, bool>>>(),
				It.IsAny<string>()))
				.ReturnsAsync(new List<VolunteerCourse> { new VolunteerCourse
				{
					Id = 1,
					VolunteerId = 1,
					CourseId = 1,
					Volunteer = new Volunteer
					{
						Id = 1,
						FullName = "Nguyen Thi Lan",
						DateOfBirth = new DateTime(1995, 1, 1),
						Gender = Gender.Female,
						Email = "lanvolunteer@example.com",
						PhoneNumber = "1234567890",
						Address = "123 Street",
						NationalId = "ID123456",
						NationalImageFront = "front.png",
						NationalImageBack = "back.png",
						VolunteerTeam = new List<VolunteerTeam>  // Thêm VolunteerTeam giả
                        {
							new VolunteerTeam
							{
								TeamId = 1,
								Team = new Team
								{
									CourseId = 1,
									TeamName = "Team 1"
								}
							}
						}
					},
					Course = new Course
					{
						Id = 1,
						CourseName = "Volunteer Training 101",
						StartDate = DateTime.Now.AddDays(-10)
					},
					Status = ProgressStatus.Approved,
					VolunteerCode = "V001",
					ApplicationDate = DateTime.Now,
					ReviewerId = 10,
					Reviewer = new User
					{
						Id = 10,
						FullName = "Reviewer One",
						Email = "reviewer1@example.com",
						Gender = Gender.Male,  // Kiểm tra Gender trong Reviewer
					},
					ReviewDate = DateTime.Now
				}});
			// Act
			var result = await _service.GetVolunteerApplicationAsync(
				courseId: 1,
				name: null,
				gender: null,
				phoneNumber: null,
				status: null,
				reviewerId: 10,  // Valid ReviewerId
				startDob: null,
				endDob: null,
				teamId: null,
				volunteerCode: null
			);

			// Assert
			Assert.IsNotNull(result);
			var resultList = result.ToList();
			Assert.AreEqual(1, resultList.Count()); // Expected 1 if we have a volunteer with reviewerId = 10 in mock data
			Assert.AreEqual(10, resultList[0].ReviewerId); // Check that ReviewerId matches the expected value
		}
		[Test]
		public async Task GetAllVolunteerApplicationAsync_WhenReviewerIdIsInvalid_ReturnsNoResults()
		{
			// Act
			var result = await _service.GetVolunteerApplicationAsync(
				courseId: 1,
				name: null,
				gender: null,
				phoneNumber: null,
				status: null,
				reviewerId: 999,  // Invalid ReviewerId, assuming no reviewer with ID 999 exists in mock data
				startDob: null,
				endDob: null,
				teamId: null,
				volunteerCode: null
			);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(0, result.Count()); // Expected no results since there is no volunteer with ReviewerId = 999
		}
		[Test]
		public void GetAllVolunteerApplicationAsync_WhenReviewerIdIsZero_ThrowsArgumentException()
		{
			// Act & Assert
			var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
				await _service.GetVolunteerApplicationAsync(
					courseId: 1,
					name: null,
					gender: null,
					phoneNumber: null,
					status: null,
					reviewerId: 0,  // Invalid ReviewerId
					startDob: null,
					endDob: null,
					teamId: null,
					volunteerCode: null
				)
			);

			Assert.AreEqual("ReviewerId không hợp lệ. Phải lớn hơn 0.", ex.Message);
		}
		[Test]
		public void GetAllVolunteerApplicationAsync_WhenReviewerIdIsNegative_ThrowsArgumentException()
		{
			// Act & Assert
			var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
				await _service.GetVolunteerApplicationAsync(
					courseId: 1,
					name: null,
					gender: null,
					phoneNumber: null,
					status: null,
					reviewerId: -1,  // Invalid ReviewerId (Negative value)
					startDob: null,
					endDob: null,
					teamId: null,
					volunteerCode: null
				)
			);

			Assert.AreEqual("ReviewerId không hợp lệ. Phải lớn hơn 0.", ex.Message);
		}
		[Test]
		public async Task GetAllVolunteerApplicationAsync_WhenVolunteerFieldsAreValid_ReturnsCorrectVolunteer()
		{
			_unitOfWorkMock.Setup(uow => uow.VolunteerApplication.FindAsync(
				It.IsAny<Expression<Func<VolunteerCourse, bool>>>(),
				It.IsAny<string>()))
				.ReturnsAsync(new List<VolunteerCourse> { new VolunteerCourse
				{
					Id = 1,
					VolunteerId = 1,
					CourseId = 1,
					Volunteer = new Volunteer
					{
						Id = 1,
						FullName = "Nguyen Thi Lan",
						DateOfBirth = new DateTime(1995, 1, 1),
						Gender = Gender.Female,
						Email = "lanvolunteer@example.com",
						PhoneNumber = "1234567890",
						Address = "123 Street",
						NationalId = "ID123456",
						NationalImageFront = "front.png",
						NationalImageBack = "back.png",
						VolunteerTeam = new List<VolunteerTeam>  // Thêm VolunteerTeam giả
                        {
							new VolunteerTeam
							{
								TeamId = 1,
								Team = new Team
								{
									CourseId = 1,
									TeamName = "Team 1"
								}
							}
						}
					},
					Course = new Course
					{
						Id = 1,
						CourseName = "Volunteer Training 101",
						StartDate = DateTime.Now.AddDays(-10)
					},
					Status = ProgressStatus.Approved,
					VolunteerCode = "V001",
					ApplicationDate = DateTime.Now,
					ReviewerId = 10,
					Reviewer = new User
					{
						Id = 10,
						FullName = "Reviewer One",
						Email = "reviewer1@example.com",
						Gender = Gender.Male,  // Kiểm tra Gender trong Reviewer
					},
					ReviewDate = DateTime.Now
				}});

			// Act
			var result = await _service.GetVolunteerApplicationAsync(
				courseId: 1,
				name: "Nguyen Thi Lan",  // Filter by valid name
				gender: Gender.Female,   // Valid gender (Female)
				phoneNumber: "1234567890",  // Valid phone number
				status: ProgressStatus.Approved, // Valid status (Approved)
				reviewerId: 10,  // Valid ReviewerId
				startDob: new DateTime(1990, 1, 1),  // Valid start date of birth
				endDob: new DateTime(2000, 12, 31),  // Valid end date of birth
				teamId: null,  // No team filter
				volunteerCode: "V001"  // Valid volunteer code
			);

			// Assert
			Assert.IsNotNull(result);
			var resultList = result.ToList();

			// Check that the volunteer in the result matches the mock data
			Assert.AreEqual(1, resultList.Count()); // Expect 1 result since we only have 1 volunteer in mock data

			var volunteer = resultList[0].Volunteer;

			// Check individual fields for the volunteer
			Assert.AreEqual("Nguyen Thi Lan", volunteer.FullName);
			Assert.AreEqual("1234567890", volunteer.PhoneNumber);
			Assert.AreEqual(Gender.Female, volunteer.Gender);
			Assert.AreEqual("V001", resultList[0].VolunteerCode);
			Assert.AreEqual(ProgressStatus.Approved, resultList[0].Status);
			Assert.AreEqual(10, resultList[0].ReviewerId); // Ensure the reviewer ID is correct
		}
		[Test]
		public void UpdateVolunteerCourseAsync_WhenIdsIsNull_ThrowsArgumentException()
		{
			// Arrange
			var updateDto = new VolunteerCourseUpdateDto
			{
				Ids = null // or new List<int>()
			};

			// Act & Assert
			var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
				await _service.UpdateVolunteerCourseAsync(updateDto)
			);

			Assert.AreEqual("Id không được để trống hoặc null.", ex.Message);
		}

		[Test]
		public void UpdateVolunteerCourseAsync_WhenReviewerIdIsLessThanZero_ThrowsArgumentException()
		{
			// Arrange
			var updateDto = new VolunteerCourseUpdateDto
			{
				Ids = new List<int> { 1 },
				ReviewerId = -1
			};

			// Act & Assert
			var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
				await _service.UpdateVolunteerCourseAsync(updateDto)
			);

			Assert.AreEqual("Reviewer Id phải lớn hơn 0.", ex.Message);
		}
		[Test]
		public void GetVolunteerCourseByIdAsync_InvalidId_ThrowsArgumentException()
		{
			// Act & Assert
			var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
				await _service.GetVolunteerCourseByIdAsync(0));  // Id = 0
			Assert.AreEqual("Id phải lớn hơn 0.", ex.Message);
		}
			[Test]
			public async Task GetVolunteerCourseByIdAsync_ValidId_ReturnsVolunteerCourseDto()
			{
				// Arrange
				int validId = 1;
				var volunteerCourse = _volunteerCourses.First(vc => vc.Id == validId);

				// Mock AutoMapper để ánh xạ VolunteerCourse sang VolunteerCourseDto
				var volunteerCourseDto = new VolunteerCourseDto
				{
					Id = volunteerCourse.Id,
					VolunteerCode = volunteerCourse.VolunteerCode,
					Status = volunteerCourse.Status,
					ReviewerId = volunteerCourse.ReviewerId,
					Reviewer = new ReviewerInforDto
					{
						Id = volunteerCourse.ReviewerId.Value,
						FullName = volunteerCourse.Reviewer.FullName,
						UserName = volunteerCourse.Reviewer.Email, // Giả sử ReviewerInforDto có Email và UserName
						Email = volunteerCourse.Reviewer.Email,
						PhoneNumber = volunteerCourse.Reviewer.PhoneNumber,
						Gender = volunteerCourse.Reviewer.Gender.Value,
					}
				};

				_mapperMock.Setup(m => m.Map<VolunteerCourseDto>(It.IsAny<VolunteerCourse>()))
					.Returns(volunteerCourseDto);

				// Act
				var result = await _service.GetVolunteerCourseByIdAsync(validId);

				// Assert
				Assert.IsNotNull(result);
				Assert.AreEqual(validId, result.Id);
				Assert.AreEqual(volunteerCourseDto.VolunteerCode, result.VolunteerCode);
				Assert.AreEqual(volunteerCourseDto.Status, result.Status);
				Assert.AreEqual(volunteerCourseDto.ReviewerId, result.ReviewerId);
				Assert.AreEqual(volunteerCourseDto.Reviewer.FullName, result.Reviewer.FullName);  // Kiểm tra thông tin Reviewer
			}
		[Test]
		public void GetVolunteerCourseByIdAsync_IdNotFound_ThrowsKeyNotFoundException()
		{
			// Arrange
			int invalidId = 999;  // ID không tồn tại

			// Mock phương thức GetAsync để trả về null khi tìm không thấy
			_unitOfWorkMock.Setup(uow => uow.VolunteerApplication.GetAsync(It.IsAny<Expression<Func<VolunteerCourse, bool>>>(), It.IsAny<bool>(), It.IsAny<string>()))
				.ReturnsAsync((VolunteerCourse)null);

			// Act & Assert
			var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
				await _service.GetVolunteerCourseByIdAsync(invalidId));  // ID không tồn tại
			Assert.AreEqual($"Không tìm thấy tình nguyện viên với ID {invalidId}.", ex.Message);
		}
		[Test]
		public async Task GetVolunteerCourseByIdAsync_ValidId_NoReviewer_ReturnsVolunteerCourseDtoWithoutReviewer()
		{
			// Arrange
			int validId = 1;
			var volunteerCourse = _volunteerCourses.First(vc => vc.Id == validId);

			// Giả sử VolunteerCourse không có thông tin Reviewer
			volunteerCourse.Reviewer = null;

			// Mock AutoMapper để ánh xạ VolunteerCourse sang VolunteerCourseDto
			var volunteerCourseDto = new VolunteerCourseDto
			{
				Id = volunteerCourse.Id,
				VolunteerCode = volunteerCourse.VolunteerCode,
				Status = volunteerCourse.Status,
				ReviewerId = volunteerCourse.ReviewerId,
				Reviewer = null  // Không có Reviewer
			};

			_mapperMock.Setup(m => m.Map<VolunteerCourseDto>(It.IsAny<VolunteerCourse>()))
				.Returns(volunteerCourseDto);

			// Act
			var result = await _service.GetVolunteerCourseByIdAsync(validId);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(validId, result.Id);
			Assert.AreEqual(volunteerCourseDto.VolunteerCode, result.VolunteerCode);
			Assert.AreEqual(volunteerCourseDto.Status, result.Status);
			Assert.AreEqual(volunteerCourseDto.ReviewerId, result.ReviewerId);
			Assert.IsNull(result.Reviewer);  // Kiểm tra không có Reviewer
		}
		[Test]
		public async Task GetVolunteerCourseByIdAsync_ValidId_ReturnsVolunteerCourseWithFullInformation()
		{
			// Arrange
			int validId = 1;
			var volunteerCourse = _volunteerCourses.First(vc => vc.Id == validId);

			var volunteerCourseDto = new VolunteerCourseDto
			{
				Id = volunteerCourse.Id,
				VolunteerCode = volunteerCourse.VolunteerCode,
				Status = volunteerCourse.Status,
				ReviewerId = volunteerCourse.ReviewerId,
				Reviewer = new ReviewerInforDto
				{
					Id = volunteerCourse.ReviewerId.Value,
					FullName = volunteerCourse.Reviewer.FullName,
					UserName = volunteerCourse.Reviewer.Email,
					Email = volunteerCourse.Reviewer.Email,
					PhoneNumber = volunteerCourse.Reviewer.PhoneNumber,
					Gender = volunteerCourse.Reviewer.Gender.Value,
				}
			};

			_mapperMock.Setup(m => m.Map<VolunteerCourseDto>(It.IsAny<VolunteerCourse>()))
				.Returns(volunteerCourseDto);

			// Act
			var result = await _service.GetVolunteerCourseByIdAsync(validId);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(validId, result.Id);
			Assert.AreEqual(volunteerCourseDto.VolunteerCode, result.VolunteerCode);
			Assert.AreEqual(volunteerCourseDto.Status, result.Status);
			Assert.AreEqual(volunteerCourseDto.ReviewerId, result.ReviewerId);
			Assert.AreEqual(volunteerCourseDto.Reviewer.FullName, result.Reviewer.FullName);  // Kiểm tra thông tin Reviewer
			Assert.AreEqual(volunteerCourseDto.Reviewer.Gender, result.Reviewer.Gender);  // Kiểm tra Gender
		}
		

		[Test]
		public async Task GetVolunteerCourseByIdAsync_ValidId_IncompleteInformation_ReturnsVolunteerCourseDtoWithNullValues()
		{
			// Arrange
			int validId = 1;
			var volunteerCourse = _volunteerCourses.First(vc => vc.Id == validId);

			// Giả sử một số thuộc tính không có sẵn
			volunteerCourse.Volunteer.PhoneNumber = null;

			var volunteerCourseDto = new VolunteerCourseDto
			{
				Id = volunteerCourse.Id,
				VolunteerCode = volunteerCourse.VolunteerCode,
				Status = volunteerCourse.Status,
				ReviewerId = volunteerCourse.ReviewerId,
				Reviewer = new ReviewerInforDto
				{
					Id = volunteerCourse.ReviewerId.Value,
					FullName = volunteerCourse.Reviewer.FullName,
					UserName = volunteerCourse.Reviewer.Email,
					Email = volunteerCourse.Reviewer.Email,
					PhoneNumber = null,  // PhoneNumber bị null
					Gender = volunteerCourse.Reviewer.Gender.Value,
				}
			};

			_mapperMock.Setup(m => m.Map<VolunteerCourseDto>(It.IsAny<VolunteerCourse>()))
				.Returns(volunteerCourseDto);

			// Act
			var result = await _service.GetVolunteerCourseByIdAsync(validId);

			// Assert
			Assert.IsNotNull(result);
			Assert.IsNull(result.Reviewer.PhoneNumber);  // Kiểm tra PhoneNumber bị null
		}
		[Test]
		public async Task GetVolunteerCourseByIdAsync_IdEqual2_ReturnsCorrectVolunteerCourseDto()
		{
			// Arrange
			int validId = 2;

			// Giả sử đã mock dữ liệu với ID = 2
			var volunteerCourse = new VolunteerCourse
			{
				Id = validId,
				VolunteerCode = "V002",
				Status = ProgressStatus.Approved,
				ReviewerId = 20,  // Nullable int
				Reviewer = new User
				{
					Id = 20,
					FullName = "Reviewer Two",
					Email = "reviewer2@example.com",
					Gender = Gender.Male
				}
			};

			// Mock trả về đối tượng volunteerCourse khi ID = 2
			_unitOfWorkMock.Setup(uow => uow.VolunteerApplication.GetAsync(It.IsAny<Expression<Func<VolunteerCourse, bool>>>(), It.IsAny<bool>(), It.IsAny<string>()))
				.ReturnsAsync(volunteerCourse);

			var volunteerCourseDto = new VolunteerCourseDto
			{
				Id = volunteerCourse.Id,
				VolunteerCode = volunteerCourse.VolunteerCode,
				Status = volunteerCourse.Status,
				ReviewerId = volunteerCourse.ReviewerId,  // Gán trực tiếp giá trị ReviewerId (nullable int)
				Reviewer = new ReviewerInforDto
				{
					Id = volunteerCourse.ReviewerId.Value,  // Sử dụng .Value vì biết là không null
					FullName = volunteerCourse.Reviewer.FullName,
					UserName = volunteerCourse.Reviewer.Email,
					Email = volunteerCourse.Reviewer.Email,
					PhoneNumber = volunteerCourse.Reviewer.PhoneNumber,
					Gender = volunteerCourse.Reviewer.Gender.Value
				}
			};

			_mapperMock.Setup(m => m.Map<VolunteerCourseDto>(It.IsAny<VolunteerCourse>()))
				.Returns(volunteerCourseDto);

			// Act
			var result = await _service.GetVolunteerCourseByIdAsync(validId);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(validId, result.Id);
			Assert.AreEqual(volunteerCourseDto.VolunteerCode, result.VolunteerCode);
			Assert.AreEqual(volunteerCourseDto.Status, result.Status);
			Assert.AreEqual(volunteerCourseDto.ReviewerId, result.ReviewerId);
			Assert.AreEqual(volunteerCourseDto.Reviewer.FullName, result.Reviewer.FullName);  // Kiểm tra thông tin Reviewer
		}

		[Test]
		public void GetVolunteerCourseByIdAsync_IdEqualNegative1_ThrowsArgumentException()
		{
			// Arrange
			int invalidId = -1;  // ID không hợp lệ

			// Act & Assert
			var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
				await _service.GetVolunteerCourseByIdAsync(invalidId));  // ID < 0
			Assert.AreEqual("Id phải lớn hơn 0.", ex.Message);  // Kiểm tra thông báo lỗi đúng
		}

		[Test]
		public void GetVolunteerCourseByIdAsync_IdEqual_ThrowsArgumentException()
		{
			// Arrange
			int invalidId = -1;  // ID không hợp lệ

			// Act & Assert
			var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
				await _service.GetVolunteerCourseByIdAsync(invalidId));  // ID < 0
			Assert.AreEqual("Id phải lớn hơn 0.", ex.Message);  // Kiểm tra thông báo lỗi đúng
		}


		[Test]
		public async Task UpdateVolunteerCourseAsync_WithNullOrEmptyIds_ThrowsArgumentException()
		{
			// Arrange
			var updateDto = new VolunteerCourseUpdateDto
			{
				Ids = null
			};

			// Act & Assert
			var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _service.UpdateVolunteerCourseAsync(updateDto));
			Assert.That(ex.Message, Is.EqualTo("Id không được để trống hoặc null."));

			updateDto.Ids = new List<int>();
			ex = Assert.ThrowsAsync<ArgumentException>(async () => await _service.UpdateVolunteerCourseAsync(updateDto));
			Assert.That(ex.Message, Is.EqualTo("Id không được để trống hoặc null."));
		}

		[Test]
		public async Task UpdateVolunteerCourseAsync_WithNonExistentVolunteerCourseId_ThrowsArgumentException()
		{
			// Arrange
			var updateDto = new VolunteerCourseUpdateDto
			{
				Ids = new List<int> { 9999 }  // Non-existent volunteer course ID
			};

			_unitOfWorkMock.Setup(uow => uow.VolunteerApplication.GetByIdAsync(It.IsAny<int>(), It.IsAny<string>()))
				.ReturnsAsync((VolunteerCourse)null); // Simulate non-existing volunteer course

			// Act & Assert
			var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _service.UpdateVolunteerCourseAsync(updateDto));
			Assert.That(ex.Message, Is.EqualTo("Đơn đăng ký tình nguyện viên có ID 9999 không tồn tại."));
		}



		
	}
}
