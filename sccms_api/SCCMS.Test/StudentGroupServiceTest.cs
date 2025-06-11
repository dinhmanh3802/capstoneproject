//using AutoMapper;
//using Moq;
//using SCCMS.Domain.DTOs.StudentGroupDtos;
//using SCCMS.Domain.Services.Implements;
//using SCCMS.Domain.Services.Interfaces;
//using SCCMS.Infrastucture.Entities;
//using SCCMS.Infrastucture.Repository.Interfaces;
//using SCCMS.Infrastucture.UnitOfWork;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using System.Threading.Tasks;
//using Utility;

//namespace SCCMS.Test
//{
//    [TestFixture]
//    public class StudentGroupServiceTest
//    {
//        private Mock<IUnitOfWork> _unitOfWorkMock;
//        private Mock<IMapper> _mapperMock;
//        private StudentGroupService _service;
//        [SetUp]
//        public void SetUp()
//        {
//            _unitOfWorkMock = new Mock<IUnitOfWork>();
//            _mapperMock = new Mock<IMapper>();
//            // _service = new StudentGroupService(_unitOfWorkMock.Object, _mapperMock.Object);
//        }
//        [Test]
//        public async Task CreateStudentGroupAsync_ShouldCreateStudentGroup_WhenDataIsValid()
//        {
//            // Arrange
//            var studentGroupCreateDto = new StudentGroupCreateDto
//            {
//                CourseId = 1,
//                GroupName = "Test Group",
//                Gender = Gender.Male,
//                SupervisorIds = new List<int> { 1, 2 }
//            };
//            var course = new Course { Id = 1, CourseName = "Test Course" };
//            var studentGroup = new StudentGroup { Id = 1, GroupName = "Test Group" };

//            _unitOfWorkMock.Setup(u => u.Course.GetByIdAsync(It.IsAny<int>(),null)).ReturnsAsync(course);
//            _unitOfWorkMock.Setup(u => u.User.GetByIdAsync(It.IsAny<int>(),null)).ReturnsAsync(new User { Id = 1 });
//            _mapperMock.Setup(m => m.Map<StudentGroup>(It.IsAny<StudentGroupCreateDto>())).Returns(studentGroup);
//            _unitOfWorkMock.Setup(u => u.StudentGroup.AddAsync(It.IsAny<StudentGroup>())).Returns(Task.CompletedTask);
//            _unitOfWorkMock.Setup(u => u.SupervisorStudentGroup.AddAsync(It.IsAny<SupervisorStudentGroup>()))
//              .Returns(Task.CompletedTask);
//            _unitOfWorkMock.Setup(u => u.SaveChangeAsync()).ReturnsAsync(1);

//            // Act
//            await _service.CreateStudentGroupAsync(studentGroupCreateDto);

//            // Assert
//            _unitOfWorkMock.Verify(u => u.StudentGroup.AddAsync(It.IsAny<StudentGroup>()), Times.Once);
//            _unitOfWorkMock.Verify(u => u.SaveChangeAsync(), Times.Exactly(2)); 
//        }
//        [Test]
//        public async Task CreateStudentGroupAsync_InvalidCourseId_ShouldThrowArgumentException()
//        {
//            // Arrange
//            var studentGroupCreateDto = new StudentGroupCreateDto
//            {
//                CourseId = -1, 
//                GroupName = "Trí",
//                SupervisorIds = new List<int> { 1 }
//            };

//            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(It.IsAny<int>(),null)).ReturnsAsync((Course)null);

//            // Act & Assert
//            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
//                await _service.CreateStudentGroupAsync(studentGroupCreateDto));

//            Assert.AreEqual("CourseId không tham chiếu đến khóa tu hợp lệ.", exception.Message);
//        }
//        [Test]
//        public async Task CreateStudentGroupAsync_ShouldThrowArgumentException_WhenCourseNotFound()
//        {
//            // Arrange
//            var studentGroupCreateDto = new StudentGroupCreateDto
//            {
//                CourseId = 99,
//                GroupName = "Test Group",
//                Gender = Gender.Male
//            };
//            _unitOfWorkMock.Setup(u => u.Course.GetByIdAsync(It.IsAny<int>(),null)).ReturnsAsync((Course?)null);

