using NUnit.Framework;
using Moq;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SCCMS.Domain.DTOs.TeamDtos;
using SCCMS.Domain.Services.Implements;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.UnitOfWork;
using System.Linq.Expressions;
using Utility;
using SCCMS.Domain.DTOs.CourseDtos;

namespace SCCMS.Test
{
    [TestFixture]
    public class TeamServiceTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IMapper> _mapperMock;
        private TeamService _teamService;

        [SetUp]
        public void SetUp()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();

            // Khởi tạo service
            _teamService = new TeamService(
                _unitOfWorkMock.Object,
                _mapperMock.Object
            );
        }

        [Test]
        public async Task CreateTeamAsync_ShouldThrowException_WhenCourseDoesNotExist()
        {
            var teamDto = new TeamCreateDto { CourseId = -1, TeamName = "Ban tri khách" };
            _unitOfWorkMock.Setup(u => u.Course.GetByIdAsync(It.IsAny<int>(), null))
                .ReturnsAsync((Course)null);

            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _teamService.CreateTeamAsync(teamDto)
            );

            Assert.That(exception.Message, Is.EqualTo("CourseId không tham chiếu đến khóa học hợp lệ."));
        }

        [Test]
        public async Task CreateTeamAsync_ShouldThrowException_WhenCourseDoesNotExist_CourseIdIs999()
        {
            var teamDto = new TeamCreateDto { CourseId = 999, TeamName = "Ban tri khách" };
            _unitOfWorkMock.Setup(u => u.Course.GetByIdAsync(It.IsAny<int>(), null))
                .ReturnsAsync((Course)null);

            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _teamService.CreateTeamAsync(teamDto)
            );

            Assert.That(exception.Message, Is.EqualTo("CourseId không tham chiếu đến khóa học hợp lệ."));
        }

        [Test]
        public async Task CreateTeamAsync_ShouldThrowException_WhenTeamNameIsTooLong()
        {
            var teamDto = new TeamCreateDto { CourseId = 1, TeamName = new string('h', 101) };

            _unitOfWorkMock.Setup(u => u.Course.GetByIdAsync(It.IsAny<int>(), null))
                .ReturnsAsync(new Course());

            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _teamService.CreateTeamAsync(teamDto)
            );

            Assert.That(exception.Message, Is.EqualTo("Tên ban không được vượt quá 100 ký tự."));
        }

        [Test]
        public async Task CreateTeamAsync_ShouldThrowException_WhenLeaderDoesNotExist()
        {
            var teamDto = new TeamCreateDto { CourseId = 1, TeamName = "Ban tri khách", LeaderId = 999 };

            _unitOfWorkMock.Setup(u => u.Course.GetByIdAsync(It.IsAny<int>(), null))
                .ReturnsAsync(new Course());

            _unitOfWorkMock.Setup(u => u.User.GetByIdAsync(It.IsAny<int>(), null))
                .ReturnsAsync((User)null);

            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _teamService.CreateTeamAsync(teamDto)
            );

            Assert.That(exception.Message, Is.EqualTo("Không tìm thấy trưởng ban."));
        }
        [Test]
        public async Task CreateTeamAsync_ShouldThrowException_WhenLeaderDoesNotExist_LeaderIdSmallerThan1()
        {
            var teamDto = new TeamCreateDto { CourseId = 1, TeamName = "Ban tri khách", LeaderId = -1 };

            _unitOfWorkMock.Setup(u => u.Course.GetByIdAsync(It.IsAny<int>(), null))
                .ReturnsAsync(new Course());

            _unitOfWorkMock.Setup(u => u.User.GetByIdAsync(It.IsAny<int>(), null))
                .ReturnsAsync((User)null);

            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _teamService.CreateTeamAsync(teamDto)
            );

            Assert.That(exception.Message, Is.EqualTo("Không tìm thấy trưởng ban."));
        }
        [Test]
        public async Task CreateTeamAsync_ShouldThrowException_WhenLeaderGenderIsNotMatching()
        {
            // Arrange
            var teamDto = new TeamCreateDto
            {
                CourseId = 1,
                TeamName = "Valid Team",
                LeaderId = 1,
                Gender = Utility.Gender.Male
            };

            // Mock Course tồn tại
            _unitOfWorkMock.Setup(u => u.Course.GetByIdAsync(It.IsAny<int>(), null))
                .ReturnsAsync(new Course { Id = 1 });

            // Mock Leader (User) tồn tại nhưng không khớp giới tính
            _unitOfWorkMock.Setup(u => u.User.GetByIdAsync(It.IsAny<int>(), null))
                .ReturnsAsync(new User { Id = 1, Gender = Utility.Gender.Female });

            // Mock danh sách các team trong khóa học
            _unitOfWorkMock.Setup(u => u.Team.GetAllAsync(It.IsAny<Expression<Func<Team, bool>>>(), null))
    .ReturnsAsync(new List<Team>());

            // Act
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _teamService.CreateTeamAsync(teamDto)
            );

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Giới tính của trưởng ban không phù hợp với giới tính yêu cầu của ban."));
        }
        [Test]
        public async Task CreateTeamAsync_ShouldThrowException_WhenTeamNameContainsSpecialCharacters()
        {
            // Arrange
            var teamDto = new TeamCreateDto
            {
                CourseId = 1,
                TeamName = "@@"
            };

            // Mock Course tồn tại
            _unitOfWorkMock.Setup(u => u.Course.GetByIdAsync(It.IsAny<int>(), null))
                .ReturnsAsync(new Course { Id = 1 });

            // Mock Leader (User) tồn tại
            _unitOfWorkMock.Setup(u => u.User.GetByIdAsync(It.IsAny<int>(), null))
                .ReturnsAsync(new User { Id = 1, Gender = Utility.Gender.Male });

            // Mock Team.GetAsync trả về null (không có team trùng tên)
            _unitOfWorkMock.Setup(u => u.Team.GetAsync(It.IsAny<Expression<Func<Team, bool>>>(), true, null))
                .ReturnsAsync((Team)null);

            // Mock Team.GetAllAsync trả về danh sách rỗng (không có team nào tồn tại)
            _unitOfWorkMock.Setup(u => u.Team.GetAllAsync(It.IsAny<Expression<Func<Team, bool>>>(), null))
                .ReturnsAsync(new List<Team>());

            // Act
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _teamService.CreateTeamAsync(teamDto)
            );

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Tên ban không được chứa ký tự đặc biệt. Vui lòng chọn tên khác."));
        }

        [Test]
        public async Task CreateTeamAsync_ShouldThrowException_WhenTeamNameAlreadyExists()
        {
            // Arrange
            var teamDto = new TeamCreateDto
            {
                CourseId = 1,
                TeamName = "Ban môi trường"
            };

            // Mock Course tồn tại
            _unitOfWorkMock.Setup(u => u.Course.GetByIdAsync(It.IsAny<int>(), null))
                .ReturnsAsync(new Course { Id = 1 });

            // Mock Leader (User) tồn tại
            _unitOfWorkMock.Setup(u => u.User.GetByIdAsync(It.IsAny<int>(), null))
                .ReturnsAsync(new User { Id = 1, Gender = Utility.Gender.Male });

            // Mock Team.GetAsync trả về team trùng tên
            _unitOfWorkMock.Setup(u => u.Team.GetAsync(It.IsAny<Expression<Func<Team, bool>>>(), true, null))
                .ReturnsAsync(new Team { TeamName = "Ban môi trường" });

            // Act
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _teamService.CreateTeamAsync(teamDto)
            );

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Tên ban đã tồn tại trong khóa học này. Vui lòng chọn tên khác."));
        }

        [Test]
        public async Task CreateTeamAsync_ShouldCreateTeam_WhenAllDataIsValid()
        {
            var teamDto = new TeamCreateDto
            {
                CourseId = 2,
                TeamName = "Ban tri khách",
                LeaderId = 2,
                Gender = Utility.Gender.Male,
                ExpectedVolunteers=2
            };

            var course = new Course { Id = 2 };
            var leader = new User { Id = 2, Gender = Utility.Gender.Male };

            _unitOfWorkMock.Setup(u => u.Course.GetByIdAsync(It.IsAny<int>(), null))
                .ReturnsAsync(course);

            _unitOfWorkMock.Setup(u => u.User.GetByIdAsync(It.IsAny<int>(), null))
                .ReturnsAsync(leader);

            _unitOfWorkMock.Setup(u => u.Team.GetAllAsync(It.IsAny<Expression<Func<Team, bool>>>(), null))
                .ReturnsAsync(new List<Team>());

            _unitOfWorkMock.Setup(u => u.Team.GetAsync(It.IsAny<Expression<Func<Team, bool>>>(), true, null))
                .ReturnsAsync((Team)null);

            _mapperMock.Setup(m => m.Map<Team>(It.IsAny<TeamCreateDto>()))
                .Returns(new Team { Id = 1 });

            _unitOfWorkMock.Setup(u => u.Team.AddAsync(It.IsAny<Team>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.SaveChangeAsync())
                .ReturnsAsync(1);

            await _teamService.CreateTeamAsync(teamDto);

            _unitOfWorkMock.Verify(u => u.Team.AddAsync(It.IsAny<Team>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangeAsync(), Times.Once);
        }
        [Test]
        public async Task CreateTeamAsync_ShouldCreateTeam_WhenCourseIdEqual1()
        {
            var teamDto = new TeamCreateDto
            {
                CourseId = 1,
                TeamName = "Ban tri khách",
                LeaderId = 2,
                Gender = Utility.Gender.Male,
                ExpectedVolunteers = 2
            };

            var course = new Course { Id = 1 };
            var leader = new User { Id = 1, Gender = Utility.Gender.Male };

            _unitOfWorkMock.Setup(u => u.Course.GetByIdAsync(It.IsAny<int>(), null))
                .ReturnsAsync(course);

            _unitOfWorkMock.Setup(u => u.User.GetByIdAsync(It.IsAny<int>(), null))
                .ReturnsAsync(leader);

            _unitOfWorkMock.Setup(u => u.Team.GetAllAsync(It.IsAny<Expression<Func<Team, bool>>>(), null))
                .ReturnsAsync(new List<Team>());

            _unitOfWorkMock.Setup(u => u.Team.GetAsync(It.IsAny<Expression<Func<Team, bool>>>(), true, null))
                .ReturnsAsync((Team)null);

            _mapperMock.Setup(m => m.Map<Team>(It.IsAny<TeamCreateDto>()))
                .Returns(new Team { Id = 1 });

            _unitOfWorkMock.Setup(u => u.Team.AddAsync(It.IsAny<Team>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.SaveChangeAsync())
                .ReturnsAsync(1);

            await _teamService.CreateTeamAsync(teamDto);

            _unitOfWorkMock.Verify(u => u.Team.AddAsync(It.IsAny<Team>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangeAsync(), Times.Once);
        }
        [Test]
        public async Task CreateTeamAsync_ShouldCreateTeam_WhenAllLeaderIdEqual1()
        {
            var teamDto = new TeamCreateDto
            {
                CourseId = 2,
                TeamName = "Ban tri khách",
                LeaderId = 1,
                Gender = Utility.Gender.Male,
                ExpectedVolunteers = 2
            };

            var course = new Course { Id = 1 };
            var leader = new User { Id = 1, Gender = Utility.Gender.Male };

            _unitOfWorkMock.Setup(u => u.Course.GetByIdAsync(It.IsAny<int>(), null))
                .ReturnsAsync(course);

            _unitOfWorkMock.Setup(u => u.User.GetByIdAsync(It.IsAny<int>(), null))
                .ReturnsAsync(leader);

            _unitOfWorkMock.Setup(u => u.Team.GetAllAsync(It.IsAny<Expression<Func<Team, bool>>>(), null))
                .ReturnsAsync(new List<Team>());

            _unitOfWorkMock.Setup(u => u.Team.GetAsync(It.IsAny<Expression<Func<Team, bool>>>(), true, null))
                .ReturnsAsync((Team)null);

            _mapperMock.Setup(m => m.Map<Team>(It.IsAny<TeamCreateDto>()))
                .Returns(new Team { Id = 1 });

            _unitOfWorkMock.Setup(u => u.Team.AddAsync(It.IsAny<Team>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.SaveChangeAsync())
                .ReturnsAsync(1);

            await _teamService.CreateTeamAsync(teamDto);

            _unitOfWorkMock.Verify(u => u.Team.AddAsync(It.IsAny<Team>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangeAsync(), Times.Once);
        }
        [Test]
        public async Task CreateTeamAsync_ShouldThrowException_WhenLeaderIsAlreadyInAnotherTeam()
        {
            var teamDto = new TeamCreateDto { CourseId = 1, TeamName = "Ban tri khách", LeaderId = 1 };

            _unitOfWorkMock.Setup(u => u.Course.GetByIdAsync(It.IsAny<int>(),null))
                .ReturnsAsync(new Course());

            _unitOfWorkMock.Setup(u => u.User.GetByIdAsync(It.IsAny<int>(),null))
                .ReturnsAsync(new User());

            _unitOfWorkMock.Setup(u => u.Team.GetAllAsync(It.IsAny<Expression<Func<Team, bool>>>(), null))
     .ReturnsAsync(new List<Team> { new Team { LeaderId = 1 } });


            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _teamService.CreateTeamAsync(teamDto)
            );

            Assert.That(exception.Message, Is.EqualTo("Trưởng ban đã làm một ban khác. Vui lòng chọn trưởng ban khác!"));
        }

        [Test]
        public async Task CreateTeamAsync_ShouldThrowException_WhenExpectedVolunteersIsZero()
        {
            // Arrange
            var teamDto = new TeamCreateDto
            {
                CourseId = 1,
                TeamName = "Ban tri khách",
                LeaderId = 1,
                ExpectedVolunteers = 0 // Số lượng dự kiến là 0
            };

            // Mock Course tồn tại
            _unitOfWorkMock.Setup(u => u.Course.GetByIdAsync(It.IsAny<int>(),null))
                .ReturnsAsync(new Course { Id = 1 });

            // Mock Leader (User) tồn tại
            _unitOfWorkMock.Setup(u => u.User.GetByIdAsync(It.IsAny<int>(),null))
                .ReturnsAsync(new User { Id = 1, Gender = Utility.Gender.Male });

            // Mock không có đội nào trùng tên trong khóa học
            _unitOfWorkMock.Setup(u => u.Team.GetAsync(It.IsAny<Expression<Func<Team, bool>>>(),  true, null))
                .ReturnsAsync((Team)null);

            // Mock danh sách các team trong khóa học trả về danh sách rỗng
            _unitOfWorkMock.Setup(u => u.Team.GetAllAsync(It.IsAny<Expression<Func<Team, bool>>>(), null))
                .ReturnsAsync(new List<Team>());

            // Act
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _teamService.CreateTeamAsync(teamDto)
            );

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Số lượng tình nguyện viên dự kiến phải lớn hơn 0."));
        }
        [Test]
        public async Task UpdateTeamAsync_ShouldThrowException_WhenTeamDoesNotExist()
        {
            // Arrange
            var teamUpdateDto = new TeamUpdateDto { CourseId = 1, TeamName = "Ban tri khách" };
            _unitOfWorkMock.Setup(u => u.Team.GetAsync(It.IsAny<Expression<Func<Team, bool>>>(), true, null))
                .ReturnsAsync((Team)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _teamService.UpdateTeamAsync(1, teamUpdateDto)
            );

            Assert.That(exception.Message, Is.EqualTo("Không tìm thấy ban."));
        }
        [Test]
        public async Task UpdateTeamAsync_ValidTeam_UpdatesTeamSuccessfully()
        {
            // Arrange
            var teamId = 1;
            var updatedTeamName = "Ban tri khách";
            var updatedLeaderId = 2;
            var updatedGender = Gender.Female;
            var updatedCourseId = 1;

            // Original team in database
            var existingTeam = new Team
            {
                Id = teamId,
                TeamName = "Ban hành đường",
                CourseId = updatedCourseId,
                LeaderId = 3,
                Gender = Gender.Female,
                VolunteerTeam = new List<VolunteerTeam>
        {
            new VolunteerTeam { Volunteer = new Volunteer { Gender = Gender.Female } }
        }
            };

            // TeamUpdateDto with the new values
            var teamUpdateDto = new TeamUpdateDto
            {
                CourseId = updatedCourseId,
                TeamName = updatedTeamName,
                LeaderId = updatedLeaderId,
                Gender = updatedGender
            };

            // Mock existing team data in UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.Team.GetAsync(It.IsAny<Expression<Func<Team, bool>>>(), true, "VolunteerTeam.Volunteer"))
                           .ReturnsAsync(existingTeam);

            // Mock Course data
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(updatedCourseId, It.IsAny<string>()))
                           .ReturnsAsync(new Course { Id = updatedCourseId });

            // Mock Leader data
            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync(updatedLeaderId, It.IsAny<string>()))
                           .ReturnsAsync(new User { Id = updatedLeaderId });

            _mapperMock.Setup(m => m.Map<Team>(It.Is<TeamUpdateDto>(dto => dto == teamUpdateDto)))
              .Returns(new Team
              {
                  Id = teamId,
                  TeamName = updatedTeamName,
                  LeaderId = updatedLeaderId,
                  Gender = updatedGender,
                  CourseId = updatedCourseId
              });
            // Mock saving changes to the team
            _unitOfWorkMock.Setup(uow => uow.Team.UpdateAsync(It.IsAny<Team>()))
                           .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
                           .ReturnsAsync(1);


            // Act
            await _teamService.UpdateTeamAsync(teamId, teamUpdateDto);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.Team.UpdateAsync(It.Is<Team>(t =>
                t.Id == teamId
            )), Times.Once);

            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once);
        }
        [Test]
        public async Task UpdateTeamAsync_ValidTeam_UpdatesTeamSuccessfully_teamIdEqual2()
        {
            // Arrange
            var teamId = 2;
            var updatedTeamName = "Ban tri khách";
            var updatedLeaderId = 2;
            var updatedGender = Gender.Female;
            var updatedCourseId = 1;

            // Original team in database
            var existingTeam = new Team
            {
                Id = teamId,
                TeamName = "Ban hành đường",
                CourseId = updatedCourseId,
                LeaderId = 3,
                Gender = Gender.Female,
                VolunteerTeam = new List<VolunteerTeam>
        {
            new VolunteerTeam { Volunteer = new Volunteer { Gender = Gender.Female } }
        }
            };

            // TeamUpdateDto with the new values
            var teamUpdateDto = new TeamUpdateDto
            {
                CourseId = updatedCourseId,
                TeamName = updatedTeamName,
                LeaderId = updatedLeaderId,
                Gender = updatedGender
            };

            // Mock existing team data in UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.Team.GetAsync(It.IsAny<Expression<Func<Team, bool>>>(), true, "VolunteerTeam.Volunteer"))
                           .ReturnsAsync(existingTeam);

            // Mock Course data
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(updatedCourseId, It.IsAny<string>()))
                           .ReturnsAsync(new Course { Id = updatedCourseId });

            // Mock Leader data
            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync(updatedLeaderId, It.IsAny<string>()))
                           .ReturnsAsync(new User { Id = updatedLeaderId });

            _mapperMock.Setup(m => m.Map<Team>(It.Is<TeamUpdateDto>(dto => dto == teamUpdateDto)))
              .Returns(new Team
              {
                  Id = teamId,
                  TeamName = updatedTeamName,
                  LeaderId = updatedLeaderId,
                  Gender = updatedGender,
                  CourseId = updatedCourseId
              });
            // Mock saving changes to the team
            _unitOfWorkMock.Setup(uow => uow.Team.UpdateAsync(It.IsAny<Team>()))
                           .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
                           .ReturnsAsync(1);


            // Act
            await _teamService.UpdateTeamAsync(teamId, teamUpdateDto);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.Team.UpdateAsync(It.Is<Team>(t =>
                t.Id == teamId
            )), Times.Once);

            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once);
        }
        [Test]
        public async Task UpdateTeamAsync_ValidTeam_UpdatesTeamSuccessfully_CourseEqualId2()
        {
            // Arrange
            var teamId = 2;
            var updatedTeamName = "Ban tri khách";
            var updatedLeaderId = 2;
            var updatedGender = Gender.Female;
            var updatedCourseId = 2;

            // Original team in database
            var existingTeam = new Team
            {
                Id = teamId,
                TeamName = "Ban hành đường",
                CourseId = updatedCourseId,
                LeaderId = 3,
                Gender = Gender.Female,
                VolunteerTeam = new List<VolunteerTeam>
        {
            new VolunteerTeam { Volunteer = new Volunteer { Gender = Gender.Female } }
        }
            };

            // TeamUpdateDto with the new values
            var teamUpdateDto = new TeamUpdateDto
            {
                CourseId = updatedCourseId,
                TeamName = updatedTeamName,
                LeaderId = updatedLeaderId,
                Gender = updatedGender
            };

            // Mock existing team data in UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.Team.GetAsync(It.IsAny<Expression<Func<Team, bool>>>(), true, "VolunteerTeam.Volunteer"))
                           .ReturnsAsync(existingTeam);

            // Mock Course data
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(updatedCourseId, It.IsAny<string>()))
                           .ReturnsAsync(new Course { Id = updatedCourseId });

            // Mock Leader data
            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync(updatedLeaderId, It.IsAny<string>()))
                           .ReturnsAsync(new User { Id = updatedLeaderId });

            _mapperMock.Setup(m => m.Map<Team>(It.Is<TeamUpdateDto>(dto => dto == teamUpdateDto)))
              .Returns(new Team
              {
                  Id = teamId,
                  TeamName = updatedTeamName,
                  LeaderId = updatedLeaderId,
                  Gender = updatedGender,
                  CourseId = updatedCourseId
              });
            // Mock saving changes to the team
            _unitOfWorkMock.Setup(uow => uow.Team.UpdateAsync(It.IsAny<Team>()))
                           .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
                           .ReturnsAsync(1);


            // Act
            await _teamService.UpdateTeamAsync(teamId, teamUpdateDto);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.Team.UpdateAsync(It.Is<Team>(t =>
                t.Id == teamId
            )), Times.Once);

            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once);
        }
        [Test]
        public async Task UpdateTeamAsync_ValidTeam_UpdatesTeamSuccessfully_LeaderEqualId1()
        {
            // Arrange
            var teamId = 1;
            var updatedTeamName = "Ban tri khách";
            var updatedLeaderId = 1;
            var updatedGender = Gender.Female;
            var updatedCourseId = 1;

            // Original team in database
            var existingTeam = new Team
            {
                Id = teamId,
                TeamName = "Ban hành đường",
                CourseId = updatedCourseId,
                LeaderId = 3,
                Gender = Gender.Female,
                VolunteerTeam = new List<VolunteerTeam>
        {
            new VolunteerTeam { Volunteer = new Volunteer { Gender = Gender.Female } }
        }
            };

            // TeamUpdateDto with the new values
            var teamUpdateDto = new TeamUpdateDto
            {
                CourseId = updatedCourseId,
                TeamName = updatedTeamName,
                LeaderId = updatedLeaderId,
                Gender = updatedGender
            };

            // Mock existing team data in UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.Team.GetAsync(It.IsAny<Expression<Func<Team, bool>>>(), true, "VolunteerTeam.Volunteer"))
                           .ReturnsAsync(existingTeam);

            // Mock Course data
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(updatedCourseId, It.IsAny<string>()))
                           .ReturnsAsync(new Course { Id = updatedCourseId });

            // Mock Leader data
            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync(updatedLeaderId, It.IsAny<string>()))
                           .ReturnsAsync(new User { Id = updatedLeaderId });

            _mapperMock.Setup(m => m.Map<Team>(It.Is<TeamUpdateDto>(dto => dto == teamUpdateDto)))
              .Returns(new Team
              {
                  Id = teamId,
                  TeamName = updatedTeamName,
                  LeaderId = updatedLeaderId,
                  Gender = updatedGender,
                  CourseId = updatedCourseId
              });
            // Mock saving changes to the team
            _unitOfWorkMock.Setup(uow => uow.Team.UpdateAsync(It.IsAny<Team>()))
                           .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
                           .ReturnsAsync(1);


            // Act
            await _teamService.UpdateTeamAsync(teamId, teamUpdateDto);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.Team.UpdateAsync(It.Is<Team>(t =>
                t.Id == teamId
            )), Times.Once);

            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once);
        }
        [Test]
        public async Task UpdateTeamAsync_TeamNameContainsSpecialCharacters_ThrowsArgumentException()
        {
            // Arrange
            var teamId = 1;
            var invalidTeamName = "@ @"; // Team name with special characters

            var teamUpdateDto = new TeamUpdateDto
            {
                TeamName = invalidTeamName,
                CourseId = 1,
                LeaderId = 2,
                Gender = Gender.Male
            };

            // Mock existing team data in UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.Team.GetAsync(It.IsAny<Expression<Func<Team, bool>>>(), true, "VolunteerTeam.Volunteer"))
                           .ReturnsAsync(new Team { Id = teamId, TeamName = "Old Team" });

            // Mock Course data
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(It.IsAny<int>(),null))
                           .ReturnsAsync(new Course { Id = 1 });

            // Mock Leader data
            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync(It.IsAny<int>(),null))
                           .ReturnsAsync(new User { Id = 2 });

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _teamService.UpdateTeamAsync(teamId, teamUpdateDto)
            );

            Assert.That(exception.Message, Is.EqualTo("Tên ban không được chứa ký tự đặc biệt. Vui lòng chọn tên khác."));

            // Verify that Map is called once
            _mapperMock.Verify(m => m.Map<Team>(It.Is<TeamUpdateDto>(dto => dto == teamUpdateDto)), Times.Never); // Should never be called if exception is thrown before
        }
        [Test]
        public async Task UpdateTeamAsync_TeamNameIsNullOrEmpty_ThrowsArgumentException()
        {
            // Arrange
            var teamId = 1;
            string invalidTeamName = null; // Team name is null

            var teamUpdateDto = new TeamUpdateDto
            {
                TeamName = invalidTeamName,
                CourseId = 1,
                LeaderId = 2,
                Gender = Gender.Male
            };

            // Mock existing team data in UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.Team.GetAsync(It.IsAny<Expression<Func<Team, bool>>>(), true, "VolunteerTeam.Volunteer"))
                           .ReturnsAsync(new Team { Id = teamId, TeamName = "Old Team" });

            // Mock Course data
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(It.IsAny<int>(), null))
                           .ReturnsAsync(new Course { Id = 1 });

            // Mock Leader data
            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync(It.IsAny<int>(), null))
                           .ReturnsAsync(new User { Id = 2 });

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _teamService.UpdateTeamAsync(teamId, teamUpdateDto)
            );

            Assert.That(exception.Message, Is.EqualTo("Tên ban không được để trống hoặc null. Vui lòng chọn tên khác."));

            // Verify that Map is called once
            _mapperMock.Verify(m => m.Map<Team>(It.Is<TeamUpdateDto>(dto => dto == teamUpdateDto)), Times.Never); // Should never be called if exception is thrown before
        }
        [Test]
        public async Task UpdateTeamAsync_TeamNameExceeds100Characters_ThrowsArgumentException()
        {
            // Arrange
            var teamId = 1;
            var invalidTeamName = new string('h', 101); // Tạo tên đội dài 101 ký tự

            var teamUpdateDto = new TeamUpdateDto
            {
                TeamName = invalidTeamName,
                CourseId = 1,
                LeaderId = 2,
                Gender = Gender.Male
            };

            // Mock existing team data in UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.Team.GetAsync(It.IsAny<Expression<Func<Team, bool>>>(), true, "VolunteerTeam.Volunteer"))
                           .ReturnsAsync(new Team { Id = teamId, TeamName = "Old Team" });

            // Mock Course data
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(It.IsAny<int>(), null))
                           .ReturnsAsync(new Course { Id = 1 });

            // Mock Leader data
            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync(It.IsAny<int>(), null))
                           .ReturnsAsync(new User { Id = 2 });

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _teamService.UpdateTeamAsync(teamId, teamUpdateDto)
            );

            Assert.That(exception.Message, Is.EqualTo("Tên ban không được vượt quá 100 ký tự."));

            // Verify that Map is called once
            _mapperMock.Verify(m => m.Map<Team>(It.Is<TeamUpdateDto>(dto => dto == teamUpdateDto)), Times.Never); // Should never be called if exception is thrown before
        }
        [Test]
        public async Task UpdateTeamAsync_VolunteerCountIsZero_ThrowsArgumentException()
        {
            // Arrange
            var teamId = 1;
            var teamUpdateDto = new TeamUpdateDto
            {
                TeamName = "Ban tri khách",
                CourseId = 1,
                LeaderId = 2,
                Gender = Gender.Male,
                ExpectedVolunteers = 0
            };

            // Mock existing team data in UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.Team.GetAsync(It.IsAny<Expression<Func<Team, bool>>>(), true, "VolunteerTeam.Volunteer"))
                .ReturnsAsync((Expression<Func<Team, bool>> predicate, bool tracked, string includeProperties) =>
                {
                    // Giả lập dữ liệu trả về nếu predicate là đúng
                    var team = new Team
                    {
                        Id = teamId,
                        TeamName = "Old Team",
                        VolunteerTeam = new List<VolunteerTeam>()
                    };
                    return predicate.Compile()(team) ? team : null;
                });
            // Mock Course data
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(It.IsAny<int>(), null))
                           .ReturnsAsync(new Course { Id = 1 });

            // Mock Leader data
            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync(It.IsAny<int>(), null))
                           .ReturnsAsync(new User { Id = 2 });

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _teamService.UpdateTeamAsync(teamId, teamUpdateDto)
            );

            Assert.That(exception.Message, Is.EqualTo("Số lượng tình nguyện viên trong ban phải lớn hơn 0."));
        }
        [Test]
        public async Task UpdateTeamAsync_LeaderNotFound_ThrowsArgumentException()
        {
            // Arrange
            var teamId = 1;
            var teamUpdateDto = new TeamUpdateDto
            {
                TeamName = "Ban tri khách",
                CourseId = 1,
                LeaderId = 999, // Giả sử trưởng ban với LeaderId này không tồn tại
                Gender = Gender.Male,
                ExpectedVolunteers = 5
            };

            // Mock existing team data in UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.Team.GetAsync(It.IsAny<Expression<Func<Team, bool>>>(), true, "VolunteerTeam.Volunteer"))
                .ReturnsAsync(new Team { Id = teamId, TeamName = "Old Team", VolunteerTeam = new List<VolunteerTeam>() });

            // Mock Course data
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(It.IsAny<int>(), null))
                .ReturnsAsync(new Course { Id = 1 });

            // Mock Leader data (LeaderId = 999 không tìm thấy)
            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync(999, null))
                .ReturnsAsync((User)null); // Không tìm thấy trưởng ban

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _teamService.UpdateTeamAsync(teamId, teamUpdateDto)
            );

            // Assert exception message
            Assert.That(exception.Message, Is.EqualTo("Không tìm thấy trưởng ban."));
        }
        [Test]
        public async Task UpdateTeamAsync_LeaderNotFound_IdSmallerthan0_ThrowsArgumentException()
        {
            // Arrange
            var teamId = 1;
            var teamUpdateDto = new TeamUpdateDto
            {
                TeamName = "Ban tri khách",
                CourseId = 1,
                LeaderId = -1, // Giả sử trưởng ban với LeaderId này không tồn tại
                Gender = Gender.Male,
                ExpectedVolunteers = 5
            };

            // Mock existing team data in UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.Team.GetAsync(It.IsAny<Expression<Func<Team, bool>>>(), true, "VolunteerTeam.Volunteer"))
                .ReturnsAsync(new Team { Id = teamId, TeamName = "Old Team", VolunteerTeam = new List<VolunteerTeam>() });

            // Mock Course data
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(It.IsAny<int>(), null))
                .ReturnsAsync(new Course { Id = 1 });

            // Mock Leader data (LeaderId = -1 không tìm thấy)
            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync(-1, null))
                .ReturnsAsync((User)null); // Không tìm thấy trưởng ban

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _teamService.UpdateTeamAsync(teamId, teamUpdateDto)
            );

            // Assert exception message
            Assert.That(exception.Message, Is.EqualTo("Không tìm thấy trưởng ban."));
        }
        [Test]
        public async Task UpdateTeamAsync_CourseNotFound_ThrowsArgumentException_CourseIdEqual999()
        {
            // Arrange
            var teamId = 1;
            var teamUpdateDto = new TeamUpdateDto
            {
                TeamName = "Ban tri khách",
                CourseId = 999, // Giả sử khóa học với CourseId này không tồn tại
                LeaderId = 2,
                Gender = Gender.Male,
                ExpectedVolunteers = 5
            };

            // Mock existing team data in UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.Team.GetAsync(It.IsAny<Expression<Func<Team, bool>>>(), true, "VolunteerTeam.Volunteer"))
                .ReturnsAsync(new Team { Id = teamId, TeamName = "Old Team", VolunteerTeam = new List<VolunteerTeam>() });

            // Mock Course data (CourseId = 999 không tìm thấy)
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(999, null))
                .ReturnsAsync((Course)null); // Không tìm thấy khóa học

            // Mock Leader data
            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync(It.IsAny<int>(), null))
                .ReturnsAsync(new User { Id = 2 });

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _teamService.UpdateTeamAsync(teamId, teamUpdateDto)
            );

            // Assert exception message
            Assert.That(exception.Message, Is.EqualTo("CourseId không tham chiếu đến khóa học hợp lệ."));
        }
        [Test]
        public async Task UpdateTeamAsync_CourseNotFound_ThrowsArgumentException_CourseIdSmallerThan0()
        {
            // Arrange
            var teamId = 1;
            var teamUpdateDto = new TeamUpdateDto
            {
                TeamName = "Ban tri khách",
                CourseId = -1, // Giả sử khóa học với CourseId này không tồn tại
                LeaderId = 2,
                Gender = Gender.Male,
                ExpectedVolunteers = 5
            };

            // Mock existing team data in UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.Team.GetAsync(It.IsAny<Expression<Func<Team, bool>>>(), true, "VolunteerTeam.Volunteer"))
                .ReturnsAsync(new Team { Id = teamId, TeamName = "Old Team", VolunteerTeam = new List<VolunteerTeam>() });

            // Mock Course data (CourseId = -1 không tìm thấy)
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(999, null))
                .ReturnsAsync((Course)null); // Không tìm thấy khóa học

            // Mock Leader data
            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync(It.IsAny<int>(), null))
                .ReturnsAsync(new User { Id = 2 });

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _teamService.UpdateTeamAsync(teamId, teamUpdateDto)
            );

            // Assert exception message
            Assert.That(exception.Message, Is.EqualTo("CourseId không tham chiếu đến khóa học hợp lệ."));
        }
        [Test]
        public async Task UpdateTeamAsync_GenderConflict_ThrowsArgumentException()
        {
            // Arrange
            var teamId = 1;
            var teamUpdateDto = new TeamUpdateDto
            {
                TeamName = "Ban tri khách",
                CourseId = 1,
                LeaderId = 2,
                Gender = Gender.Male, // Cập nhật giới tính của đội thành Male
                ExpectedVolunteers = 5
            };

            // Mock existing team data in UnitOfWork
            var existingTeam = new Team
            {
                Id = teamId,
                TeamName = "Old Team",
                Gender = Gender.Female, // Giới tính của đội hiện tại là Female
                VolunteerTeam = new List<VolunteerTeam>
        {
            new VolunteerTeam { Volunteer = new Volunteer { Gender = Gender.Male } }, // Tình nguyện viên này có giới tính Male
            new VolunteerTeam { Volunteer = new Volunteer { Gender = Gender.Female } }  // Tình nguyện viên này có giới tính Female
        }
            };

            // Mocking GetAsync để trả về team với tình nguyện viên
            _unitOfWorkMock.Setup(uow => uow.Team.GetAsync(It.IsAny<Expression<Func<Team, bool>>>(), true, "VolunteerTeam.Volunteer"))
                .ReturnsAsync(existingTeam);  // Đảm bảo trả về existingTeam có tình nguyện viên

            // Mock Course data
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(It.IsAny<int>(), null))
                .ReturnsAsync(new Course { Id = 1 });

            // Mock Leader data
            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync(It.IsAny<int>(), null))
                .ReturnsAsync(new User { Id = 2 });

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _teamService.UpdateTeamAsync(teamId, teamUpdateDto)
            );

            // Assert exception message
            Assert.That(exception.Message, Is.EqualTo("Tất cả học sinh đều phải có giới tính nam!"));
        }
        [Test]
        public async Task UpdateTeamAsync_TeamNameDuplicateInCourse_ThrowsArgumentException()
        {
            // Arrange
            var teamId = 1;
            var teamUpdateDto = new TeamUpdateDto
            {
                TeamName = "Ban môi trường", // Tên đội mới trùng với đội đã có
                CourseId = 1,               // ID khóa học
                LeaderId = 2,
                Gender = Gender.Male,
                ExpectedVolunteers = 5
            };

            // Giả lập dữ liệu đội hiện tại
            var existingTeam = new Team
            {
                Id = teamId,
                TeamName = "Old Team",
                CourseId = 1,  // Cùng khóa học
                VolunteerTeam = new List<VolunteerTeam>(),
                ExpectedVolunteers = 5  // Đảm bảo số lượng tình nguyện viên hợp lệ
            };

            // Giả lập dữ liệu đội trùng tên trong cùng khóa học
            var teamsInCourse = new List<Team>
    {
        new Team
        {
            Id = 2,
            TeamName = "Ban môi trường", // Tên đội trùng
            CourseId = 1,
            ExpectedVolunteers = 5 // Cùng khóa học
        }
    };

            // Mock các phương thức trong UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.Team.GetAsync(
                    It.Is<Expression<Func<Team, bool>>>(expr => expr.Compile().Invoke(new Team { Id = teamId })),  // Mock chính xác predicate
                    true,
                    "VolunteerTeam.Volunteer"))
                .ReturnsAsync(existingTeam);  // Trả về đội hiện tại khi tìm theo teamId

            // Mock kiểm tra danh sách đội trong khóa học
            _unitOfWorkMock.Setup(uow => uow.Team.GetAsync(It.IsAny<Expression<Func<Team, bool>>>(), true,null))
                .ReturnsAsync(teamsInCourse.FirstOrDefault(t => t.TeamName == "Ban môi trường" && t.CourseId == 1 && t.Id != teamId));  // Trả về đội trùng tên nếu có

            // Mock khóa học hợp lệ
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(It.IsAny<int>(), null))
                .ReturnsAsync(new Course { Id = 1 });

            // Mock trưởng ban hợp lệ
            _unitOfWorkMock.Setup(uow => uow.User.GetByIdAsync(It.IsAny<int>(), null))
                .ReturnsAsync(new User { Id = 2 });

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _teamService.UpdateTeamAsync(teamId, teamUpdateDto)
            );

            // Assert exception message
            Assert.That(exception.Message, Is.EqualTo("Tên ban đã tồn tại trong khóa học này. Vui lòng chọn tên khác."));
        }
        [Test]
        public async Task DeleteTeamAsync_ValidId_DeletesTeam()
        {
            // Arrange
            int validId = 1;

            var team = new Team
            {
                Id = validId,
                TeamName = "Team A",
                CourseId = 1,
                ExpectedVolunteers = 5,
                VolunteerTeam = new List<VolunteerTeam>()
            };

            // Giả lập tìm thấy đội với Id hợp lệ
            _unitOfWorkMock.Setup(uow => uow.Team.GetByIdAsync(validId,null))
                .ReturnsAsync(team);

            _unitOfWorkMock.Setup(uow => uow.Team.DeleteAsync(team))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
                .ReturnsAsync(1);

            // Act
            await _teamService.DeleteTeamAsync(validId);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.Team.DeleteAsync(It.Is<Team>(t => t.Id == validId)), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once);
        }
        [Test]
        public async Task DeleteTeamAsync_ValidId_DeletesTeam_TeamIdEqual2()
        {
            // Arrange
            int validId = 2;

            var team = new Team
            {
                Id = validId,
                TeamName = "Team A",
                CourseId = 1,
                ExpectedVolunteers = 5,
                VolunteerTeam = new List<VolunteerTeam>()
            };

            // Giả lập tìm thấy đội với Id hợp lệ
            _unitOfWorkMock.Setup(uow => uow.Team.GetByIdAsync(validId, null))
                .ReturnsAsync(team);

            _unitOfWorkMock.Setup(uow => uow.Team.DeleteAsync(team))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
                .ReturnsAsync(1);

            // Act
            await _teamService.DeleteTeamAsync(validId);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.Team.DeleteAsync(It.Is<Team>(t => t.Id == validId)), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once);
        }
        [Test]
        public async Task DeleteTeamAsync_InvalidId_ThrowsArgumentException()
        {
            // Arrange
            int invalidId = -1;

            // Giả lập không tìm thấy đội với Id không hợp lệ
            _unitOfWorkMock.Setup(uow => uow.Team.GetByIdAsync(invalidId,null))
                .ReturnsAsync((Team)null); // Trả về null khi không tìm thấy đội

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _teamService.DeleteTeamAsync(invalidId));
            Assert.AreEqual("Không tìm thấy ban.", ex.Message);

            // Đảm bảo rằng các hàm DeleteAsync và SaveChangeAsync không được gọi
            _unitOfWorkMock.Verify(uow => uow.Team.DeleteAsync(It.IsAny<Team>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Never);
        }
        [Test]
        public async Task DeleteTeamAsync_InvalidId999_ThrowsArgumentException()
        {
            // Arrange
            int invalidId = 999;

            // Giả lập không tìm thấy đội với Id không hợp lệ
            _unitOfWorkMock.Setup(uow => uow.Team.GetByIdAsync(invalidId, null))
                .ReturnsAsync((Team)null); // Trả về null khi không tìm thấy đội

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _teamService.DeleteTeamAsync(invalidId));
            Assert.AreEqual("Không tìm thấy ban.", ex.Message);

            // Đảm bảo rằng các hàm DeleteAsync và SaveChangeAsync không được gọi
            _unitOfWorkMock.Verify(uow => uow.Team.DeleteAsync(It.IsAny<Team>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Never);
        }
        [Test]
        public async Task GetTeamByIdAsync_InvalidId_ThrowsArgumentException()
        {
            // Arrange
            int invalidId = -1;

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _teamService.GetTeamByIdAsync(invalidId)
            );
            Assert.AreEqual("Id không hợp lệ, phải lớn hơn 0.", ex.Message, "Expected exception message to match");
        }
        [Test]
        public async Task GetTeamByIdAsync_NonExistentId_ReturnsNull()
        {
            // Arrange
            int nonExistentId = 9999;

            _unitOfWorkMock.Setup(uow => uow.Team.GetAsync(
                    It.IsAny<Expression<Func<Team, bool>>>(),
                    true, null)
                ).ReturnsAsync((Team)null); // Simulate no team found for the non-existent ID

            _mapperMock.Setup(m => m.Map<TeamDto>(It.IsAny<Team>()))
                .Returns((TeamDto)null); // No mapping should occur for a null team

            // Act
            var result = await _teamService.GetTeamByIdAsync(nonExistentId);

            // Assert
            Assert.IsNull(result, "Expected null for a non-existent team ID");
        }
        [Test]
        public async Task GetTeamByIdAsync_ShouldReturnMappedTeamDto_WhenTeamFound()
        {
            // Arrange
            int validId = 1;
            var team = new Team
            {
                Id = validId,
                CourseId = 1,
                LeaderId = 1,
                TeamName = "Team A"
            };

            // Setup mock cho UnitOfWork - đảm bảo đúng tham số
            _unitOfWorkMock.Setup(u => u.Team.GetAsync(
                It.IsAny<Expression<Func<Team, bool>>>(),true, "Course,Leader,VolunteerTeam.Volunteer.VolunteerCourse"))
                .ReturnsAsync(team); // Trả về team mock

            // Setup mock cho Mapper
            _mapperMock.Setup(m => m.Map<TeamDto>(It.IsAny<Team>())).Returns(new TeamDto
            {
                Id = team.Id,
                CourseId = team.CourseId,
                LeaderId = team.LeaderId,
                TeamName = team.TeamName
            });

            // Act
            var result = await _teamService.GetTeamByIdAsync(validId);

            // Assert
            Assert.IsNotNull(result);  // Kiểm tra result không phải null
            Assert.AreEqual(validId, result.Id);  // Kiểm tra Id đúng
            Assert.AreEqual("Team A", result.TeamName);  // Kiểm tra tên đội đúng
        }
        [Test]
        public async Task GetTeamByIdAsync_ShouldReturnMappedTeamDto_WhenTeamIdEqual2()
        {
            // Arrange
            int validId = 2;
            var team = new Team
            {
                Id = validId,
                CourseId = 1,
                LeaderId = 1,
                TeamName = "Team A"
            };

            // Setup mock cho UnitOfWork - đảm bảo đúng tham số
            _unitOfWorkMock.Setup(u => u.Team.GetAsync(
                It.IsAny<Expression<Func<Team, bool>>>(), true, "Course,Leader,VolunteerTeam.Volunteer.VolunteerCourse"))
                .ReturnsAsync(team); // Trả về team mock

            // Setup mock cho Mapper
            _mapperMock.Setup(m => m.Map<TeamDto>(It.IsAny<Team>())).Returns(new TeamDto
            {
                Id = team.Id,
                CourseId = team.CourseId,
                LeaderId = team.LeaderId,
                TeamName = team.TeamName
            });

            // Act
            var result = await _teamService.GetTeamByIdAsync(validId);

            // Assert
            Assert.IsNotNull(result);  // Kiểm tra result không phải null
            Assert.AreEqual(validId, result.Id);  // Kiểm tra Id đúng
            Assert.AreEqual("Team A", result.TeamName);  // Kiểm tra tên đội đúng
        }
        [Test]
        public void GetAllTeamsByCourseIdAsync_ShouldThrowArgumentException_WhenCourseIdIsInvalid()
        {
            // Arrange
            int invalidCourseId = 0; // CourseId không hợp lệ

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _teamService.GetAllTeamsByCourseIdAsync(invalidCourseId));
        }
        [Test]
        public async Task GetAllTeamsByCourseIdAsync_ShouldReturnEmptyList_WhenNoTeamsFound()
        {
            // Arrange
            int validCourseId = 1;

            // Mock GetAllAsync trả về danh sách rỗng
            _unitOfWorkMock.Setup(u => u.Team.GetAllAsync(It.IsAny<Expression<Func<Team, bool>>>(), "Course,Leader,VolunteerTeam.Volunteer.VolunteerCourse"))
                           .ReturnsAsync(new List<Team>());

            // Act
            var result = await _teamService.GetAllTeamsByCourseIdAsync(validCourseId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);  // Đảm bảo trả về danh sách rỗng khi không có team nào
        }
        [Test]
        public async Task GetAllTeamsByCourseIdAsync_ShouldReturnMappedTeamDtos_WhenTeamsFound()
        {
            // Arrange
            int validCourseId = 1;
            var teams = new List<Team>
    {
        new Team
        {
            Id = 1,
            CourseId = validCourseId,
            LeaderId = 1,
            TeamName = "Team A"
        },
        new Team
        {
            Id = 2,
            CourseId = validCourseId,
            LeaderId = 2,
            TeamName = "Team B"
        }
    };

            // Mock repository trả về danh sách teams
            _unitOfWorkMock.Setup(u => u.Team.GetAllAsync(
                It.IsAny<Expression<Func<Team, bool>>>(),
                "Course,Leader,VolunteerTeam.Volunteer.VolunteerCourse"))
                .ReturnsAsync(teams);

            // Mock mapper để chuyển đổi từ Team thành TeamDto
            _mapperMock.Setup(m => m.Map<List<TeamDto>>(It.IsAny<List<Team>>()))
                       .Returns(new List<TeamDto>
                       {
                   new TeamDto
                   {
                       Id = 1,
                       CourseId = validCourseId,
                       LeaderId = 1,
                       TeamName = "Team A"
                   },
                   new TeamDto
                   {
                       Id = 2,
                       CourseId = validCourseId,
                       LeaderId = 2,
                       TeamName = "Team B"
                   }
                       });

            // Act
            var result = await _teamService.GetAllTeamsByCourseIdAsync(validCourseId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("Team A", result.First().TeamName);
            Assert.AreEqual("Team B", result.Last().TeamName);
        }
        [Test]
        public async Task GetAllTeamsByCourseIdAsync_ShouldReturnMappedTeamDtos_WhenTeamIdEqual2()
        {
            // Arrange
            int validCourseId = 1;
            var teams = new List<Team>
    {
        new Team
        {
            Id = 1,
            CourseId = validCourseId,
            LeaderId = 1,
            TeamName = "Team A"
        },
        new Team
        {
            Id = 2,
            CourseId = validCourseId,
            LeaderId = 2,
            TeamName = "Team B"
        }
    };

            // Mock repository trả về danh sách teams
            _unitOfWorkMock.Setup(u => u.Team.GetAllAsync(
                It.IsAny<Expression<Func<Team, bool>>>(),
                "Course,Leader,VolunteerTeam.Volunteer.VolunteerCourse"))
                .ReturnsAsync(teams);

            // Mock mapper để chuyển đổi từ Team thành TeamDto
            _mapperMock.Setup(m => m.Map<List<TeamDto>>(It.IsAny<List<Team>>()))
                       .Returns(new List<TeamDto>
                       {
                   new TeamDto
                   {
                       Id = 1,
                       CourseId = validCourseId,
                       LeaderId = 1,
                       TeamName = "Team A"
                   },
                   new TeamDto
                   {
                       Id = 2,
                       CourseId = validCourseId,
                       LeaderId = 2,
                       TeamName = "Team B"
                   }
                       });

            // Act
            var result = await _teamService.GetAllTeamsByCourseIdAsync(validCourseId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("Team A", result.First().TeamName);
            Assert.AreEqual("Team B", result.Last().TeamName);
        }



















    }
}

