using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SCCMS.API.Services;
using SCCMS.Domain.DTOs.EmailDtos;
using SCCMS.Infrastucture.Entities;
using System.Net;

namespace SCCMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private ApiResponse _response;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
            _response = new ApiResponse();
        }

        [HttpPost()]
        public IActionResult SendEmails([FromBody] SendBulkEmailRequestDto model)
        {
            try
            {
                _emailService.SendBulkEmailAsync(model);

                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = "Quá trình gửi email đã bắt đầu.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add($"Lỗi khi bắt đầu quá trình gửi email: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, _response);
            }
        }
    }
}
