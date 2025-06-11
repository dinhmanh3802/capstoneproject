using NUnit.Framework;
using Moq;
using SCCMS.Domain.DTOs.Auth.Login;
using SCCMS.Infrastucture.Repository;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using AutoMapper;
using SCCMS.API.Services;
using SCCMS.Domain.Services.Implements;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.UnitOfWork;
using Utility;
using System.Reflection;

namespace SCCMS.Tests.Services
{
    [TestFixture]
    public class AuthServiceTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IMapper> _mockMapper;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IEmailService> _mockEmailService;
        private AuthService _authService;
        private string _secretKey = "fadsfasdfsadfasrtbrgdfgvsdfhvvdsttbhuyfhdfgvhrthdfchddcbgthvdstvcrtg!"; // Example secret key

        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockEmailService = new Mock<IEmailService>();

            var mockSection = new Mock<IConfigurationSection>();
            mockSection.Setup(a => a.Value).Returns(_secretKey);
            _mockConfiguration.Setup(a => a.GetSection("ApiSettings:Secret")).Returns(mockSection.Object);

            _authService = new AuthService(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockConfiguration.Object,
                _mockEmailService.Object
            );

            // Clear OTP storage between tests
            ClearOtpStorage();
        }

        /// <summary>
        /// Helper method to clear the static OTP storage.
        /// </summary>
        private void ClearOtpStorage()
        {
            var field = typeof(AuthService).GetField("otpStorage", BindingFlags.Static | BindingFlags.NonPublic);
            var dictionary = (ConcurrentDictionary<string, OTPModel>)field.GetValue(null);
            dictionary.Clear();
        }

        /// <summary>
        /// Helper method to set OTP in storage.
        /// </summary>
        private void SetOtpInStorage(string email, string otp, DateTime expiry)
        {
            var field = typeof(AuthService).GetField("otpStorage", BindingFlags.Static | BindingFlags.NonPublic);
            var dictionary = (ConcurrentDictionary<string, OTPModel>)field.GetValue(null);
            dictionary[email] = new OTPModel { OTP = otp, Expiry = expiry };
        }

        /// <summary>
        /// Helper method to set up a user by email.
        /// </summary>
        private void SetupUserRepositoryByEmail(string email, int userId, string currentPassword = "CurrentPass123!")
        {
            var user = new User
            {
                Id = userId,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(currentPassword)
            };

            _mockUnitOfWork.Setup(u => u.User.GetAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<string>()
            )).ReturnsAsync(user);
        }

        /// <summary>
        /// Helper method to set up a non-existent user.
        /// </summary>
        private void SetupNonExistentUser(string email)
        {
            _mockUnitOfWork.Setup(u => u.User.GetAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<string>()
            )).ReturnsAsync((User)null);
        }

        /// <summary>
        /// Helper method to simulate updating the user's password.
        /// </summary>
        private void SetupPasswordUpdate(string email, string newPassword)
        {
            // Assuming UpdateAsync returns Task
            _mockUnitOfWork.Setup(u => u.User.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            // Assuming SaveChangeAsync returns Task<int>
            _mockUnitOfWork.Setup(u => u.SaveChangeAsync()).Returns(Task.FromResult(1));
        }

        /// <summary>
        /// Helper method to create a user entity.
        /// </summary>
        private User CreateUser(int id, string username, string password, int roleId, string roleName, UserStatus status = UserStatus.Active)
        {
            return new User
            {
                Id = id,
                UserName = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                RoleId = roleId,
                Status = status
            };
        }

        /// <summary>
        /// Helper method to create a role entity.
        /// </summary>
        private Role CreateRole(int id, string roleName)
        {
            return new Role
            {
                Id = id,
                RoleName = roleName
            };
        }

        /// <summary>
        /// Helper method to set up user and role repositories.
        /// </summary>
        private void SetupUserAndRoleRepositories(string username, string password, int id, int roleId, string roleName, UserStatus status = UserStatus.Active)
        {
            var user = CreateUser(id, username, password, roleId, roleName, status);
            var role = CreateRole(roleId, roleName);

            // Setup User repository GetAsync
            _mockUnitOfWork.Setup(u => u.User.GetAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<string>()
            )).ReturnsAsync(user);

            // Setup Role repository GetAsync
            _mockUnitOfWork.Setup(u => u.Role.GetAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<Role, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<string>()
            )).ReturnsAsync(role);
        }

        #region Login Test Cases

        [Test]
        public async Task Login_Successful_ReturnsTokenAndUserId()
        {
            // Test Case 1: Đăng nhập thành công
            // Precondition: Can connect với server
            // Username: "manhd1"
            // Password: "Manh123@"
            // Expected Return: { Token = token, Id = user.Id }

            // Arrange
            var username = "manhd1";
            var password = "Manh123@";
            var userId = 1;
            var roleId = 1;
            var roleName = "User";

            SetupUserAndRoleRepositories(username, password, userId, roleId, roleName);

            var loginRequest = new LoginRequestDto
            {
                UserName = username,
                Password = password
            };

            // Act
            var result = await _authService.Login(loginRequest);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result.Token);
            Assert.AreEqual(userId, result.Id);
        }

        [Test]
        public async Task Login_UsernameCaseInsensitive_ReturnsTokenAndUserId()
        {
            // Test Case 2: Đăng nhập với username không phân biệt chữ hoa chữ thường
            // Precondition: Can connect với server
            // Username: "MANHD1"
            // Password: "Manh123@"
            // Expected Return: { Token = token, Id = user.Id }

            // Arrange
            var username = "manhd1";
            var password = "Manh123@";
            var inputUsername = "MANHD1";
            var userId = 1;
            var roleId = 1;
            var roleName = "User";

            SetupUserAndRoleRepositories(username, password, userId, roleId, roleName);

            var loginRequest = new LoginRequestDto
            {
                UserName = inputUsername,
                Password = password
            };

            // Act
            var result = await _authService.Login(loginRequest);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result.Token);
            Assert.AreEqual(userId, result.Id);
        }

        [Test]
        public async Task Login_UsernameDoesNotExist_ReturnsEmptyTokenAndZeroId()
        {
            // Test Case 3: Đăng nhập với username không tồn tại
            // Precondition: Can connect với server
            // Username: "notexistusername"
            // Password: "Manh123@"
            // Expected Return: { Token = "", Id = 0 }

            // Arrange
            var username = "notexistusername";
            var password = "Manh123@";

            // Setup User repository to return null
            SetupNonExistentUser(username);

            var loginRequest = new LoginRequestDto
            {
                UserName = username,
                Password = password
            };

            // Act
            var result = await _authService.Login(loginRequest);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result.Token);
            Assert.AreEqual(0, result.Id); // According to original code, Id = 0 when user not found
        }

        [Test]
        public async Task Login_AccountInactive_ReturnsEmptyTokenAndNegativeId()
        {
            // Test Case 4: Đăng nhập với tài khoản không hoạt động
            // Precondition: Can connect với server
            // Username: "manhd2"
            // Password: "Manh123@"
            // Expected Return: { Token = "", Id = -1 }

            // Arrange
            var username = "manhd2";
            var password = "Manh123@";
            var userId = 2;
            var roleId = 1;
            var roleName = "User";
            var status = UserStatus.DeActive; // Assuming "DeActive" represents inactive status

            SetupUserAndRoleRepositories(username, password, userId, roleId, roleName, status);

            var loginRequest = new LoginRequestDto
            {
                UserName = username,
                Password = password
            };

            // Act
            var result = await _authService.Login(loginRequest);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result.Token);
            Assert.AreEqual(-1, result.Id);
        }

        [Test]
        public async Task Login_WrongPassword_ReturnsEmptyTokenAndZeroId()
        {
            // Test Case 5: Đăng nhập với mật khẩu sai
            // Precondition: Can connect với server
            // Username: "manhd1"
            // Password: "WrongPassword"
            // Expected Return: { Token = "", Id = 0 }

            // Arrange
            var username = "manhd1";
            var correctPassword = "Manh123@";
            var wrongPassword = "WrongPassword";
            var userId = 1;
            var roleId = 1;
            var roleName = "User";

            SetupUserAndRoleRepositories(username, correctPassword, userId, roleId, roleName);

            var loginRequest = new LoginRequestDto
            {
                UserName = username,
                Password = wrongPassword
            };

            // Act
            var result = await _authService.Login(loginRequest);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result.Token);
            Assert.AreEqual(0, result.Id);
        }

        [Test]
        public async Task Login_PasswordCaseInsensitive_ReturnsEmptyTokenAndZeroId()
        {
            // Test Case 6: Đăng nhập với mật khẩu không chính xác, username hợp lệ
            // Precondition: Can connect với server
            // Username: "manhd1"
            // Password: "MANH123@"
            // Expected Return: { Token = "", Id = 0 }

            // Arrange
            var username = "manhd1";
            var correctPassword = "Manh123@";
            var wrongPassword = "MANH123@"; // Different case
            var userId = 1;
            var roleId = 1;
            var roleName = "User";

            SetupUserAndRoleRepositories(username, correctPassword, userId, roleId, roleName);

            var loginRequest = new LoginRequestDto
            {
                UserName = username,
                Password = wrongPassword
            };

            // Act
            var result = await _authService.Login(loginRequest);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result.Token);
            Assert.AreEqual(0, result.Id);
        }

        [Test]
        public async Task Login_CorrectPasswordInactiveUser_ReturnsEmptyTokenAndNegativeId()
        {
            // Test Case 7: Đăng nhập với mật khẩu đúng nhưng với username đã bị inactive
            // Precondition: Can connect với server
            // Username: "manhd2"
            // Password: "Manh123@"
            // Expected Return: { Token = "", Id = -1 }

            // Arrange
            var username = "manhd2";
            var password = "Manh123@";
            var userId = 2;
            var roleId = 1;
            var roleName = "User";
            var status = UserStatus.DeActive; // Inactive status

            SetupUserAndRoleRepositories(username, password, userId, roleId, roleName, status);

            var loginRequest = new LoginRequestDto
            {
                UserName = username,
                Password = password
            };

            // Act
            var result = await _authService.Login(loginRequest);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result.Token);
            Assert.AreEqual(-1, result.Id);
        }

        [Test]
        public async Task Login_EmptyUsername_ReturnsEmptyTokenAndZeroId()
        {
            // Test Case 8: Đăng nhập với ô Username trống
            // Precondition: Can connect với server
            // Username: ""
            // Password: "Manh123@"
            // Expected Return: { Token = "", Id = 0 }

            // Arrange
            var username = "";
            var password = "Manh123@";

            // Setup User repository to return null since username is empty
            SetupNonExistentUser(username);

            var loginRequest = new LoginRequestDto
            {
                UserName = username,
                Password = password
            };

            // Act
            var result = await _authService.Login(loginRequest);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result.Token);
            Assert.AreEqual(0, result.Id); // According to original code, Id = 0 when user not found
        }

        #endregion

        #region VerifyOTP Test Cases

        /// <summary>
        /// Test Case 1: Kiểm tra xác minh OTP với email hợp lệ và OTP đúng
        /// Precondition: OTP "1234" đã được gửi đến "dinhmanh3802@gmail.com" trong vòng 5 phút
        /// Expected: Return TRUE
        /// </summary>
        [Test]
        public async Task VerifyOTPAsync_ValidEmailAndCorrectOTP_ReturnsTrue()
        {
            // Arrange
            SetOtpInStorage("dinhmanh3802@gmail.com", "1234", DateTime.Now.AddMinutes(5));

            // Act
            var result = await _authService.VerifyOTPAsync("dinhmanh3802@gmail.com", "1234");

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Test Case 2: Kiểm tra xác minh OTP với email hợp lệ nhưng OTP sai
        /// Precondition: OTP "1234" đã được gửi đến "dinhmanh3802@gmail.com"
        /// Expected: Return FALSE
        /// </summary>
        [Test]
        public async Task VerifyOTPAsync_ValidEmailButWrongOTP_ReturnsFalse()
        {
            // Arrange
            SetOtpInStorage("dinhmanh3802@gmail.com", "1234", DateTime.Now.AddMinutes(5));

            // Act
            var result = await _authService.VerifyOTPAsync("dinhmanh3802@gmail.com", "0000");

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Test Case 3: Kiểm tra xác minh OTP với email không hợp lệ
        /// Precondition: OTP "1234" đã được gửi đến "dinhmanh3802@gmail.com"
        /// Email: abc@gmail.com, OTP: 1234
        /// Expected: Return FALSE
        /// </summary>
        [Test]
        public async Task VerifyOTPAsync_InvalidEmail_ReturnsFalse()
        {
            // Arrange
            SetOtpInStorage("dinhmanh3802@gmail.com", "1234", DateTime.Now.AddMinutes(5));

            // Act
            var result = await _authService.VerifyOTPAsync("abc@gmail.com", "1234");

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Test Case 4: Kiểm tra xác minh OTP khi email trống
        /// Precondition: OTP "1234" đã được gửi đến "dinhmanh3802@gmail.com"
        /// Email: "", OTP: "1234"
        /// Expected: Return FALSE
        /// </summary>
        [Test]
        public async Task VerifyOTPAsync_EmptyEmail_ReturnsFalse()
        {
            // Arrange
            SetOtpInStorage("dinhmanh3802@gmail.com", "1234", DateTime.Now.AddMinutes(5));

            // Act
            var result = await _authService.VerifyOTPAsync("", "1234");

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Test Case 5: Kiểm tra xác minh OTP khi OTP trống
        /// Precondition: OTP "1234" đã được gửi đến "dinhmanh3802@gmail.com"
        /// Email: "dinhmanh3802@gmail.com", OTP: ""
        /// Expected: Return FALSE
        /// </summary>
        [Test]
        public async Task VerifyOTPAsync_EmptyOTP_ReturnsFalse()
        {
            // Arrange
            SetOtpInStorage("dinhmanh3802@gmail.com", "1234", DateTime.Now.AddMinutes(5));

            // Act
            var result = await _authService.VerifyOTPAsync("dinhmanh3802@gmail.com", "");

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Test Case 6: Kiểm tra xác minh OTP khi OTP đã gửi hơn 5 phút (hết hạn)
        /// Precondition: OTP "1234" đã được gửi đến "dinhmanh3802@gmail.com" hơn 5 phút trước
        /// Email: "dinhmanh3802@gmail.com", OTP: "1234"
        /// Expected: Return FALSE
        /// </summary>
        [Test]
        public async Task VerifyOTPAsync_ExpiredOTP_ReturnsFalse()
        {
            // Arrange
            SetOtpInStorage("dinhmanh3802@gmail.com", "1234", DateTime.Now.AddMinutes(-1));

            // Act
            var result = await _authService.VerifyOTPAsync("dinhmanh3802@gmail.com", "1234");

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Test Case 7: Kiểm tra xác minh OTP với email dạng chữ hoa
        /// Precondition: OTP "1234" đã được gửi đến "dinhmanh3802@gmail.com"
        /// Email: "DINHMANH3802@GMAIL.COM", OTP: "1234"
        /// Expected: Return TRUE
        /// </summary>
        [Test]
        public async Task VerifyOTPAsync_UpperCaseEmail_ReturnsTrue()
        {
            // Arrange
            SetOtpInStorage("DINHMANH3802@GMAIL.COM", "1234", DateTime.Now.AddMinutes(5));

            // Act
            var result = await _authService.VerifyOTPAsync("DINHMANH3802@GMAIL.COM", "1234");

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Test Case 8: Kiểm tra xác minh OTP với email không khớp (OTP sai)
        /// Precondition: OTP "1234" đã được gửi đến "dinhmanh3802@gmail.com"
        /// Email: "dinhmanh3802@gmail.com", OTP: "0000"
        /// Expected: Return FALSE
        /// </summary>
        [Test]
        public async Task VerifyOTPAsync_CorrectEmailButWrongOTP_ReturnsFalse()
        {
            // Arrange
            SetOtpInStorage("dinhmanh3802@gmail.com", "1234", DateTime.Now.AddMinutes(5));

            // Act
            var result = await _authService.VerifyOTPAsync("dinhmanh3802@gmail.com", "0000");

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Test Case 9: Kiểm tra xác minh OTP was sent to "dinhmanh3802@gmail.com" more than 5 minutes ago
        /// Precondition: OTP '1234' was sent to "dinhmanh3802@gmail.com" more than 5 minutes ago
        /// but we verify with "dinhmanh3602@gmail.com"
        /// Expected: Return FALSE
        /// </summary>
        [Test]
        public async Task VerifyOTPAsync_DifferentEmailAfterExpiredTime_ReturnsFalse()
        {
            // Arrange
            SetOtpInStorage("dinhmanh3802@gmail.com", "1234", DateTime.Now.AddMinutes(-10));

            // Act
            var result = await _authService.VerifyOTPAsync("dinhmanh3602@gmail.com", "1234");

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region ResetPassword Test Cases

        /// <summary>
        /// Test Case 1: Đặt lại mật khẩu với userId hợp lệ và mật khẩu hợp lệ
        /// Precondition: Có thể kết nối với server.
        /// userId: 1
        /// password: Password123!
        /// Kết quả kỳ vọng: Reset Password Successfully.
        /// </summary>
        [Test]
        public async Task ResetPasswordAsync_ValidEmailAndValidPassword_ReturnsTrue()
        {
            // Arrange
            string email = "dinhmanh3802@gmail.com";
            int userId = 1;
            string newPassword = "Password123!";
            SetupUserRepositoryByEmail(email, userId);
            SetupPasswordUpdate(email, newPassword);

            // Act
            var result = await _authService.ResetPasswordAsync(email, newPassword);

            // Assert
            Assert.IsTrue(result, "Password should be reset successfully.");
            _mockUnitOfWork.Verify(u => u.User.UpdateAsync(It.Is<User>(usr =>
                usr.Email == email && BCrypt.Net.BCrypt.Verify(newPassword, usr.PasswordHash))), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangeAsync(), Times.Once);
        }

        /// <summary>
        /// Test Case 2: Đặt lại mật khẩu với userId không hợp lệ (99)
        /// Precondition: Có thể kết nối với server.
        /// userId: 99
        /// password: Password123!
        /// Kết quả kỳ vọng: Return FALSE.
        /// </summary>
        [Test]
        public async Task ResetPasswordAsync_InvalidEmail_ReturnsFalse()
        {
            // Arrange
            string email = "nonexistentuser@gmail.com";
            string newPassword = "Password123!";
            SetupNonExistentUser(email);

            // Act
            var result = await _authService.ResetPasswordAsync(email, newPassword);

            // Assert
            Assert.IsFalse(result, "Reset should fail for non-existent user.");
            _mockUnitOfWork.Verify(u => u.User.UpdateAsync(It.IsAny<User>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveChangeAsync(), Times.Never);
        }

        /// <summary>
        /// Test Case 3: Đặt lại mật khẩu với userId hợp lệ và mật khẩu null
        /// Precondition: Có thể kết nối với server.
        /// userId: 1
        /// password: null
        /// Kết quả kỳ vọng: ArgumentException (Mật khẩu phải có ít nhất 8 ký tự.)
        /// </summary>
        [Test]
        public void ResetPasswordAsync_ValidEmailButNullPassword_ThrowsArgumentException()
        {
            // Arrange
            string email = "dinhmanh3802@gmail.com";
            int userId = 1;
            string newPassword = "";
            SetupUserRepositoryByEmail(email, userId);

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _authService.ResetPasswordAsync(email, newPassword));

            Assert.AreEqual("Mật khẩu phải có ít nhất 8 ký tự.", ex.Message);
            _mockUnitOfWork.Verify(u => u.User.UpdateAsync(It.IsAny<User>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveChangeAsync(), Times.Never);
        }



        /// <summary>
        /// Test Case 4: Đặt lại mật khẩu với userId hợp lệ và mật khẩu không chứa ký tự hoa
        /// Precondition: Có thể kết nối với server.
        /// userId: 1
        /// password: pass1234
        /// Kết quả kỳ vọng: ArgumentException (Mật khẩu phải có ít nhất 1 chữ hoa.)
        /// </summary>
        [Test]
        public void ResetPasswordAsync_ValidEmailButNoUpperCasePassword_ThrowsArgumentException()
        {
            // Arrange
            string email = "dinhmanh3802@gmail.com";
            int userId = 1;
            string newPassword = "pass1234";
            SetupUserRepositoryByEmail(email, userId);

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _authService.ResetPasswordAsync(email, newPassword));

            Assert.AreEqual("Mật khẩu phải có ít nhất 1 chữ hoa.", ex.Message);
            _mockUnitOfWork.Verify(u => u.User.UpdateAsync(It.IsAny<User>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveChangeAsync(), Times.Never);
        }

        /// <summary>
        /// Test Case 5: Đặt lại mật khẩu với userId hợp lệ và mật khẩu không chứa ký tự số
        /// Precondition: Có thể kết nối với server.
        /// userId: 1
        /// password: Abcdefgh
        /// Kết quả kỳ vọng: ArgumentException (Mật khẩu phải có ít nhất 1 số.)
        /// </summary>
        [Test]
        public void ResetPasswordAsync_ValidEmailButNoNumberPassword_ThrowsArgumentException()
        {
            // Arrange
            string email = "dinhmanh3802@gmail.com";
            int userId = 1;
            string newPassword = "Abcdefgh";
            SetupUserRepositoryByEmail(email, userId);

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _authService.ResetPasswordAsync(email, newPassword));

            Assert.AreEqual("Mật khẩu phải có ít nhất 1 số.", ex.Message);
            _mockUnitOfWork.Verify(u => u.User.UpdateAsync(It.IsAny<User>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveChangeAsync(), Times.Never);
        }

        /// <summary>
        /// Test Case 6: Đặt lại mật khẩu với userId hợp lệ và mật khẩu không đủ dài (dưới 8 ký tự)
        /// Precondition: Có thể kết nối với server.
        /// userId: 1
        /// password: Abc123
        /// Kết quả kỳ vọng: ArgumentException (Mật khẩu phải có ít nhất 8 ký tự.)
        /// </summary>
        [Test]
        public void ResetPasswordAsync_ValidEmailButShortPassword_ThrowsArgumentException()
        {
            // Arrange
            string email = "dinhmanh3802@gmail.com";
            int userId = 1;
            string newPassword = "Abc123";
            SetupUserRepositoryByEmail(email, userId);

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _authService.ResetPasswordAsync(email, newPassword));

            Assert.AreEqual("Mật khẩu phải có ít nhất 8 ký tự.", ex.Message);
            _mockUnitOfWork.Verify(u => u.User.UpdateAsync(It.IsAny<User>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveChangeAsync(), Times.Never);
        }

        /// <summary>
        /// Test Case 7: Đặt lại mật khẩu với userId hợp lệ và mật khẩu không chứa ký tự đặc biệt
        /// Precondition: Có thể kết nối với server.
        /// userId: 1
        /// password: Abcdefgh1
        /// Kết quả kỳ vọng: ArgumentException (Mật khẩu phải có ít nhất 1 ký tự đặc biệt.)
        /// </summary>
        [Test]
        public void ResetPasswordAsync_ValidEmailButNoSpecialCharPassword_ThrowsArgumentException()
        {
            // Arrange
            string email = "dinhmanh3802@gmail.com";
            int userId = 1;
            string newPassword = "Abcdefgh1";
            SetupUserRepositoryByEmail(email, userId);

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _authService.ResetPasswordAsync(email, newPassword));

            Assert.AreEqual("Mật khẩu phải có ít nhất 1 ký tự đặc biệt.", ex.Message);
            _mockUnitOfWork.Verify(u => u.User.UpdateAsync(It.IsAny<User>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveChangeAsync(), Times.Never);
        }

        /// <summary>
        /// Test Case 8: Đặt lại mật khẩu với userId hợp lệ và mật khẩu không chứa ký tự đặc biệt (duplicate of Test Case 7)
        /// Precondition: Có thể kết nối với server.
        /// userId: 1
        /// password: Password1
        /// Kết quả kỳ vọng: ArgumentException (Mật khẩu phải có ít nhất 1 ký tự đặc biệt.)
        /// </summary>
        [Test]
        public void ResetPasswordAsync_ValidEmailButNoSpecialCharPasswordVariant_ThrowsArgumentException()
        {
            // Arrange
            string email = "dinhmanh3802@gmail.com";
            int userId = 1;
            string newPassword = "Password1"; // No special character
            SetupUserRepositoryByEmail(email, userId);

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _authService.ResetPasswordAsync(email, newPassword));

            Assert.AreEqual("Mật khẩu phải có ít nhất 1 ký tự đặc biệt.", ex.Message);
            _mockUnitOfWork.Verify(u => u.User.UpdateAsync(It.IsAny<User>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveChangeAsync(), Times.Never);
        }

        /// <summary>
        /// Test Case 9: Đặt lại mật khẩu với userId hợp lệ và mật khẩu trống
        /// Precondition: Có thể kết nối với server.
        /// userId: 1
        /// password: ""
        /// Kết quả kỳ vọng: ArgumentException (Mật khẩu phải có ít nhất 8 ký tự.)
        /// </summary>
        [Test]
        public void ResetPasswordAsync_ValidEmailButEmptyPassword_ThrowsArgumentException()
        {
            // Arrange
            string email = "dinhmanh3802@gmail.com";
            int userId = 1;
            string newPassword = "";
            SetupUserRepositoryByEmail(email, userId);

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _authService.ResetPasswordAsync(email, newPassword));

            Assert.AreEqual("Mật khẩu phải có ít nhất 8 ký tự.", ex.Message);
            _mockUnitOfWork.Verify(u => u.User.UpdateAsync(It.IsAny<User>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveChangeAsync(), Times.Never);
        }

        /// <summary>
        /// Test Case 10: Đặt lại mật khẩu với userId hợp lệ và mật khẩu phù hợp tất cả các điều kiện
        /// Precondition: Có thể kết nối với server.
        /// userId: 1
        /// password: Abcdefg1!
        /// Kết quả kỳ vọng: Reset Password Successfully.
        /// </summary>
        [Test]
        public async Task ResetPasswordAsync_ValidEmailAndStrongPassword_ReturnsTrue()
        {
            // Arrange
            string email = "dinhmanh3802@gmail.com";
            int userId = 1;
            string newPassword = "Abcdefg1!";
            SetupUserRepositoryByEmail(email, userId);
            SetupPasswordUpdate(email, newPassword);

            // Act
            var result = await _authService.ResetPasswordAsync(email, newPassword);

            // Assert
            Assert.IsTrue(result, "Password should be reset successfully with strong password.");
            _mockUnitOfWork.Verify(u => u.User.UpdateAsync(It.Is<User>(usr =>
                usr.Email == email && BCrypt.Net.BCrypt.Verify(newPassword, usr.PasswordHash))), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangeAsync(), Times.Once);
        }

        /// <summary>
        /// Test Case 11: Đặt lại mật khẩu với userId hợp lệ và mật khẩu chứa khoảng trắng
        /// Precondition: Có thể kết nối với server.
        /// userId: 1
        /// password: "Pass word1!"
        /// Kết quả kỳ vọng: ArgumentException (Mật khẩu phải có ít nhất 1 ký tự đặc biệt.
        /// Note: Ensure that the ValidatePassword method in AuthService includes this validation.
        /// </summary>
        [Test]
        public void ResetPasswordAsync_ValidEmailButPasswordWithSpaces_ThrowsArgumentException()
        {
            // Arrange
            string email = "dinhmanh3802@gmail.com";
            int userId = 1;
            string newPassword = "Password1";
            SetupUserRepositoryByEmail(email, userId);
                
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _authService.ResetPasswordAsync(email, newPassword));

            Assert.AreEqual("Mật khẩu phải có ít nhất 1 ký tự đặc biệt.", ex.Message);
            _mockUnitOfWork.Verify(u => u.User.UpdateAsync(It.IsAny<User>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveChangeAsync(), Times.Never);
        }


        #endregion
    }
}