//            // Act & Assert
//            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
//                await _service.CreateStudentGroupAsync(studentGroupCreateDto));
//            Assert.That(ex.Message, Is.EqualTo("CourseId không tham chiếu đến khóa tu hợp lệ."));
//        }
//        [Test]
//        public async Task CreateStudentGroupAsync_EmptySupervisorIds_ShouldCreateStudentGroupWithoutSupervisors()
//        {
//            // Arrange
//            var studentGroupCreateDto = new StudentGroupCreateDto
//            {
//                CourseId = 1,
//                GroupName = "Trí",
//                SupervisorIds = new List<int>()  
//            };

//            var course = new Course { Id = 1 };

//            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(studentGroupCreateDto.CourseId, null))
//                .ReturnsAsync(course); 

//            _unitOfWorkMock.Setup(uow => uow.StudentGroup.AddAsync(It.IsAny<StudentGroup>()))
//                .Returns(Task.CompletedTask); 

//            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
//                .ReturnsAsync(1);  

//            // Act
//            await _service.CreateStudentGroupAsync(studentGroupCreateDto);

//            // Assert
//            _unitOfWorkMock.Verify(uow => uow.StudentGroup.AddAsync(It.IsAny<StudentGroup>()), Times.Once);
//            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once);
//            _unitOfWorkMock.Verify(uow => uow.SupervisorStudentGroup.AddAsync(It.IsAny<SupervisorStudentGroup>()), Times.Never);
//        }
//        [Test]
//        public async Task CreateStudentGroupAsync_ShouldThrowArgumentException_WhenGroupNameIsEmpty()
//        {
//            // Arrange
//            var studentGroupCreateDto = new StudentGroupCreateDto
//            {
//                CourseId = 1,
//                GroupName = "", 
//                Gender = Gender.Male,
//                SupervisorIds = new List<int> { 1, 2 }
//            };

//            var course = new Course { Id = 1, CourseName = "Test Course" };

//            _unitOfWorkMock.Setup(u => u.Course.GetByIdAsync(It.IsAny<int>(), null)).ReturnsAsync(course);

//            // Act & Assert
//            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
//                await _service.CreateStudentGroupAsync(studentGroupCreateDto));
//            Assert.That(ex.Message, Is.EqualTo("Tên chánh không được để trống."));
//        }
//        [Test]
//        public async Task CreateStudentGroupAsync_ShouldThrowArgumentException_WhenGroupNameIsTooLong()
//        {
//            // Arrange
//            var studentGroupCreateDto = new StudentGroupCreateDto
//            {
//                CourseId = 1,
//                GroupName = new string('A', 101),
//                Gender = Gender.Male,
//                SupervisorIds = new List<int> { 1, 2 }
//            };

//            var course = new Course { Id = 1, CourseName = "Test Course" };

//            _unitOfWorkMock.Setup(u => u.Course.GetByIdAsync(It.IsAny<int>(), null)).ReturnsAsync(course);

//            _unitOfWorkMock.Setup(u => u.User.GetByIdAsync(It.IsAny<int>(), null))
//                           .ReturnsAsync(new User { Id = 1 });

//            _mapperMock.Setup(m => m.Map<StudentGroup>(It.IsAny<StudentGroupCreateDto>()))
//                       .Returns(new StudentGroup { Id = 1, GroupName = "Test Group" });

//            _unitOfWorkMock.Setup(u => u.StudentGroup.AddAsync(It.IsAny<StudentGroup>())).Returns(Task.CompletedTask);
//            _unitOfWorkMock.Setup(u => u.SaveChangeAsync()).ReturnsAsync(1);

//            // Act & Assert
//            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
//                await _service.CreateStudentGroupAsync(studentGroupCreateDto));

