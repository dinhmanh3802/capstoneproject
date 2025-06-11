using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SCCMS.Domain.DTOs.Auth.ForgotPassword;
using SCCMS.Domain.DTOs.Auth.Login;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Context;
using SCCMS.Infrastucture.Entities;
using System.Net;

namespace SCCMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private ApiResponse _response;


        public AuthController(IAuthService authService)
        {
            _response = new ApiResponse();
          _authService = authService;
            
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto model)
        {
            var loginResponse = await _authService.Login(model);

            if (loginResponse.Id == -1) // Account is disabled
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Tài khoản đã bị vô hiệu hóa.");
                return BadRequest(_response);
            }

            if (loginResponse.Id == 0 || string.IsNullOrEmpty(loginResponse.Token))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Tên đăng nhập hoặc mật khẩu không đúng.");
                return BadRequest(_response);
            }

            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = loginResponse;
            return Ok(_response);
        }


        [HttpPost("request-otp")]
        public async Task<IActionResult> RequestOTP([FromBody] OTPRequestDto model)
        {
            var result = await _authService.SendOTPAsync(model.Email);
            if (!result)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Không thể gửi OTP.");
                return BadRequest(_response);
            }
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = "OTP đã được gửi.";
            return Ok(_response);
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOTP([FromBody] OTPVerifyDto model)
        {
            var isValid = await _authService.VerifyOTPAsync(model.Email, model.OTP);
            if (!isValid)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("OTP không hợp lệ.");
                return BadRequest(_response);
            }
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            var result = await _authService.ResetPasswordAsync(model.Email, model.NewPassword);
            if (!result)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Không thể đặt lại mật khẩu.");
                return BadRequest(_response);
            }
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = "Đặt lại mật khẩu thành công.";
            return Ok(_response);
        }

    }
}
