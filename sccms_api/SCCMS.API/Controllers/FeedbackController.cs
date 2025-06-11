using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SCCMS.Domain.DTOs.FeedbackDtos;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using System.Net;

namespace SCCMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFeedbacks([FromQuery] int courseId,
                                                         [FromQuery] DateTime? feedbackDateStart,
                                                         [FromQuery] DateTime? feedbackDateEnd)
        {
            try
            {
                var feedbacks = await _feedbackService.GetAllFeedbacksAsync(courseId, feedbackDateStart, feedbackDateEnd);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, feedbacks));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message, ex.InnerException?.Message }));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFeedbackById(int id)
        {
            try
            {
                var feedback = await _feedbackService.GetFeedbackByIdAsync(id);
                if (feedback == null)
                {
                    return NotFound(new ApiResponse(HttpStatusCode.NotFound, false, null));
                }
                return Ok(new ApiResponse(HttpStatusCode.OK, true, feedback));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateFeedback([FromBody] FeedbackCreateDto feedbackCreateDto)
        {
            if (feedbackCreateDto == null)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, "Feedback rỗng."));
            }

            try
            {
                await _feedbackService.CreateFeedbackAsync(feedbackCreateDto);
                return StatusCode((int)HttpStatusCode.Created, new ApiResponse(HttpStatusCode.Created, true, "Gửi feedback thành công"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            try
            {
                await _feedbackService.DeleteFeedbackAsync(id);
                return StatusCode((int)HttpStatusCode.OK, new ApiResponse(HttpStatusCode.NoContent, true, "delete success"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }

        [HttpDelete("bulk-delete")]
        public async Task<IActionResult> DeleteFeedbacksByIds([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, "Feedback is null."));
            }

            try
            {
                await _feedbackService.DeleteFeedbacksByIdsAsync(ids);
                return StatusCode((int)HttpStatusCode.OK, new ApiResponse(HttpStatusCode.NoContent, true, "delete success"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse(HttpStatusCode.InternalServerError, false, new List<string> { ex.Message }));
            }
        }
    }
}

