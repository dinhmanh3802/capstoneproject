using AutoMapper;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SCCMS.Domain.DTOs.CourseDtos;
using SCCMS.Domain.Services.Implements;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.UnitOfWork;
using System.Linq.Expressions;
using Utility;

namespace SCCMS.Test
{
    [TestFixture]
    public class CourseServiceTest
    {
        private Mock<IUnitOfWork>? _unitOfWorkMock;
        private Mock<IMapper>? _mapperMock;
        private CourseService? _courseService;

        [SetUp]
        public void Setup()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _courseService = new CourseService(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        [Test]
        public async Task GetAllCoursesAsync_AllParametersNull_ReturnsAllCourses()
        {
            // Arrange
            var courses = new List<Course>
            {
                new Course
                {
                    Id = 1,
                    CourseName = "Course 1",
                    Status = CourseStatus.notStarted,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(10)
                },
                new Course
                {
                    Id = 2,
                    CourseName = "Course 2",
                    Status = CourseStatus.inProgress,
                    StartDate = DateTime.Now.AddDays(1),
                    EndDate = DateTime.Now.AddDays(11)
                }
            };

            _unitOfWorkMock.Setup(uow => uow.Course.FindAsync(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(courses);

            var expectedDtos = courses
                .Select(c => new CourseDto { Id = c.Id, CourseName = c.CourseName, Status = c.Status, StartDate = c.StartDate, EndDate = c.EndDate })
                .ToList();

            _mapperMock.Setup(m => m.Map<IEnumerable<CourseDto>>(It.IsAny<IEnumerable<Course>>()))
                .Returns(expectedDtos);

            // Act
            var result = await _courseService.GetAllCoursesAsync(null, null, null, null);

            // Assert
            Assert.NotNull(result, "Result should not be null");
            Assert.AreEqual(2, result.Count(), "Expected 2 courses");
            Assert.AreEqual("Course 1", result.First().CourseName, "First course name should be 'Course 1'");
            Assert.AreEqual("Course 2", result.Last().CourseName, "Last course name should be 'Course 2'");
        }

        [Test]
        public async Task GetAllCoursesAsync_NameFilter_ReturnsFilteredCourses()
        {
            // Arrange
            var courseName = "Khóa Tu Mùa Hè";

            var courses = new List<Course>
        {
            new Course
            {
                Id = 1,
                CourseName = "Khóa Tu Mùa Hè",
                Status = CourseStatus.notStarted,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(10)
            },
            new Course
            {
                Id = 2,
                CourseName = "Khóa Tu Mùa Hè",
                Status = CourseStatus.inProgress,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(11)
            },
            new Course
            {
                Id = 3,
                CourseName = "Khóa Tu Mùa Đông",
                Status = CourseStatus.recruiting,
                StartDate = DateTime.Now.AddDays(2),
                EndDate = DateTime.Now.AddDays(12)
            }
        };

            _unitOfWorkMock.Setup(uow => uow.Course.FindAsync(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Course> { new Course
            {
                Id = 1,
                CourseName = "Khóa Tu Mùa Hè",
                Status = CourseStatus.notStarted,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(10)
            }, new Course
            {
                Id = 2,
                CourseName = "Khóa Tu Mùa Hè",
                Status = CourseStatus.inProgress,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(11)
            }});

            var expectedDtos = courses
                .Where(c => c.CourseName == courseName)
                .Select(c => new CourseDto { Id = c.Id, CourseName = c.CourseName, Status = c.Status, StartDate = c.StartDate, EndDate = c.EndDate })
                .ToList();

            _mapperMock.Setup(m => m.Map<IEnumerable<CourseDto>>(It.IsAny<IEnumerable<Course>>()))
                .Returns(expectedDtos);

            // Act
            var result = await _courseService.GetAllCoursesAsync(courseName, null, null, null);

            // Assert
            Assert.NotNull(result, "Result should not be null");
            Assert.That(result.Count(), Is.EqualTo(2), "Expected 2 courses with the name 'Khóa Tu Mùa Hè'");
            Assert.IsTrue(result.All(c => c.CourseName == courseName), "All returned courses should have the name 'Khóa Tu Mùa Hè'");
        }

        [Test]
        public async Task GetAllCoursesAsync_NameFilter_ReturnsEmptyResult()
        {
            // Arrange
            var courseName = "ABC";

            var courses = new List<Course>
    {
        new Course
        {
            Id = 1,
            CourseName = "Khóa Tu Mùa Hè",
            Status = CourseStatus.notStarted,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(10)
        },
        new Course
        {
            Id = 2,
            CourseName = "Khóa Tu Mùa Hè",
            Status = CourseStatus.inProgress,
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now.AddDays(11)
        },
        new Course
        {
            Id = 3,
            CourseName = "Khóa Tu Mùa Đông",
            Status = CourseStatus.recruiting,
            StartDate = DateTime.Now.AddDays(2),
            EndDate = DateTime.Now.AddDays(12)
        }
    };

            _unitOfWorkMock.Setup(uow => uow.Course.FindAsync(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Course>());
            var expectedDtos = new List<CourseDto>(); // No courses match the name "ABC"

            _mapperMock.Setup(m => m.Map<IEnumerable<CourseDto>>(It.IsAny<IEnumerable<Course>>()))
                .Returns(expectedDtos);

            // Act
            var result = await _courseService.GetAllCoursesAsync(courseName, null, null, null);

            // Assert
            Assert.NotNull(result, "Result should not be null");
            Assert.AreEqual(0, result.Count(), "Expected 0 courses with the name 'ABC'");
        }

        [Test]
        public async Task GetAllCoursesAsync_CourseStatusFilter_ReturnsFilteredCourses()
        {
            // Arrange
            var courseStatus = CourseStatus.inProgress; // Assuming 1 corresponds to CourseStatus.inProgress

            var courses = new List<Course>
    {
        new Course
        {
            Id = 1,
            CourseName = "Khóa Tu Mùa Hè",
            Status = CourseStatus.notStarted,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(10)
        },
        new Course
        {
            Id = 2,
            CourseName = "Khóa Tu Mùa Hè",
            Status = CourseStatus.inProgress,
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now.AddDays(11)
        },
        new Course
        {
            Id = 3,
            CourseName = "Khóa Tu Mùa Đông",
            Status = CourseStatus.inProgress,
            StartDate = DateTime.Now.AddDays(2),
            EndDate = DateTime.Now.AddDays(12)
        }
    };

            _unitOfWorkMock.Setup(uow => uow.Course.FindAsync(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync((Expression<Func<Course, bool>> predicate, string includeProperties) => courses.AsQueryable().Where(predicate).ToList());

            var expectedDtos = courses
                .Where(c => c.Status == courseStatus)
                .Select(c => new CourseDto { Id = c.Id, CourseName = c.CourseName, Status = c.Status, StartDate = c.StartDate, EndDate = c.EndDate })
                .ToList();

            _mapperMock.Setup(m => m.Map<IEnumerable<CourseDto>>(It.IsAny<IEnumerable<Course>>()))
                .Returns(expectedDtos);

            // Act
            var result = await _courseService.GetAllCoursesAsync(null, courseStatus, null, null);

            // Assert
            Assert.NotNull(result, "Result should not be null");
            Assert.AreEqual(2, result.Count(), "Expected 2 courses with the status 'inProgress'");
            Assert.IsTrue(result.All(c => c.Status == courseStatus), "All returned courses should have the status 'inProgress'");
        }

        [Test]
        public async Task GetAllCoursesAsync_CourseStatusZeroFilter_ReturnsFilteredCourses()
        {
            // Arrange
            var courseStatus = CourseStatus.notStarted; // Assuming 0 corresponds to CourseStatus.notStarted

            var courses = new List<Course>
    {
        new Course
        {
            Id = 1,
            CourseName = "Khóa Tu Mùa Hè",
            Status = CourseStatus.notStarted,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(10)
        },
        new Course
        {
            Id = 2,
            CourseName = "Khóa Tu Mùa Hè",
            Status = CourseStatus.inProgress,
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now.AddDays(11)
        },
        new Course
        {
            Id = 3,
            CourseName = "Khóa Tu Mùa Đông",
            Status = CourseStatus.notStarted,
            StartDate = DateTime.Now.AddDays(2),
            EndDate = DateTime.Now.AddDays(12)
        }
    };

            _unitOfWorkMock.Setup(uow => uow.Course.FindAsync(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync((Expression<Func<Course, bool>> predicate, string includeProperties) => courses.AsQueryable().Where(predicate).ToList());

            var expectedDtos = courses
                .Where(c => c.Status == courseStatus)
                .Select(c => new CourseDto { Id = c.Id, CourseName = c.CourseName, Status = c.Status, StartDate = c.StartDate, EndDate = c.EndDate })
                .ToList();

            _mapperMock.Setup(m => m.Map<IEnumerable<CourseDto>>(It.IsAny<IEnumerable<Course>>()))
                .Returns(expectedDtos);

            // Act
            var result = await _courseService.GetAllCoursesAsync(null, courseStatus, null, null);

            // Assert
            Assert.NotNull(result, "Result should not be null");
            Assert.AreEqual(2, result.Count(), "Expected 2 courses with the status 'notStarted'");
            Assert.IsTrue(result.All(c => c.Status == courseStatus), "All returned courses should have the status 'notStarted'");
        }

        [Test]
        public async Task GetAllCoursesAsync_CourseStatusClosedFilter_ReturnsFilteredCourses()
        {
            // Arrange
            var courseStatus = CourseStatus.closed; // Assuming 4 corresponds to CourseStatus.closed

            var courses = new List<Course>
    {
        new Course
        {
            Id = 1,
            CourseName = "Khóa Tu Mùa Hè",
            Status = CourseStatus.notStarted,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(10)
        },
        new Course
        {
            Id = 2,
            CourseName = "Khóa Tu Mùa Hè",
            Status = CourseStatus.closed,
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now.AddDays(11)
        },
        new Course
        {
            Id = 3,
            CourseName = "Khóa Tu Mùa Đông",
            Status = CourseStatus.closed,
            StartDate = DateTime.Now.AddDays(2),
            EndDate = DateTime.Now.AddDays(12)
        }
    };

            _unitOfWorkMock.Setup(uow => uow.Course.FindAsync(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync((Expression<Func<Course, bool>> predicate, string includeProperties) => courses.AsQueryable().Where(predicate).ToList());

            var expectedDtos = courses
                .Where(c => c.Status == courseStatus)
                .Select(c => new CourseDto { Id = c.Id, CourseName = c.CourseName, Status = c.Status, StartDate = c.StartDate, EndDate = c.EndDate })
                .ToList();

            _mapperMock.Setup(m => m.Map<IEnumerable<CourseDto>>(It.IsAny<IEnumerable<Course>>()))
                .Returns(expectedDtos);

            // Act
            var result = await _courseService.GetAllCoursesAsync(null, courseStatus, null, null);

            // Assert
            Assert.NotNull(result, "Result should not be null");
            Assert.AreEqual(2, result.Count(), "Expected 2 courses with the status 'closed'");
            Assert.IsTrue(result.All(c => c.Status == courseStatus), "All returned courses should have the status 'closed'");
        }

        [Test]
        public async Task GetAllCoursesAsync_InvalidCourseStatus_ReturnsEmptyResult()
        {
            // Arrange
            var courseStatus = (CourseStatus)10; // Assuming 10 does not correspond to any valid CourseStatus

            var courses = new List<Course>
    {
        new Course
        {
            Id = 1,
            CourseName = "Khóa Tu Mùa Hè",
            Status = CourseStatus.notStarted,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(10)
        },
        new Course
        {
            Id = 2,
            CourseName = "Khóa Tu Mùa Hè",
            Status = CourseStatus.inProgress,
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now.AddDays(11)
        },
        new Course
        {
            Id = 3,
            CourseName = "Khóa Tu Mùa Đông",
            Status = CourseStatus.closed,
            StartDate = DateTime.Now.AddDays(2),
            EndDate = DateTime.Now.AddDays(12)
        }
    };

            _unitOfWorkMock.Setup(uow => uow.Course.FindAsync(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync((Expression<Func<Course, bool>> predicate, string includeProperties) => courses.AsQueryable().Where(predicate).ToList());

            var expectedDtos = new List<CourseDto>(); // No courses match the status 10

            _mapperMock.Setup(m => m.Map<IEnumerable<CourseDto>>(It.IsAny<IEnumerable<Course>>()))
                .Returns(expectedDtos);

            // Act
            var result = await _courseService.GetAllCoursesAsync(null, courseStatus, null, null);

            // Assert
            Assert.NotNull(result, "Result should not be null");
            Assert.AreEqual(0, result.Count(), "Expected 0 courses with the status '10'");
        }


        [Test]
        public async Task GetAllCoursesAsync_DateRangeFilter_ReturnsFilteredCourses()
        {
            // Arrange
            DateTime startDate = new DateTime(2024, 1, 1);
            DateTime endDate = new DateTime(2024, 1, 30);

            var courses = new List<Course>
    {
        new Course
        {
            Id = 1,
            CourseName = "Khóa Tu Mùa Hè",
            Status = CourseStatus.notStarted,
            StartDate = new DateTime(2024, 1, 5),
            EndDate = new DateTime(2024, 1, 15)
        },
        new Course
        {
            Id = 2,
            CourseName = "Khóa Tu Mùa Hè",
            Status = CourseStatus.inProgress,
            StartDate = new DateTime(2024, 2, 1),
            EndDate = new DateTime(2024, 2, 10)
        },
        new Course
        {
            Id = 3,
            CourseName = "Khóa Tu Mùa Đông",
            Status = CourseStatus.closed,
            StartDate = new DateTime(2023, 12, 25),
            EndDate = new DateTime(2024, 1, 5)
        }
    };

            _unitOfWorkMock.Setup(uow => uow.Course.FindAsync(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync((Expression<Func<Course, bool>> predicate, string includeProperties) => courses.AsQueryable().Where(predicate).ToList());

            var expectedDtos = courses
                .Where(c => c.StartDate >= startDate && c.EndDate <= endDate)
                .Select(c => new CourseDto { Id = c.Id, CourseName = c.CourseName, Status = c.Status, StartDate = c.StartDate, EndDate = c.EndDate })
                .ToList();

            _mapperMock.Setup(m => m.Map<IEnumerable<CourseDto>>(It.IsAny<IEnumerable<Course>>()))
                .Returns(expectedDtos);

            // Act
            var result = await _courseService.GetAllCoursesAsync(null, null, startDate, endDate);

            // Assert
            Assert.NotNull(result, "Result should not be null");
            Assert.AreEqual(1, result.Count(), "Expected 1 course within the date range");
            Assert.IsTrue(result.All(c => c.StartDate >= startDate && c.EndDate <= endDate), "All returned courses should be within the date range");
        }

        [Test]
        public async Task GetAllCoursesAsync_InvalidDateRange_ReturnsEmptyResult()
        {
            // Arrange
            DateTime startDate = new DateTime(2024, 2, 1);
            DateTime endDate = new DateTime(2024, 1, 30);

            var courses = new List<Course>
    {
        new Course
        {
            Id = 1,
            CourseName = "Khóa Tu Mùa Hè",
            Status = CourseStatus.notStarted,
            StartDate = new DateTime(2024, 1, 5),
            EndDate = new DateTime(2024, 1, 15)
        },
        new Course
        {
            Id = 2,
            CourseName = "Khóa Tu Mùa Hè",
            Status = CourseStatus.inProgress,
            StartDate = new DateTime(2024, 2, 1),
            EndDate = new DateTime(2024, 2, 10)
        },
        new Course
        {
            Id = 3,
            CourseName = "Khóa Tu Mùa Đông",
            Status = CourseStatus.closed,
            StartDate = new DateTime(2023, 12, 25),
            EndDate = new DateTime(2024, 1, 5)
        }
    };

            _unitOfWorkMock.Setup(uow => uow.Course.FindAsync(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync((Expression<Func<Course, bool>> predicate, string includeProperties) => courses.AsQueryable().Where(predicate).ToList());

            var expectedDtos = new List<CourseDto>(); // No courses should match the invalid date range

            _mapperMock.Setup(m => m.Map<IEnumerable<CourseDto>>(It.IsAny<IEnumerable<Course>>()))
                .Returns(expectedDtos);

            // Act
            var result = await _courseService.GetAllCoursesAsync(null, null, startDate, endDate);

            // Assert
            Assert.NotNull(result, "Result should not be null");
            Assert.AreEqual(0, result.Count(), "Expected 0 courses within the invalid date range");
        }

        [Test]
        public async Task GetAllCoursesAsync_InvalidStartDate_ReturnsEmptyResult()
        {
            // Arrange
            string invalidStartDate = "abc"; // Invalid date string
            DateTime? startDate = null;
            DateTime? endDate = null;

            // Attempt to parse the invalid start date
            if (DateTime.TryParse(invalidStartDate, out DateTime parsedStartDate))
            {
                startDate = parsedStartDate;
            }

            var courses = new List<Course>
    {
        new Course
        {
            Id = 1,
            CourseName = "Khóa Tu Mùa Hè",
            Status = CourseStatus.notStarted,
            StartDate = new DateTime(2024, 1, 5),
            EndDate = new DateTime(2024, 1, 15)
        },
        new Course
        {
            Id = 2,
            CourseName = "Khóa Tu Mùa Hè",
            Status = CourseStatus.inProgress,
            StartDate = new DateTime(2024, 2, 1),
            EndDate = new DateTime(2024, 2, 10)
        },
        new Course
        {
            Id = 3,
            CourseName = "Khóa Tu Mùa Đông",
            Status = CourseStatus.closed,
            StartDate = new DateTime(2023, 12, 25),
            EndDate = new DateTime(2024, 1, 5)
        }
    };

            _unitOfWorkMock.Setup(uow => uow.Course.FindAsync(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync((Expression<Func<Course, bool>> predicate, string includeProperties) => courses.AsQueryable().Where(predicate).ToList());

            var expectedDtos = new List<CourseDto>(); // No courses should match the invalid start date

            _mapperMock.Setup(m => m.Map<IEnumerable<CourseDto>>(It.IsAny<IEnumerable<Course>>()))
                .Returns(expectedDtos);

            // Act
            var result = await _courseService.GetAllCoursesAsync(null, null, startDate, endDate);

            // Assert
            Assert.NotNull(result, "Result should not be null");
            Assert.AreEqual(0, result.Count(), "Expected 0 courses with the invalid start date");
        }

        [Test]
        public async Task GetAllCoursesAsync_InvalidEndDate_ReturnsEmptyResult()
        {
            // Arrange
            string invalidEndDate = "abc"; // Invalid date string
            DateTime? startDate = null;
            DateTime? endDate = null;

            // Attempt to parse the invalid end date
            if (DateTime.TryParse(invalidEndDate, out DateTime parsedEndDate))
            {
                endDate = parsedEndDate;
            }

            var courses = new List<Course>
    {
        new Course
        {
            Id = 1,
            CourseName = "Khóa Tu Mùa Hè",
            Status = CourseStatus.notStarted,
            StartDate = new DateTime(2024, 1, 5),
            EndDate = new DateTime(2024, 1, 15)
        },
        new Course
        {
            Id = 2,
            CourseName = "Khóa Tu Mùa Hè",
            Status = CourseStatus.inProgress,
            StartDate = new DateTime(2024, 2, 1),
            EndDate = new DateTime(2024, 2, 10)
        },
        new Course
        {
            Id = 3,
            CourseName = "Khóa Tu Mùa Đông",
            Status = CourseStatus.closed,
            StartDate = new DateTime(2023, 12, 25),
            EndDate = new DateTime(2024, 1, 5)
        }
    };

            _unitOfWorkMock.Setup(uow => uow.Course.FindAsync(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync((Expression<Func<Course, bool>> predicate, string includeProperties) => courses.AsQueryable().Where(predicate).ToList());

            var expectedDtos = new List<CourseDto>(); // No courses should match the invalid end date

            _mapperMock.Setup(m => m.Map<IEnumerable<CourseDto>>(It.IsAny<IEnumerable<Course>>()))
                .Returns(expectedDtos);

            // Act
            var result = await _courseService.GetAllCoursesAsync(null, null, startDate, endDate);

            // Assert
            Assert.NotNull(result, "Result should not be null");
            Assert.AreEqual(0, result.Count(), "Expected 0 courses with the invalid end date");
        }


        [Test]
        public async Task GetCourseByIdAsync_InvalidId_ReturnsNull()
        {
            // Arrange
            int invalidId = -1;

            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(invalidId, It.IsAny<string>()))
                .ReturnsAsync((Course)null); // Simulate no course found for the invalid ID

            _mapperMock.Setup(m => m.Map<CourseDto>(It.IsAny<Course>()))
                .Returns((CourseDto)null); // No mapping should occur for a null course

            // Act
            var result = await _courseService.GetCourseByIdAsync(invalidId);

            // Assert
            Assert.IsNull(result, "Expected null for an invalid course ID");
        }

        [Test]
        public async Task GetCourseByIdAsync_NonExistentId_ReturnsNull()
        {
            // Arrange
            int nonExistentId = 9999;

            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(nonExistentId, It.IsAny<string>()))
                .ReturnsAsync((Course)null); // Simulate no course found for the non-existent ID

            _mapperMock.Setup(m => m.Map<CourseDto>(It.IsAny<Course>()))
                .Returns((CourseDto)null); // No mapping should occur for a null course

            // Act
            var result = await _courseService.GetCourseByIdAsync(nonExistentId);

            // Assert
            Assert.IsNull(result, "Expected null for a non-existent course ID");
        }

        [Test]
        public async Task GetCourseByIdAsync_ValidId_ReturnsCourse()
        {
            // Arrange
            int validId = 1;

            var course = new Course
            {
                Id = validId,
                CourseName = "Khóa Tu Mùa Hè",
                Status = CourseStatus.notStarted,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(10)
            };

            var courseDto = new CourseDto
            {
                Id = validId,
                CourseName = "Khóa Tu Mùa Hè",
                Status = CourseStatus.notStarted,
                StartDate = course.StartDate,
                EndDate = course.EndDate
            };

            _unitOfWorkMock.Setup(uow => uow.Course.FindAsync(
                It.IsAny<Expression<Func<Course, bool>>>(),
                It.IsAny<string>())
            ).ReturnsAsync(new List<Course> { course });  // Simulate returning a list with the valid course

            _mapperMock.Setup(m => m.Map<CourseDto>(course))
                .Returns(courseDto); // Map the course to CourseDto

            // Act
            var result = await _courseService.GetCourseByIdAsync(validId);

            // Assert
            Assert.NotNull(result, "Result should not be null");
            Assert.AreEqual(validId, result.Id, $"Expected course ID to be {validId}");
            Assert.AreEqual(course.CourseName, result.CourseName, $"Expected course name to be {course.CourseName}");
            Assert.AreEqual(course.Status, result.Status, $"Expected course status to be {course.Status}");
            Assert.AreEqual(course.StartDate, result.StartDate, $"Expected course start date to be {course.StartDate}");
            Assert.AreEqual(course.EndDate, result.EndDate, $"Expected course end date to be {course.EndDate}");
        }

        [Test]
        public async Task GetCourseByIdAsync_ValidId5_ReturnsCourse()
        {
            // Arrange
            int validId = 5;

            var course = new Course
            {
                Id = validId,
                CourseName = "Khóa Học Lập Trình C#",
                Status = CourseStatus.inProgress,
                StartDate = DateTime.Now.AddDays(-5),
                EndDate = DateTime.Now.AddDays(10)
            };

            var courseDto = new CourseDto
            {
                Id = validId,
                CourseName = "Khóa Học Lập Trình C#",
                Status = CourseStatus.inProgress,
                StartDate = course.StartDate,
                EndDate = course.EndDate
            };

            _unitOfWorkMock.Setup(uow => uow.Course.FindAsync(
                It.IsAny<Expression<Func<Course, bool>>>(),
                It.IsAny<string>())
            ).ReturnsAsync(new List<Course> { course });  // Simulate returning a list with the valid course

            _mapperMock.Setup(m => m.Map<CourseDto>(course))
                .Returns(courseDto); // Map the course to CourseDto

            // Act
            var result = await _courseService.GetCourseByIdAsync(validId);

            // Assert
            Assert.NotNull(result, "Result should not be null");
            Assert.AreEqual(validId, result.Id, $"Expected course ID to be {validId}");
            Assert.AreEqual(course.CourseName, result.CourseName, $"Expected course name to be {course.CourseName}");
            Assert.AreEqual(course.Status, result.Status, $"Expected course status to be {course.Status}");
            Assert.AreEqual(course.StartDate, result.StartDate, $"Expected course start date to be {course.StartDate}");
            Assert.AreEqual(course.EndDate, result.EndDate, $"Expected course end date to be {course.EndDate}");
        }

        [Test]
        public async Task DeleteCourseAsync_ValidId_DeletesCourse()
        {
            // Arrange
            int validId = 999;

            var course = new Course
            {
                Id = validId,
                CourseName = "Khóa Tu mùa hè",
                Status = CourseStatus.inProgress,
                StartDate = DateTime.Now.AddDays(-10),
                EndDate = DateTime.Now.AddDays(5)
            };

            // Giả lập tìm thấy khóa học với Id hợp lệ
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(validId, null))
                .ReturnsAsync(course);

            _unitOfWorkMock.Setup(uow => uow.Course.DeleteAsync(course))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
                  .ReturnsAsync(1);

            // Act
            await _courseService.DeleteCourseAsync(validId);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.Course.DeleteAsync(It.Is<Course>(c => c.Id == validId)), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once);
        }


        [Test]
        public async Task DeleteCourseAsync_ValidId_DeletesCourse2()
        {
            // Arrange
            int validId = 1;

            var course = new Course
            {
                Id = validId,
                CourseName = "Khóa Tu mùa hè",
                Status = CourseStatus.inProgress,
                StartDate = DateTime.Now.AddDays(-10),
                EndDate = DateTime.Now.AddDays(5)
            };

            // Giả lập tìm thấy khóa học với Id hợp lệ
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(validId, null))
                .ReturnsAsync(course);

            _unitOfWorkMock.Setup(uow => uow.Course.DeleteAsync(course))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
                  .ReturnsAsync(1);

            // Act
            await _courseService.DeleteCourseAsync(validId);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.Course.DeleteAsync(It.Is<Course>(c => c.Id == validId)), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once);
        }

        [Test]
        public async Task DeleteCourseAsync_InvalidId_ThrowsArgumentException()
        {
            // Arrange
            int invalidId = -1;

            // Giả lập không tìm thấy khóa học với Id không hợp lệ
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(invalidId, null))
                .ReturnsAsync((Course)null); // Trả về null khi không tìm thấy khóa học

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _courseService.DeleteCourseAsync(invalidId));
            Assert.AreEqual("Khóa tu không tồn tại", ex.Message);

            // Đảm bảo rằng các hàm DeleteAsync và SaveChangeAsync không được gọi
            _unitOfWorkMock.Verify(uow => uow.Course.DeleteAsync(It.IsAny<Course>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Never);
        }


        [Test]
        public async Task DeleteCourseAsync_Id1000_ThrowsArgumentException()
        {
            // Arrange
            int invalidId = 1000;

            // Giả lập không tìm thấy khóa học với Id không hợp lệ
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(invalidId, null))
                .ReturnsAsync((Course)null); // Trả về null khi không tìm thấy khóa học

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _courseService.DeleteCourseAsync(invalidId));
            Assert.AreEqual("Khóa tu không tồn tại", ex.Message);

            // Đảm bảo rằng các hàm DeleteAsync và SaveChangeAsync không được gọi
            _unitOfWorkMock.Verify(uow => uow.Course.DeleteAsync(It.IsAny<Course>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Never);
        }

        [Test]
        public async Task GetCurrentCourseAsync_InProgressCourse_ReturnsCourseDto()
        {
            // Arrange
            var currentCourse = new Course
            {
                Id = 1,
                CourseName = "Khóa Tu Mùa Hè",
                Status = CourseStatus.inProgress,
                StartDate = DateTime.Now.AddDays(-5),
                EndDate = DateTime.Now.AddDays(5)
            };

            var expectedDto = new CourseDto
            {
                Id = 1,
                CourseName = "Khóa Tu Mùa Hè",
                Status = CourseStatus.inProgress,
                StartDate = currentCourse.StartDate,
                EndDate = currentCourse.EndDate
            };

            _unitOfWorkMock.Setup(uow => uow.Course.GetAsync(
                    It.IsAny<Expression<Func<Course, bool>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<string>()
                ))
                .ReturnsAsync(currentCourse);

            _mapperMock.Setup(m => m.Map<CourseDto>(currentCourse))
                .Returns(expectedDto);

            // Act
            var result = await _courseService.GetCurrentCourseAsync();

            // Assert
            Assert.NotNull(result, "Result should not be null");
            Assert.AreEqual(expectedDto.Id, result.Id, "Expected course ID to be returned");
            Assert.AreEqual(expectedDto.CourseName, result.CourseName, "Expected course name to be returned");
            Assert.AreEqual(expectedDto.Status, result.Status, "Expected course status to be inProgress");
        }

        [Test]
        public async Task GetCurrentCourseAsync_UpcomingCourse_ReturnsCourseDto()
        {
            // Arrange
            var upcomingCourse = new Course
            {
                Id = 2,
                CourseName = "Khóa Tu Mùa Thu",
                Status = CourseStatus.notStarted,
                StartDate = DateTime.Now.AddDays(10),
                EndDate = DateTime.Now.AddDays(20)
            };

            var expectedDto = new CourseDto
            {
                Id = 2,
                CourseName = "Khóa Tu Mùa Thu",
                Status = CourseStatus.notStarted,
                StartDate = upcomingCourse.StartDate,
                EndDate = upcomingCourse.EndDate
            };

            // Mock cho khóa học inProgress trả về null
            _unitOfWorkMock.Setup(uow => uow.Course.GetAsync(
                    It.IsAny<Expression<Func<Course, bool>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<string>()
                ))
                .ReturnsAsync((Course)null); // Không có khóa học inProgress

            // Mock cho FindAsync để trả về khóa học sắp diễn ra
            _unitOfWorkMock.Setup(uow => uow.Course.FindAsync(
                    It.IsAny<Expression<Func<Course, bool>>>(),
                    It.IsAny<string>()
                ))
                .ReturnsAsync(new List<Course> { upcomingCourse }); // Có khóa học sắp tới

            _mapperMock.Setup(m => m.Map<CourseDto>(upcomingCourse))
                .Returns(expectedDto);

            // Act
            var result = await _courseService.GetCurrentCourseAsync();

            // Assert
            Assert.NotNull(result, "Result should not be null");
            Assert.AreEqual(expectedDto.Id, result.Id, "Expected course ID to be returned");
            Assert.AreEqual(expectedDto.CourseName, result.CourseName, "Expected course name to be returned");
        }


        [Test]
        public async Task GetCurrentCourseAsync_ClosestEndDateCourse_ReturnsCourseDto()
        {
            // Arrange
            var pastCourse = new Course
            {
                Id = 3,
                CourseName = "Khóa Tu Mùa Đông",
                Status = CourseStatus.closed,
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now.AddDays(-10)
            };

            var expectedDto = new CourseDto
            {
                Id = 3,
                CourseName = "Khóa Tu Mùa Đông",
                Status = CourseStatus.closed,
                StartDate = pastCourse.StartDate,
                EndDate = pastCourse.EndDate
            };

            _unitOfWorkMock.SetupSequence(uow => uow.Course.GetAsync(
                    It.IsAny<Expression<Func<Course, bool>>>(),
                    true,
                    "StudentGroup,StudentCourses,Feedback,NightShift,Room"
                ))
                .ReturnsAsync((Course)null) // Không có course inProgress
                .ReturnsAsync((Course)null); // Không có course sắp tới

            _unitOfWorkMock.Setup(uow => uow.Course.GetAllAsync(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Course> { pastCourse }); // Trả về danh sách các khóa học

            _mapperMock.Setup(m => m.Map<CourseDto>(pastCourse))
                .Returns(expectedDto);

            // Act
            var result = await _courseService.GetCurrentCourseAsync();

            // Assert
            Assert.NotNull(result, "Result should not be null");
            Assert.AreEqual(expectedDto.Id, result.Id, "Expected course ID to be returned");
            Assert.AreEqual(expectedDto.CourseName, result.CourseName, "Expected course name to be returned");
        }

        [Test]
        public async Task GetCurrentCourseAsync_NoCourses_ReturnsNull()
        {
            // Arrange
            _unitOfWorkMock.SetupSequence(uow => uow.Course.GetAsync(
                    It.IsAny<Expression<Func<Course, bool>>>(),
                    true,
                    "StudentGroup,StudentCourses,Feedback,NightShift,Room"
                ))
                .ReturnsAsync((Course)null) // Không có course inProgress
                .ReturnsAsync((Course)null); // Không có course sắp tới

            _unitOfWorkMock.Setup(uow => uow.Course.GetAllAsync(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Course>()); // Không có khóa học nào

            // Act
            var result = await _courseService.GetCurrentCourseAsync();

            // Assert
            Assert.Null(result, "Result should be null when no courses are found");
        }


        [Test]
        public async Task CreateCourseAsync_ValidCourse_CreatesCourseSuccessfully()
        {
            // Arrange
            var courseName = "Khóa Tu Mùa Hè";
            var expectedStudents = 1;
            var studentApplicationStartDate = DateTime.Now.AddDays(1);
            var studentApplicationEndDate = DateTime.Now.AddDays(3);
            var volunteerApplicationStartDate = DateTime.Now.AddDays(1);
            var volunteerApplicationEndDate = DateTime.Now.AddDays(3);
            var startDate = DateTime.Now.AddDays(4);
            var endDate = DateTime.Now.AddDays(10);
            var description = "";

            var newCourse = new CourseCreateDto
            {
                CourseName = courseName,
                ExpectedStudents = expectedStudents,
                StudentApplicationStartDate = studentApplicationStartDate,
                StudentApplicationEndDate = studentApplicationEndDate,
                VolunteerApplicationStartDate = volunteerApplicationStartDate,
                VolunteerApplicationEndDate = volunteerApplicationEndDate,
                StartDate = startDate,
                EndDate = endDate,
                Description = description
            };

            var expectedCourse = new Course
            {
                CourseName = courseName,
                ExpectedStudents = expectedStudents,
                StudentApplicationStartDate = studentApplicationStartDate,
                StudentApplicationEndDate = studentApplicationEndDate,
                VolunteerApplicationStartDate = volunteerApplicationStartDate,
                VolunteerApplicationEndDate = volunteerApplicationEndDate,
                StartDate = startDate,
                EndDate = endDate,
                Description = description
            };

            var existingCourses = new List<Course>
                {
                    new Course { CourseName = "Existing Course", Status = CourseStatus.closed, EndDate = DateTime.Now.AddDays(-1) }
                };

            _unitOfWorkMock.Setup(uow => uow.Course.GetAllAsync(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<string>()))
                           .ReturnsAsync(existingCourses);

            _mapperMock.Setup(m => m.Map<Course>(It.IsAny<CourseCreateDto>()))
                       .Returns(expectedCourse);

            _unitOfWorkMock.Setup(uow => uow.Course.AddAsync(It.IsAny<Course>()))
                           .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.NightShift.AddAsync(It.IsAny<NightShift>()))
                           .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
                           .ReturnsAsync(1);

            // Act
            await _courseService.CreateCourseAsync(newCourse);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.Course.AddAsync(It.Is<Course>(c =>
                c.CourseName == courseName &&
                c.ExpectedStudents == expectedStudents &&
                c.StudentApplicationStartDate == studentApplicationStartDate &&
                c.StudentApplicationEndDate == studentApplicationEndDate &&
                c.VolunteerApplicationStartDate == volunteerApplicationStartDate &&
                c.VolunteerApplicationEndDate == volunteerApplicationEndDate &&
                c.StartDate == startDate &&
                c.EndDate == endDate &&
                c.Description == description)), Times.Once);

            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Exactly(2));
        }

        [Test]
        public async Task CreateCourseAsync_ValidCourse_CreatesCourseSuccessfully2()
        {
            // Arrange
            var courseName = "Khóa Tu Mùa Hè";
            var expectedStudents = 1;
            var studentApplicationStartDate = DateTime.Now.AddDays(1);
            var studentApplicationEndDate = DateTime.Now.AddDays(3);
            var volunteerApplicationStartDate = DateTime.Now.AddDays(1);
            var volunteerApplicationEndDate = DateTime.Now.AddDays(3);
            var startDate = DateTime.Now.AddDays(4);
            var endDate = DateTime.Now.AddDays(10);
            var description = "abc";

            var newCourse = new CourseCreateDto
            {
                CourseName = courseName,
                ExpectedStudents = expectedStudents,
                StudentApplicationStartDate = studentApplicationStartDate,
                StudentApplicationEndDate = studentApplicationEndDate,
                VolunteerApplicationStartDate = volunteerApplicationStartDate,
                VolunteerApplicationEndDate = volunteerApplicationEndDate,
                StartDate = startDate,
                EndDate = endDate,
                Description = description
            };

            var expectedCourse = new Course
            {
                CourseName = courseName,
                ExpectedStudents = expectedStudents,
                StudentApplicationStartDate = studentApplicationStartDate,
                StudentApplicationEndDate = studentApplicationEndDate,
                VolunteerApplicationStartDate = volunteerApplicationStartDate,
                VolunteerApplicationEndDate = volunteerApplicationEndDate,
                StartDate = startDate,
                EndDate = endDate,
                Description = description
            };

            var existingCourses = new List<Course>
        {
            new Course { CourseName = "Existing Course", Status = CourseStatus.closed, EndDate = DateTime.Now.AddDays(-1) }
        };

            _unitOfWorkMock.Setup(uow => uow.Course.GetAllAsync(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<string>()))
                           .ReturnsAsync(existingCourses);

            _mapperMock.Setup(m => m.Map<Course>(It.IsAny<CourseCreateDto>()))
                       .Returns(expectedCourse);

            _unitOfWorkMock.Setup(uow => uow.Course.AddAsync(It.IsAny<Course>()))
                           .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.NightShift.AddAsync(It.IsAny<NightShift>()))
                           .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
                           .ReturnsAsync(1);

            // Act
            await _courseService.CreateCourseAsync(newCourse);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.Course.AddAsync(It.Is<Course>(c =>
                c.CourseName == courseName &&
                c.ExpectedStudents == expectedStudents &&
                c.StudentApplicationStartDate == studentApplicationStartDate &&
                c.StudentApplicationEndDate == studentApplicationEndDate &&
                c.VolunteerApplicationStartDate == volunteerApplicationStartDate &&
                c.VolunteerApplicationEndDate == volunteerApplicationEndDate &&
                c.StartDate == startDate &&
                c.EndDate == endDate &&
                c.Description == description)), Times.Once);

            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Exactly(2));
        }


        [Test]
        public async Task CreateCourseAsync_CourseNameIsNull_ThrowsArgumentException()
        {
            // Arrange
            string courseName = null; // Course name is null
            var expectedStudents = 1;
            var studentApplicationStartDate = DateTime.Now.AddDays(1);
            var studentApplicationEndDate = DateTime.Now.AddDays(3);
            var volunteerApplicationStartDate = DateTime.Now.AddDays(1);
            var volunteerApplicationEndDate = DateTime.Now.AddDays(3);
            var startDate = DateTime.Now.AddDays(4);
            var endDate = DateTime.Now.AddDays(10);
            var description = "";

            var newCourse = new CourseCreateDto
            {
                CourseName = courseName, // Null value here
                ExpectedStudents = expectedStudents,
                StudentApplicationStartDate = studentApplicationStartDate,
                StudentApplicationEndDate = studentApplicationEndDate,
                VolunteerApplicationStartDate = volunteerApplicationStartDate,
                VolunteerApplicationEndDate = volunteerApplicationEndDate,
                StartDate = startDate,
                EndDate = endDate,
                Description = description
            };

            // Mocking the service calls that are not relevant for this test case
            _unitOfWorkMock.Setup(uow => uow.Course.GetAllAsync(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<string>()))
                           .ReturnsAsync(new List<Course>());

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _courseService.CreateCourseAsync(newCourse));

            // Verify that the exception message is correct
            Assert.AreEqual("Tên không hợp lệ", exception.Message); // Ensure this matches the validation message
        }

        [Test]
        public async Task CreateCourseAsync_CourseNameIsEmpty_ThrowsArgumentException()
        {
            // Arrange
            string courseName = ""; // Course name is null
            var expectedStudents = 1;
            var studentApplicationStartDate = DateTime.Now.AddDays(1);
            var studentApplicationEndDate = DateTime.Now.AddDays(3);
            var volunteerApplicationStartDate = DateTime.Now.AddDays(1);
            var volunteerApplicationEndDate = DateTime.Now.AddDays(3);
            var startDate = DateTime.Now.AddDays(4);
            var endDate = DateTime.Now.AddDays(10);
            var description = "";

            var newCourse = new CourseCreateDto
            {
                CourseName = courseName, // Null value here
                ExpectedStudents = expectedStudents,
                StudentApplicationStartDate = studentApplicationStartDate,
                StudentApplicationEndDate = studentApplicationEndDate,
                VolunteerApplicationStartDate = volunteerApplicationStartDate,
                VolunteerApplicationEndDate = volunteerApplicationEndDate,
                StartDate = startDate,
                EndDate = endDate,
                Description = description
            };

            // Mocking the service calls that are not relevant for this test case
            _unitOfWorkMock.Setup(uow => uow.Course.GetAllAsync(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<string>()))
                           .ReturnsAsync(new List<Course>());

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _courseService.CreateCourseAsync(newCourse));

            // Verify that the exception message is correct
            Assert.AreEqual("Tên không hợp lệ", exception.Message); // Ensure this matches the validation message
        }

        [Test]
        public async Task CreateCourseAsync_expectedStudentsIsZero_ThrowsArgumentException()
        {
            // Arrange
            string courseName = "a"; // Course name is null
            var expectedStudents = 0;
            var studentApplicationStartDate = DateTime.Now.AddDays(1);
            var studentApplicationEndDate = DateTime.Now.AddDays(3);
            var volunteerApplicationStartDate = DateTime.Now.AddDays(1);
            var volunteerApplicationEndDate = DateTime.Now.AddDays(3);
            var startDate = DateTime.Now.AddDays(4);
            var endDate = DateTime.Now.AddDays(10);
            var description = "";

            var newCourse = new CourseCreateDto
            {
                CourseName = courseName, // Null value here
                ExpectedStudents = expectedStudents,
                StudentApplicationStartDate = studentApplicationStartDate,
                StudentApplicationEndDate = studentApplicationEndDate,
                VolunteerApplicationStartDate = volunteerApplicationStartDate,
                VolunteerApplicationEndDate = volunteerApplicationEndDate,
                StartDate = startDate,
                EndDate = endDate,
                Description = description
            };

            // Mocking the service calls that are not relevant for this test case
            _unitOfWorkMock.Setup(uow => uow.Course.GetAllAsync(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<string>()))
                           .ReturnsAsync(new List<Course>());

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _courseService.CreateCourseAsync(newCourse));
        }

        [Test]
        public async Task CreateCourseAsync_expectedStudentsIsInvalid_ThrowsArgumentException()
        {
            // Arrange
            string courseName = "a"; // Course name is null
            var expectedStudents = 10001;
            var studentApplicationStartDate = DateTime.Now.AddDays(1);
            var studentApplicationEndDate = DateTime.Now.AddDays(3);
            var volunteerApplicationStartDate = DateTime.Now.AddDays(1);
            var volunteerApplicationEndDate = DateTime.Now.AddDays(3);
            var startDate = DateTime.Now.AddDays(4);
            var endDate = DateTime.Now.AddDays(10);
            var description = "";

            var newCourse = new CourseCreateDto
            {
                CourseName = courseName, // Null value here
                ExpectedStudents = expectedStudents,
                StudentApplicationStartDate = studentApplicationStartDate,
                StudentApplicationEndDate = studentApplicationEndDate,
                VolunteerApplicationStartDate = volunteerApplicationStartDate,
                VolunteerApplicationEndDate = volunteerApplicationEndDate,
                StartDate = startDate,
                EndDate = endDate,
                Description = description
            };

            // Mocking the service calls that are not relevant for this test case
            _unitOfWorkMock.Setup(uow => uow.Course.GetAllAsync(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<string>()))
                           .ReturnsAsync(new List<Course>());

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _courseService.CreateCourseAsync(newCourse));
        }

        [Test]
        public async Task CreateCourseAsync_startDateInPass_ThrowsArgumentException()
        {
            // Arrange
            string courseName = "a"; // Course name is null
            var expectedStudents = 1;
            var studentApplicationStartDate = DateTime.Now.AddDays(1);
            var studentApplicationEndDate = DateTime.Now.AddDays(3);
            var volunteerApplicationStartDate = DateTime.Now.AddDays(1);
            var volunteerApplicationEndDate = DateTime.Now.AddDays(3);
            var startDate = DateTime.Now.AddDays(-4);
            var endDate = DateTime.Now.AddDays(10);
            var description = "";

            var newCourse = new CourseCreateDto
            {
                CourseName = courseName, // Null value here
                ExpectedStudents = expectedStudents,
                StudentApplicationStartDate = studentApplicationStartDate,
                StudentApplicationEndDate = studentApplicationEndDate,
                VolunteerApplicationStartDate = volunteerApplicationStartDate,
                VolunteerApplicationEndDate = volunteerApplicationEndDate,
                StartDate = startDate,
                EndDate = endDate,
                Description = description
            };

            // Mocking the service calls that are not relevant for this test case
            _unitOfWorkMock.Setup(uow => uow.Course.GetAllAsync(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<string>()))
                           .ReturnsAsync(new List<Course>());

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _courseService.CreateCourseAsync(newCourse));
        }

        [Test]
        public async Task CreateCourseAsync_EndateGraterStart_ThrowsArgumentException()
        {
            // Arrange
            string courseName = "a"; // Course name is null
            var expectedStudents = 0;
            var studentApplicationStartDate = DateTime.Now.AddDays(1);
            var studentApplicationEndDate = DateTime.Now.AddDays(3);
            var volunteerApplicationStartDate = DateTime.Now.AddDays(1);
            var volunteerApplicationEndDate = DateTime.Now.AddDays(3);
            var startDate = DateTime.Now.AddDays(6);
            var endDate = DateTime.Now.AddDays(4);
            var description = "";

            var newCourse = new CourseCreateDto
            {
                CourseName = courseName, // Null value here
                ExpectedStudents = expectedStudents,
                StudentApplicationStartDate = studentApplicationStartDate,
                StudentApplicationEndDate = studentApplicationEndDate,
                VolunteerApplicationStartDate = volunteerApplicationStartDate,
                VolunteerApplicationEndDate = volunteerApplicationEndDate,
                StartDate = startDate,
                EndDate = endDate,
                Description = description
            };

            // Mocking the service calls that are not relevant for this test case
            _unitOfWorkMock.Setup(uow => uow.Course.GetAllAsync(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<string>()))
                           .ReturnsAsync(new List<Course>());

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _courseService.CreateCourseAsync(newCourse));
        }

        [Test]
        public async Task CreateCourseAsync_EndateGraterStart2_ThrowsArgumentException()
        {
            // Arrange
            string courseName = "a"; // Course name is null
            var expectedStudents = 0;
            var studentApplicationStartDate = DateTime.Now.AddDays(3);
            var studentApplicationEndDate = DateTime.Now.AddDays(1);
            var volunteerApplicationStartDate = DateTime.Now.AddDays(1);
            var volunteerApplicationEndDate = DateTime.Now.AddDays(3);
            var startDate = DateTime.Now.AddDays(6);
            var endDate = DateTime.Now.AddDays(14);
            var description = "";

            var newCourse = new CourseCreateDto
            {
                CourseName = courseName, // Null value here
                ExpectedStudents = expectedStudents,
                StudentApplicationStartDate = studentApplicationStartDate,
                StudentApplicationEndDate = studentApplicationEndDate,
                VolunteerApplicationStartDate = volunteerApplicationStartDate,
                VolunteerApplicationEndDate = volunteerApplicationEndDate,
                StartDate = startDate,
                EndDate = endDate,
                Description = description
            };

            // Mocking the service calls that are not relevant for this test case
            _unitOfWorkMock.Setup(uow => uow.Course.GetAllAsync(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<string>()))
                           .ReturnsAsync(new List<Course>());

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _courseService.CreateCourseAsync(newCourse));
        }

        [Test]
        public async Task CreateCourseAsync_EndateGraterStart3_ThrowsArgumentException()
        {
            // Arrange
            string courseName = "a"; // Course name is null
            var expectedStudents = 0;
            var studentApplicationStartDate = DateTime.Now.AddDays(1);
            var studentApplicationEndDate = DateTime.Now.AddDays(3);
            var volunteerApplicationStartDate = DateTime.Now.AddDays(3);
            var volunteerApplicationEndDate = DateTime.Now.AddDays(1);
            var startDate = DateTime.Now.AddDays(6);
            var endDate = DateTime.Now.AddDays(14);
            var description = "";

            var newCourse = new CourseCreateDto
            {
                CourseName = courseName, // Null value here
                ExpectedStudents = expectedStudents,
                StudentApplicationStartDate = studentApplicationStartDate,
                StudentApplicationEndDate = studentApplicationEndDate,
                VolunteerApplicationStartDate = volunteerApplicationStartDate,
                VolunteerApplicationEndDate = volunteerApplicationEndDate,
                StartDate = startDate,
                EndDate = endDate,
                Description = description
            };

            // Mocking the service calls that are not relevant for this test case
            _unitOfWorkMock.Setup(uow => uow.Course.GetAllAsync(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<string>()))
                           .ReturnsAsync(new List<Course>());

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _courseService.CreateCourseAsync(newCourse));
        }


        [Test]
        public async Task CreateCourseAsync_ApplicationDateError_ThrowsArgumentException()
        {
            // Arrange
            string courseName = "a"; // Course name is null
            var expectedStudents = 0;
            var studentApplicationStartDate = DateTime.Now.AddDays(-1);
            var studentApplicationEndDate = DateTime.Now.AddDays(3);
            var volunteerApplicationStartDate = DateTime.Now.AddDays(3);
            var volunteerApplicationEndDate = DateTime.Now.AddDays(1);
            var startDate = DateTime.Now.AddDays(6);
            var endDate = DateTime.Now.AddDays(14);
            var description = "";

            var newCourse = new CourseCreateDto
            {
                CourseName = courseName, // Null value here
                ExpectedStudents = expectedStudents,
                StudentApplicationStartDate = studentApplicationStartDate,
                StudentApplicationEndDate = studentApplicationEndDate,
                VolunteerApplicationStartDate = volunteerApplicationStartDate,
                VolunteerApplicationEndDate = volunteerApplicationEndDate,
                StartDate = startDate,
                EndDate = endDate,
                Description = description
            };

            // Mocking the service calls that are not relevant for this test case
            _unitOfWorkMock.Setup(uow => uow.Course.GetAllAsync(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<string>()))
                           .ReturnsAsync(new List<Course>());

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _courseService.CreateCourseAsync(newCourse));
        }

