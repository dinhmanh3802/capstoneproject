using SCCMS.Domain.DTOs.Auth.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> Login(LoginRequestDto loginRequestDTO);
        Task<bool> SendOTPAsync(string email);
        Task<bool> VerifyOTPAsync(string email, string otp);
        Task<bool> ResetPasswordAsync(string email, string newPassword);

    }
}