//            Assert.That(ex.Message, Is.EqualTo("Tên chánh không được vượt quá 100 ký tự."));

//            _unitOfWorkMock.Verify(u => u.StudentGroup.AddAsync(It.IsAny<StudentGroup>()), Times.Never);
//            _unitOfWorkMock.Verify(u => u.SupervisorStudentGroup.AddAsync(It.IsAny<SupervisorStudentGroup>()), Times.Never);

//            _unitOfWorkMock.Verify(u => u.SaveChangeAsync(), Times.Never);
//        }
        
//        [Test]
//        public async Task CreateStudentGroupAsync_ShouldThrowArgumentException_WhenSupervisorNotFound()
//        {
//            // Arrange
//            var studentGroupCreateDto = new StudentGroupCreateDto
//            {
//                CourseId = 1,
//                GroupName = "Test Group",
//                Gender = Gender.Male,
//                SupervisorIds = new List<int> { 1, 2 } 
//            };

//            var course = new Course { Id = 1, CourseName = "Test Course" };

//            _unitOfWorkMock.Setup(u => u.Course.GetByIdAsync(It.IsAny<int>(), null)).ReturnsAsync(course);
//            _unitOfWorkMock.Setup(u => u.User.GetByIdAsync(1, null)).ReturnsAsync(new User { Id = 1 });
//            _unitOfWorkMock.Setup(u => u.User.GetByIdAsync(2, null)).ReturnsAsync((User)null); 

//            // Act & Assert
//            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
//                await _service.CreateStudentGroupAsync(studentGroupCreateDto));
//            Assert.That(ex.Message, Is.EqualTo("Không tìm thấy huynh trưởng nào"));
//        }
//        [Test]
//        public async Task CreateStudentGroupAsync_ShouldThrowArgumentException_WhenSupervisorIdIsInvalid()
//        {
//            // Arrange
//            var studentGroupCreateDto = new StudentGroupCreateDto
//            {
//                CourseId = 1,
//                GroupName = "Test Group",
//                Gender = Gender.Male,
//                SupervisorIds = new List<int> { -1 } // Invalid supervisor ID
//            };

//            var course = new Course { Id = 1, CourseName = "Test Course" };

//            _unitOfWorkMock.Setup(u => u.Course.GetByIdAsync(It.IsAny<int>(), null)).ReturnsAsync(course);

//            _unitOfWorkMock.Setup(u => u.User.GetByIdAsync(It.IsAny<int>(), null))
//                           .ReturnsAsync(new User { Id = 1 });

//            _mapperMock.Setup(m => m.Map<StudentGroup>(It.IsAny<StudentGroupCreateDto>()))
//                       .Returns(new StudentGroup { Id = 1, GroupName = "Test Group" });

//            _unitOfWorkMock.Setup(u => u.StudentGroup.AddAsync(It.IsAny<StudentGroup>())).Returns(Task.CompletedTask);
//            _unitOfWorkMock.Setup(u => u.SupervisorStudentGroup.AddAsync(It.IsAny<SupervisorStudentGroup>()))
//                           .Returns(Task.CompletedTask);
//            _unitOfWorkMock.Setup(u => u.SaveChangeAsync()).ReturnsAsync(1);

//            // Act & Assert
//            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
//                await _service.CreateStudentGroupAsync(studentGroupCreateDto));

//            Assert.That(ex.Message, Is.EqualTo("SupervisorId không hợp lệ, phải lớn hơn 0."));

//            _unitOfWorkMock.Verify(u => u.SupervisorStudentGroup.AddAsync(It.IsAny<SupervisorStudentGroup>()), Times.Never);

//            _unitOfWorkMock.Verify(u => u.SaveChangeAsync(), Times.Never);
//        }
//        [Test]
//        public async Task UpdateStudentGroupAsync_ShouldThrowArgumentException_WhenGroupNameIsTooLong()
//        {
//            // Arrange
//            var studentGroupId = 1;
//            var studentGroupUpdateDto = new StudentGroupUpdateDto
//            {
//                GroupName = new string('A', 101), 
//                SupervisorIds = new List<int> { 1, 2 }
//            };

