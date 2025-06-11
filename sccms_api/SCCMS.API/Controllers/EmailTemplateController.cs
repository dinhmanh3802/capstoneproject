using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using System.Net;

namespace SCCMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailTemplateController : ControllerBase
    {
        private readonly IEmailTemplateService _emailTemplateService;
        protected ApiResponse _response;

        public EmailTemplateController(IEmailTemplateService emailTemplateService)
        {
            _emailTemplateService = emailTemplateService;
            _response = new ApiResponse();
        }

        [HttpGet("name/{emailTemplateName}")]
        public async Task<IActionResult> GetEmailTemplateByNameAsync(string emailTemplateName)
        {
            var emailTemplate = await _emailTemplateService.GetTemplateByNameAsync(emailTemplateName);
            _response.Result = emailTemplate;
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmailTemplate(int id)
        {
            var emailTemplate = await _emailTemplateService.GetTemplate(id);
            _response.Result = emailTemplate;
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmailTemplate()
        {
            var emailTemplates = await _emailTemplateService.GetAllEmailTemplateAsync();
            _response.Result = emailTemplates;
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }
    }
}
