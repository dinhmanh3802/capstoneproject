using AutoMapper;
using Moq;
using SCCMS.Domain.DTOs.RoomDtos;
using SCCMS.Domain.Services.Implements;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Test
{
    [TestFixture]
    public class RoomServiceTest
    {
        private Mock<IUnitOfWork>? _unitOfWorkMock;
        private Mock<IMapper>? _mapperMock;
        private RoomService? _roomService;

        [SetUp]
        public void Setup()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _roomService = new RoomService(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        [Test]
        public async Task CreateRoomAsync_ValidInput_RoomCreatedSuccessfully()
        {
            // Arrange
            var roomDto = new RoomCreateDto
            {
                CourseId = 1,
                Name = "abc",
                Gender = Gender.Male,
                NumberOfStaff = 5,
                StudentGroupId = new[] { 1 }
            };

            var course = new Course
            {
                Id = 1,
                Status = CourseStatus.inProgress, // Assuming the course is in progress and can accept rooms
            };

            var studentGroup = new List<StudentGroup>
        {
            new StudentGroup { Id = 1, RoomId = null }
        };

            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(roomDto.CourseId, It.IsAny<string>()))
                .ReturnsAsync(course);

            _unitOfWorkMock.Setup(uow => uow.Room.GetAsync(It.IsAny<Expression<Func<Room, bool>>>(), It.IsAny<bool>(), It.IsAny<string>()))
                .ReturnsAsync((Room)null); // No existing room with the same name for the same course

            _mapperMock.Setup(m => m.Map<Room>(It.IsAny<RoomCreateDto>()))
                .Returns(new Room { Id = 1, Name = roomDto.Name, CourseId = roomDto.CourseId });

            _unitOfWorkMock.Setup(uow => uow.Room.AddAsync(It.IsAny<Room>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
                .ReturnsAsync(1);

            _unitOfWorkMock.Setup(uow => uow.StudentGroup.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(studentGroup);

            _unitOfWorkMock.Setup(uow => uow.StudentGroup.UpdateRangeAsync(It.IsAny<IEnumerable<StudentGroup>>()))
                .Returns(Task.CompletedTask);

            // Act
            await _roomService.CreateRoomAsync(roomDto);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.Room.AddAsync(It.IsAny<Room>()), Times.Once, "Room should be added");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Exactly(2), "SaveChanges should be called twice");
            Assert.AreEqual(1, studentGroup[0].RoomId, "Student group should be assigned the new room Id");
        }

        [Test]
        public async Task CreateRoomAsync_ValidInput2_RoomCreatedSuccessfully()
        {
            // Arrange
            var roomDto = new RoomCreateDto
            {
                CourseId = 1,
                Name = "abc",
                Gender = Gender.Female,
                NumberOfStaff = 5,
                StudentGroupId = new[] { 1 }
            };

            var course = new Course
            {
                Id = 1,
                Status = CourseStatus.inProgress, // Assuming the course is in progress and can accept rooms
            };

            var studentGroup = new List<StudentGroup>
        {
            new StudentGroup { Id = 1, RoomId = null }
        };

            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(roomDto.CourseId, It.IsAny<string>()))
                .ReturnsAsync(course);

            _unitOfWorkMock.Setup(uow => uow.Room.GetAsync(It.IsAny<Expression<Func<Room, bool>>>(), It.IsAny<bool>(), It.IsAny<string>()))
                .ReturnsAsync((Room)null); // No existing room with the same name for the same course

            _mapperMock.Setup(m => m.Map<Room>(It.IsAny<RoomCreateDto>()))
                .Returns(new Room { Id = 1, Name = roomDto.Name, CourseId = roomDto.CourseId });

            _unitOfWorkMock.Setup(uow => uow.Room.AddAsync(It.IsAny<Room>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
                .ReturnsAsync(1);

            _unitOfWorkMock.Setup(uow => uow.StudentGroup.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(studentGroup);

            _unitOfWorkMock.Setup(uow => uow.StudentGroup.UpdateRangeAsync(It.IsAny<IEnumerable<StudentGroup>>()))
                .Returns(Task.CompletedTask);

            // Act
            await _roomService.CreateRoomAsync(roomDto);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.Room.AddAsync(It.IsAny<Room>()), Times.Once, "Room should be added");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Exactly(2), "SaveChanges should be called twice");
            Assert.AreEqual(1, studentGroup[0].RoomId, "Student group should be assigned the new room Id");
        }

        [Test]
        public async Task CreateRoomAsync_ValidInput3_RoomCreatedSuccessfully()
        {
            // Arrange
            var roomDto = new RoomCreateDto
            {
                CourseId = 1,
                Name = "a",
                Gender = Gender.Male,
                NumberOfStaff = 5,
                StudentGroupId = new[] { 1 }
            };

            var course = new Course
            {
                Id = 1,
                Status = CourseStatus.inProgress, // Assuming the course is in progress and can accept rooms
            };

            var studentGroup = new List<StudentGroup>
        {
            new StudentGroup { Id = 1, RoomId = null }
        };

            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(roomDto.CourseId, It.IsAny<string>()))
                .ReturnsAsync(course);

            _unitOfWorkMock.Setup(uow => uow.Room.GetAsync(It.IsAny<Expression<Func<Room, bool>>>(), It.IsAny<bool>(), It.IsAny<string>()))
                .ReturnsAsync((Room)null); // No existing room with the same name for the same course

            _mapperMock.Setup(m => m.Map<Room>(It.IsAny<RoomCreateDto>()))
                .Returns(new Room { Id = 1, Name = roomDto.Name, CourseId = roomDto.CourseId });

            _unitOfWorkMock.Setup(uow => uow.Room.AddAsync(It.IsAny<Room>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
                .ReturnsAsync(1);

            _unitOfWorkMock.Setup(uow => uow.StudentGroup.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(studentGroup);

            _unitOfWorkMock.Setup(uow => uow.StudentGroup.UpdateRangeAsync(It.IsAny<IEnumerable<StudentGroup>>()))
                .Returns(Task.CompletedTask);

            // Act
            await _roomService.CreateRoomAsync(roomDto);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.Room.AddAsync(It.IsAny<Room>()), Times.Once, "Room should be added");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Exactly(2), "SaveChanges should be called twice");
            Assert.AreEqual(1, studentGroup[0].RoomId, "Student group should be assigned the new room Id");
        }


        [Test]
        public async Task CreateRoomAsync_CourseClosed_ThrowsInvalidOperationException()
        {
            // Arrange
            var roomDto = new RoomCreateDto
            {
                CourseId = 1,
                Name = "abc",
                Gender = Gender.Male,
                NumberOfStaff = 5,
                StudentGroupId = new[] { 1 }
            };

            var course = new Course
            {
                Id = 1,
                Status = CourseStatus.closed, // Course is closed, should throw exception
            };

            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(roomDto.CourseId, It.IsAny<string>()))
                .ReturnsAsync(course);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _roomService.CreateRoomAsync(roomDto));
            Assert.AreEqual("Không thể thêm phòng vào khóa tu đã kết thúc.", ex.Message, "Exception message should match");
        }

        [Test]
        public async Task CreateRoomAsync_RoomNameAlreadyExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var roomDto = new RoomCreateDto
            {
                CourseId = 1,
                Name = "abc",
                Gender = Gender.Male,
                NumberOfStaff = 5,
                StudentGroupId = new[] { 1 }
            };

            var course = new Course
            {
                Id = 1,
                Status = CourseStatus.inProgress, // Course is active
            };

            var existingRoom = new Room
            {
                Id = 1,
                Name = "abc",
                CourseId = 1
            };

            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(roomDto.CourseId, It.IsAny<string>()))
                .ReturnsAsync(course);

            _unitOfWorkMock.Setup(uow => uow.Room.GetAsync(It.IsAny<Expression<Func<Room, bool>>>(), It.IsAny<bool>(), It.IsAny<string>()))
                .ReturnsAsync(existingRoom);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _roomService.CreateRoomAsync(roomDto));
            Assert.AreEqual("Tên phòng đã tồn tại. Vui lòng chọn tên khác.", ex.Message, "Exception message should match");
        }





        //Get Room By Id
        [Test]
        public async Task GetRoomByIdAsync_RoomExists_ReturnsRoomDto()
        {
            // Arrange
            var roomId = 5;
            var room = new Room
            {
                Id = roomId,
                Name = "Room 5",
                CourseId = 1,
                Gender = Gender.Male,
                NumberOfStaff = 5,
                StudentGroups = new List<StudentGroup>
            {
                new StudentGroup { Id = 1, RoomId = roomId }
            }
            };

            var roomDto = new RoomDto
            {
                Id = roomId,
                Name = room.Name,
                Gender = room.Gender,
                NumberOfStaff = room.NumberOfStaff
            };

            _unitOfWorkMock.Setup(uow => uow.Room.GetAsync(It.IsAny<Expression<Func<Room, bool>>>(), It.IsAny<bool>(), It.IsAny<string>()))
                .ReturnsAsync(room);

            _mapperMock.Setup(m => m.Map<RoomDto>(room))
                .Returns(roomDto);

            // Act
            var result = await _roomService.GetRoomByIdAsync(roomId);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual(roomId, result.Id, "Room ID should match");
            Assert.AreEqual("Room 5", result.Name, "Room name should match");
            _unitOfWorkMock.Verify(uow => uow.Room.GetAsync(It.IsAny<Expression<Func<Room, bool>>>(), It.IsAny<bool>(), "StudentGroups,StudentGroups.StudentGroupAssignment.Student"), Times.Once, "GetRoomByIdAsync should call Room.GetAsync once");
        }

        [Test]
        public async Task GetRoomByIdAsync_RoomDoesNotExist_ThrowsArgumentException()
        {
            // Arrange
            var roomId = 1;

            _unitOfWorkMock.Setup(uow => uow.Room.GetAsync(It.IsAny<Expression<Func<Room, bool>>>(), It.IsAny<bool>(), It.IsAny<string>()))
                .ReturnsAsync((Room)null); // Room not found

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _roomService.GetRoomByIdAsync(roomId));
            Assert.AreEqual("Phòng không tồn tại.", ex.Message, "Exception message should match");
        }

        [Test]
        public async Task GetRoomByIdAsync_InvalidRoomId_ThrowsArgumentException()
        {
            // Arrange
            var roomId = 0; // Invalid roomId

            _unitOfWorkMock.Setup(uow => uow.Room.GetAsync(It.IsAny<Expression<Func<Room, bool>>>(), It.IsAny<bool>(), It.IsAny<string>()))
                .ReturnsAsync((Room)null); // Room not found for an invalid ID

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _roomService.GetRoomByIdAsync(roomId));
            Assert.AreEqual("Phòng không tồn tại.", ex.Message, "Exception message should match");
        }



        //Delete Room
        [Test]
        public async Task DeleteRoomAsync_RoomExists_DeletesRoomSuccessfully()
        {
            // Arrange
            var roomId = 5;
            var room = new Room
            {
                Id = roomId,
                Name = "Room 5",
                CourseId = 1,
                Gender = Gender.Male,
                NumberOfStaff = 5,
                StudentGroups = new List<StudentGroup>
            {
                new StudentGroup { Id = 1, RoomId = roomId }
            }
            };

            var studentGroups = new List<StudentGroup>
        {
            new StudentGroup { Id = 1, RoomId = roomId }
        };

            var nightShiftAssignments = new List<NightShiftAssignment>
        {
            new NightShiftAssignment { Id = 1, RoomId = roomId }
        };

            _unitOfWorkMock.Setup(uow => uow.Room.GetByIdAsync(roomId, "StudentGroups"))
                .ReturnsAsync(room);

            _unitOfWorkMock.Setup(uow => uow.StudentGroup.FindAsync(It.IsAny<Expression<Func<StudentGroup, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(studentGroups);

            _unitOfWorkMock.Setup(uow => uow.NightShiftAssignment.FindAsync(It.IsAny<Expression<Func<NightShiftAssignment, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(nightShiftAssignments);

            _unitOfWorkMock.Setup(uow => uow.StudentGroup.UpdateAsync(It.IsAny<StudentGroup>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.NightShiftAssignment.DeleteRangeAsync(It.IsAny<IEnumerable<NightShiftAssignment>>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.Room.DeleteAsync(It.IsAny<Room>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
                .ReturnsAsync(1);

            // Act
            await _roomService.DeleteRoomAsync(roomId);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.Room.DeleteAsync(It.IsAny<Room>()), Times.Once, "Room should be deleted");
            _unitOfWorkMock.Verify(uow => uow.StudentGroup.UpdateAsync(It.IsAny<StudentGroup>()), Times.Once, "Student group should be updated to remove room ID");
            _unitOfWorkMock.Verify(uow => uow.NightShiftAssignment.DeleteRangeAsync(It.IsAny<IEnumerable<NightShiftAssignment>>()), Times.Once, "Night shift assignments should be deleted");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once, "Save changes should be called once");
        }

        [Test]
        public async Task DeleteRoomAsync_RoomDoesNotExist_ThrowsArgumentException()
        {
            // Arrange
            var roomId = 1;

            _unitOfWorkMock.Setup(uow => uow.Room.GetByIdAsync(roomId, "StudentGroups"))
                .ReturnsAsync((Room)null); // Room not found

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _roomService.DeleteRoomAsync(roomId));
            Assert.AreEqual("Phòng không tồn tại.", ex.Message, "Exception message should match");
        }

        [Test]
        public async Task DeleteRoomAsync_InvalidRoomId_ThrowsArgumentException()
        {
            // Arrange
            var roomId = 0; // Invalid roomId

            _unitOfWorkMock.Setup(uow => uow.Room.GetByIdAsync(roomId, "StudentGroups"))
                .ReturnsAsync((Room)null); // Room not found

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _roomService.DeleteRoomAsync(roomId));
            Assert.AreEqual("Phòng không tồn tại.", ex.Message, "Exception message should match");
        }








        //Update Room
        [Test]
        public async Task UpdateRoomAsync_RoomAndCourseValid_UpdatesRoomSuccessfully()
        {
            // Arrange
            var roomId = 5;
            var roomDto = new RoomUpdateDto
            {
                Name = "abc",
                CourseId = 1,
                StudentGroupId = new[] { 1, 2 },
                Gender = Gender.Female
            };

            var room = new Room { Id = roomId, Name = "Room 5", CourseId = 1 };
            var course = new Course { Id = 1, Status = CourseStatus.recruiting };
            var studentGroups = new List<StudentGroup>
        {
            new StudentGroup { Id = 1, RoomId = roomId },
            new StudentGroup { Id = 2, RoomId = roomId }
        };

            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(roomDto.CourseId, It.IsAny<string>()))
                .ReturnsAsync(course);

            _unitOfWorkMock.Setup(uow => uow.Room.GetByIdAsync(roomId, It.IsAny<string>()))
                .ReturnsAsync(room);

            _unitOfWorkMock.Setup(uow => uow.Room.GetAsync(It.IsAny<Expression<Func<Room, bool>>>(), It.IsAny<bool>(), It.IsAny<string>()))
                .ReturnsAsync((Room)null); // No duplicate room

            _unitOfWorkMock.Setup(uow => uow.StudentGroup.FindAsync(It.IsAny<Expression<Func<StudentGroup, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(studentGroups);

            _unitOfWorkMock.Setup(uow => uow.StudentGroup.UpdateRangeAsync(It.IsAny<IEnumerable<StudentGroup>>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.Room.UpdateAsync(It.IsAny<Room>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
                .ReturnsAsync(1);

            // Act
            await _roomService.UpdateRoomAsync(roomId, roomDto);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.Room.UpdateAsync(It.IsAny<Room>()), Times.Once, "Room should be updated");
            _unitOfWorkMock.Verify(uow => uow.StudentGroup.UpdateRangeAsync(It.IsAny<IEnumerable<StudentGroup>>()), Times.Exactly(2), "Student groups should be updated twice");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once, "Save changes should be called once");
        }

        [Test]
        public async Task UpdateRoomAsync_RoomAndCourseValid2_UpdatesRoomSuccessfully()
        {
            // Arrange
            var roomId = 5;
            var roomDto = new RoomUpdateDto
            {
                Name = "abc",
                CourseId = 1,
                StudentGroupId = new[] { 1, 2 },
                Gender = Gender.Male,
            };

            var room = new Room { Id = roomId, Name = "Room 5", CourseId = 1 };
            var course = new Course { Id = 1, Status = CourseStatus.recruiting };
            var studentGroups = new List<StudentGroup>
        {
            new StudentGroup { Id = 1, RoomId = roomId },
            new StudentGroup { Id = 2, RoomId = roomId }
        };

            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(roomDto.CourseId, It.IsAny<string>()))
                .ReturnsAsync(course);

            _unitOfWorkMock.Setup(uow => uow.Room.GetByIdAsync(roomId, It.IsAny<string>()))
                .ReturnsAsync(room);

            _unitOfWorkMock.Setup(uow => uow.Room.GetAsync(It.IsAny<Expression<Func<Room, bool>>>(), It.IsAny<bool>(), It.IsAny<string>()))
                .ReturnsAsync((Room)null); // No duplicate room

            _unitOfWorkMock.Setup(uow => uow.StudentGroup.FindAsync(It.IsAny<Expression<Func<StudentGroup, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(studentGroups);

            _unitOfWorkMock.Setup(uow => uow.StudentGroup.UpdateRangeAsync(It.IsAny<IEnumerable<StudentGroup>>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.Room.UpdateAsync(It.IsAny<Room>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
                .ReturnsAsync(1);

            // Act
            await _roomService.UpdateRoomAsync(roomId, roomDto);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.Room.UpdateAsync(It.IsAny<Room>()), Times.Once, "Room should be updated");
            _unitOfWorkMock.Verify(uow => uow.StudentGroup.UpdateRangeAsync(It.IsAny<IEnumerable<StudentGroup>>()), Times.Exactly(2), "Student groups should be updated twice");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once, "Save changes should be called once");
        }

        [Test]
        public async Task UpdateRoomAsync_RoomAndCourseValid3_UpdatesRoomSuccessfully()
        {
            // Arrange
            var roomId = 5;
            var roomDto = new RoomUpdateDto
            {
                Name = "a",
                CourseId = 1,
                StudentGroupId = new[] { 1, 2 },
                Gender = Gender.Male,
            };

            var room = new Room { Id = roomId, Name = "Room 5", CourseId = 1 };
            var course = new Course { Id = 1, Status = CourseStatus.recruiting };
            var studentGroups = new List<StudentGroup>
        {
            new StudentGroup { Id = 1, RoomId = roomId },
            new StudentGroup { Id = 2, RoomId = roomId }
        };

            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(roomDto.CourseId, It.IsAny<string>()))
                .ReturnsAsync(course);

            _unitOfWorkMock.Setup(uow => uow.Room.GetByIdAsync(roomId, It.IsAny<string>()))
                .ReturnsAsync(room);

            _unitOfWorkMock.Setup(uow => uow.Room.GetAsync(It.IsAny<Expression<Func<Room, bool>>>(), It.IsAny<bool>(), It.IsAny<string>()))
                .ReturnsAsync((Room)null); // No duplicate room

            _unitOfWorkMock.Setup(uow => uow.StudentGroup.FindAsync(It.IsAny<Expression<Func<StudentGroup, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(studentGroups);

            _unitOfWorkMock.Setup(uow => uow.StudentGroup.UpdateRangeAsync(It.IsAny<IEnumerable<StudentGroup>>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.Room.UpdateAsync(It.IsAny<Room>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
                .ReturnsAsync(1);

            // Act
            await _roomService.UpdateRoomAsync(roomId, roomDto);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.Room.UpdateAsync(It.IsAny<Room>()), Times.Once, "Room should be updated");
            _unitOfWorkMock.Verify(uow => uow.StudentGroup.UpdateRangeAsync(It.IsAny<IEnumerable<StudentGroup>>()), Times.Exactly(2), "Student groups should be updated twice");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once, "Save changes should be called once");
        }

        [Test]
        public async Task UpdateRoomAsync_RoomAndCourseValid4_UpdatesRoomSuccessfully()
        {
            // Arrange
            var roomId = 1;
            var roomDto = new RoomUpdateDto
            {
                Name = "abc",
                CourseId = 1,
                StudentGroupId = new[] { 1, 2 },
                Gender = Gender.Female
            };

            var room = new Room { Id = roomId, Name = "Room 5", CourseId = 1 };
            var course = new Course { Id = 1, Status = CourseStatus.recruiting };
            var studentGroups = new List<StudentGroup>
        {
            new StudentGroup { Id = 1, RoomId = roomId },
            new StudentGroup { Id = 2, RoomId = roomId }
        };

            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(roomDto.CourseId, It.IsAny<string>()))
                .ReturnsAsync(course);

            _unitOfWorkMock.Setup(uow => uow.Room.GetByIdAsync(roomId, It.IsAny<string>()))
                .ReturnsAsync(room);

            _unitOfWorkMock.Setup(uow => uow.Room.GetAsync(It.IsAny<Expression<Func<Room, bool>>>(), It.IsAny<bool>(), It.IsAny<string>()))
                .ReturnsAsync((Room)null); // No duplicate room

            _unitOfWorkMock.Setup(uow => uow.StudentGroup.FindAsync(It.IsAny<Expression<Func<StudentGroup, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(studentGroups);

            _unitOfWorkMock.Setup(uow => uow.StudentGroup.UpdateRangeAsync(It.IsAny<IEnumerable<StudentGroup>>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.Room.UpdateAsync(It.IsAny<Room>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
                .ReturnsAsync(1);

            // Act
            await _roomService.UpdateRoomAsync(roomId, roomDto);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.Room.UpdateAsync(It.IsAny<Room>()), Times.Once, "Room should be updated");
            _unitOfWorkMock.Verify(uow => uow.StudentGroup.UpdateRangeAsync(It.IsAny<IEnumerable<StudentGroup>>()), Times.Exactly(2), "Student groups should be updated twice");
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once, "Save changes should be called once");
        }



        [Test]
        public async Task UpdateRoomAsync_CourseIsClosed_ThrowsInvalidOperationException()
        {
            // Arrange
            var roomId = 5;
            var roomDto = new RoomUpdateDto
            {
                Name = "Room 5 Updated",
                CourseId = 1,
                StudentGroupId = new[] { 1, 2 }
            };

            var course = new Course { Id = 1, Status = CourseStatus.closed }; // Course closed
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(roomDto.CourseId, It.IsAny<string>()))
                .ReturnsAsync(course);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _roomService.UpdateRoomAsync(roomId, roomDto));
            Assert.AreEqual("Không thể sửa thông tin khóa tu đã kết thúc.", ex.Message, "Exception message should match");
        }

        [Test]
        public async Task UpdateRoomAsync_RoomDoesNotExist_ThrowsArgumentException()
        {
            // Arrange
            var roomId = 5;
            var roomDto = new RoomUpdateDto
            {
                Name = "Room 5 Updated",
                CourseId = 1
            };

            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(roomDto.CourseId, It.IsAny<string>()))
                .ReturnsAsync(new Course { Id = 1, Status = CourseStatus.recruiting });

            _unitOfWorkMock.Setup(uow => uow.Room.GetByIdAsync(roomId, It.IsAny<string>()))
                .ReturnsAsync((Room)null); // Room does not exist

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _roomService.UpdateRoomAsync(roomId, roomDto));
            Assert.AreEqual("Phòng không tồn tại.", ex.Message, "Exception message should match");
        }

        [Test]
        public async Task UpdateRoomAsync_RoomNameDuplicate_ThrowsInvalidOperationException()
        {
            // Arrange
            var roomId = 5;
            var roomDto = new RoomUpdateDto
            {
                Name = "Room 5 Updated",
                CourseId = 1,
                StudentGroupId = new[] { 1, 2 }
            };

            var room = new Room { Id = roomId, Name = "Room 5", CourseId = 1 };
            var course = new Course { Id = 1, Status = CourseStatus.recruiting };

            var duplicateRoom = new Room { Id = 6, Name = "Room 5 Updated", CourseId = 1 }; // Duplicate room name

            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(roomDto.CourseId, It.IsAny<string>()))
                .ReturnsAsync(course);

            _unitOfWorkMock.Setup(uow => uow.Room.GetByIdAsync(roomId, It.IsAny<string>()))
                .ReturnsAsync(room);

            _unitOfWorkMock.Setup(uow => uow.Room.GetAsync(It.IsAny<Expression<Func<Room, bool>>>(), It.IsAny<bool>(), It.IsAny<string>()))
                .ReturnsAsync(duplicateRoom); // Duplicate room found

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _roomService.UpdateRoomAsync(roomId, roomDto));
            Assert.AreEqual("Tên phòng đã tồn tại. Vui lòng chọn tên khác.", ex.Message, "Exception message should match");
        }

    }

}