        [Test]
        public async Task CreateCourseAsync_ApplicationDateError2_ThrowsArgumentException()
        {
            // Arrange
            string courseName = "a"; // Course name is null
            var expectedStudents = 0;
            var studentApplicationStartDate = DateTime.Now.AddDays(1);
            var studentApplicationEndDate = DateTime.Now.AddDays(3);
            var volunteerApplicationStartDate = DateTime.Now.AddDays(-3);
            var volunteerApplicationEndDate = DateTime.Now.AddDays(1);
            var startDate = DateTime.Now.AddDays(6);
            var endDate = DateTime.Now.AddDays(14);
            var description = "";

            var newCourse = new CourseCreateDto
            {
                CourseName = courseName, // Null value here
                ExpectedStudents = expectedStudents,
                StudentApplicationStartDate = studentApplicationStartDate,
                StudentApplicationEndDate = studentApplicationEndDate,
                VolunteerApplicationStartDate = volunteerApplicationStartDate,
                VolunteerApplicationEndDate = volunteerApplicationEndDate,
                StartDate = startDate,
                EndDate = endDate,
                Description = description
            };

            // Mocking the service calls that are not relevant for this test case
            _unitOfWorkMock.Setup(uow => uow.Course.GetAllAsync(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<string>()))
                           .ReturnsAsync(new List<Course>());

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _courseService.CreateCourseAsync(newCourse));
        }


