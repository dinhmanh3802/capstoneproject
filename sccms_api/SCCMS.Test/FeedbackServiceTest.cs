using AutoMapper;
using Moq;
using SCCMS.Domain.DTOs.FeedbackDtos;
using SCCMS.Domain.Services.Implements;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Test
{
    [TestFixture]
    public class FeedbackServiceTest
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IMapper> _mockMapper;
        private FeedbackService _feedbackService;

        [SetUp]
        public void SetUp()
        {
            // Khởi tạo các mock objects
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();

            // Khởi tạo FeedbackService với các mock dependencies
            _feedbackService = new FeedbackService(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        
        [Test]
        public void CreateFeedbackAsync_ShouldThrowException_WhenCourseIdIsInvalid()
        {
            // Arrange
            var feedbackCreateDto = new FeedbackCreateDto
            {
                CourseId = -1,  // CourseId không hợp lệ
                StudentCode = "123456789",
                Content = "Great course!"
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(() => _feedbackService.CreateFeedbackAsync(feedbackCreateDto));
            Assert.That(ex.Message, Is.EqualTo("CourseId phải lớn hơn 0."));
        }
       
        [Test]
        public void CreateFeedbackAsync_ShouldThrowException_WhenStudentCodeIsEmpty()
        {
            // Arrange
            var feedbackCreateDto = new FeedbackCreateDto
            {
                CourseId = 1,
                StudentCode = "",  // StudentCode trống
                Content = "Great course!"
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(() => _feedbackService.CreateFeedbackAsync(feedbackCreateDto));
            Assert.That(ex.Message, Is.EqualTo("Mã khóa sinh là trường bắt buộc."));
        }
        [Test]
        public void CreateFeedbackAsync_ShouldThrowException_WhenStudentCodeLengthIsInvalid()
        {
            // Arrange
            var feedbackCreateDto = new FeedbackCreateDto
            {
                CourseId = 1,
                StudentCode = "12345",  // StudentCode không có 9 ký tự
                Content = "Great course!"
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(() => _feedbackService.CreateFeedbackAsync(feedbackCreateDto));
            Assert.That(ex.Message, Is.EqualTo("Mã khóa sinh phải có 9 ký tự."));
        }

        [Test]
        public void CreateFeedbackAsync_ShouldThrowException_WhenContentIsEmpty()
        {
            // Arrange
            var feedbackCreateDto = new FeedbackCreateDto
            {
                CourseId = 1,
                StudentCode = "123456789",
                Content = ""  // Content trống
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(() => _feedbackService.CreateFeedbackAsync(feedbackCreateDto));
            Assert.That(ex.Message, Is.EqualTo("Nội dung không được để trống."));
        }

        [Test]
        public async Task CreateFeedbackAsync_ShouldThrowException_WhenStudentCodeDoesNotExistInStudentCourse()
        {
            // Arrange
            var feedbackCreateDto = new FeedbackCreateDto
            {
                CourseId = 1,
                StudentCode = "999999999", // StudentCode hợp lệ
                Content = "Great course!"
            };

            // Giả sử khi tìm kiếm `StudentCode` trong bảng `StudentCourse`, không tìm thấy
            _mockUnitOfWork.Setup(u => u.StudentCourse.GetAsync(It.IsAny<Expression<Func<StudentCourse, bool>>>(),true, null))
                .ReturnsAsync((StudentCourse)null);  // Không tìm thấy `StudentCourse`

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _feedbackService.CreateFeedbackAsync(feedbackCreateDto));

            // Kiểm tra thông báo lỗi
            Assert.AreEqual("Mã khóa sinh không tồn tại.", ex.Message);
        }
        [Test]
        public async Task CreateFeedbackAsync_ShouldThrowException_WhenStudentCodeDoesNotMatchCourseId()
        {
            // Arrange
            var feedbackCreateDto = new FeedbackCreateDto
            {
                CourseId = 1,  // Expected course
                StudentCode = "012345678",  // Student enrolled in a different course
                Content = "Great course!"
            };

            var studentCourse = new StudentCourse { CourseId = 2, StudentCode = "123456789" };

            // Mocking the repository to return a different CourseId for the StudentCode
            _mockUnitOfWork.Setup(u => u.StudentCourse.GetByStudentCodeAsync(feedbackCreateDto.StudentCode))
                           .ReturnsAsync(studentCourse);

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () => await _feedbackService.CreateFeedbackAsync(feedbackCreateDto));
            Assert.AreEqual("Khóa sinh không tham gia khóa tu này.", ex.Message);
        }
        [Test]
        public async Task CreateFeedbackAsync_ShouldThrowException_WhenCourseDoesNotExist()
        {
            // Arrange
            var feedbackCreateDto = new FeedbackCreateDto
            {
                CourseId = 999,  // Giả lập CourseId không tồn tại
                StudentCode = "123456789",
                Content = "Great course!"
            };

            // Mock trả về null khi tìm khóa học theo CourseId
            _mockUnitOfWork.Setup(u => u.Course.GetByIdAsync(feedbackCreateDto.CourseId, null))
                           .ReturnsAsync((Course)null);  // Giả lập khóa học không tồn tại

            // Mock trả về một StudentCourse hợp lệ để không gặp lỗi với mã sinh viên
            var existingStudentCourse = new StudentCourse
            {
                StudentCode = feedbackCreateDto.StudentCode,
                CourseId = feedbackCreateDto.CourseId
            };
            _mockUnitOfWork.Setup(u => u.StudentCourse.GetByStudentCodeAsync(feedbackCreateDto.StudentCode))
                           .ReturnsAsync(existingStudentCourse);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _feedbackService.CreateFeedbackAsync(feedbackCreateDto)
            );

            // Kiểm tra thông báo lỗi
            Assert.That(exception.Message, Is.EqualTo("Khóa tu này không tồn tại"));
        }


        [Test]
        public async Task CreateFeedbackAsync_ShouldCreateFeedbackSuccessfully_WhenValidDataProvided()
        {
            // Arrange
            var feedbackCreateDto = new FeedbackCreateDto
            {
                CourseId = 1,
                StudentCode = "123456789",
                Content = "Great course!"
            };

            // Giả lập thông tin khóa học và sinh viên tham gia khóa học
            var existingCourse = new Course
            {
                Id = 1,
                CourseName = "Khóa Tu Mùa Hè"
            };

            var existingStudentCourse = new StudentCourse
            {
                StudentCode = "123456789",
                CourseId = 1
            };

            // Giả lập phản hồi mong muốn (Feedback)
            var expectedFeedback = new Feedback
            {
                CourseId = feedbackCreateDto.CourseId,
                StudentCode = feedbackCreateDto.StudentCode,
                Content = feedbackCreateDto.Content
            };

            // Mock phương thức GetByIdAsync trả về khóa học hợp lệ
            _mockUnitOfWork.Setup(u => u.Course.GetByIdAsync(feedbackCreateDto.CourseId, null))
                           .ReturnsAsync(existingCourse);

            // Mock phương thức GetByStudentCodeAsync trả về sinh viên tham gia khóa học
            _mockUnitOfWork.Setup(u => u.StudentCourse.GetByStudentCodeAsync(feedbackCreateDto.StudentCode))
                           .ReturnsAsync(existingStudentCourse);

            // Mock Mapper chuyển đổi FeedbackCreateDto thành Feedback
            _mockMapper.Setup(m => m.Map<Feedback>(It.IsAny<FeedbackCreateDto>()))
                       .Returns(expectedFeedback);

            // Mock phương thức AddAsync để lưu feedback vào cơ sở dữ liệu
            _mockUnitOfWork.Setup(uow => uow.Feedback.AddAsync(It.IsAny<Feedback>()))
                           .Returns(Task.CompletedTask);

            // Mock phương thức SaveChangeAsync để commit dữ liệu vào cơ sở dữ liệu
            _mockUnitOfWork.Setup(uow => uow.SaveChangeAsync())
                           .ReturnsAsync(1);  // Giả lập trả về 1 bản ghi được lưu thành công

            // Act
            await _feedbackService.CreateFeedbackAsync(feedbackCreateDto);

            // Assert
            // Kiểm tra xem phương thức AddAsync đã được gọi với đối tượng Feedback chính xác
            _mockUnitOfWork.Verify(u => u.Feedback.AddAsync(It.IsAny<Feedback>()), Times.Once);

            // Kiểm tra phương thức SaveChangeAsync được gọi đúng
            _mockUnitOfWork.Verify(u => u.SaveChangeAsync(), Times.Once);

            // Kiểm tra nếu feedback đã được lưu thành công
            _mockUnitOfWork.Verify(u => u.Feedback.AddAsync(It.Is<Feedback>(f =>
                f.CourseId == feedbackCreateDto.CourseId &&
                f.StudentCode == feedbackCreateDto.StudentCode &&
                f.Content == feedbackCreateDto.Content)), Times.Once);
        }
        [Test]
        public async Task CreateFeedbackAsync_ShouldCreateFeedbackSuccessfully_WhenValidDataProvided_CourseIdEqual2()
        {
            // Arrange
            var feedbackCreateDto = new FeedbackCreateDto
            {
                CourseId = 2,
                StudentCode = "123456789",
                Content = "Great course!"
            };

            // Giả lập thông tin khóa học và sinh viên tham gia khóa học
            var existingCourse = new Course
            {
                Id = 2,
                CourseName = "Khóa Tu Mùa Hè"
            };

            var existingStudentCourse = new StudentCourse
            {
                StudentCode = "123456789",
                CourseId = 2
            };

            // Giả lập phản hồi mong muốn (Feedback)
            var expectedFeedback = new Feedback
            {
                CourseId = feedbackCreateDto.CourseId,
                StudentCode = feedbackCreateDto.StudentCode,
                Content = feedbackCreateDto.Content
            };

            // Mock phương thức GetByIdAsync trả về khóa học hợp lệ
            _mockUnitOfWork.Setup(u => u.Course.GetByIdAsync(feedbackCreateDto.CourseId, null))
                           .ReturnsAsync(existingCourse);

            // Mock phương thức GetByStudentCodeAsync trả về sinh viên tham gia khóa học
            _mockUnitOfWork.Setup(u => u.StudentCourse.GetByStudentCodeAsync(feedbackCreateDto.StudentCode))
                           .ReturnsAsync(existingStudentCourse);

            // Mock Mapper chuyển đổi FeedbackCreateDto thành Feedback
            _mockMapper.Setup(m => m.Map<Feedback>(It.IsAny<FeedbackCreateDto>()))
                       .Returns(expectedFeedback);

            // Mock phương thức AddAsync để lưu feedback vào cơ sở dữ liệu
            _mockUnitOfWork.Setup(uow => uow.Feedback.AddAsync(It.IsAny<Feedback>()))
                           .Returns(Task.CompletedTask);

            // Mock phương thức SaveChangeAsync để commit dữ liệu vào cơ sở dữ liệu
            _mockUnitOfWork.Setup(uow => uow.SaveChangeAsync())
                           .ReturnsAsync(1);  // Giả lập trả về 1 bản ghi được lưu thành công

            // Act
            await _feedbackService.CreateFeedbackAsync(feedbackCreateDto);

            // Assert
            // Kiểm tra xem phương thức AddAsync đã được gọi với đối tượng Feedback chính xác
            _mockUnitOfWork.Verify(u => u.Feedback.AddAsync(It.IsAny<Feedback>()), Times.Once);

            // Kiểm tra phương thức SaveChangeAsync được gọi đúng
            _mockUnitOfWork.Verify(u => u.SaveChangeAsync(), Times.Once);

            // Kiểm tra nếu feedback đã được lưu thành công
            _mockUnitOfWork.Verify(u => u.Feedback.AddAsync(It.Is<Feedback>(f =>
                f.CourseId == feedbackCreateDto.CourseId &&
                f.StudentCode == feedbackCreateDto.StudentCode &&
                f.Content == feedbackCreateDto.Content)), Times.Once);
        }
        [Test]
        public async Task DeleteFeedbackAsync_ShouldThrowException_WhenFeedbackNotFound()
        {
            // Arrange
            var feedbackId = -1;

            // Giả lập không tìm thấy phản hồi
            _mockUnitOfWork.Setup(u => u.Feedback.GetByIdAsync(feedbackId,null))
                           .ReturnsAsync((Feedback)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _feedbackService.DeleteFeedbackAsync(feedbackId)
            );

            // Kiểm tra thông báo lỗi
            Assert.That(exception.Message, Is.EqualTo("Phản hồi không tồn tại."));
        }
        [Test]
        public async Task DeleteFeedbackAsync_ShouldThrowException_WhenFeedbackNotFound_Id999()
        {
            // Arrange
            var feedbackId = 999;

            // Giả lập không tìm thấy phản hồi
            _mockUnitOfWork.Setup(u => u.Feedback.GetByIdAsync(feedbackId, null))
                           .ReturnsAsync((Feedback)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _feedbackService.DeleteFeedbackAsync(feedbackId)
            );

            // Kiểm tra thông báo lỗi
            Assert.That(exception.Message, Is.EqualTo("Phản hồi không tồn tại."));
        }
        [Test]
        public async Task DeleteFeedbackAsync_ShouldDeleteFeedback_WhenFeedbackExists()
        {
            // Arrange
            var feedbackId = 1;
            var existingFeedback = new Feedback { Id = feedbackId };

            // Giả lập phản hồi tồn tại
            _mockUnitOfWork.Setup(u => u.Feedback.GetByIdAsync(feedbackId, null))
                           .ReturnsAsync(existingFeedback);

            _mockUnitOfWork.Setup(u => u.Feedback.DeleteAsync(It.IsAny<Feedback>()))
                           .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(u => u.SaveChangeAsync())
                           .ReturnsAsync(1);

            // Act
            await _feedbackService.DeleteFeedbackAsync(feedbackId);

            // Assert
            _mockUnitOfWork.Verify(u => u.Feedback.DeleteAsync(It.Is<Feedback>(f => f.Id == feedbackId)), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangeAsync(), Times.Once);
        }
        [Test]
        public async Task DeleteFeedbackAsync_ShouldDeleteFeedback_WhenFeedbackExists_Id2()
        {
            // Arrange
            var feedbackId = 2;
            var existingFeedback = new Feedback { Id = feedbackId };

            // Giả lập phản hồi tồn tại
            _mockUnitOfWork.Setup(u => u.Feedback.GetByIdAsync(feedbackId, null))
                           .ReturnsAsync(existingFeedback);

            _mockUnitOfWork.Setup(u => u.Feedback.DeleteAsync(It.IsAny<Feedback>()))
                           .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(u => u.SaveChangeAsync())
                           .ReturnsAsync(1);

            // Act
            await _feedbackService.DeleteFeedbackAsync(feedbackId);

            // Assert
            _mockUnitOfWork.Verify(u => u.Feedback.DeleteAsync(It.Is<Feedback>(f => f.Id == feedbackId)), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangeAsync(), Times.Once);
        }
        [Test]
        public async Task GetAllFeedbacksAsync_ShouldThrowException_WhenCourseNotFound()
        {
            // Arrange
            var courseId = 999; // Giả lập courseId không tồn tại
            DateTime? feedbackDateStart = null;
            DateTime? feedbackDateEnd = null;

            // Giả lập không tìm thấy khóa học với courseId
            _mockUnitOfWork.Setup(u => u.Course.GetByIdAsync(courseId, null))
                           .ReturnsAsync((Course)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _feedbackService.GetAllFeedbacksAsync(courseId, feedbackDateStart, feedbackDateEnd)
            );

            // Kiểm tra thông báo lỗi
            Assert.That(exception.Message, Is.EqualTo("Course ID does not exist in the system."));
        }
        [Test]
        public async Task GetAllFeedbacksAsync_ShouldThrowException_WhenCourseSmallerThan1()
        {
            // Arrange
            var courseId = -1; // Giả lập courseId không tồn tại
            DateTime? feedbackDateStart = null;
            DateTime? feedbackDateEnd = null;

            // Giả lập không tìm thấy khóa học với courseId
            _mockUnitOfWork.Setup(u => u.Course.GetByIdAsync(courseId, null))
                           .ReturnsAsync((Course)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _feedbackService.GetAllFeedbacksAsync(courseId, feedbackDateStart, feedbackDateEnd)
            );

            // Kiểm tra thông báo lỗi
            Assert.That(exception.Message, Is.EqualTo("Course ID does not exist in the system."));
        }
        [Test]
        public async Task GetAllFeedbacksAsync_ShouldReturnEmptyList_WhenNoFeedbacksFound()
        {
            // Arrange
            var courseId = 1;
            DateTime? feedbackDateStart = null;
            DateTime? feedbackDateEnd = null;

            // Giả lập khóa học tồn tại
            var existingCourse = new Course { Id = courseId };
            _mockUnitOfWork.Setup(u => u.Course.GetByIdAsync(courseId, null))
                           .ReturnsAsync(existingCourse);

            // Giả lập không có phản hồi nào cho khóa học này
            _mockUnitOfWork.Setup(u => u.Feedback.FindAsync(It.IsAny<Expression<Func<Feedback, bool>>>(),null))
                           .ReturnsAsync(new List<Feedback>());

            // Act
            var result = await _feedbackService.GetAllFeedbacksAsync(courseId, feedbackDateStart, feedbackDateEnd);

            // Assert
            Assert.IsEmpty(result); // Kiểm tra trả về danh sách trống
        }
        [Test]
        public async Task GetAllFeedbacksAsync_ShouldReturnFilteredFeedbacks_WhenFeedbacksExist()
        {
            // Arrange
            var courseId = 1;
            var feedbackDateStart = new DateTime(2024, 10, 1);
            var feedbackDateEnd = new DateTime(2024, 10, 31);

            // Giả lập khóa học tồn tại
            var existingCourse = new Course { Id = courseId };
            _mockUnitOfWork.Setup(u => u.Course.GetByIdAsync(courseId, null))
                           .ReturnsAsync(existingCourse);

            // Giả lập phản hồi trong phạm vi ngày cho khóa học này
            var feedbacks = new List<Feedback>
    {
        new Feedback { CourseId = courseId, SubmissionDate = new DateTime(2024, 10, 5) },
        new Feedback { CourseId = courseId, SubmissionDate = new DateTime(2024, 10, 10) }
    };

            _mockUnitOfWork.Setup(u => u.Feedback.FindAsync(It.IsAny<Expression<Func<Feedback, bool>>>(),null))
                           .ReturnsAsync(feedbacks);

            // Giả lập Mapper để chuyển đổi Feedback thành FeedbackDto
            var feedbackDtos = new List<FeedbackDto>
    {
        new FeedbackDto { CourseId = courseId, SubmissionDate = new DateTime(2024, 10, 5) },
        new FeedbackDto { CourseId = courseId, SubmissionDate = new DateTime(2024, 10, 10) }
    };
            _mockMapper.Setup(m => m.Map<IEnumerable<FeedbackDto>>(It.IsAny<IEnumerable<Feedback>>()))
                       .Returns(feedbackDtos);

            // Act
            var result = await _feedbackService.GetAllFeedbacksAsync(courseId, feedbackDateStart, feedbackDateEnd);

            // Assert
            Assert.AreEqual(2, result.Count()); // Kiểm tra số lượng phản hồi trả về là 2
            Assert.That(result.First().SubmissionDate, Is.EqualTo(new DateTime(2024, 10, 5))); // Kiểm tra ngày của phản hồi đầu tiên
        }
        [Test]
        public async Task GetAllFeedbacksAsync_ShouldReturnFilteredFeedbacks_WhenFilterByCourseId()
        {
            // Arrange
            var courseId = 1;
            DateTime? feedbackDateStart = null;
            DateTime? feedbackDateEnd = null;

            // Giả lập khóa học tồn tại
            var existingCourse = new Course { Id = courseId };
            _mockUnitOfWork.Setup(u => u.Course.GetByIdAsync(courseId, null))
                           .ReturnsAsync(existingCourse);

            // Giả lập phản hồi trong phạm vi ngày cho khóa học này
            var feedbacks = new List<Feedback>
    {
        new Feedback { CourseId = courseId, SubmissionDate = new DateTime(2024, 10, 5) },
        new Feedback { CourseId = courseId, SubmissionDate = new DateTime(2024, 10, 10) }
    };

            _mockUnitOfWork.Setup(u => u.Feedback.FindAsync(It.IsAny<Expression<Func<Feedback, bool>>>(), null))
                           .ReturnsAsync(feedbacks);

            // Giả lập Mapper để chuyển đổi Feedback thành FeedbackDto
            var feedbackDtos = new List<FeedbackDto>
    {
        new FeedbackDto { CourseId = courseId, SubmissionDate = new DateTime(2024, 10, 5) },
        new FeedbackDto { CourseId = courseId, SubmissionDate = new DateTime(2024, 10, 10) }
    };
            _mockMapper.Setup(m => m.Map<IEnumerable<FeedbackDto>>(It.IsAny<IEnumerable<Feedback>>()))
                       .Returns(feedbackDtos);

            // Act
            var result = await _feedbackService.GetAllFeedbacksAsync(courseId, feedbackDateStart, feedbackDateEnd);

            // Assert
            Assert.AreEqual(2, result.Count()); // Kiểm tra số lượng phản hồi trả về là 2
            Assert.That(result.First().SubmissionDate, Is.EqualTo(new DateTime(2024, 10, 5))); // Kiểm tra ngày của phản hồi đầu tiên
        }
        [Test]
        public async Task GetAllFeedbacksAsync_ShouldFilterByDateRange_WhenDatesAreProvided()
        {
            // Arrange
            var courseId = 1;
            var feedbackDateStart = new DateTime(2024, 10, 5);
            var feedbackDateEnd = new DateTime(2024, 10, 10);

            // Giả lập khóa học tồn tại
            var existingCourse = new Course { Id = courseId };
            _mockUnitOfWork.Setup(u => u.Course.GetByIdAsync(courseId, null))
                           .ReturnsAsync(existingCourse);

            // Giả lập phản hồi với ngày ngoài phạm vi tìm kiếm
            var feedbacks = new List<Feedback>
    {
        new Feedback { CourseId = courseId, SubmissionDate = new DateTime(2024, 10, 1) }, // Không nằm trong phạm vi
        new Feedback { CourseId = courseId, SubmissionDate = new DateTime(2024, 10, 6) }, // Nằm trong phạm vi
        new Feedback { CourseId = courseId, SubmissionDate = new DateTime(2024, 10, 12) }  // Không nằm trong phạm vi
    };

            _mockUnitOfWork.Setup(u => u.Feedback.FindAsync(It.IsAny<Expression<Func<Feedback, bool>>>(), null))
                           .ReturnsAsync(feedbacks);

            // Giả lập Mapper để chuyển đổi Feedback thành FeedbackDto
            var feedbackDtos = new List<FeedbackDto>
    {
        new FeedbackDto { CourseId = courseId, SubmissionDate = new DateTime(2024, 10, 6) }
    };
            _mockMapper.Setup(m => m.Map<IEnumerable<FeedbackDto>>(It.IsAny<IEnumerable<Feedback>>()))
                       .Returns(feedbackDtos);

            // Act
            var result = await _feedbackService.GetAllFeedbacksAsync(courseId, feedbackDateStart, feedbackDateEnd);

            // Assert
            Assert.AreEqual(1, result.Count()); // Kiểm tra chỉ có 1 phản hồi trong phạm vi ngày
            Assert.That(result.First().SubmissionDate, Is.EqualTo(new DateTime(2024, 10, 6))); // Kiểm tra ngày của phản hồi
        }
        [Test]
        public async Task GetAllFeedbacksAsync_ShouldFilterByDateRange_WhenEndDatesIsProvided()
        {
            // Arrange
            var courseId = 1;
            DateTime? feedbackDateStart = null;
            var feedbackDateEnd = new DateTime(2024, 10, 10);

            // Giả lập khóa học tồn tại
            var existingCourse = new Course { Id = courseId };
            _mockUnitOfWork.Setup(u => u.Course.GetByIdAsync(courseId, null))
                           .ReturnsAsync(existingCourse);

            // Giả lập phản hồi với ngày ngoài phạm vi tìm kiếm
            var feedbacks = new List<Feedback>
    {
        new Feedback { CourseId = courseId, SubmissionDate = new DateTime(2024, 10, 1) }, // Không nằm trong phạm vi
        new Feedback { CourseId = courseId, SubmissionDate = new DateTime(2024, 10, 6) }, // Nằm trong phạm vi
        new Feedback { CourseId = courseId, SubmissionDate = new DateTime(2024, 10, 12) }  // Không nằm trong phạm vi
    };

            _mockUnitOfWork.Setup(u => u.Feedback.FindAsync(It.IsAny<Expression<Func<Feedback, bool>>>(), null))
                           .ReturnsAsync(feedbacks);

            // Giả lập Mapper để chuyển đổi Feedback thành FeedbackDto
            var feedbackDtos = new List<FeedbackDto>
    {
        new FeedbackDto { CourseId = courseId, SubmissionDate = new DateTime(2024, 10, 6) }
    };
            _mockMapper.Setup(m => m.Map<IEnumerable<FeedbackDto>>(It.IsAny<IEnumerable<Feedback>>()))
                       .Returns(feedbackDtos);

            // Act
            var result = await _feedbackService.GetAllFeedbacksAsync(courseId, feedbackDateStart, feedbackDateEnd);

            // Assert
            Assert.AreEqual(1, result.Count()); // Kiểm tra chỉ có 1 phản hồi trong phạm vi ngày
            Assert.That(result.First().SubmissionDate, Is.EqualTo(new DateTime(2024, 10, 6))); // Kiểm tra ngày của phản hồi
        }
        [Test]
        public async Task GetAllFeedbacksAsync_ShouldFilterByDateRange_WhenStartDatesIsProvided()
        {
            // Arrange
            var courseId = 1;
            var feedbackDateStart = new DateTime(2024, 10, 5);
            DateTime? feedbackDateEnd = null;

            // Giả lập khóa học tồn tại
            var existingCourse = new Course { Id = courseId };
            _mockUnitOfWork.Setup(u => u.Course.GetByIdAsync(courseId, null))
                           .ReturnsAsync(existingCourse);

            // Giả lập phản hồi với ngày ngoài phạm vi tìm kiếm
            var feedbacks = new List<Feedback>
    {
        new Feedback { CourseId = courseId, SubmissionDate = new DateTime(2024, 10, 1) }, // Không nằm trong phạm vi
        new Feedback { CourseId = courseId, SubmissionDate = new DateTime(2024, 10, 6) }, // Nằm trong phạm vi
        new Feedback { CourseId = courseId, SubmissionDate = new DateTime(2024, 10, 12) }  // Không nằm trong phạm vi
    };

            _mockUnitOfWork.Setup(u => u.Feedback.FindAsync(It.IsAny<Expression<Func<Feedback, bool>>>(), null))
                           .ReturnsAsync(feedbacks);

            // Giả lập Mapper để chuyển đổi Feedback thành FeedbackDto
            var feedbackDtos = new List<FeedbackDto>
    {
        new FeedbackDto { CourseId = courseId, SubmissionDate = new DateTime(2024, 10, 6) }
    };
            _mockMapper.Setup(m => m.Map<IEnumerable<FeedbackDto>>(It.IsAny<IEnumerable<Feedback>>()))
                       .Returns(feedbackDtos);

            // Act
            var result = await _feedbackService.GetAllFeedbacksAsync(courseId, feedbackDateStart, feedbackDateEnd);

            // Assert
            Assert.AreEqual(1, result.Count()); // Kiểm tra chỉ có 1 phản hồi trong phạm vi ngày
            Assert.That(result.First().SubmissionDate, Is.EqualTo(new DateTime(2024, 10, 6))); // Kiểm tra ngày của phản hồi
        }
        [Test]
        public async Task GetFeedbackByIdAsync_ShouldThrowException_WhenFeedbackNotFound()
        {
            // Arrange
            var validId = 1; // Giả lập id tồn tại nhưng không có phản hồi trong DB

            // Giả lập không tìm thấy phản hồi với id
            _mockUnitOfWork.Setup(u => u.Feedback.GetByIdAsync(validId,null))
                           .ReturnsAsync((Feedback)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _feedbackService.GetFeedbackByIdAsync(validId)
            );

            // Kiểm tra thông báo lỗi
            Assert.That(exception.Message, Is.EqualTo("Phản hồi không tồn tại."));
        }
        [Test]
        public async Task GetFeedbackByIdAsync_ShouldThrowException_WhenFeedbackNotFound_IdSmallerThan1()
        {
            // Arrange
            var validId = 0; // Giả lập id tồn tại nhưng không có phản hồi trong DB

            // Giả lập không tìm thấy phản hồi với id
            _mockUnitOfWork.Setup(u => u.Feedback.GetByIdAsync(validId, null))
                           .ReturnsAsync((Feedback)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _feedbackService.GetFeedbackByIdAsync(validId)
            );

            // Kiểm tra thông báo lỗi
            Assert.That(exception.Message, Is.EqualTo("Phản hồi không tồn tại."));
        }
        [Test]
        public async Task GetFeedbackByIdAsync_ShouldReturnFeedbackDto_WhenFeedbackFound()
        {
            // Arrange
            var validId = 1; // Giả lập id tồn tại

            var feedback = new Feedback
            {
                Id = validId,
                CourseId = 1,
                StudentCode = "123456789",
                Content = "Good course!"
            };

            var feedbackDto = new FeedbackDto
            {
                Id = validId,
                CourseId = 1,
                StudentCode = "123456789",
                Content = "Good course!"
            };

            // Giả lập trả về phản hồi
            _mockUnitOfWork.Setup(u => u.Feedback.GetByIdAsync(validId, null))
                           .ReturnsAsync(feedback);

            // Giả lập Mapper
            _mockMapper.Setup(m => m.Map<FeedbackDto>(It.IsAny<Feedback>()))
                       .Returns(feedbackDto);

            // Act
            var result = await _feedbackService.GetFeedbackByIdAsync(validId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(feedbackDto.Id, result?.Id);
            Assert.AreEqual(feedbackDto.CourseId, result?.CourseId);
            Assert.AreEqual(feedbackDto.StudentCode, result?.StudentCode);
            Assert.AreEqual(feedbackDto.Content, result?.Content);
        }
    }
}