//            var existingStudentGroup = new StudentGroup
//            {
//                Id = studentGroupId,
//                GroupName = "Old Group Name",
//                SupervisorStudentGroup = new List<SupervisorStudentGroup>
//        {
//            new SupervisorStudentGroup { StudentGroupId = studentGroupId, SupervisorId = 1 },
//            new SupervisorStudentGroup { StudentGroupId = studentGroupId, SupervisorId = 2 }
//        } 
//            };
//            _unitOfWorkMock.Setup(u => u.StudentGroup.GetAsync(It.IsAny<Expression<Func<StudentGroup, bool>>>(), false, "SupervisorStudentGroup"))
//                .ReturnsAsync(existingStudentGroup);

//            _mapperMock.Setup(m => m.Map(It.IsAny<StudentGroupUpdateDto>(), It.IsAny<StudentGroup>()))
//                .Callback<StudentGroupUpdateDto, StudentGroup>((src, dest) =>
//                {
//                    dest.GroupName = src.GroupName;
//                });

//            // Act & Assert
//            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
//                await _service.UpdateStudentGroupAsync(studentGroupId, studentGroupUpdateDto));

//            Assert.That(ex.Message, Is.EqualTo("Tên chánh không được vượt quá 100 ký tự."));

//            _unitOfWorkMock.Verify(u => u.StudentGroup.UpdateAsync(It.IsAny<StudentGroup>()), Times.Never);
//            _unitOfWorkMock.Verify(u => u.SaveChangeAsync(), Times.Never);
//        }
//        [Test]
//        public async Task UpdateStudentGroupAsync_ShouldThrowArgumentException_WhenCourseNotFound()
//        {
//            // Arrange
//            var studentGroupId = 1;
//            var studentGroupUpdateDto = new StudentGroupUpdateDto
//            {
//                CourseId = 99,
//                GroupName = "Valid Group Name",
//                SupervisorIds = new List<int> { 1, 2 }
//            };

//            var existingStudentGroup = new StudentGroup { Id = studentGroupId, GroupName = "Old Group Name" };

//            _unitOfWorkMock.Setup(u => u.StudentGroup.GetAsync(It.IsAny<Expression<Func<StudentGroup, bool>>>(), false, "SupervisorStudentGroup"))
//                .ReturnsAsync(existingStudentGroup);

//            _unitOfWorkMock.Setup(u => u.Course.GetByIdAsync(It.IsAny<int>(),null))
//                .ReturnsAsync((Course?)null);

//            // Act & Assert
//            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
//                await _service.UpdateStudentGroupAsync(studentGroupId, studentGroupUpdateDto));

//            Assert.That(ex.Message, Is.EqualTo("CourseId không tham chiếu đến khóa tu hợp lệ."));
//        }
        
//        [Test]
//        public async Task UpdateStudentGroupAsync_ShouldUpdateStudentGroup_WhenDataIsValid()
//        {
//            // Arrange
//            var studentGroupId = 1;
//            var studentGroupUpdateDto = new StudentGroupUpdateDto
//            {
//                GroupName = "Valid Group Name",
//                SupervisorIds = new List<int> { 1, 2 },
//                CourseId = 1 // Mock course with valid ID
//            };

//            // Mock an existing student group
//            var existingStudentGroup = new StudentGroup
//            {
//                Id = studentGroupId,
//                GroupName = "Old Group Name",
//                SupervisorStudentGroup = new List<SupervisorStudentGroup>
//        {
//            new SupervisorStudentGroup { StudentGroupId = studentGroupId, SupervisorId = 1 },
//            new SupervisorStudentGroup { StudentGroupId = studentGroupId, SupervisorId = 2 }
//        }
//            };

//            // Mock valid course
//            var course = new Course { Id = 1, CourseName = "Test Course" };

