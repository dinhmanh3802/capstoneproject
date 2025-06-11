using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SCCMS.API.Services;
using SCCMS.Domain.DTOs.Auth.ForgotPassword;
using SCCMS.Domain.DTOs.Auth.Login;
using SCCMS.Domain.DTOs.UserDtos;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.UnitOfWork;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Timers;

namespace SCCMS.Domain.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly string secretKey;
        private static ConcurrentDictionary<string, OTPModel> otpStorage = new();
        private static System.Timers.Timer otpCleanupTimer; // Khai báo Timer

        public AuthService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration, IEmailService emailService)
        {
            secretKey = configuration.GetSection("ApiSettings:Secret").Value;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;

            // Khởi tạo Timer chỉ một lần
            if (otpCleanupTimer == null)
            {
                otpCleanupTimer = new System.Timers.Timer(300000); // 300000 milliseconds = 5 minutes
                otpCleanupTimer.Elapsed += CleanupExpiredOTPs;
                otpCleanupTimer.AutoReset = true; // Thực hiện lặp lại
                otpCleanupTimer.Enabled = true; // Bật Timer
            }
        }

        public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDTO)
        {
            var user = await _unitOfWork.User.GetAsync(u => u.UserName.ToLower() == loginRequestDTO.UserName.ToLower());

            if (user == null)
            {
                return new LoginResponseDto() { Token = "", Id = 0 };
            }

            // Check if the account is disabled
            if (user.Status == Utility.UserStatus.DeActive)
            {
                return new LoginResponseDto() { Token = "", Id = -1 };
            }

            bool isValid = BCrypt.Net.BCrypt.Verify(loginRequestDTO.Password, user.PasswordHash);

            if (!isValid)
            {
                return new LoginResponseDto() { Token = "", Id = 0 };
            }

            // Generate JWT Token
            var role = await _unitOfWork.Role.GetAsync(x => x.Id == user.RoleId);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim("userId", user.Id.ToString()),
            new Claim("username", user.UserName),
            new Claim(ClaimTypes.Role, role.RoleName),
            new Claim("roleId", role.Id.ToString())
                }),
                Expires = DateTime.Now.AddHours(240),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new LoginResponseDto() { Token = tokenHandler.WriteToken(token), Id = user.Id };
        }


        public async Task<bool> SendOTPAsync(string email)
        {
            var user = await _unitOfWork.User.GetAsync(u => u.Email == email);
            if (user == null) return false;

            var otp = new Random().Next(1000, 9999).ToString();
            otpStorage[email] = new OTPModel { OTP = otp, Expiry = DateTime.Now.AddMinutes(5)};

            // Tạo nội dung email
            var parameters = new Dictionary<string, string>
            {
                { "FullName", user.FullName },
                { "OTP", otp },
            };
            await _emailService.SendEmailAsync(user.Email, "SendOTPForResetPassword", parameters);
            return true;
        }

        public async Task<bool> VerifyOTPAsync(string email, string otp)
        {
            if (!otpStorage.TryGetValue(email, out OTPModel otpModel))
            {
                return false; // OTP không tồn tại
            }

            // Kiểm tra hạn sử dụng
            if (otpModel.Expiry < DateTime.Now)
            {
                otpStorage.TryRemove(email, out _); // Xóa OTP nếu đã hết hạn
                return false;
            }

            // Kiểm tra OTP
            if (otpModel.OTP != otp)
            {
                return false; // OTP không hợp lệ
            }

            otpStorage.TryRemove(email, out _); // Xóa OTP khi xác minh thành công
            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            var user = await _unitOfWork.User.GetAsync(u => u.Email == email);
            if (user == null) return false;

            // Validate mật khẩu mới
            var passwordError = ValidatePassword(newPassword);
            if (!string.IsNullOrEmpty(passwordError))
            {
                throw new ArgumentException(passwordError);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _unitOfWork.User.UpdateAsync(user);
            await _unitOfWork.SaveChangeAsync();
            return true;
        }

        private void CleanupExpiredOTPs(object sender, ElapsedEventArgs e)
        {
            var expiredEmails = otpStorage
                .Where(kvp => kvp.Value.Expiry < DateTime.Now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var email in expiredEmails)
            {
                otpStorage.TryRemove(email, out _);
            }
        }

        // Thêm phương thức ValidatePassword
        private string ValidatePassword(string password)
        {
            const int minLength = 8;
            var hasUpperCase = password.Any(char.IsUpper);
            var hasLowerCase = password.Any(char.IsLower);
            var hasNumbers = password.Any(char.IsDigit);
            var hasSpecialChars = password.Any(ch => "!@#$%^&*(),.?\":{}|<>".Contains(ch));

            if (password.Length < minLength)
            {
                return "Mật khẩu phải có ít nhất 8 ký tự.";
            }
            if (!hasUpperCase)
            {
                return "Mật khẩu phải có ít nhất 1 chữ hoa.";
            }
            if (!hasLowerCase)
            {
                return "Mật khẩu phải có ít nhất 1 chữ thường.";
            }
            if (!hasNumbers)
            {
                return "Mật khẩu phải có ít nhất 1 số.";
            }
            if (!hasSpecialChars)
            {
                return "Mật khẩu phải có ít nhất 1 ký tự đặc biệt.";
            }
            return string.Empty;
        }
    }
}
