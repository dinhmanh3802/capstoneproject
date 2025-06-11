using AutoMapper;
using Moq;
using NUnit.Framework;
using SCCMS.API.Services;
using SCCMS.Domain.DTOs.StudentCourseDtos;
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
    public class StudentApplicationServiceTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IMapper> _mapperMock;
        private Mock<IEmailService> _emailServiceMock;
        private Mock<INotificationService> _notificationServiceMock;
        private StudentApplicationService _service;
        private List<StudentCourse> _studentCourses;
        private List<User> _users;

        [SetUp]
        public void SetUp()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _emailServiceMock = new Mock<IEmailService>();
            _notificationServiceMock = new Mock<INotificationService>();
            // Thiết lập mock cho IMapper
            _mapperMock.Setup(m => m.Map<StudentCourseDto>(It.IsAny<StudentCourse>()))
                .Returns((StudentCourse sc) => new StudentCourseDto
                {
                    Id = sc.Id,
                    StudentCode = sc.StudentCode,
                    // Mapping các thuộc tính khác...
                });
            _service = new StudentApplicationService(
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _emailServiceMock.Object,
                _notificationServiceMock.Object);

            // Khởi tạo dữ liệu mock đầy đủ
            _studentCourses = new List<StudentCourse>
            {
                new StudentCourse
                {
                    Id = 1,
                    StudentId = 1,
                    CourseId = 1,
                    Student = new Student
                    {
                        FullName = "Do Thanh Thuy",
                        ParentName = "Do Thanh Lan",
                        EmergencyContact = "1234567890",
                        Gender = Gender.Female,
                        DateOfBirth = new DateTime(2000, 1, 1),
                        Email = "thuy@example.com",
                        Address = "123 Street",
                        NationalId = "ID123456",
                        NationalImageFront = "front.png",
                        NationalImageBack = "back.png",
                        Conduct = "Good",
                        AcademicPerformance = "Excellent",
                        StudentGroupAssignment = new List<StudentGroupAssignment>
                        {
                            new StudentGroupAssignment
                            {
                                StudentGroup = new StudentGroup
                                {
                                    Id = 1,
                                    GroupName = "Group A",
                                    CourseId = 1
                                }
                            }
                        }
                    },
                    Course = new Course { Id = 1, CourseName = "Math 101", StartDate = new DateTime(2024, 12, 20) }, // Đã bắt đầu
                    Status = ProgressStatus.Approved,
                    StudentCode = "241220001",
                    ApplicationDate = DateTime.Now,
                    Note = "Approved",
                    ReviewerId = 10,
                    Reviewer = new User
                    {
                        Id = 10,
                        FullName = "Reviewer One",
                        UserName = "reviewer1",
                        Email = "reviewer1@example.com",
                        PhoneNumber = "0123456789",
                        Gender = Gender.Male
                    },
                    ReviewDate = DateTime.Now,
                    DateModified = DateTime.Now
                },
                new StudentCourse
                {
                    Id = 2,
                    StudentId = 2,
                    CourseId = 2,
                    Student = new Student
                    {
                        FullName = "Nguyen Hoai An",
                        ParentName = "Nguyen Thi An",
                        EmergencyContact = "0987654321",
                        Gender = Gender.Male,
                        DateOfBirth = new DateTime(1999, 5, 15),
                        Email = "an@example.com",
                        Address = "456 Avenue",
                        NationalId = "ID654321",
                        NationalImageFront = "front2.png",
                        NationalImageBack = "back2.png",
                        Conduct = "Excellent",
                        AcademicPerformance = "Outstanding",
                        StudentGroupAssignment = new List<StudentGroupAssignment>
                        {
                            new StudentGroupAssignment
                            {
                                StudentGroup = new StudentGroup
                                {
                                    Id = 2,
                                    GroupName = "Group B",
                                    CourseId = 2
                                }
                            }
                        }
                    },
                    Course = new Course { Id = 2, CourseName = "Physics 201", StartDate = new DateTime(2024, 11, 20) }, // Đã bắt đầu
                    Status = ProgressStatus.Approved,
                    StudentCode = "241120002",
                    ApplicationDate = DateTime.Now,
                    Note = "Completed",
                    ReviewerId = 20,
                    Reviewer = new User
                    {
                        Id = 20,
                        FullName = "Reviewer Two",
                        UserName = "reviewer2",
                        Email = "reviewer2@example.com",
                        PhoneNumber = "0987654321",
                        Gender = Gender.Female
                    },
                    ReviewDate = DateTime.Now,
                    DateModified = DateTime.Now
                },
                new StudentCourse
                {
                    Id = 3,
                    StudentId = 3,
                    CourseId = 1,
                    Student = new Student
                    {
                        FullName = "Tran Manh Quan",
                        ParentName = "Tran Thi Lan",
                        EmergencyContact = "1122334455",
                        Gender = Gender.Male,
                        DateOfBirth = new DateTime(2001, 7, 20),
                        Email = "quan@example.com",
                        Address = "789 Boulevard",
                        NationalId = "ID789012",
                        NationalImageFront = "front3.png",
                        NationalImageBack = "back3.png",
                        Conduct = "Fair",
                        AcademicPerformance = "Good",
                        StudentGroupAssignment = new List<StudentGroupAssignment>
                        {
                            new StudentGroupAssignment
                            {
                                StudentGroup = new StudentGroup
                                {
                                    Id = 1,
                                    GroupName = "Group A",
                                    CourseId = 1
                                }
                            }
                        }
                    },
                    Course = new Course { Id = 1, CourseName = "Math 101", StartDate = new DateTime(2024, 12, 20)  }, // Sẽ bắt đầu
                    Status = ProgressStatus.Pending,
                    StudentCode = "241220002",
                    ApplicationDate = DateTime.Now,
                    Note = "Awaiting approval",
                    ReviewerId = null,
                    Reviewer = null,
                    ReviewDate = null,
                    DateModified = DateTime.Now
                }
            };

            _users = new List<User>
            {
                new User
                {
                    Id = 10,
                    FullName = "Reviewer One",
                    UserName = "reviewer1",
                    Email = "reviewer1@example.com",
                    PhoneNumber = "0123456789",
                    Gender = Gender.Male
                },
                new User
                {
                    Id = 20,
                    FullName = "Reviewer Two",
                    UserName = "reviewer2",
                    Email = "reviewer2@example.com",
                    PhoneNumber = "0987654321",
                    Gender = Gender.Female
                }
            };

            // Thiết lập mock cho StudentCourse.FindAsync với 2 tham số (biểu thức và includeProperties)
            _unitOfWorkMock.Setup(uow => uow.StudentCourse.FindAsync(
                It.IsAny<Expression<Func<StudentCourse, bool>>>(),
                It.IsAny<string>()))
                .ReturnsAsync((Expression<Func<StudentCourse, bool>> filter, string includeProperties) =>
                    _studentCourses.Where(filter.Compile()).ToList()
                );

            // Thiết lập mock cho User.FindAsync với 2 tham số (biểu thức và includeProperties)
            _unitOfWorkMock.Setup(uow => uow.User.FindAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<string>()))
                .ReturnsAsync((Expression<Func<User, bool>> filter, string includeProperties) =>
                    _users.Where(filter.Compile()).ToList()
                );

            // Thiết lập mock cho Course.GetByIdAsync với 2 tham số (id, includeProperties)
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(
                It.IsAny<int>(),
                It.IsAny<string>()))
                .ReturnsAsync((int id, string includeProperties) =>
                    _studentCourses.Select(sc => sc.Course).FirstOrDefault(c => c.Id == id)
                );

            // Thiết lập mock cho StudentCourse.GetByIdAsync với 2 tham số (id, includeProperties)
            _unitOfWorkMock.Setup(uow => uow.StudentCourse.GetByIdAsync(
                It.IsAny<int>(),
                It.IsAny<string>()))
                .ReturnsAsync((int id, string includeProperties) =>
                    _studentCourses.FirstOrDefault(sc => sc.Id == id)
                );

            // Thiết lập mock cho StudentGroupAssignment.FindAsync với 2 tham số (biểu thức và includeProperties)
            _unitOfWorkMock.Setup(uow => uow.StudentGroupAssignment.FindAsync(
                It.IsAny<Expression<Func<StudentGroupAssignment, bool>>>(),
                It.IsAny<string>()))
                .ReturnsAsync((Expression<Func<StudentGroupAssignment, bool>> filter, string includeProperties) =>
                    _studentCourses.SelectMany(sc => sc.Student.StudentGroupAssignment)
                                  .Where(filter.Compile())
                                  .ToList()
                );

            // Mock DeleteAsync cho StudentGroupAssignment
            _unitOfWorkMock.Setup(uow => uow.StudentGroupAssignment.DeleteAsync(It.IsAny<StudentGroupAssignment>()))
                .Callback<StudentGroupAssignment>(sga =>
                {
                    foreach (var sc in _studentCourses)
                    {
                        var itemsToRemove = sc.Student.StudentGroupAssignment
                                               .Where(a => a == sga)
                                               .ToList(); // Tạo danh sách để tránh lỗi khi thay đổi collection trong vòng lặp
                        foreach (var item in itemsToRemove)
                        {
                            sc.Student.StudentGroupAssignment.Remove(item);
                        }
                    }
                })
                .Returns(Task.CompletedTask);

            // Mock UpdateAsync cho StudentCourse
            _unitOfWorkMock.Setup(uow => uow.StudentCourse.UpdateAsync(It.IsAny<StudentCourse>()))
                .Returns(Task.CompletedTask);

            // Mock SaveChangeAsync với ReturnsAsync(int)
            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
                          .ReturnsAsync(1); // Trả về số lượng bản ghi đã thay đổi, ví dụ: 1

        }

        #region GetAllStudentApplicationAsync Tests

        [Test]
        public async Task GetAllStudentApplicationAsync_WhenNoFilters_ReturnsAllApplications()
        {
            // Act
            var result = await _service.GetAllStudentApplicationAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());

            var resultList = result.ToList();

            // Kiểm tra các thuộc tính cụ thể
            Assert.AreEqual("Do Thanh Thuy", resultList[0].Student.FullName);
            Assert.AreEqual("Math 101", resultList[0].Course.CourseName);

            Assert.AreEqual("Nguyen Hoai An", resultList[1].Student.FullName);
            Assert.AreEqual("Physics 201", resultList[1].Course.CourseName);

            Assert.AreEqual("Tran Manh Quan", resultList[2].Student.FullName);
            Assert.AreEqual("Math 101", resultList[2].Course.CourseName);
        }

        [Test]
        public async Task GetAllStudentApplicationAsync_WithStudentNameFilter_ReturnsMatchingApplications()
        {
            // Arrange
            string studentNameFilter = "Do Thanh Thuy";

            _unitOfWorkMock.Setup(uow => uow.StudentCourse.FindAsync(
    It.IsAny<Expression<Func<StudentCourse, bool>>>(),
    It.IsAny<string>()))
    .ReturnsAsync(new List<StudentCourse> { new StudentCourse
                {
                    Id = 1,
                    StudentId = 1,
                    CourseId = 1,
                    Student = new Student
                    {
                        FullName = "Do Thanh Thuy",
                        ParentName = "Do Thanh Lan",
                        EmergencyContact = "1234567890",
                        Gender = Gender.Female,
                        DateOfBirth = new DateTime(2000, 1, 1),
                        Email = "thuy@example.com",
                        Address = "123 Street",
                        NationalId = "ID123456",
                        NationalImageFront = "front.png",
                        NationalImageBack = "back.png",
                        Conduct = "Good",
                        AcademicPerformance = "Excellent",
                        StudentGroupAssignment = new List<StudentGroupAssignment>
                        {
                            new StudentGroupAssignment
                            {
                                StudentGroup = new StudentGroup
                                {
                                    Id = 1,
                                    GroupName = "Group A",
                                    CourseId = 1
                                }
                            }
                        }
                    },
                    Course = new Course { Id = 1, CourseName = "Math 101", StartDate = new DateTime(2024, 12, 20) }, // Đã bắt đầu
                    Status = ProgressStatus.Approved,
                    StudentCode = "241220001",
                    ApplicationDate = DateTime.Now,
                    Note = "Approved",
                    ReviewerId = 10,
                    Reviewer = new User
                    {
                        Id = 10,
                        FullName = "Reviewer One",
                        UserName = "reviewer1",
                        Email = "reviewer1@example.com",
                        PhoneNumber = "0123456789",
                        Gender = Gender.Male
                    },
                    ReviewDate = DateTime.Now,
                    DateModified = DateTime.Now
                }});

            // Act
            var result = await _service.GetAllStudentApplicationAsync(studentName: studentNameFilter);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());

            var resultList = result.ToList();
            Assert.AreEqual("Do Thanh Thuy", resultList[0].Student.FullName);
            Assert.AreEqual("Math 101", resultList[0].Course.CourseName);
        }

        [Test]
        public async Task GetAllStudentApplicationAsync_WithStudentNameFilter_ReturnsNotFoundMatchingApplications()
        {
            // Arrange
            string partialStudentNameFilter = "HIHI";

            _unitOfWorkMock.Setup(uow => uow.StudentCourse.FindAsync(
                It.IsAny<Expression<Func<StudentCourse, bool>>>(),
                It.IsAny<string>()))
                .ReturnsAsync(new List<StudentCourse>());

            // Act
            var result = await _service.GetAllStudentApplicationAsync(studentName: partialStudentNameFilter);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());

        }

        [Test]
        public async Task GetAllStudentApplicationAsync_WithParentNameFilter_ReturnsMatchingApplications()
        {
            // Arrange
            string parentNameFilter = "Nguyen Thi An";
            _unitOfWorkMock.Setup(uow => uow.StudentCourse.FindAsync(
    It.IsAny<Expression<Func<StudentCourse, bool>>>(),
    It.IsAny<string>()))
    .ReturnsAsync(new List<StudentCourse> { new StudentCourse
                {
                    Id = 2,
                    StudentId = 2,
                    CourseId = 2,
                    Student = new Student
                    {
                        FullName = "Nguyen Hoai An",
                        ParentName = "Nguyen Thi An",
                        EmergencyContact = "0987654321",
                        Gender = Gender.Male,
                        DateOfBirth = new DateTime(1999, 5, 15),
                        Email = "an@example.com",
                        Address = "456 Avenue",
                        NationalId = "ID654321",
                        NationalImageFront = "front2.png",
                        NationalImageBack = "back2.png",
                        Conduct = "Excellent",
                        AcademicPerformance = "Outstanding",
                        StudentGroupAssignment = new List<StudentGroupAssignment>
                        {
                            new StudentGroupAssignment
                            {
                                StudentGroup = new StudentGroup
                                {
                                    Id = 2,
                                    GroupName = "Group B",
                                    CourseId = 2
                                }
                            }
                        }
                    },
                    Course = new Course { Id = 2, CourseName = "Physics 201", StartDate = new DateTime(2024, 11, 20) }, // Đã bắt đầu
                    Status = ProgressStatus.Approved,
                    StudentCode = "241120002",
                    ApplicationDate = DateTime.Now,
                    Note = "Completed",
                    ReviewerId = 20,
                    Reviewer = new User
                    {
                        Id = 20,
                        FullName = "Reviewer Two",
                        UserName = "reviewer2",
                        Email = "reviewer2@example.com",
                        PhoneNumber = "0987654321",
                        Gender = Gender.Female
                    },
                    ReviewDate = DateTime.Now,
                    DateModified = DateTime.Now
                }});

            // Act
            var result = await _service.GetAllStudentApplicationAsync(parentName: parentNameFilter);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());

            var resultList = result.ToList();
            Assert.AreEqual("Nguyen Hoai An", resultList[0].Student.FullName);
            Assert.AreEqual("Physics 201", resultList[0].Course.CourseName);
        }

        [Test]
        public async Task GetAllStudentApplicationAsync_WithParentNameFilter_NotFoundApplications()
        {
            // Arrange
            string partialParentNameFilter = "HIHI";
            _unitOfWorkMock.Setup(uow => uow.StudentCourse.FindAsync(
                It.IsAny<Expression<Func<StudentCourse, bool>>>(),
                It.IsAny<string>()))
                .ReturnsAsync(new List<StudentCourse>());
            // Act
            var result = await _service.GetAllStudentApplicationAsync(parentName: partialParentNameFilter);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());

        }


        [Test]
        public async Task GetAllStudentApplicationAsync_WithPhoneNumberFilter_ReturnsMatchingApplications()
        {
            // Arrange
            string phoneNumberFilter = "1122334455";

            // Act
            var result = await _service.GetAllStudentApplicationAsync(phoneNumber: phoneNumberFilter);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());

            var resultList = result.ToList();
            Assert.AreEqual("Tran Manh Quan", resultList[0].Student.FullName);
            Assert.AreEqual("Math 101", resultList[0].Course.CourseName);
        }


        [Test]
        public void GetAllStudentApplicationAsync_WithInvalidPhoneNumberFormat_ThrowsArgumentException()
        {
            // Arrange
            string invalidPhoneNumber = "12345"; // Invalid format, less than 10 digits

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _service.GetAllStudentApplicationAsync(phoneNumber: invalidPhoneNumber));

            Assert.That(ex.Message, Is.EqualTo("Định dạng số điện thoại không hợp lệ. Phải có 10 chữ số."));
        }

        [Test]
        public async Task GetAllStudentApplicationAsync_WithGenderFilter_ReturnsMatchingApplications()
        {
            // Arrange
            Gender genderFilter = Gender.Female;

            // Act
            var result = await _service.GetAllStudentApplicationAsync(gender: genderFilter);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());

            var resultList = result.ToList();
            Assert.AreEqual(Gender.Female, resultList[0].Student.Gender);
            Assert.AreEqual("Do Thanh Thuy", resultList[0].Student.FullName);
            Assert.AreEqual("Math 101", resultList[0].Course.CourseName);
        }

        [Test]
        public async Task GetAllStudentApplicationAsync_WithMaleGenderFilter_ReturnsMatchingApplications()
        {
            // Arrange
            Gender genderFilter = Gender.Male;

            // Act
            var result = await _service.GetAllStudentApplicationAsync(gender: genderFilter);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());

            var resultList = result.ToList();
            Assert.IsTrue(resultList.All(r => r.Student.Gender == Gender.Male));
            Assert.IsTrue(resultList.Any(r => r.Student.FullName == "Nguyen Hoai An"));
            Assert.IsTrue(resultList.Any(r => r.Student.FullName == "Tran Manh Quan"));
        }



        [Test]
        public async Task GetAllStudentApplicationAsync_WithCourseIdFilter_ReturnsMatchingApplications()
        {
            // Arrange
            int courseIdFilter = 1;

            // Act
            var result = await _service.GetAllStudentApplicationAsync(courseId: courseIdFilter);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());

            var resultList = result.ToList();
            Assert.IsTrue(resultList.All(r => r.CourseId == courseIdFilter));
            Assert.IsTrue(resultList.All(r => r.Course.CourseName == "Math 101"));
        }

        [Test]
        public async Task GetAllStudentApplicationAsync_WithCourseId2Filter_ReturnsMatchingApplications()
        {
            // Arrange
            int courseIdFilter = 2;

            // Act
            var result = await _service.GetAllStudentApplicationAsync(courseId: courseIdFilter);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());

            var resultList = result.ToList();
            Assert.IsTrue(resultList.All(r => r.CourseId == courseIdFilter));
            Assert.IsTrue(resultList.All(r => r.Course.CourseName == "Physics 201"));
        }

        [Test]
        public async Task GetAllStudentApplicationAsync_WithNonExistingCourseIdFilter_ReturnsEmptyList()
        {
            // Arrange
            int nonExistingCourseId = 999;

            // Act
            var result = await _service.GetAllStudentApplicationAsync(courseId: nonExistingCourseId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }
        [Test]
        public void GetAllStudentApplicationAsync_WithInvalidCourseId_ThrowsArgumentException()
        {
            // Arrange
            int invalidCourseId = -1;

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _service.GetAllStudentApplicationAsync(courseId: invalidCourseId));

            Assert.That(ex.Message, Is.EqualTo("CourseId không hợp lệ. Phải lớn hơn 0."));
        }


        [Test]
        public async Task GetAllStudentApplicationAsync_WithStatusFilter_ReturnsMatchingApplications()
        {
            // Arrange
            ProgressStatus statusFilter = ProgressStatus.Approved;

            // Act
            var result = await _service.GetAllStudentApplicationAsync(status: statusFilter);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());

            var resultList = result.ToList();
            Assert.AreEqual(ProgressStatus.Approved, resultList[0].Status);
            Assert.AreEqual("Do Thanh Thuy", resultList[0].Student.FullName);
            Assert.AreEqual("Math 101", resultList[0].Course.CourseName);
        }


        [Test]
        public async Task GetAllStudentApplicationAsync_WithReviewerIdFilter_ReturnsMatchingApplications()
        {
            // Arrange
            int reviewerIdFilter = 20;

            // Act
            var result = await _service.GetAllStudentApplicationAsync(reviewerId: reviewerIdFilter);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());

            var resultList = result.ToList();
            Assert.AreEqual(reviewerIdFilter, resultList[0].ReviewerId);
            Assert.AreEqual("Nguyen Hoai An", resultList[0].Student.FullName);
            Assert.AreEqual("Physics 201", resultList[0].Course.CourseName);
        }



        [Test]
        public async Task GetAllStudentApplicationAsync_WithNonExistingReviewerIdFilter_ReturnsEmptyList()
        {
            // Arrange
            int nonExistingReviewerId = 999;

            // Act
            var result = await _service.GetAllStudentApplicationAsync(reviewerId: nonExistingReviewerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void GetAllStudentApplicationAsync_WithInvalidReviewerId_ThrowsArgumentException()
        {
            // Arrange
            int invalidReviewerId = -1;

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _service.GetAllStudentApplicationAsync(reviewerId: invalidReviewerId));

            Assert.That(ex.Message, Is.EqualTo("ReviewerId không hợp lệ. Phải lớn hơn 0."));
        }


        [Test]
        public async Task GetAllStudentApplicationAsync_WithStartDobAndEndDobFilter_ReturnsMatchingApplications()
        {
            // Arrange
            DateTime startDob = new DateTime(1999, 1, 1);
            DateTime endDob = new DateTime(2000, 12, 31);

            // Act
            var result = await _service.GetAllStudentApplicationAsync(startDob: startDob, endDob: endDob);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());

            var resultList = result.ToList();
            Assert.IsTrue(resultList.All(r => r.Student.DateOfBirth >= startDob && r.Student.DateOfBirth <= endDob));
            Assert.IsTrue(resultList.Any(r => r.Student.FullName == "Do Thanh Thuy"));
            Assert.IsTrue(resultList.Any(r => r.Student.FullName == "Nguyen Hoai An"));
        }

        [Test]
        public async Task GetAllStudentApplicationAsync_WithStartDobOnlyFilter_ReturnsMatchingApplications()
        {
            // Arrange
            DateTime startDob = new DateTime(2000, 1, 1);

            // Act
            var result = await _service.GetAllStudentApplicationAsync(startDob: startDob);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());

            var resultList = result.ToList();
            Assert.IsTrue(resultList.All(r => r.Student.DateOfBirth >= startDob));
            Assert.IsTrue(resultList.Any(r => r.Student.FullName == "Do Thanh Thuy"));
            Assert.IsTrue(resultList.Any(r => r.Student.FullName == "Tran Manh Quan"));
        }

        [Test]
        public async Task GetAllStudentApplicationAsync_WithEndDobOnlyFilter_ReturnsMatchingApplications()
        {
            // Arrange
            DateTime endDob = new DateTime(2000, 12, 31);

            // Act
            var result = await _service.GetAllStudentApplicationAsync(endDob: endDob);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());

            var resultList = result.ToList();
            Assert.IsTrue(resultList.All(r => r.Student.DateOfBirth <= endDob));
            Assert.IsTrue(resultList.Any(r => r.Student.FullName == "Do Thanh Thuy"));
            Assert.IsTrue(resultList.Any(r => r.Student.FullName == "Nguyen Hoai An"));
        }

        #endregion

        #region UpdateStatusStudentApplicationAsync Tests

        [Test]
        public async Task UpdateStatusStudentApplicationAsync_ValidUpdate_Success()
        {
            // Arrange
            var updateDto = new StudentCourseUpdateDto
            {
                Ids = new List<int> { 3 }, // StudentCourseId =3, Status = Pending
                Status = ProgressStatus.Approved,
                ReviewerId = 10,
                Note = "Approved successfully."
            };

            // Thiết lập mock cho StudentCourse.UpdateAsync và SaveChangeAsync
            _unitOfWorkMock.Setup(uow => uow.StudentCourse.UpdateAsync(It.IsAny<StudentCourse>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
                          .ReturnsAsync(1); // Số lượng bản ghi đã thay đổi

            // Act
            await _service.UpdateStatusStudentApplicationAsync(updateDto);

            // Assert
            var updatedStudentCourse = _studentCourses.First(sc => sc.Id == 3);
            Assert.AreEqual(ProgressStatus.Approved, updatedStudentCourse.Status);
            Assert.AreEqual("Approved successfully.", updatedStudentCourse.Note);
            Assert.AreEqual(10, updatedStudentCourse.ReviewerId);
            Assert.IsNotNull(updatedStudentCourse.ReviewDate);
            Assert.IsNotNull(updatedStudentCourse.DateModified);
        }
        [Test]
        public async Task UpdateStatusStudentApplicationAsync_IdsIsNull_ThrowsArgumentException()
        {
            // Arrange
            var updateDto = new StudentCourseUpdateDto
            {
                Ids = null,  // Ids = null
                Status = ProgressStatus.Approved,
                ReviewerId = 10,
                Note = "Approved successfully."
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _service.UpdateStatusStudentApplicationAsync(updateDto)
            );
            Assert.That(ex.Message, Is.EqualTo("Id không được để trống hoặc null."));
        }
        [Test]
        public async Task UpdateStatusStudentApplicationAsync_IdsContainsNegativeValue_ThrowsArgumentException()
        {
            // Arrange
            var updateDto = new StudentCourseUpdateDto
            {
                Ids = new List<int> { -1 },  // ID nhỏ hơn 0
                Status = ProgressStatus.Approved,
                ReviewerId = 10,
                Note = "Approved successfully."
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _service.UpdateStatusStudentApplicationAsync(updateDto)
            );
            Assert.That(ex.Message, Is.EqualTo("Id phải lớn hơn 0."));
        }

        [Test]
        public async Task UpdateStatusStudentApplicationAsync_IdsNotExist_ThrowsArgumentException()
        {
            // Arrange
            var updateDto = new StudentCourseUpdateDto
            {
                Ids = new List<int> { 999 },  // ID không tồn tại
                Status = ProgressStatus.Approved,
                ReviewerId = 10,
                Note = "Approved successfully."
            };

            // Mock phương thức GetByIdAsync trả về null cho ID không tồn tại
            _unitOfWorkMock.Setup(uow => uow.StudentCourse.GetByIdAsync(999,null))
                .ReturnsAsync((StudentCourse)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _service.UpdateStatusStudentApplicationAsync(updateDto)
            );
            Assert.That(ex.Message, Is.EqualTo("Đơn đăng ký có ID 999 không tồn tại."));
        }
        [Test]
        public void UpdateStatusStudentApplicationAsync_InvalidReviewerId_ThrowsArgumentException()
        {
            // Arrange
            var updateDto = new StudentCourseUpdateDto
            {
                Ids = new List<int> { 1 },
                Status = ProgressStatus.Approved,
                ReviewerId = -1, // Invalid ReviewerId
                Note = "Approved successfully."
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _service.UpdateStatusStudentApplicationAsync(updateDto));

            Assert.That(ex.Message, Is.EqualTo("Reviewer Id phải lớn hơn 0."));
        }
        [Test]
        public async Task UpdateStatusStudentApplicationAsync_ReviewerIdNotExist_ThrowsArgumentException()
        {
            // Arrange
            var updateDto = new StudentCourseUpdateDto
            {
                Ids = new List<int> { 3 },  // Ids hợp lệ
                Status = ProgressStatus.Approved,
                ReviewerId = 999,  // ReviewerId không tồn tại
                Note = "Approved successfully."
            };

            // Mock phương thức FindAsync trả về null cho ReviewerId không tồn tại
            _unitOfWorkMock.Setup(uow => uow.User.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),null))
                .ReturnsAsync((List<User>)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _service.UpdateStatusStudentApplicationAsync(updateDto)
            );
            Assert.That(ex.Message, Is.EqualTo("Người duyệt có ID 999 không tồn tại."));
        }


        [Test]
        public async Task UpdateStatusStudentApplicationAsync_ChangeStatusToApproved_GeneratesStudentCode()
        {
            // Arrange
            var updateDto = new StudentCourseUpdateDto
            {
                Ids = new List<int> { 3 }, // StudentCourseId =3, CourseId=1
                Status = ProgressStatus.Approved,
                ReviewerId = 10,
                Note = "Approved successfully."
            };

            // Thiết lập mock cho StudentCourse.UpdateAsync và SaveChangeAsync
            _unitOfWorkMock.Setup(uow => uow.StudentCourse.UpdateAsync(It.IsAny<StudentCourse>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
                          .ReturnsAsync(1); // Số lượng bản ghi đã thay đổi

            // Act
            await _service.UpdateStatusStudentApplicationAsync(updateDto);

            // Assert
            var updatedStudentCourse = _studentCourses.First(sc => sc.Id == 3);
            Assert.AreEqual(ProgressStatus.Approved, updatedStudentCourse.Status);
            Assert.AreEqual("241220003", updatedStudentCourse.StudentCode); // Nếu logic tạo StudentCode đúng
            Assert.AreEqual("Approved successfully.", updatedStudentCourse.Note);
            Assert.AreEqual(10, updatedStudentCourse.ReviewerId);
            Assert.IsNotNull(updatedStudentCourse.ReviewDate);
            Assert.IsNotNull(updatedStudentCourse.DateModified);
        }

        [Test]
        public async Task UpdateStatusStudentApplicationAsync_UpdateReviewerId_WhenStatusIsPending()
        {
            // Arrange
            var updateDto = new StudentCourseUpdateDto
            {
                Ids = new List<int> { 3 }, // StudentCourseId =3, Status = Pending
                Status = ProgressStatus.Pending,
                ReviewerId = 10,
                Note = "Reviewer assigned."
            };

            // Thiết lập mock cho StudentCourse.UpdateAsync và SaveChangeAsync
            _unitOfWorkMock.Setup(uow => uow.StudentCourse.UpdateAsync(It.IsAny<StudentCourse>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
                          .ReturnsAsync(1); // Số lượng bản ghi đã thay đổi

            // Act
            await _service.UpdateStatusStudentApplicationAsync(updateDto);

            // Assert
            var updatedStudentCourse = _studentCourses.First(sc => sc.Id == 3);
            Assert.AreEqual(ProgressStatus.Pending, updatedStudentCourse.Status);
            Assert.AreEqual(10, updatedStudentCourse.ReviewerId);
            Assert.AreEqual("Reviewer assigned.", updatedStudentCourse.Note);
            Assert.IsNotNull(updatedStudentCourse.ReviewDate);
            Assert.IsNotNull(updatedStudentCourse.DateModified);
        }





        #endregion
        [Test]
        public void GetStudentApplicationByIdAsync_InvalidId_ThrowsArgumentException()
        {
            // Arrange
            int invalidId = 0;

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _service.GetStudentApplicationByIdAsync(invalidId)
            );
            Assert.That(ex.Message, Is.EqualTo("Id phải lớn hơn 0."));
        }
        [Test]
        public async Task GetStudentApplicationByIdAsync_IdNotExist_ReturnsNull()
        {
            // Arrange
            int nonExistingId = 999;

            // Act
            var result = await _service.GetStudentApplicationByIdAsync(nonExistingId);

            // Assert
            Assert.IsNull(result);
        }
        [Test]
        public async Task GetStudentApplicationByIdAsync_ValidId_ReturnsStudentCourseDto()
        {
            // Arrange
            int existingId = 3;

            // Act
            var result = await _service.GetStudentApplicationByIdAsync(existingId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(existingId, result.Id);
            Assert.AreEqual("241220002", result.StudentCode);
            Assert.AreEqual(ProgressStatus.Pending, result.Status);
            Assert.AreEqual("Awaiting approval", result.Note);
            Assert.IsNull(result.Reviewer); // Vì ReviewerId = null

            // Kiểm tra thuộc tính Course
            Assert.IsNotNull(result.Course);
            Assert.AreEqual(1, result.Course.Id);
            Assert.AreEqual("Math 101", result.Course.CourseName);

            // Kiểm tra thuộc tính Student
            Assert.IsNotNull(result.Student);
            Assert.AreEqual(3, result.Student.Id);
            Assert.AreEqual("Tran Manh Quan", result.Student.FullName);
            Assert.AreEqual("1122334455", result.Student.EmergencyContact);
            Assert.AreEqual("ID789012", result.Student.NationalId);
            Assert.AreEqual(Gender.Male, result.Student.Gender);
            Assert.AreEqual("quan@example.com", result.Student.Email);

            // Kiểm tra thuộc tính StudentGroups
            Assert.IsNotNull(result.Student.StudentGroups);
            Assert.AreEqual(1, result.Student.StudentGroups.Count);
        }

        [Test]
        public async Task AutoAssignApplicationsAsync_NoSecretary_ThrowsInvalidOperationException()
        {
            // Arrange
            var courseId = 1;

            // Setup mock cho _unitOfWork.User.FindAsync trả về danh sách rỗng (không có thư ký)
            _unitOfWorkMock.Setup(uow => uow.User.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),null))
                .ReturnsAsync(new List<User>());

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.AutoAssignApplicationsAsync(courseId));

            Assert.AreEqual("Không có thư ký nào để phân chia", ex.Message);
        }
        [Test]
        public async Task AutoAssignApplicationsAsync_NoUnassignedApplications_ThrowsInvalidOperationException()
        {
            // Arrange
            var courseId = 1;

            // Setup mock cho _unitOfWork.User.FindAsync trả về một thư ký
            _unitOfWorkMock.Setup(uow => uow.User.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),null))
                .ReturnsAsync(new List<User>
                {
            new User { Id = 1, RoleId = SD.RoleId_Secretary, Status = UserStatus.Active }
                });

            // Setup mock cho _unitOfWork.StudentCourse.FindAsync trả về danh sách rỗng (không có đơn chưa phân chia)
            _unitOfWorkMock.Setup(uow => uow.StudentCourse.FindAsync(It.IsAny<Expression<Func<StudentCourse, bool>>>(),null))
                .ReturnsAsync(new List<StudentCourse>());

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.AutoAssignApplicationsAsync(courseId));

            Assert.AreEqual("Tất cả đơn đăng ký đã được phân chia", ex.Message);
        }

        [Test]
        public async Task AutoAssignApplicationsAsync_AssignsApplicationsSuccessfully()
        {
            // Arrange
            var courseId = 1;

            // Thêm thư ký hoạt động
            var secretaries = new List<User>
    {
        new User { Id = 1, RoleId = SD.RoleId_Secretary, Status = UserStatus.Active },
        new User { Id = 2, RoleId = SD.RoleId_Secretary, Status = UserStatus.Active }
    };

            _unitOfWorkMock.Setup(uow => uow.User.FindAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                null))
                .ReturnsAsync(secretaries);

            // Thêm các đơn chưa được gán
            var unassignedApplications = new List<StudentCourse>
    {
        new StudentCourse { Id = 4, CourseId = courseId, ReviewerId = null },
        new StudentCourse { Id = 5, CourseId = courseId, ReviewerId = null },
        new StudentCourse { Id = 6, CourseId = courseId, ReviewerId = null }
    };

            _unitOfWorkMock.Setup(uow => uow.StudentCourse.FindAsync(
                It.IsAny<Expression<Func<StudentCourse, bool>>>(),
                null))
                .ReturnsAsync(unassignedApplications);

            // Thiết lập số lượng đơn đã được gán cho mỗi thư ký
            _unitOfWorkMock.Setup(uow => uow.StudentCourse.CountAsync(
                It.IsAny<Expression<Func<StudentCourse, bool>>>()))
                .ReturnsAsync((Expression<Func<StudentCourse, bool>> filter) =>
                {
                    // Giả sử thư ký với Id=1 đã có 1 đơn, Id=2 đã có 0 đơn
                    if (filter.Compile().Invoke(new StudentCourse { ReviewerId = 1, CourseId = courseId }))
                        return 1;
                    if (filter.Compile().Invoke(new StudentCourse { ReviewerId = 2, CourseId = courseId }))
                        return 0;
                    return 0;
                });

            // Mock UpdateAsync cho StudentCourse
            _unitOfWorkMock.Setup(uow => uow.StudentCourse.UpdateAsync(It.IsAny<StudentCourse>()))
                .Callback<StudentCourse>(sc =>
                {
                    var existing = _studentCourses.FirstOrDefault(c => c.Id == sc.Id);
                    if (existing != null)
                    {
                        existing.ReviewerId = sc.ReviewerId;
                        existing.DateModified = sc.DateModified;
                        existing.ReviewDate = sc.ReviewDate;
                    }
                })
                .Returns(Task.CompletedTask);

            // Mock SaveChangeAsync
            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
                          .ReturnsAsync(1);

            // Act
            await _service.AutoAssignApplicationsAsync(courseId);

            // Assert
            // Kiểm tra rằng các đơn đã được gán ReviewerId
            foreach (var app in unassignedApplications)
            {
                Assert.IsNotNull(app.ReviewerId, $"Application ID {app.Id} should have been assigned a ReviewerId.");
                Assert.IsTrue(secretaries.Any(s => s.Id == app.ReviewerId), $"ReviewerId {app.ReviewerId} should be a valid secretary.");
            }

            // Kiểm tra rằng SaveChangeAsync được gọi
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once);

            // Kiểm tra rằng NotifyUserAsync được gọi đúng số lần
            // Trong trường hợp này, có thể cả hai thư ký đều nhận được thông báo nếu có đơn được gán cho họ
            _notificationServiceMock.Verify(ns => ns.NotifyUserAsync(
                It.IsAny<int>(),
                It.Is<string>(s => s.Contains("Bạn có đơn đăng ký mới cần duyệt. Vui lòng kiểm tra!")),
                It.Is<string>(s => s.Contains("/student-applications"))),
                Times.AtLeastOnce);

            // Optional: Kiểm tra phân bổ hợp lý (ví dụ: thư ký có ít đơn hơn sẽ nhận thêm đơn)
            // Trong ví dụ này, thư ký với Id=2 sẽ nhận nhiều hơn vì ban đầu họ có ít hoặc không có đơn
            var assignedToSecretary1 = unassignedApplications.Count(a => a.ReviewerId == 1);
            var assignedToSecretary2 = unassignedApplications.Count(a => a.ReviewerId == 2);

            Assert.IsTrue(assignedToSecretary2 >= assignedToSecretary1, "Secretary with fewer assignments should receive more applications.");
        }




    }
}