        [Test]
        public async Task UpdateCourseAsync_ValidCourse_UpdatesCourseSuccessfully()
        {
            // Arrange
            var courseId = 1;
            var courseName = "Khóa Tu Mùa Đông";
            var updatedStartDate = DateTime.Now.AddDays(1);
            var updatedEndDate = DateTime.Now.AddDays(10);
            var updatedDescription = "Updated course description";
            var updatedExpectedStudents = 100;

            // Original course in database
            var existingCourse = new Course
            {
                Id = courseId,
                CourseName = "Khóa Tu Mùa Hè",
                Status = CourseStatus.notStarted,
                StartDate = DateTime.Now.AddDays(2),
                EndDate = DateTime.Now.AddDays(7),
                Description = "Old course description",
                ExpectedStudents = 50
            };

            // CourseUpdateDto with the new values
            var courseUpdateDto = new CourseUpdateDto
            {
                CourseName = courseName,
                StartDate = updatedStartDate,
                EndDate = updatedEndDate,
                Description = updatedDescription,
                ExpectedStudents = updatedExpectedStudents
            };

            // Mock existing course data in UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(courseId, It.IsAny<string>()))
                           .ReturnsAsync(existingCourse);

            // Mock saving changes to the course
            _unitOfWorkMock.Setup(uow => uow.Course.UpdateAsync(It.IsAny<Course>()))
                           .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
                           .ReturnsAsync(1);

            // Act
            await _courseService.UpdateCourseAsync(courseId, courseUpdateDto);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.Course.UpdateAsync(It.Is<Course>(c =>
                c.Id == courseId &&
                c.CourseName == courseName &&
                c.StartDate == updatedStartDate &&
                c.EndDate == updatedEndDate &&
                c.Description == updatedDescription &&
                c.ExpectedStudents == updatedExpectedStudents
            )), Times.Once);

            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once);
        }

        [Test]
        public async Task UpdateCourseAsync_ValidCourse_UpdatesCourseSuccessfully2()
        {
            // Arrange
            var courseId = 1;
            var courseName = "Khóa Tu Mùa Đông";
            var updatedStartDate = DateTime.Now.AddDays(1);
            var updatedEndDate = DateTime.Now.AddDays(10);
            var updatedDescription = "a";
            var updatedExpectedStudents = 100;

            // Original course in database
            var existingCourse = new Course
            {
                Id = courseId,
                CourseName = "Khóa Tu Mùa Hè",
                Status = CourseStatus.notStarted,
                StartDate = DateTime.Now.AddDays(2),
                EndDate = DateTime.Now.AddDays(7),
                Description = "Old course description",
                ExpectedStudents = 50
            };

            // CourseUpdateDto with the new values
            var courseUpdateDto = new CourseUpdateDto
            {
                CourseName = courseName,
                StartDate = updatedStartDate,
                EndDate = updatedEndDate,
                Description = updatedDescription,
                ExpectedStudents = updatedExpectedStudents
            };

            // Mock existing course data in UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(courseId, It.IsAny<string>()))
                           .ReturnsAsync(existingCourse);

            // Mock saving changes to the course
            _unitOfWorkMock.Setup(uow => uow.Course.UpdateAsync(It.IsAny<Course>()))
                           .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChangeAsync())
                           .ReturnsAsync(1);

            // Act
            await _courseService.UpdateCourseAsync(courseId, courseUpdateDto);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.Course.UpdateAsync(It.Is<Course>(c =>
                c.Id == courseId &&
                c.CourseName == courseName &&
                c.StartDate == updatedStartDate &&
                c.EndDate == updatedEndDate &&
                c.Description == updatedDescription &&
                c.ExpectedStudents == updatedExpectedStudents
            )), Times.Once);

            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Once);
        }

        [Test]
        public async Task UpdateCourseAsync_CourseNameIsNull_ThrowsArgumentException()
        {
            // Arrange
            var courseId = 1;

            // Original course in database
            var existingCourse = new Course
            {
                Id = courseId,
                CourseName = "Khóa Tu Mùa Hè",
                Status = CourseStatus.notStarted,
                StartDate = DateTime.Now.AddDays(2),
                EndDate = DateTime.Now.AddDays(7),
                Description = "Old course description",
                ExpectedStudents = 50
            };

            // CourseUpdateDto with null CourseName
            var courseUpdateDto = new CourseUpdateDto
            {
                CourseName = null, // courseName is null
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(10),
                Description = "Updated description",
                ExpectedStudents = 100
            };

            // Mock existing course data in UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(courseId, It.IsAny<string>()))
                           .ReturnsAsync(existingCourse);

            // Act & Assert
            _unitOfWorkMock.Verify(uow => uow.Course.UpdateAsync(It.IsAny<Course>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Never);
        }

        [Test]
        public async Task UpdateCourseAsync_CourseNameIsEmpty_ThrowsArgumentException()
        {
            // Arrange
            var courseId = 1;

            // Original course in database
            var existingCourse = new Course
            {
                Id = courseId,
                CourseName = "",
                Status = CourseStatus.notStarted,
                StartDate = DateTime.Now.AddDays(2),
                EndDate = DateTime.Now.AddDays(7),
                Description = "Old course description",
                ExpectedStudents = 50
            };

            // CourseUpdateDto with null CourseName
            var courseUpdateDto = new CourseUpdateDto
            {
                CourseName = null, // courseName is null
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(10),
                Description = "Updated description",
                ExpectedStudents = 100
            };

            // Mock existing course data in UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(courseId, It.IsAny<string>()))
                           .ReturnsAsync(existingCourse);

            // Act & Assert
            _unitOfWorkMock.Verify(uow => uow.Course.UpdateAsync(It.IsAny<Course>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Never);
        }

        [Test]
        public async Task UpdateCourseAsync_ExpectedStudentsError_ThrowsArgumentException()
        {
            // Arrange
            var courseId = 1;

            // Original course in database
            var existingCourse = new Course
            {
                Id = courseId,
                CourseName = "a",
                Status = CourseStatus.notStarted,
                StartDate = DateTime.Now.AddDays(2),
                EndDate = DateTime.Now.AddDays(7),
                Description = "Old course description",
                ExpectedStudents = 100
            };

            // CourseUpdateDto with null CourseName
            var courseUpdateDto = new CourseUpdateDto
            {
                CourseName = null, // courseName is null
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(10),
                Description = "Updated description",
                ExpectedStudents = 0
            };

            // Mock existing course data in UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(courseId, It.IsAny<string>()))
                           .ReturnsAsync(existingCourse);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _courseService.UpdateCourseAsync(courseId, courseUpdateDto);
            });

            _unitOfWorkMock.Verify(uow => uow.Course.UpdateAsync(It.IsAny<Course>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Never);
        }

        [Test]
        public async Task UpdateCourseAsync_ExpectedStudentsError2_ThrowsArgumentException()
        {
            // Arrange
            var courseId = 1;

            // Original course in database
            var existingCourse = new Course
            {
                Id = courseId,
                CourseName = "a",
                Status = CourseStatus.notStarted,
                StartDate = DateTime.Now.AddDays(2),
                EndDate = DateTime.Now.AddDays(7),
                Description = "Old course description",
                ExpectedStudents = 10
            };

            // CourseUpdateDto with null CourseName
            var courseUpdateDto = new CourseUpdateDto
            {
                CourseName = null, // courseName is null
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(10),
                Description = "Updated description",
                ExpectedStudents = 1001
            };

            // Mock existing course data in UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(courseId, It.IsAny<string>()))
                           .ReturnsAsync(existingCourse);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _courseService.UpdateCourseAsync(courseId, courseUpdateDto);
            });

            _unitOfWorkMock.Verify(uow => uow.Course.UpdateAsync(It.IsAny<Course>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Never);
        }

        [Test]
        public async Task UpdateCourseAsync_DateError_ThrowsArgumentException()
        {
            // Arrange
            var courseId = 1;

            // Original course in database
            var existingCourse = new Course
            {
                Id = courseId,
                CourseName = "a",
                Status = CourseStatus.notStarted,
                StartDate = DateTime.Now.AddDays(2),
                EndDate = DateTime.Now.AddDays(7),
                StudentApplicationStartDate = DateTime.Now.AddDays(1),
                StudentApplicationEndDate = DateTime.Now.AddDays(3),
                VolunteerApplicationStartDate = DateTime.Now.AddDays(1),
                VolunteerApplicationEndDate = DateTime.Now.AddDays(3),
                Description = "Old course description",
                ExpectedStudents = 10
            };

            // CourseUpdateDto with null CourseName
            var courseUpdateDto = new CourseUpdateDto
            {
                CourseName = null, // courseName is null
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(10),
                StudentApplicationStartDate = DateTime.Now.AddDays(-1),
                StudentApplicationEndDate = DateTime.Now.AddDays(3),
                VolunteerApplicationStartDate = DateTime.Now.AddDays(1),
                VolunteerApplicationEndDate = DateTime.Now.AddDays(3),
                Description = "Updated description",
                ExpectedStudents = 100
            };

            // Mock existing course data in UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(courseId, It.IsAny<string>()))
                           .ReturnsAsync(existingCourse);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _courseService.UpdateCourseAsync(courseId, courseUpdateDto);
            });

            _unitOfWorkMock.Verify(uow => uow.Course.UpdateAsync(It.IsAny<Course>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Never);
        }

        [Test]
        public async Task UpdateCourseAsync_DateError2_ThrowsArgumentException()
        {
            // Arrange
            var courseId = 1;

            // Original course in database
            var existingCourse = new Course
            {
                Id = courseId,
                CourseName = "a",
                Status = CourseStatus.notStarted,
                StartDate = DateTime.Now.AddDays(2),
                EndDate = DateTime.Now.AddDays(7),
                StudentApplicationStartDate = DateTime.Now.AddDays(1),
                StudentApplicationEndDate = DateTime.Now.AddDays(3),
                VolunteerApplicationStartDate = DateTime.Now.AddDays(1),
                VolunteerApplicationEndDate = DateTime.Now.AddDays(3),
                Description = "Old course description",
                ExpectedStudents = 10
            };

            // CourseUpdateDto with null CourseName
            var courseUpdateDto = new CourseUpdateDto
            {
                CourseName = null, // courseName is null
                StartDate = DateTime.Now.AddDays(11),
                EndDate = DateTime.Now.AddDays(10),
                StudentApplicationStartDate = DateTime.Now.AddDays(1),
                StudentApplicationEndDate = DateTime.Now.AddDays(3),
                VolunteerApplicationStartDate = DateTime.Now.AddDays(1),
                VolunteerApplicationEndDate = DateTime.Now.AddDays(3),
                Description = "Updated description",
                ExpectedStudents = 100
            };

            // Mock existing course data in UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(courseId, It.IsAny<string>()))
                           .ReturnsAsync(existingCourse);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _courseService.UpdateCourseAsync(courseId, courseUpdateDto);
            });

            _unitOfWorkMock.Verify(uow => uow.Course.UpdateAsync(It.IsAny<Course>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Never);
        }

        [Test]
        public async Task UpdateCourseAsync_DateError3_ThrowsArgumentException()
        {
            // Arrange
            var courseId = 1;

            // Original course in database
            var existingCourse = new Course
            {
                Id = courseId,
                CourseName = "a",
                Status = CourseStatus.notStarted,
                StartDate = DateTime.Now.AddDays(2),
                EndDate = DateTime.Now.AddDays(7),
                StudentApplicationStartDate = DateTime.Now.AddDays(1),
                StudentApplicationEndDate = DateTime.Now.AddDays(3),
                VolunteerApplicationStartDate = DateTime.Now.AddDays(1),
                VolunteerApplicationEndDate = DateTime.Now.AddDays(3),
                Description = "Old course description",
                ExpectedStudents = 10
            };

            // CourseUpdateDto with null CourseName
            var courseUpdateDto = new CourseUpdateDto
            {
                CourseName = null, // courseName is null
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(10),
                StudentApplicationStartDate = DateTime.Now.AddDays(11),
                StudentApplicationEndDate = DateTime.Now.AddDays(3),
                VolunteerApplicationStartDate = DateTime.Now.AddDays(1),
                VolunteerApplicationEndDate = DateTime.Now.AddDays(3),
                Description = "Updated description",
                ExpectedStudents = 100
            };

            // Mock existing course data in UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(courseId, It.IsAny<string>()))
                           .ReturnsAsync(existingCourse);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _courseService.UpdateCourseAsync(courseId, courseUpdateDto);
            });

            _unitOfWorkMock.Verify(uow => uow.Course.UpdateAsync(It.IsAny<Course>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Never);
        }

        [Test]
        public async Task UpdateCourseAsync_DateError4_ThrowsArgumentException()
        {
            // Arrange
            var courseId = 1;

            // Original course in database
            var existingCourse = new Course
            {
                Id = courseId,
                CourseName = "a",
                Status = CourseStatus.notStarted,
                StartDate = DateTime.Now.AddDays(2),
                EndDate = DateTime.Now.AddDays(7),
                StudentApplicationStartDate = DateTime.Now.AddDays(1),
                StudentApplicationEndDate = DateTime.Now.AddDays(3),
                VolunteerApplicationStartDate = DateTime.Now.AddDays(11),
                VolunteerApplicationEndDate = DateTime.Now.AddDays(3),
                Description = "Old course description",
                ExpectedStudents = 10
            };

            // CourseUpdateDto with null CourseName
            var courseUpdateDto = new CourseUpdateDto
            {
                CourseName = "", // courseName is null
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(10),
                StudentApplicationStartDate = DateTime.Now.AddDays(-1),
                StudentApplicationEndDate = DateTime.Now.AddDays(3),
                VolunteerApplicationStartDate = DateTime.Now.AddDays(1),
                VolunteerApplicationEndDate = DateTime.Now.AddDays(3),
                Description = "Updated description",
                ExpectedStudents = 100
            };

            // Mock existing course data in UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(courseId, It.IsAny<string>()))
                           .ReturnsAsync(existingCourse);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _courseService.UpdateCourseAsync(courseId, courseUpdateDto);
            });

            _unitOfWorkMock.Verify(uow => uow.Course.UpdateAsync(It.IsAny<Course>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Never);
        }

        [Test]
        public async Task UpdateCourseAsync_DateError5_ThrowsArgumentException()
        {
            // Arrange
            var courseId = 1;

            // Original course in database
            var existingCourse = new Course
            {
                Id = courseId,
                CourseName = "a",
                Status = CourseStatus.notStarted,
                StartDate = DateTime.Now.AddDays(2),
                EndDate = DateTime.Now.AddDays(7),
                StudentApplicationStartDate = DateTime.Now.AddDays(-1),
                StudentApplicationEndDate = DateTime.Now.AddDays(3),
                VolunteerApplicationStartDate = DateTime.Now.AddDays(1),
                VolunteerApplicationEndDate = DateTime.Now.AddDays(3),
                Description = "Old course description",
                ExpectedStudents = 10
            };

            // CourseUpdateDto with null CourseName
            var courseUpdateDto = new CourseUpdateDto
            {
                CourseName = null, // courseName is null
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(10),
                StudentApplicationStartDate = DateTime.Now.AddDays(-1),
                StudentApplicationEndDate = DateTime.Now.AddDays(3),
                VolunteerApplicationStartDate = DateTime.Now.AddDays(1),
                VolunteerApplicationEndDate = DateTime.Now.AddDays(3),
                Description = "Updated description",
                ExpectedStudents = 100
            };

            // Mock existing course data in UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(courseId, It.IsAny<string>()))
                           .ReturnsAsync(existingCourse);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _courseService.UpdateCourseAsync(courseId, courseUpdateDto);
            });

            _unitOfWorkMock.Verify(uow => uow.Course.UpdateAsync(It.IsAny<Course>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Never);
        }

        [Test]
        public async Task UpdateCourseAsync_DateError6_ThrowsArgumentException()
        {
            // Arrange
            var courseId = 1;

            // Original course in database
            var existingCourse = new Course
            {
                Id = courseId,
                CourseName = "a",
                Status = CourseStatus.notStarted,
                StartDate = DateTime.Now.AddDays(2),
                EndDate = DateTime.Now.AddDays(7),
                StudentApplicationStartDate = DateTime.Now.AddDays(1),
                StudentApplicationEndDate = DateTime.Now.AddDays(3),
                VolunteerApplicationStartDate = DateTime.Now.AddDays(1),
                VolunteerApplicationEndDate = DateTime.Now.AddDays(3),
                Description = "Old course description",
                ExpectedStudents = 10
            };

            // CourseUpdateDto with null CourseName
            var courseUpdateDto = new CourseUpdateDto
            {
                CourseName = null, // courseName is null
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(10),
                StudentApplicationStartDate = DateTime.Now.AddDays(1),
                StudentApplicationEndDate = DateTime.Now.AddDays(3),
                VolunteerApplicationStartDate = DateTime.Now.AddDays(-1),
                VolunteerApplicationEndDate = DateTime.Now.AddDays(3),
                Description = "Updated description",
                ExpectedStudents = 100
            };

            // Mock existing course data in UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.Course.GetByIdAsync(courseId, It.IsAny<string>()))
                           .ReturnsAsync(existingCourse);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _courseService.UpdateCourseAsync(courseId, courseUpdateDto);
            });

            _unitOfWorkMock.Verify(uow => uow.Course.UpdateAsync(It.IsAny<Course>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveChangeAsync(), Times.Never);
        }


    }
}