//            // Mock GetAsync to return existing student group
//            _unitOfWorkMock.Setup(u => u.StudentGroup.GetAsync(It.IsAny<Expression<Func<StudentGroup, bool>>>(), false, "SupervisorStudentGroup"))
//                .ReturnsAsync(existingStudentGroup);

//            // Mock GetByIdAsync to return valid course
//            _unitOfWorkMock.Setup(u => u.Course.GetByIdAsync(It.IsAny<int>(), null)).ReturnsAsync(course);

//            _mapperMock.Setup(m => m.Map(It.IsAny<StudentGroupUpdateDto>(), It.IsAny<StudentGroup>()))
//                .Callback<StudentGroupUpdateDto, StudentGroup>((src, dest) =>
//                {
//                    dest.GroupName = src.GroupName;
//                });

//            _unitOfWorkMock.Setup(u => u.User.GetByIdAsync(It.Is<int>(id => id == 1),null))
//                .ReturnsAsync(new User { Id = 1 });
//            _unitOfWorkMock.Setup(u => u.User.GetByIdAsync(It.Is<int>(id => id == 2),null))
//                .ReturnsAsync(new User { Id = 2 });

//            _unitOfWorkMock.Setup(u => u.SupervisorStudentGroup.AddAsync(It.IsAny<SupervisorStudentGroup>()))
//                .Returns(Task.CompletedTask);
//            _unitOfWorkMock.Setup(u => u.SupervisorStudentGroup.DeleteAsync(It.IsAny<SupervisorStudentGroup>()))
//                .Returns(Task.CompletedTask);

//            // Mock StudentGroup UpdateAsync
//            _unitOfWorkMock.Setup(u => u.StudentGroup.UpdateAsync(It.IsAny<StudentGroup>()))
//                .Returns(Task.CompletedTask);

//            // Mock SaveChangeAsync
//            _unitOfWorkMock.Setup(u => u.SaveChangeAsync()).ReturnsAsync(1);

//            // Act
//            await _service.UpdateStudentGroupAsync(studentGroupId, studentGroupUpdateDto);

//            // Assert
//            _unitOfWorkMock.Verify(u => u.StudentGroup.UpdateAsync(It.IsAny<StudentGroup>()), Times.Once);

//            _unitOfWorkMock.Verify(u => u.SaveChangeAsync(), Times.Once);

//            _unitOfWorkMock.Verify(u => u.SupervisorStudentGroup.AddAsync(It.IsAny<SupervisorStudentGroup>()), Times.Never); // No new supervisors to add
//            _unitOfWorkMock.Verify(u => u.SupervisorStudentGroup.DeleteAsync(It.IsAny<SupervisorStudentGroup>()), Times.Never); // No supervisors to delete
//        }
//        [Test]
//        public void GetAllStudentGroupByCourseIdAsync_ShouldThrowArgumentException_WhenCourseIdIsInvalid()
//        {
//            // Arrange
//            var courseId = -1;

//            // Act & Assert
//            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
//                await _service.GetAllStudentGroupByCourseIdAsync(courseId));
//            Assert.That(ex.Message, Is.EqualTo("CourseId không hợp lệ, phải lớn hơn 0."));
//        }
//        [Test]
//        public async Task GetAllStudentGroupByCourseIdAsync_ShouldReturnEmptyList_WhenNoStudentGroupExists()
//        {
//            // Arrange
//            var courseId = 999;  // Id không tồn tại
//            _unitOfWorkMock.Setup(u => u.StudentGroup.GetAllAsync(
//                It.IsAny<Expression<Func<StudentGroup, bool>>>(),
//                "Course,StudentGroupAssignment.Student,SupervisorStudentGroup.Supervisor,Report"))
//                .ReturnsAsync(new List<StudentGroup>());  // Trả về danh sách rỗng

//            // Act
//            var result = await _service.GetAllStudentGroupByCourseIdAsync(courseId);

