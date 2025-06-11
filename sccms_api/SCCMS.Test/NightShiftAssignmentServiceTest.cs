using AutoMapper;
using Moq;
using NUnit.Framework;
using SCCMS.Domain.DTOs.NightShiftDtos;
using SCCMS.Domain.Services.Implements;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Tests.Services
{
    [TestFixture]
    public class NightShiftAssignmentServiceTest
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IMapper> _mapperMock;
        private Mock<INotificationService> _notificationServiceMock;
        private NightShiftAssignmentService _nightShiftService;

        [SetUp]
        public void SetUp()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _notificationServiceMock = new Mock<INotificationService>();
            _nightShiftService = new NightShiftAssignmentService(_unitOfWorkMock.Object, _mapperMock.Object, _notificationServiceMock.Object);
        }

        #region Test: AssignStaffToShiftAsync_CreatesNewAssignmentsForUsers

        [Test]
        public async Task AssignStaffToShiftAsync_CreatesNewAssignmentsForUsers()
        {
            // Arrange
            var assignStaffDto = new NightShiftAssignmentCreateDto
            {
                NightShiftId = 5,
                RoomId = 5,
                Date = new DateTime(2024, 12, 10),
                UserIds = new List<int> { 1 }
            };

            // Không có assignments hiện có
            _unitOfWorkMock.Setup(uow => uow.NightShiftAssignment.GetAllAsync(
                    It.IsAny<Expression<Func<NightShiftAssignment, bool>>>(),
                    It.IsAny<string>()))
                .ReturnsAsync(new List<NightShiftAssignment>());

            // Room tồn tại
            _unitOfWorkMock.Setup(uow => uow.Room.GetByIdAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new Room { Id = 5 });

            // NightShift tồn tại
            _unitOfWorkMock.Setup(uow => uow.NightShift.GetByIdAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new NightShift(1, new TimeSpan(22, 0, 0), new TimeSpan(1, 0, 0)));

            // User tồn tại
            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new User { Id = 1 });

            // Act
            await _nightShiftService.AssignStaffToShiftAsync(assignStaffDto);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.AddAsync(It.Is<NightShiftAssignment>(
                a => a.NightShiftId == assignStaffDto.NightShiftId &&
                     a.RoomId == assignStaffDto.RoomId &&
                     a.UserId == 1 &&
                     a.Date == assignStaffDto.Date &&
                     a.Status == NightShiftAssignmentStatus.notStarted)),
                Times.Once, "AddAsync should be called once for the user.");

            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once, "SaveChangeAsync should be called once.");

            _notificationServiceMock.Verify(ns => ns.NotifyUserAsync(
                1,
                $"Bạn được phân công ca trực mới vào ngày {assignStaffDto.Date.ToString("dd/MM/yyyy")}. Truy cập vào lịch trực để xem chi tiết",
                "my-night-shift"),
                Times.Once, "NotifyUserAsync should be called once for the user.");
        }

        #endregion

        #region Test: AssignStaffToShiftAsync_UpdatesExistingAssignmentsIfUserAlreadyAssigned

        [Test]
        public async Task AssignStaffToShiftAsync_UpdatesExistingAssignmentsIfUserAlreadyAssigned()
        {
            // Arrange
            var assignStaffDto = new NightShiftAssignmentCreateDto
            {
                NightShiftId = 1,
                RoomId = 5,
                Date = new DateTime(2024, 12, 10),
                UserIds = new List<int> { 1, 2, 3 }
            };

            var existingAssignments = new List<NightShiftAssignment>
            {
                new NightShiftAssignment { UserId = 1, NightShiftId = 1, RoomId = 5, Date = assignStaffDto.Date, Status = NightShiftAssignmentStatus.completed },
                new NightShiftAssignment { UserId = 2, NightShiftId = 1, RoomId = 5, Date = assignStaffDto.Date, Status = NightShiftAssignmentStatus.completed }
            };

            _unitOfWorkMock.Setup(uow => uow.NightShiftAssignment.GetAllAsync(
                    It.IsAny<Expression<Func<NightShiftAssignment, bool>>>(),
                    It.IsAny<string>()))
                .ReturnsAsync(existingAssignments); // Existing assignments for UserId 1 and 2

            _unitOfWorkMock.Setup(uow => uow.Room.GetByIdAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new Room { Id = 5 }); // Room exists

            _unitOfWorkMock.Setup(uow => uow.NightShift.GetByIdAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new NightShift(1, new TimeSpan(22, 0, 0), new TimeSpan(1, 0, 0))); // NightShift exists

            // User tồn tại
            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync(1, It.IsAny<string>()))
                .ReturnsAsync(new User { Id = 1 });

            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync(2, It.IsAny<string>()))
                .ReturnsAsync(new User { Id = 2 });

            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync(3, It.IsAny<string>()))
                .ReturnsAsync(new User { Id = 3 });

            // Act
            await _nightShiftService.AssignStaffToShiftAsync(assignStaffDto);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.AddAsync(It.Is<NightShiftAssignment>(
                a => a.NightShiftId == assignStaffDto.NightShiftId &&
                     a.RoomId == assignStaffDto.RoomId &&
                     a.UserId == 3 &&
                     a.Date == assignStaffDto.Date &&
                     a.Status == NightShiftAssignmentStatus.notStarted)),
                Times.Once, "AddAsync should be called once for UserId 3.");

            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.UpdateAsync(It.Is<NightShiftAssignment>(
                a => (a.UserId == 1 || a.UserId == 2) &&
                     a.Status == NightShiftAssignmentStatus.notStarted)),
                Times.Exactly(2), "UpdateAsync should be called twice for existing assignments.");

            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once, "SaveChangeAsync should be called once.");

            _notificationServiceMock.Verify(ns => ns.NotifyUserAsync(
                3,
                $"Bạn được phân công ca trực mới vào ngày {assignStaffDto.Date.ToString("dd/MM/yyyy")}. Truy cập vào lịch trực để xem chi tiết",
                "my-night-shift"),
                Times.Once, "NotifyUserAsync should be called once for UserId 3.");
        }

        #endregion

        #region Test: AssignStaffToShiftAsync_ThrowsExceptionIfRoomIdDoesNotExist

        [Test]
        public async Task AssignStaffToShiftAsync_ThrowsExceptionIfRoomIdDoesNotExist()
        {
            // Arrange
            var assignStaffDto = new NightShiftAssignmentCreateDto
            {
                NightShiftId = 1,
                RoomId = 0, // A RoomId that doesn't exist
                Date = new DateTime(2024, 12, 10),
                UserIds = new List<int> { 1, 2, 3 } // 3 users
            };
            var existingAssignments = new List<NightShiftAssignment>
            {
                new NightShiftAssignment { UserId = 4, NightShiftId = 2, RoomId = 6, Date = assignStaffDto.Date, Status = NightShiftAssignmentStatus.notStarted },
                new NightShiftAssignment { UserId = 5, NightShiftId = 2, RoomId = 6, Date = assignStaffDto.Date, Status = NightShiftAssignmentStatus.completed }
            };

            _unitOfWorkMock.Setup(uow => uow.NightShiftAssignment.GetAllAsync(
                    It.IsAny<Expression<Func<NightShiftAssignment, bool>>>(),
                    It.IsAny<string>()))
                .ReturnsAsync(existingAssignments); // Existing assignments for UserId 4 and 5

            // Giả lập Room không tồn tại
            _unitOfWorkMock.Setup(uow => uow.Room.GetByIdAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync((Room)null); // Room does not exist

            // Act & Assert
            var exception =  Assert.ThrowsAsync<ArgumentException>(async () =>
                await _nightShiftService.AssignStaffToShiftAsync(assignStaffDto));
            Assert.AreEqual("Room does not exist.", exception.Message);

            // Verify rằng NightShift.GetByIdAsync không được gọi vì Room không tồn tại
            _unitOfWorkMock.Verify(uow => uow.NightShift.GetByIdAsync(It.IsAny<int>(), It.IsAny<string>()),
                Times.Never, "NightShift.GetByIdAsync should not be called when Room does not exist.");

            // Verify rằng không có assignments hoặc notifications nào được thực hiện
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.AddAsync(It.IsAny<NightShiftAssignment>()),
                Times.Never, "AddAsync should not be called as Room does not exist.");
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.UpdateAsync(It.IsAny<NightShiftAssignment>()),
                Times.Never, "UpdateAsync should not be called as Room does not exist.");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(),
                Times.Never, "SaveChangeAsync should not be called as Room does not exist.");
            _notificationServiceMock.Verify(ns => ns.NotifyUserAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Never, "NotifyUserAsync should not be called as Room does not exist.");
        }

        #endregion

        #region Test: AssignStaffToShiftAsync_ThrowsExceptionIfNightShiftIdDoesNotExist

        [Test]
        public async Task AssignStaffToShiftAsync_ThrowsExceptionIfNightShiftIdDoesNotExist()
        {
            // Arrange
            var assignStaffDto = new NightShiftAssignmentCreateDto
            {
                NightShiftId = 0, // A NightShiftId that doesn't exist
                RoomId = 5,
                Date = new DateTime(2024, 12, 10),
                UserIds = new List<int> { 1, 2, 3 } // 3 users
            };
            var existingAssignments = new List<NightShiftAssignment>
            {
                new NightShiftAssignment { UserId = 4, NightShiftId = 2, RoomId = 6, Date = assignStaffDto.Date, Status = NightShiftAssignmentStatus.notStarted },
                new NightShiftAssignment { UserId = 5, NightShiftId = 2, RoomId = 6, Date = assignStaffDto.Date, Status = NightShiftAssignmentStatus.completed }
            };

            _unitOfWorkMock.Setup(uow => uow.NightShiftAssignment.GetAllAsync(
                    It.IsAny<Expression<Func<NightShiftAssignment, bool>>>(),
                    It.IsAny<string>()))
                .ReturnsAsync(existingAssignments); // Existing assignments for UserId 4 and 5


            _unitOfWorkMock.Setup(uow => uow.Room.GetByIdAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new Room { Id = 5 }); // Room exists

            // Giả lập NightShift không tồn tại
            _unitOfWorkMock.Setup(uow => uow.NightShift.GetByIdAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync((NightShift)null); // NightShift does not exist

            // Act & Assert
            var exception =  Assert.ThrowsAsync<ArgumentException>(async () =>
                await _nightShiftService.AssignStaffToShiftAsync(assignStaffDto));
            Assert.AreEqual("Night shift does not exist.", exception.Message);

            // Verify rằng assignments không được thực hiện vì NightShift không tồn tại
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.AddAsync(It.IsAny<NightShiftAssignment>()),
                Times.Never, "AddAsync should not be called as NightShift does not exist.");
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.UpdateAsync(It.IsAny<NightShiftAssignment>()),
                Times.Never, "UpdateAsync should not be called as NightShift does not exist.");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(),
                Times.Never, "SaveChangeAsync should not be called as NightShift does not exist.");
            _notificationServiceMock.Verify(ns => ns.NotifyUserAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Never, "NotifyUserAsync should not be called as NightShift does not exist.");
        }

        #endregion

        #region Test: AssignStaffToShiftAsync_ThrowsExceptionIfUserDoesNotExist

        [Test]
        public async Task AssignStaffToShiftAsync_ThrowsExceptionIfUserDoesNotExist()
        {

            // Arrange
            var assignStaffDto = new NightShiftAssignmentCreateDto
            {
                NightShiftId = 1,
                RoomId = 5,
                Date = new DateTime(2024, 12, 10),
                UserIds = new List<int> { 1, 2, 3 } // 3 users
            };
            var existingAssignments = new List<NightShiftAssignment>
            {
                new NightShiftAssignment { UserId = 4, NightShiftId = 2, RoomId = 6, Date = assignStaffDto.Date, Status = NightShiftAssignmentStatus.notStarted },
                new NightShiftAssignment { UserId = 5, NightShiftId = 2, RoomId = 6, Date = assignStaffDto.Date, Status = NightShiftAssignmentStatus.completed }
            };

            _unitOfWorkMock.Setup(uow => uow.NightShiftAssignment.GetAllAsync(
                    It.IsAny<Expression<Func<NightShiftAssignment, bool>>>(),
                    It.IsAny<string>()))
                .ReturnsAsync(existingAssignments); // Existing assignments for UserId 4 and 5

            _unitOfWorkMock.Setup(uow => uow.NightShiftAssignment.GetAllAsync(
                    It.IsAny<Expression<Func<NightShiftAssignment, bool>>>(),
                    It.IsAny<string>()))
                .ReturnsAsync(new List<NightShiftAssignment>()); // No existing assignments

            _unitOfWorkMock.Setup(uow => uow.Room.GetByIdAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new Room { Id = 5 }); // Room exists

            _unitOfWorkMock.Setup(uow => uow.NightShift.GetByIdAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new NightShift(1, new TimeSpan(22, 0, 0), new TimeSpan(1, 0, 0))); // NightShift exists

            // Giả lập users
            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync(1, It.IsAny<string>()))
                .ReturnsAsync(new User { Id = 1 });

            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync(2, It.IsAny<string>()))
                .ReturnsAsync((User)null); // User 2 không tồn tại

            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync(3, It.IsAny<string>()))
                .ReturnsAsync(new User { Id = 3 });

            // Act & Assert
            var exception =  Assert.ThrowsAsync<ArgumentException>(async () =>
                await _nightShiftService.AssignStaffToShiftAsync(assignStaffDto));
            Assert.AreEqual("User does not exist.", exception.Message);

            // Verify rằng không có assignments hoặc notifications nào được thực hiện
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.AddAsync(It.IsAny<NightShiftAssignment>()),
                Times.Never, "AddAsync should not be called as at least one user does not exist.");
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.UpdateAsync(It.IsAny<NightShiftAssignment>()),
                Times.Never, "UpdateAsync should not be called as at least one user does not exist.");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(),
                Times.Never, "SaveChangeAsync should not be called as at least one user does not exist.");
            _notificationServiceMock.Verify(ns => ns.NotifyUserAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Never, "NotifyUserAsync should not be called as at least one user does not exist.");
        }

        #endregion

        #region Test: AssignStaffToShiftAsync_ThrowsExceptionIfAnyUserDoesNotExist

        [Test]
        public async Task AssignStaffToShiftAsync_ThrowsExceptionIfAnyUserDoesNotExist()
        {
            // Arrange
            var assignStaffDto = new NightShiftAssignmentCreateDto
            {
                NightShiftId = 1,
                RoomId = 5,
                Date = new DateTime(2024, 12, 10),
                UserIds = new List<int> { 1, 2, 3 } // 3 users
            };

            _unitOfWorkMock.Setup(uow => uow.NightShiftAssignment.GetAllAsync(
                    It.IsAny<Expression<Func<NightShiftAssignment, bool>>>(),
                    It.IsAny<string>()))
                .ReturnsAsync(new List<NightShiftAssignment>()); // No existing assignments

            _unitOfWorkMock.Setup(uow => uow.Room.GetByIdAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new Room { Id = 5 }); // Room exists

            _unitOfWorkMock.Setup(uow => uow.NightShift.GetByIdAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new NightShift(1, new TimeSpan(22, 0, 0), new TimeSpan(1, 0, 0))); // NightShift exists

            // Giả lập users
            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync(1, It.IsAny<string>()))
                .ReturnsAsync(new User { Id = 1 });
            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync(2, It.IsAny<string>()))
                .ReturnsAsync(new User { Id = 2 });
            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync(3, It.IsAny<string>()))
                .ReturnsAsync((User)null); // User 3 không tồn tại

            // Act & Assert
            var exception =  Assert.ThrowsAsync<ArgumentException>(async () =>
                await _nightShiftService.AssignStaffToShiftAsync(assignStaffDto));
            Assert.AreEqual("User does not exist.", exception.Message);

            // Verify rằng không có assignments hoặc notifications nào được thực hiện
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.AddAsync(It.IsAny<NightShiftAssignment>()),
                Times.Never, "AddAsync should not be called as at least one user does not exist.");
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.UpdateAsync(It.IsAny<NightShiftAssignment>()),
                Times.Never, "UpdateAsync should not be called as at least one user does not exist.");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(),
                Times.Never, "SaveChangeAsync should not be called as at least one user does not exist.");
            _notificationServiceMock.Verify(ns => ns.NotifyUserAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Never, "NotifyUserAsync should not be called as at least one user does not exist.");
        }

        #endregion



        [Test]
        public async Task UpdateAssignmentAsync_SuccessfullyUpdatesAssignment_WhenRoleIsManagerAndAssignmentNotToday()
        {
            // Arrange
            var updateDto = new NightShiftAssignmentUpdateDto
            {
                Id = 1,
                NightShiftId = 5,
                RoomId = 5,
                UserId = 1,
                Status = NightShiftAssignmentStatus.notStarted
                // Các thuộc tính khác cần cập nhật
            };

            var existingAssignment = new NightShiftAssignment
            {
                Id = 1,
                NightShiftId = 5,
                RoomId = 5,
                UserId = 1,
                Date = DateTime.Now.AddDays(-1).Date, // Không phải hôm nay
                Status = NightShiftAssignmentStatus.notStarted
            };

            _unitOfWorkMock.Setup(uow => uow.NightShiftAssignment.GetByIdAsync(updateDto.Id, It.IsAny<string>()))
                .ReturnsAsync(existingAssignment);

            _unitOfWorkMock.Setup(uow => uow.Room.GetByIdAsync((int)updateDto.RoomId, It.IsAny<string>()))
                .ReturnsAsync(new Room { Id = 5 });

            _unitOfWorkMock.Setup(uow => uow.NightShift.GetByIdAsync(updateDto.NightShiftId, It.IsAny<string>()))
                .ReturnsAsync(new NightShift(5, new TimeSpan(22, 0, 0), new TimeSpan(1, 0, 0)));

            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync((int)updateDto.UserId, It.IsAny<string>()))
                .ReturnsAsync(new User { Id = 1 });

            // Act
            await _nightShiftService.UpdateAssignmentAsync(updateDto, 2);

            // Assert
            _mapperMock.Verify(m => m.Map(updateDto, existingAssignment), Times.Once, "Mapper.Map should be called once.");
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.UpdateAsync(existingAssignment), Times.Once, "UpdateAsync should be called once.");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once, "SaveChangeAsync should be called once.");
        }

        [Test]
        public async Task UpdateAssignmentAsync_SuccessfullyUpdatesAssignment_WhenRoleIsManagerAndAssignmentNotToday2()
        {
            // Arrange
            var updateDto = new NightShiftAssignmentUpdateDto
            {
                Id = 1,
                NightShiftId = 5,
                RoomId = 1,
                UserId = 1,
                Status = NightShiftAssignmentStatus.notStarted
                // Các thuộc tính khác cần cập nhật
            };

            var existingAssignment = new NightShiftAssignment
            {
                Id = 1,
                NightShiftId = 5,
                RoomId = 5,
                UserId = 1,
                Date = DateTime.Now.AddDays(-1).Date, // Không phải hôm nay
                Status = NightShiftAssignmentStatus.notStarted
            };

            _unitOfWorkMock.Setup(uow => uow.NightShiftAssignment.GetByIdAsync(updateDto.Id, It.IsAny<string>()))
                .ReturnsAsync(existingAssignment);

            _unitOfWorkMock.Setup(uow => uow.Room.GetByIdAsync((int)updateDto.RoomId, It.IsAny<string>()))
                .ReturnsAsync(new Room { Id = 5 });

            _unitOfWorkMock.Setup(uow => uow.NightShift.GetByIdAsync(updateDto.NightShiftId, It.IsAny<string>()))
                .ReturnsAsync(new NightShift(5, new TimeSpan(22, 0, 0), new TimeSpan(1, 0, 0)));

            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync((int)updateDto.UserId, It.IsAny<string>()))
                .ReturnsAsync(new User { Id = 1 });

            // Act
            await _nightShiftService.UpdateAssignmentAsync(updateDto, 2);

            // Assert
            _mapperMock.Verify(m => m.Map(updateDto, existingAssignment), Times.Once, "Mapper.Map should be called once.");
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.UpdateAsync(existingAssignment), Times.Once, "UpdateAsync should be called once.");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once, "SaveChangeAsync should be called once.");
        }

        [Test]
        public async Task UpdateAssignmentAsync_SuccessfullyUpdatesAssignment_WhenRoleIsManagerAndAssignmentNotToday3()
        {
            // Arrange
            var updateDto = new NightShiftAssignmentUpdateDto
            {
                Id = 1,
                NightShiftId = 1,
                RoomId = 5,
                UserId = 1,
                Status = NightShiftAssignmentStatus.notStarted
                // Các thuộc tính khác cần cập nhật
            };

            var existingAssignment = new NightShiftAssignment
            {
                Id = 1,
                NightShiftId = 5,
                RoomId = 5,
                UserId = 1,
                Date = DateTime.Now.AddDays(-1).Date, // Không phải hôm nay
                Status = NightShiftAssignmentStatus.notStarted
            };

            _unitOfWorkMock.Setup(uow => uow.NightShiftAssignment.GetByIdAsync(updateDto.Id, It.IsAny<string>()))
                .ReturnsAsync(existingAssignment);

            _unitOfWorkMock.Setup(uow => uow.Room.GetByIdAsync((int)updateDto.RoomId, It.IsAny<string>()))
                .ReturnsAsync(new Room { Id = 5 });

            _unitOfWorkMock.Setup(uow => uow.NightShift.GetByIdAsync(updateDto.NightShiftId, It.IsAny<string>()))
                .ReturnsAsync(new NightShift(5, new TimeSpan(22, 0, 0), new TimeSpan(1, 0, 0)));

            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync((int)updateDto.UserId, It.IsAny<string>()))
                .ReturnsAsync(new User { Id = 1 });

            // Act
            await _nightShiftService.UpdateAssignmentAsync(updateDto, 2);

            // Assert
            _mapperMock.Verify(m => m.Map(updateDto, existingAssignment), Times.Once, "Mapper.Map should be called once.");
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.UpdateAsync(existingAssignment), Times.Once, "UpdateAsync should be called once.");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once, "SaveChangeAsync should be called once.");
        }

        [Test]
        public async Task UpdateAssignmentAsync_ThrowsArgumentException_WhenRoomDoesNotExist()
        {
            // Arrange
            var updateDto = new NightShiftAssignmentUpdateDto
            {
                Id = 5,
                NightShiftId = 9,
                RoomId = 0, // RoomId không tồn tại
                UserId = 5,
                Status = NightShiftAssignmentStatus.completed
                // Các thuộc tính khác cần cập nhật
            };

            var existingAssignment = new NightShiftAssignment
            {
                Id = 5,
                NightShiftId = 9,
                RoomId = 0,
                UserId = 5,
                Date = DateTime.Now.AddDays(-1).Date, // Không phải hôm nay
                Status = NightShiftAssignmentStatus.notStarted
            };

            _unitOfWorkMock.Setup(uow => uow.NightShiftAssignment.GetByIdAsync(updateDto.Id, It.IsAny<string>()))
                .ReturnsAsync(existingAssignment);

            _unitOfWorkMock.Setup(uow => uow.Room.GetByIdAsync((int)updateDto.RoomId, It.IsAny<string>()))
                .ReturnsAsync((Room)null); // Room không tồn tại

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _nightShiftService.UpdateAssignmentAsync(updateDto, 2));
            Assert.AreEqual("Room does not exist.", exception.Message);

            // Verify rằng không có thao tác nào khác được thực hiện
            _mapperMock.Verify(m => m.Map(It.IsAny<NightShiftAssignmentUpdateDto>(), It.IsAny<NightShiftAssignment>()), Times.Never, "Mapper.Map should not be called.");
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.UpdateAsync(It.IsAny<NightShiftAssignment>()), Times.Never, "UpdateAsync should not be called.");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Never, "SaveChangeAsync should not be called.");
        }

        [Test]
        public async Task UpdateAssignmentAsync_ThrowsArgumentException_WhenNightShiftDoesNotExist()
        {
            // Arrange
            var updateDto = new NightShiftAssignmentUpdateDto
            {
                Id = 6,
                NightShiftId = 0, // NightShiftId không tồn tại
                RoomId = 6,
                UserId = 6,
                Status = NightShiftAssignmentStatus.completed
                // Các thuộc tính khác cần cập nhật
            };

            var existingAssignment = new NightShiftAssignment
            {
                Id = 6,
                NightShiftId = 0,
                RoomId = 6,
                UserId = 6,
                Date = DateTime.Now.AddDays(-1).Date, // Không phải hôm nay
                Status = NightShiftAssignmentStatus.notStarted
            };

            _unitOfWorkMock.Setup(uow => uow.NightShiftAssignment.GetByIdAsync(updateDto.Id, It.IsAny<string>()))
                .ReturnsAsync(existingAssignment);

            _unitOfWorkMock.Setup(uow => uow.Room.GetByIdAsync((int)updateDto.RoomId, It.IsAny<string>()))
                .ReturnsAsync(new Room { Id = 6 });

            _unitOfWorkMock.Setup(uow => uow.NightShift.GetByIdAsync(updateDto.NightShiftId, It.IsAny<string>()))
                .ReturnsAsync((NightShift)null); // NightShift không tồn tại

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _nightShiftService.UpdateAssignmentAsync(updateDto, 2));
            Assert.AreEqual("Night shift does not exist.", exception.Message);

            // Verify rằng không có thao tác nào khác được thực hiện
            _mapperMock.Verify(m => m.Map(It.IsAny<NightShiftAssignmentUpdateDto>(), It.IsAny<NightShiftAssignment>()), Times.Never, "Mapper.Map should not be called.");
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.UpdateAsync(It.IsAny<NightShiftAssignment>()), Times.Never, "UpdateAsync should not be called.");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Never, "SaveChangeAsync should not be called.");
        }



        [Test]
        public async Task UpdateAssignmentAsync_ThrowsArgumentException_WhenUserDoesNotExist()
        {
            // Arrange
            var updateDto = new NightShiftAssignmentUpdateDto
            {
                Id = 7,
                NightShiftId = 10,
                RoomId = 10,
                UserId = 0, // UserId không tồn tại
                Status = NightShiftAssignmentStatus.completed
                // Các thuộc tính khác cần cập nhật
            };

            var existingAssignment = new NightShiftAssignment
            {
                Id = 7,
                NightShiftId = 10,
                RoomId = 10,
                UserId = 0,
                Date = DateTime.Now.AddDays(-1).Date, // Không phải hôm nay
                Status = NightShiftAssignmentStatus.notStarted
            };

            _unitOfWorkMock.Setup(uow => uow.NightShiftAssignment.GetByIdAsync(updateDto.Id, It.IsAny<string>()))
                .ReturnsAsync(existingAssignment);

            _unitOfWorkMock.Setup(uow => uow.Room.GetByIdAsync((int)updateDto.RoomId, It.IsAny<string>()))
                .ReturnsAsync(new Room { Id = 10 });

            _unitOfWorkMock.Setup(uow => uow.NightShift.GetByIdAsync(updateDto.NightShiftId, It.IsAny<string>()))
                .ReturnsAsync(new NightShift(10, new TimeSpan(22, 0, 0), new TimeSpan(1, 0, 0)));

            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync((int)updateDto.UserId, It.IsAny<string>()))
                .ReturnsAsync((User)null); // User không tồn tại

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _nightShiftService.UpdateAssignmentAsync(updateDto, 2));
            Assert.AreEqual("User does not exist.", exception.Message);

            // Verify rằng không có thao tác nào khác được thực hiện
            _mapperMock.Verify(m => m.Map(It.IsAny<NightShiftAssignmentUpdateDto>(), It.IsAny<NightShiftAssignment>()), Times.Never, "Mapper.Map should not be called.");
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.UpdateAsync(It.IsAny<NightShiftAssignment>()), Times.Never, "UpdateAsync should not be called.");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Never, "SaveChangeAsync should not be called.");
        }

        [Test]
        public async Task UpdateAssignmentAsync_ThrowsArgumentException_WhenNíghtShiftAssignmentNotExist()
        {
            // Arrange
            var updateDto = new NightShiftAssignmentUpdateDto
            {
                Id = 0,
                NightShiftId = 10,
                RoomId = 10,
                UserId = 0, // UserId không tồn tại
                Status = NightShiftAssignmentStatus.completed
                // Các thuộc tính khác cần cập nhật
            };

            var existingAssignment = new NightShiftAssignment
            {
                Id = 7,
                NightShiftId = 10,
                RoomId = 10,
                UserId = 0,
                Date = DateTime.Now.AddDays(-1).Date, // Không phải hôm nay
                Status = NightShiftAssignmentStatus.notStarted
            };

            _unitOfWorkMock.Setup(uow => uow.NightShiftAssignment.GetByIdAsync(updateDto.Id, It.IsAny<string>()))
                .ReturnsAsync((NightShiftAssignment)null);

            _unitOfWorkMock.Setup(uow => uow.Room.GetByIdAsync((int)updateDto.RoomId, It.IsAny<string>()))
                .ReturnsAsync(new Room { Id = 10 });

            _unitOfWorkMock.Setup(uow => uow.NightShift.GetByIdAsync(updateDto.NightShiftId, It.IsAny<string>()))
                .ReturnsAsync(new NightShift(10, new TimeSpan(22, 0, 0), new TimeSpan(1, 0, 0)));

            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync((int)updateDto.UserId, It.IsAny<string>()))
                .ReturnsAsync((User)null); // User không tồn tại

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _nightShiftService.UpdateAssignmentAsync(updateDto, 2));
            Assert.AreEqual("User does not exist.", exception.Message);

            // Verify rằng không có thao tác nào khác được thực hiện
            _mapperMock.Verify(m => m.Map(It.IsAny<NightShiftAssignmentUpdateDto>(), It.IsAny<NightShiftAssignment>()), Times.Never, "Mapper.Map should not be called.");
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.UpdateAsync(It.IsAny<NightShiftAssignment>()), Times.Never, "UpdateAsync should not be called.");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Never, "SaveChangeAsync should not be called.");
        }



        [Test]
        public async Task UpdateAssignmentStatusAsync_SuccessfullyRejectsAssignment_WhenUserIsManagerAndAssignmentNotToday()
        {
            // Arrange
            var updateDto = new NightShiftAssignmentRejectDto
            {
                Id = 1,
                Status = NightShiftAssignmentStatus.rejected,
                RejectionReason = "Không đủ thời gian.",

            };

            var existingAssignment = new NightShiftAssignment
            {
                Id = 1,
                NightShiftId = 5,
                RoomId = 5,
                UserId = 1,
                Date = DateTime.Now.AddDays(-1).Date, // Không phải hôm nay
                Status = NightShiftAssignmentStatus.notStarted,
                RejectionReason = null,
                User = new User { UserName = "JohnDoe" }
            };

            _unitOfWorkMock.Setup(uow => uow.NightShiftAssignment.GetByIdAsync(updateDto.Id, It.IsAny<string>()))
                .ReturnsAsync(existingAssignment);

            _unitOfWorkMock.Setup(uow => uow.Room.GetByIdAsync((int)existingAssignment.RoomId, It.IsAny<string>()))
                .ReturnsAsync(new Room { Id = 5 });

            _unitOfWorkMock.Setup(uow => uow.NightShift.GetByIdAsync((int)existingAssignment.NightShiftId, It.IsAny<string>()))
                .ReturnsAsync(new NightShift(5, new TimeSpan(22, 0, 0), new TimeSpan(1, 0, 0)));

            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync((int)existingAssignment.UserId, It.IsAny<string>()))
                .ReturnsAsync(new User { Id = 1, UserName = "JohnDoe" });

            _unitOfWorkMock.Setup(uow => uow.User.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(new List<User>
                {
                    new User { Id = 10, RoleId = 2 },
                    new User { Id = 11, RoleId = 2 }
                });
            _unitOfWorkMock.Setup(uow => uow.StaffFreeTime.FindAsync(It.IsAny<Expression<Func<StaffFreeTime, bool>>>(), It.IsAny<string>()))
          .ReturnsAsync(new List<StaffFreeTime>
          {
          });

            // Act
            await _nightShiftService.UpdateAssignmentStatusAsync(updateDto, 2);

            // Assert
            Assert.AreEqual(NightShiftAssignmentStatus.rejected, existingAssignment.Status, "Assignment status should be updated to rejected.");
            Assert.AreEqual("Không đủ thời gian.", existingAssignment.RejectionReason, "RejectionReason should be set correctly.");
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.UpdateAsync(existingAssignment), Times.Once, "UpdateAsync should be called once.");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once, "SaveChangeAsync should be called once.");
            _notificationServiceMock.Verify(ns => ns.NotifyUserAsync(
                10,
                "JohnDoe đã hủy ca trực ở ngày " + existingAssignment.Date.ToString("dd/MM/yyyy"),
                $"reject-night-shift?id={existingAssignment.Id}"),
                Times.Once, "NotifyUserAsync should be called once for Manager.");
            _notificationServiceMock.Verify(ns => ns.NotifyUserAsync(
                11,
                "JohnDoe đã hủy ca trực ở ngày " + existingAssignment.Date.ToString("dd/MM/yyyy"),
                $"reject-night-shift?id={existingAssignment.Id}"),
                Times.Once, "NotifyUserAsync should be called once for Secretary.");
        }

        [Test]
        public async Task UpdateAssignmentStatusAsync_SuccessfullyRejectsAssignment_WhenUserIsManagerAndAssignmentNotToday2()
        {
            // Arrange
            var updateDto = new NightShiftAssignmentRejectDto
            {
                Id = 5,
                Status = NightShiftAssignmentStatus.rejected,
                RejectionReason = "Không đủ thời gian.",

            };

            var existingAssignment = new NightShiftAssignment
            {
                Id = 5,
                NightShiftId = 5,
                RoomId = 5,
                UserId = 1,
                Date = DateTime.Now.AddDays(-1).Date, // Không phải hôm nay
                Status = NightShiftAssignmentStatus.notStarted,
                RejectionReason = null,
                User = new User { UserName = "JohnDoe" }
            };

            _unitOfWorkMock.Setup(uow => uow.NightShiftAssignment.GetByIdAsync(updateDto.Id, It.IsAny<string>()))
                .ReturnsAsync(existingAssignment);

            _unitOfWorkMock.Setup(uow => uow.Room.GetByIdAsync((int)existingAssignment.RoomId, It.IsAny<string>()))
                .ReturnsAsync(new Room { Id = 5 });

            _unitOfWorkMock.Setup(uow => uow.NightShift.GetByIdAsync((int)existingAssignment.NightShiftId, It.IsAny<string>()))
                .ReturnsAsync(new NightShift(5, new TimeSpan(22, 0, 0), new TimeSpan(1, 0, 0)));

            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync((int)existingAssignment.UserId, It.IsAny<string>()))
                .ReturnsAsync(new User { Id = 1, UserName = "JohnDoe" });

            _unitOfWorkMock.Setup(uow => uow.User.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(new List<User>
                {
                    new User { Id = 10, RoleId = 2 },
                    new User { Id = 11, RoleId = 2 }
                });
            _unitOfWorkMock.Setup(uow => uow.StaffFreeTime.FindAsync(It.IsAny<Expression<Func<StaffFreeTime, bool>>>(), It.IsAny<string>()))
          .ReturnsAsync(new List<StaffFreeTime>
          {
          });

            // Act
            await _nightShiftService.UpdateAssignmentStatusAsync(updateDto, 2);

            // Assert
            Assert.AreEqual(NightShiftAssignmentStatus.rejected, existingAssignment.Status, "Assignment status should be updated to rejected.");
            Assert.AreEqual("Không đủ thời gian.", existingAssignment.RejectionReason, "RejectionReason should be set correctly.");
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.UpdateAsync(existingAssignment), Times.Once, "UpdateAsync should be called once.");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once, "SaveChangeAsync should be called once.");
            _notificationServiceMock.Verify(ns => ns.NotifyUserAsync(
                10,
                "JohnDoe đã hủy ca trực ở ngày " + existingAssignment.Date.ToString("dd/MM/yyyy"),
                $"reject-night-shift?id={existingAssignment.Id}"),
                Times.Once, "NotifyUserAsync should be called once for Manager.");
            _notificationServiceMock.Verify(ns => ns.NotifyUserAsync(
                11,
                "JohnDoe đã hủy ca trực ở ngày " + existingAssignment.Date.ToString("dd/MM/yyyy"),
                $"reject-night-shift?id={existingAssignment.Id}"),
                Times.Once, "NotifyUserAsync should be called once for Secretary.");
        }

        [Test]
        public async Task UpdateAssignmentStatusAsync_SuccessfullyRejectsAssignment_WhenUserIsManagerAndAssignmentNotToday3()
        {
            // Arrange
            var updateDto = new NightShiftAssignmentRejectDto
            {
                Id = 1,
                Status = NightShiftAssignmentStatus.rejected,
                RejectionReason = "Không đủ thời gian.",

            };

            var existingAssignment = new NightShiftAssignment
            {
                Id = 1,
                NightShiftId = 5,
                RoomId = 5,
                UserId = 1,
                Date = DateTime.Now.AddDays(-1).Date, // Không phải hôm nay
                Status = NightShiftAssignmentStatus.notStarted,
                RejectionReason = null,
                User = new User { UserName = "JohnDoe" }
            };

            _unitOfWorkMock.Setup(uow => uow.NightShiftAssignment.GetByIdAsync(updateDto.Id, It.IsAny<string>()))
                .ReturnsAsync(existingAssignment);

            _unitOfWorkMock.Setup(uow => uow.Room.GetByIdAsync((int)existingAssignment.RoomId, It.IsAny<string>()))
                .ReturnsAsync(new Room { Id = 5 });

            _unitOfWorkMock.Setup(uow => uow.NightShift.GetByIdAsync((int)existingAssignment.NightShiftId, It.IsAny<string>()))
                .ReturnsAsync(new NightShift(5, new TimeSpan(22, 0, 0), new TimeSpan(1, 0, 0)));

            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync((int)existingAssignment.UserId, It.IsAny<string>()))
                .ReturnsAsync(new User { Id = 1, UserName = "JohnDoe" });

            _unitOfWorkMock.Setup(uow => uow.User.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(new List<User>
                {
                    new User { Id = 10, RoleId = 2 },
                    new User { Id = 11, RoleId = 2 }
                });
            _unitOfWorkMock.Setup(uow => uow.StaffFreeTime.FindAsync(It.IsAny<Expression<Func<StaffFreeTime, bool>>>(), It.IsAny<string>()))
                    .ReturnsAsync(new List<StaffFreeTime>
                    {
                    });

            // Act
            await _nightShiftService.UpdateAssignmentStatusAsync(updateDto, 2);

            // Assert
            Assert.AreEqual(NightShiftAssignmentStatus.rejected, existingAssignment.Status, "Assignment status should be updated to rejected.");
            Assert.AreEqual("Không đủ thời gian.", existingAssignment.RejectionReason, "RejectionReason should be set correctly.");
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.UpdateAsync(existingAssignment), Times.Once, "UpdateAsync should be called once.");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once, "SaveChangeAsync should be called once.");
            _notificationServiceMock.Verify(ns => ns.NotifyUserAsync(
                10,
                "JohnDoe đã hủy ca trực ở ngày " + existingAssignment.Date.ToString("dd/MM/yyyy"),
                $"reject-night-shift?id={existingAssignment.Id}"),
                Times.Once, "NotifyUserAsync should be called once for Manager.");
            _notificationServiceMock.Verify(ns => ns.NotifyUserAsync(
                11,
                "JohnDoe đã hủy ca trực ở ngày " + existingAssignment.Date.ToString("dd/MM/yyyy"),
                $"reject-night-shift?id={existingAssignment.Id}"),
                Times.Once, "NotifyUserAsync should be called once for Secretary.");
        }


        [Test]
        public async Task UpdateAssignmentStatusAsync_ThrowsArgumentException_WhenAssignmentDoesNotExist()
        {
            // Arrange
            var updateDto = new NightShiftAssignmentRejectDto
            {
                Id = 4,
                Status = NightShiftAssignmentStatus.rejected,
                RejectionReason = "Không phù hợp."
            };

            _unitOfWorkMock.Setup(uow => uow.NightShiftAssignment.GetByIdAsync(updateDto.Id, It.IsAny<string>()))
                .ReturnsAsync((NightShiftAssignment)null); // Assignment không tồn tại

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _nightShiftService.UpdateAssignmentStatusAsync(updateDto, 2));
            Assert.AreEqual("Ca trực không tồn tại.", exception.Message);

            // Verify rằng không có thao tác nào khác được thực hiện
            _mapperMock.Verify(m => m.Map(It.IsAny<NightShiftAssignmentRejectDto>(), It.IsAny<NightShiftAssignment>()), Times.Never, "Mapper.Map should not be called.");
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.UpdateAsync(It.IsAny<NightShiftAssignment>()), Times.Never, "UpdateAsync should not be called.");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Never, "SaveChangeAsync should not be called.");
            _notificationServiceMock.Verify(ns => ns.NotifyUserAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never, "NotifyUserAsync should not be called.");
        }

        [Test]
        public async Task UpdateAssignmentStatusAsync_ThrowsArgumentException_WhenRejectingWithoutRejectionReason()
        {
            // Arrange
            var updateDto = new NightShiftAssignmentRejectDto
            {
                Id = 8,
                Status = NightShiftAssignmentStatus.rejected,
                RejectionReason = "" // Không có lý do từ chối
            };

            var existingAssignment = new NightShiftAssignment
            {
                Id = 8,
                NightShiftId = 8,
                RoomId = 8,
                UserId = 5,
                Date = DateTime.Now.AddDays(-1).Date, // Không phải hôm nay
                Status = NightShiftAssignmentStatus.notStarted,
                RejectionReason = null,
                User = new User { UserName = "EveFoster" }
            };

            _unitOfWorkMock.Setup(uow => uow.NightShiftAssignment.GetByIdAsync(updateDto.Id, It.IsAny<string>()))
                .ReturnsAsync(existingAssignment);

            _unitOfWorkMock.Setup(uow => uow.Room.GetByIdAsync((int)existingAssignment.RoomId, It.IsAny<string>()))
                .ReturnsAsync(new Room { Id = 8 });

            _unitOfWorkMock.Setup(uow => uow.NightShift.GetByIdAsync((int)existingAssignment.NightShiftId, It.IsAny<string>()))
                .ReturnsAsync(new NightShift(8, new TimeSpan(22, 0, 0), new TimeSpan(1, 0, 0)));

            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync((int)existingAssignment.UserId, It.IsAny<string>()))
                .ReturnsAsync(new User { Id = 5, UserName = "EveFoster" });

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _nightShiftService.UpdateAssignmentStatusAsync(updateDto, 2));
            Assert.AreEqual("Lý do từ chối là bắt buộc khi từ chối ca trực.", exception.Message);

            // Verify rằng không có thao tác nào khác được thực hiện
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.UpdateAsync(It.IsAny<NightShiftAssignment>()), Times.Never, "UpdateAsync should not be called.");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Never, "SaveChangeAsync should not be called.");
            _notificationServiceMock.Verify(ns => ns.NotifyUserAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never, "NotifyUserAsync should not be called.");
        }


        #region Test: DeleteAssignmentAsync_SuccessfullyDeletesAssignment_WithUserId (assignmentId=5)

        [Test]
        public async Task DeleteAssignmentAsync_SuccessfullyDeletesAssignment_WithUserId()
        {
            // Arrange
            int assignmentId = 5;

            var existingAssignment = new NightShiftAssignment
            {
                Id = assignmentId,
                NightShiftId = 5,
                RoomId = 5,
                UserId = 10, // Có UserId
                Date = new DateTime(2024, 12, 10),
                Status = NightShiftAssignmentStatus.notStarted,
                User = new User { Id = 10, UserName = "JohnDoe" }
            };

            _unitOfWorkMock.Setup(uow => uow.NightShiftAssignment.GetByIdAsync(assignmentId, It.IsAny<string>()))
                .ReturnsAsync(existingAssignment);

            _unitOfWorkMock.Setup(uow => uow.NightShiftAssignment.DeleteAsync(existingAssignment))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
                .ReturnsAsync(1);

            // Setup FindAsync to return users with Manager and Secretary roles for notifications
            _unitOfWorkMock.Setup(uow => uow.User.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(new List<User>
                {
                    new User { Id = 10, RoleId = 2 },
                    new User { Id = 11, RoleId = 2 }
                });

            // Act
            await _nightShiftService.DeleteAssignmentAsync(assignmentId);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.GetByIdAsync(assignmentId, It.IsAny<string>()), Times.Once, "GetByIdAsync should be called once.");
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.DeleteAsync(existingAssignment), Times.Once, "DeleteAsync should be called once.");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once, "SaveChangeAsync should be called once.");
            _notificationServiceMock.Verify(ns => ns.NotifyUserAsync(
                10,
                $"Bạn được hủy ca trực vào ngày {existingAssignment.Date.ToString("dd/MM/yyyy")}. Truy cập vào lịch trực để xem chi tiết",
                "my-night-shift"),
                Times.Once, "NotifyUserAsync should be called once for the user.");
        }

        #endregion

        #region Test: DeleteAssignmentAsync_SuccessfullyDeletesAssignment_WithoutUserId (assignmentId=1)

        [Test]
        public async Task DeleteAssignmentAsync_SuccessfullyDeletesAssignment_WithoutUserId()
        {
            // Arrange
            int assignmentId = 1;

            var existingAssignment = new NightShiftAssignment
            {
                Id = assignmentId,
                NightShiftId = 1,
                RoomId = 1,
                UserId = null, // Không có UserId
                Date = new DateTime(2024, 12, 11),
                Status = NightShiftAssignmentStatus.completed,
                User = null
            };

            _unitOfWorkMock.Setup(uow => uow.NightShiftAssignment.GetByIdAsync(assignmentId, It.IsAny<string>()))
                .ReturnsAsync(existingAssignment);

            _unitOfWorkMock.Setup(uow => uow.NightShiftAssignment.DeleteAsync(existingAssignment))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
                .ReturnsAsync(1);

            // Act
            await _nightShiftService.DeleteAssignmentAsync(assignmentId);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.GetByIdAsync(assignmentId, It.IsAny<string>()), Times.Once, "GetByIdAsync should be called once.");
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.DeleteAsync(existingAssignment), Times.Once, "DeleteAsync should be called once.");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once, "SaveChangeAsync should be called once.");
            // Không có UserId, nên không gọi NotifyUserAsync
            _notificationServiceMock.Verify(ns => ns.NotifyUserAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never, "NotifyUserAsync should not be called as UserId is null.");
        }

        #endregion

        #region Test: DeleteAssignmentAsync_ThrowsArgumentException_WhenAssignmentDoesNotExist (assignmentId=0)

        [Test]
        public async Task DeleteAssignmentAsync_ThrowsArgumentException_WhenAssignmentDoesNotExist()
        {
            // Arrange
            int assignmentId = 0;

            _unitOfWorkMock.Setup(uow => uow.NightShiftAssignment.GetByIdAsync(assignmentId, It.IsAny<string>()))
                .ReturnsAsync((NightShiftAssignment)null); // Assignment không tồn tại

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _nightShiftService.DeleteAssignmentAsync(assignmentId));
            Assert.AreEqual("Không tìm thấy ca trực", exception.Message, "Exception message should be 'Assignment not found.'");

            // Verify rằng không có thao tác nào khác được thực hiện
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.DeleteAsync(It.IsAny<NightShiftAssignment>()), Times.Never, "DeleteAsync should not be called.");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Never, "SaveChangeAsync should not be called.");
            _notificationServiceMock.Verify(ns => ns.NotifyUserAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never, "NotifyUserAsync should not be called.");
        }

        #endregion

    }
}
