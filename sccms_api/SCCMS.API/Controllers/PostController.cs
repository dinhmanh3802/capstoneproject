using Microsoft.AspNetCore.Mvc;
using SCCMS.Domain.DTOs.PostDtos;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using System.Net;
using Utility;

namespace SCCMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        protected ApiResponse _response;

        public PostController(IPostService postService)
        {
            _postService = postService;
            _response = new ApiResponse();
        }

        // Route: /api/post
        [HttpGet]
        public async Task<IActionResult> GetAllPosts([FromQuery] string? title,
                                                     [FromQuery] string? content,
                                                     [FromQuery] DateTime? postDateStart,
                                                     [FromQuery] DateTime? postDateEnd,
                                                     [FromQuery] PostStatus? status,
                                                     [FromQuery] PostType? postType,
                                                     [FromQuery] int? createdBy, 
                                                     int pageNumber, int pageSize)
        {
            var posts = await _postService.GetAllPostsAsync(title, content, postDateStart, postDateEnd, status, postType, createdBy, pageNumber, pageSize);
            _response.Result = posts;
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }

        // Route: /api/post/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostById(int id)
        {
            var post = await _postService.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            _response.Result = post;
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }

        // Route: /api/post
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromForm] PostCreateDto postCreateDto)
        {
            try
            {
                await _postService.CreatePostAsync(postCreateDto);

                _response.StatusCode = HttpStatusCode.Created;
                _response.IsSuccess = true;
                _response.Result = "create success";

                return StatusCode((int)HttpStatusCode.Created, _response);
            }
            catch (ArgumentException ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);

                return StatusCode((int)HttpStatusCode.BadRequest, _response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("InternalServerError");
                _response.ErrorMessages.Add(ex.Message);
                _response.ErrorMessages.Add(ex.InnerException?.ToString());

                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }

        // Route: /api/post/{postId}
        [HttpPut("{postId}")]
        public async Task<IActionResult> UpdatePost(int postId, [FromForm] PostUpdateDto postUpdateDto)
        {
            try
            {
                await _postService.UpdatePostAsync(postId, postUpdateDto);

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = "update success";

                return StatusCode((int)HttpStatusCode.OK, _response);
            }
            catch (ArgumentException ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("invalid data");
                _response.ErrorMessages.Add(ex.Message);

                return StatusCode((int)HttpStatusCode.BadRequest, _response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("InternalServerError");
                _response.ErrorMessages.Add(ex.Message);
                _response.ErrorMessages.Add(ex.InnerException?.ToString());

                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }

        // Route: /api/post/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            try
            {
                await _postService.DeletePostAsync(id);

                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                _response.Result = "delete success";

                return StatusCode((int)HttpStatusCode.OK, _response);
            }
            catch (ArgumentException ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);

                return StatusCode((int)HttpStatusCode.BadRequest, _response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("InternalServerError");
                _response.ErrorMessages.Add(ex.Message);
                _response.ErrorMessages.Add(ex.InnerException?.ToString());

                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }

    }
}
