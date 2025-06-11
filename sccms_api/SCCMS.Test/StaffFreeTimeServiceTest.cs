using AutoMapper;
using Moq;
using SCCMS.Domain.DTOs.StaffFreeTimeDtos;
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
    public class StaffFreeTimeServiceTest
    {
        private Mock<IUnitOfWork>? _unitOfWorkMock;
        private Mock<IMapper>? _mapperMock;
        private StaffFreeTimeService? _staffFreeTimeService;

        [SetUp]
        public void Setup()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _staffFreeTimeService = new StaffFreeTimeService(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        [Test]
        public async Task GetAllStaffFreeTimesAsync_UserIdFilter_ReturnsFilteredStaffFreeTimes()
        {
            // Arrange
            var userId = 1;
            var freeTimes = new List<StaffFreeTime>
    {
        new StaffFreeTime { Id = 1, UserId = 1, CourseId = 1, Date = DateTime.Now },
        new StaffFreeTime { Id = 2, UserId = 2, CourseId = 1, Date = DateTime.Now.AddDays(1) }
    };

            _unitOfWorkMock.Setup(uow => uow.StaffFreeTime.GetAllAsync(It.IsAny<Expression<Func<StaffFreeTime, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(freeTimes.Where(ft => ft.UserId == userId).ToList());

            var expectedDtos = freeTimes
                .Where(ft => ft.UserId == userId)
                .Select(ft => new StaffFreeTimeDto { Id = ft.Id, UserId = ft.UserId, CourseId = ft.CourseId, Date = ft.Date })
                .ToList();

            _mapperMock.Setup(m => m.Map<IEnumerable<StaffFreeTimeDto>>(It.IsAny<IEnumerable<StaffFreeTime>>()))
                .Returns(expectedDtos);

            // Act
            var result = await _staffFreeTimeService.GetAllStaffFreeTimesAsync(userId, null, null);

            // Assert
            Assert.NotNull(result, "Result should not be null");
            Assert.AreEqual(1, result.Count(), "Expected 1 free time entry for UserId = 1");
            Assert.AreEqual(userId, result.First().UserId, "The userId of the result should be 1");
        }

        [Test]
        public async Task GetAllStaffFreeTimesAsync_CourseIdFilter_ReturnsFilteredStaffFreeTimes()
        {
            // Arrange
            var courseId = 1;
            var freeTimes = new List<StaffFreeTime>
    {
        new StaffFreeTime { Id = 1, UserId = 1, CourseId = 1, Date = DateTime.Now },
        new StaffFreeTime { Id = 2, UserId = 2, CourseId = 1, Date = DateTime.Now.AddDays(1) },
        new StaffFreeTime { Id = 3, UserId = 1, CourseId = 2, Date = DateTime.Now.AddDays(2) }
    };

            _unitOfWorkMock.Setup(uow => uow.StaffFreeTime.GetAllAsync(It.IsAny<Expression<Func<StaffFreeTime, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(freeTimes.Where(ft => ft.CourseId == courseId).ToList());

            var expectedDtos = freeTimes
                .Where(ft => ft.CourseId == courseId)
                .Select(ft => new StaffFreeTimeDto { Id = ft.Id, UserId = ft.UserId, CourseId = ft.CourseId, Date = ft.Date })
                .ToList();

            _mapperMock.Setup(m => m.Map<IEnumerable<StaffFreeTimeDto>>(It.IsAny<IEnumerable<StaffFreeTime>>()))
                .Returns(expectedDtos);

            // Act
            var result = await _staffFreeTimeService.GetAllStaffFreeTimesAsync(null, courseId, null);

            // Assert
            Assert.NotNull(result, "Result should not be null");
            Assert.AreEqual(2, result.Count(), "Expected 2 free time entries for CourseId = 1");
        }


        [Test]
        public async Task GetAllStaffFreeTimesAsync_DateTimeFilter_ReturnsFilteredStaffFreeTimes()
        {
            // Arrange
            var dateTime = DateTime.Now.Date;
            var freeTimes = new List<StaffFreeTime>
    {
        new StaffFreeTime { Id = 1, UserId = 1, CourseId = 1, Date = DateTime.Now },
        new StaffFreeTime { Id = 2, UserId = 2, CourseId = 1, Date = DateTime.Now.AddDays(1) },
        new StaffFreeTime { Id = 3, UserId = 1, CourseId = 2, Date = DateTime.Now.AddDays(-1) }
    };

            _unitOfWorkMock.Setup(uow => uow.StaffFreeTime.GetAllAsync(It.IsAny<Expression<Func<StaffFreeTime, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(freeTimes.Where(ft => ft.Date.Date == dateTime).ToList());

            var expectedDtos = freeTimes
                .Where(ft => ft.Date.Date == dateTime)
                .Select(ft => new StaffFreeTimeDto { Id = ft.Id, UserId = ft.UserId, CourseId = ft.CourseId, Date = ft.Date })
                .ToList();

            _mapperMock.Setup(m => m.Map<IEnumerable<StaffFreeTimeDto>>(It.IsAny<IEnumerable<StaffFreeTime>>()))
                .Returns(expectedDtos);

            // Act
            var result = await _staffFreeTimeService.GetAllStaffFreeTimesAsync(null, null, dateTime);

            // Assert
            Assert.NotNull(result, "Result should not be null");
            Assert.AreEqual(1, result.Count(), "Expected 1 free time entry for the given date");
        }

        [Test]
        public async Task GetAllStaffFreeTimesAsync_MultipleFilters_ReturnsFilteredStaffFreeTimes()
        {
            // Arrange
            var userId = 1;
            var courseId = 1;
            var dateTime = DateTime.Now.Date;

            var freeTimes = new List<StaffFreeTime>
    {
        new StaffFreeTime { Id = 1, UserId = 1, CourseId = 1, Date = DateTime.Now },
        new StaffFreeTime { Id = 2, UserId = 2, CourseId = 1, Date = DateTime.Now.AddDays(1) },
        new StaffFreeTime { Id = 3, UserId = 1, CourseId = 2, Date = DateTime.Now.AddDays(-1) }
    };

            _unitOfWorkMock.Setup(uow => uow.StaffFreeTime.GetAllAsync(It.IsAny<Expression<Func<StaffFreeTime, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(freeTimes.Where(ft => ft.UserId == userId && ft.CourseId == courseId && ft.Date.Date == dateTime).ToList());

            var expectedDtos = freeTimes
                .Where(ft => ft.UserId == userId && ft.CourseId == courseId && ft.Date.Date == dateTime)
                .Select(ft => new StaffFreeTimeDto { Id = ft.Id, UserId = ft.UserId, CourseId = ft.CourseId, Date = ft.Date })
                .ToList();

            _mapperMock.Setup(m => m.Map<IEnumerable<StaffFreeTimeDto>>(It.IsAny<IEnumerable<StaffFreeTime>>()))
                .Returns(expectedDtos);

            // Act
            var result = await _staffFreeTimeService.GetAllStaffFreeTimesAsync(userId, courseId, dateTime);

            // Assert
            Assert.NotNull(result, "Result should not be null");
            Assert.AreEqual(1, result.Count(), "Expected 1 free time entry for the specified filters");
        }




        [Test]
        public async Task GetStaffFreeTimeByIdAsync_ValidId_ReturnsStaffFreeTime()
        {
            // Arrange
            var id = 5;
            var freeTime = new StaffFreeTime
            {
                Id = id,
                UserId = 1,
                CourseId = 1,
                Date = DateTime.Now
            };

            var freeTimeDto = new StaffFreeTimeDto
            {
                Id = freeTime.Id,
                UserId = freeTime.UserId,
                CourseId = freeTime.CourseId,
                Date = freeTime.Date
            };

            _unitOfWorkMock.Setup(uow => uow.StaffFreeTime.GetAsync(It.IsAny<Expression<Func<StaffFreeTime, bool>>>(), It.IsAny<bool>(),It.IsAny<string>()))
                .ReturnsAsync(freeTime);

            _mapperMock.Setup(m => m.Map<StaffFreeTimeDto>(It.IsAny<StaffFreeTime>()))
                .Returns(freeTimeDto);

            // Act
            var result = await _staffFreeTimeService.GetStaffFreeTimeByIdAsync(id);

            // Assert
            Assert.NotNull(result, "Result should not be null");
            Assert.AreEqual(id, result.Id, "The Id of the result should match the requested Id");
            Assert.AreEqual(freeTime.UserId, result.UserId, "The UserId should match the expected value");
            Assert.AreEqual(freeTime.CourseId, result.CourseId, "The CourseId should match the expected value");
            Assert.AreEqual(freeTime.Date, result.Date, "The Date should match the expected value");
        }


        [Test]
        public void GetStaffFreeTimeByIdAsync_IdDoesNotExist_ThrowsArgumentException()
        {
            // Arrange
            var id = 0; // Assuming 0 is an invalid ID
            _unitOfWorkMock.Setup(uow => uow.StaffFreeTime.GetAsync(It.IsAny<Expression<Func<StaffFreeTime, bool>>>(), It.IsAny<bool>(), It.IsAny<string>()))
                .ReturnsAsync((StaffFreeTime)null);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _staffFreeTimeService.GetStaffFreeTimeByIdAsync(id),
                "Thời gian rảnh không tồn tại."); // Ensure the exception message matches
        }

        [Test]
        public async Task GetStaffFreeTimeByIdAsync_ValidId_ReturnsCorrectFreeTime()
        {
            // Arrange
            var id = 1;
            var freeTime = new StaffFreeTime
            {
                Id = id,
                UserId = 1,
                CourseId = 1,
                Date = DateTime.Now.AddDays(2)
            };

            var freeTimeDto = new StaffFreeTimeDto
            {
                Id = freeTime.Id,
                UserId = freeTime.UserId,
                CourseId = freeTime.CourseId,
                Date = freeTime.Date
            };

            _unitOfWorkMock.Setup(uow => uow.StaffFreeTime.GetAsync(It.IsAny<Expression<Func<StaffFreeTime, bool>>>(), It.IsAny<bool>(), It.IsAny<string>()))
                .ReturnsAsync(freeTime);

            _mapperMock.Setup(m => m.Map<StaffFreeTimeDto>(It.IsAny<StaffFreeTime>()))
                .Returns(freeTimeDto);

            // Act
            var result = await _staffFreeTimeService.GetStaffFreeTimeByIdAsync(id);

            // Assert
            Assert.NotNull(result, "Result should not be null");
            Assert.AreEqual(id, result.Id, "The Id of the result should match the requested Id");
            Assert.AreEqual(freeTime.UserId, result.UserId, "The UserId should match the expected value");
            Assert.AreEqual(freeTime.CourseId, result.CourseId, "The CourseId should match the expected value");
            Assert.AreEqual(freeTime.Date, result.Date, "The Date should match the expected value");
        }

        [Test]
        public async Task DeleteStaffFreeTimeAsync_ValidId_SuccessfullyDeletesFreeTime()
        {
            // Arrange
            var id = 5;
            var freeTime = new StaffFreeTime
            {
                Id = id,
                UserId = 1,
                CourseId = 1,
                Date = DateTime.Now
            };

            _unitOfWorkMock.Setup(uow => uow.StaffFreeTime.GetByIdAsync(id, It.IsAny<string>()))
                .ReturnsAsync(freeTime);  // Return the free time entity with the specified id

            // Act
            await _staffFreeTimeService.DeleteStaffFreeTimeAsync(id);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.StaffFreeTime.DeleteAsync(freeTime), Times.Once, "DeleteAsync should have been called once");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once, "SaveChangeAsync should have been called once");
        }

        [Test]
        public void DeleteStaffFreeTimeAsync_InvalidId_ThrowsArgumentException()
        {
            // Arrange
            var id = 0;  // Assuming 0 is an invalid ID
            _unitOfWorkMock.Setup(uow => uow.StaffFreeTime.GetByIdAsync(id, It.IsAny<string>()))
                .ReturnsAsync((StaffFreeTime)null);  // Return null since no record exists

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _staffFreeTimeService.DeleteStaffFreeTimeAsync(id));
            Assert.That(ex.Message, Is.EqualTo("Thời gian rảnh không tồn tại."), "Expected exception message should match.");
        }

        [Test]
        public async Task DeleteStaffFreeTimeAsync_ExistingRecord_SuccessfulDeletion_VerifyUnitOfWork()
        {
            // Arrange
            var id = 1;
            var freeTime = new StaffFreeTime
            {
                Id = id,
                UserId = 2,
                CourseId = 3,
                Date = DateTime.Now.AddDays(1)
            };

            _unitOfWorkMock.Setup(uow => uow.StaffFreeTime.GetByIdAsync(id, It.IsAny<string>()))
                .ReturnsAsync(freeTime);  // Return the free time entity

            // Act
            await _staffFreeTimeService.DeleteStaffFreeTimeAsync(id);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.StaffFreeTime.DeleteAsync(freeTime), Times.Once, "DeleteAsync should be called once");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once, "SaveChangeAsync should be called once to persist the changes");
        }
    }
}