//            // Assert
//            Assert.IsNotNull(result);  // Kết quả không null
//            Assert.IsEmpty(result);    // Danh sách rỗng
//        }
//        [Test]
//        public async Task GetAllStudentGroupByCourseIdAsync_ShouldReturnStudentGroupDtos_WhenStudentGroupExists()
//        {
//            // Arrange
//            var courseId = 1;
//            var studentGroups = new List<StudentGroup>
//    {
//        new StudentGroup { CourseId = courseId, GroupName = "Test Group" }
//    };

//            var studentGroupDtos = new List<StudentGroupDto>
//    {
//        new StudentGroupDto { GroupName = "Test Group" }
//    };

//            _unitOfWorkMock.Setup(u => u.StudentGroup.GetAllAsync(
//                It.Is<Expression<Func<StudentGroup, bool>>>(sg => sg.Compile()(new StudentGroup { CourseId = courseId })),
//                "Course,StudentGroupAssignment.Student,SupervisorStudentGroup.Supervisor,Report"))
//                .ReturnsAsync(studentGroups);

//            _mapperMock.Setup(m => m.Map<List<StudentGroupDto>>(studentGroups))
//                .Returns(studentGroupDtos);

//            // Act
//            var result = await _service.GetAllStudentGroupByCourseIdAsync(courseId);

//            // Assert
//            Assert.IsNotNull(result);  // Đảm bảo kết quả không null
//   //         Assert.AreEqual(1, result.Count);  // Đảm bảo có 1 StudentGroupDto trong danh sách
//    //        Assert.AreEqual("Test Group", result[0].GroupName);  // Đảm bảo tên của nhóm là "Test Group"
//        }
//        [Test]
//        public void GetStudentGroupByIdAsync_ShouldThrowArgumentException_WhenIdIsInvalid()
//        {
//            // Arrange
//            var id = -1; // Invalid Id

//            // Act & Assert
//            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
//                await _service.GetStudentGroupByIdAsync(id));
//            Assert.That(ex.Message, Is.EqualTo("Id không hợp lệ, phải lớn hơn 0."));
//        }

//        [Test]
//        public async Task GetStudentGroupByIdAsync_ShouldReturnNull_WhenNoStudentGroupExists()
//        {
//            // Arrange
//            var id = 999; // Id không tồn tại
//            _unitOfWorkMock.Setup(u => u.StudentGroup.GetAsync(
//                It.IsAny<Expression<Func<StudentGroup, bool>>>(),
//                It.IsAny<bool>(), "Course,StudentGroupAssignment.Student,SupervisorStudentGroup.Supervisor,Report"))
//                .ReturnsAsync((StudentGroup)null);

//            // Act
//            var result = await _service.GetStudentGroupByIdAsync(id);

//            // Assert
//            Assert.IsNull(result);
//        }

//        [Test]
//        public async Task GetStudentGroupByIdAsync_ShouldReturnStudentGroup_WhenStudentGroupExists()
//        {
//            // Arrange
//            var id = 1;
//            var studentGroup = new StudentGroup
//            {
//                Id = id,
//                GroupName = "Test Group"
//            };

//            var studentGroupDto = new StudentGroupDto
//            {
//                GroupName = "Test Group"
//            };

//            _unitOfWorkMock.Setup(u => u.StudentGroup.GetAsync(
//                It.Is<Expression<Func<StudentGroup, bool>>>(sg => sg.Compile()(new StudentGroup { Id = id })),
//                It.IsAny<bool>(), "Course,StudentGroupAssignment.Student,SupervisorStudentGroup.Supervisor,Report"))
//                .ReturnsAsync(studentGroup);

//            _mapperMock.Setup(m => m.Map<StudentGroupDto>(studentGroup))
//                .Returns(studentGroupDto);

//            // Act
//            var result = await _service.GetStudentGroupByIdAsync(id);

//            // Assert
//            Assert.IsNotNull(result, "Expected a valid StudentGroupDto but got null.");
//            Assert.AreEqual("Test Group", result.GroupName);
//        }
//    }
//}


