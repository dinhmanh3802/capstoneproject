using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SCCMS.Domain.DTOs.PostDtos;
using SCCMS.Domain.DTOs.StudentDtos;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.Services.Implements
{
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IBlobService _blobService;

        public PostService(IUnitOfWork unitOfWork, IBlobService blobService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _blobService = blobService;
        }
        public async Task CreatePostAsync(PostCreateDto postCreateDto)
        {


            // Ràng buộc 1: Tiêu đề bài viết không được trống và phải có độ dài dưới 255 ký tự
            if (string.IsNullOrWhiteSpace(postCreateDto.Title) || postCreateDto.Title.Length > 255)
            {
                throw new ArgumentException("Title is required and must not exceed 255 characters.");
            }

            // Ràng buộc 2: Nội dung bài viết không được trống
            if (string.IsNullOrWhiteSpace(postCreateDto.Content))
            {
                throw new ArgumentException("Content is required.");
            }
            Post post = _mapper.Map<Post>(postCreateDto);
            string fileNameImage = $"{Guid.NewGuid()}{Path.GetExtension(postCreateDto.Image.FileName)}";
            post.Image = await _blobService.UploadBlob(fileNameImage, SD.Storage_Container, postCreateDto.Image);

            //TOOO: Người chỉnh sửa, người tạo
            post.DateCreated = DateTime.Now;
            post.DateModified = DateTime.Now;
            post.Status = PostStatus.Draft;
            // Thêm bài đăng vào cơ sở dữ liệu
            await _unitOfWork.Post.AddAsync(post);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task<IEnumerable<PostDto>> GetAllPostsAsync(
            string? title = null,
            string? content = null,
            DateTime? postDateStart = null,
            DateTime? postDateEnd = null,
            PostStatus? status = PostStatus.Active,
            PostType? postType = null,
            int? createdBy = null,
            int pageNumber = 0,
            int pageSize = 10)
        {
            // Loại bỏ dấu cách dư thừa từ các tham số đầu vào
            title = title?.Trim();
            content = content?.Trim();

            // Gọi hàm FindPagedAsync và truyền điều kiện lọc với hỗ trợ tìm kiếm không dấu
            var posts = await _unitOfWork.Post.FindPagedAsync(p =>
                (string.IsNullOrEmpty(title) ||
                    EF.Functions.Collate(p.Title, "Latin1_General_CI_AI").Contains(title)) &&
                (string.IsNullOrEmpty(content) ||
                    EF.Functions.Collate(p.Content, "Latin1_General_CI_AI").Contains(content)) &&
                (!postDateStart.HasValue || p.DateCreated.Date >= postDateStart.Value.Date) &&
                (!postDateEnd.HasValue || p.DateCreated.Date <= postDateEnd.Value.Date) &&
                (status == null || p.Status == status) &&
                (postType == null || p.PostType == postType) &&
                (createdBy == null || p.CreatedBy == createdBy),
                pageNumber, pageSize);

            if (posts == null || !posts.Any())
            {
                return new List<PostDto>();
            }

            // Sắp xếp theo trạng thái và ngày chỉnh sửa
            posts = posts.OrderBy(p => p.Status == PostStatus.Draft ? 0 :
                                       p.Status == PostStatus.Active ? 1 : 2)
                         .ThenByDescending(p => p.DateModified);

            // Ánh xạ kết quả thành DTO
            var postDtos = _mapper.Map<IEnumerable<PostDto>>(posts);

            return postDtos;
        }



        public async Task DeletePostAsync(int id)
        {
            Post post = await _unitOfWork.Post.GetByIdAsync(id);
            if (post == null)
            {
                throw new ArgumentException("Post does not exist.");
            }

            // Xóa cứng bài đăng khỏi cơ sở dữ liệu
            _unitOfWork.Post.DeleteAsync(post);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task UpdatePostAsync(int postId, PostUpdateDto postUpdateDto)
        {
          
            // Tìm kiếm bài đăng hiện có theo ID
            var existingPost = await _unitOfWork.Post.GetByIdAsync(postId);
            if (existingPost == null)
            {
                throw new ArgumentException("Post not found.");
            }
            string imageUrl = existingPost.Image;



            // Ràng buộc 2: Tiêu đề bài viết không được trống và phải có độ dài dưới 255 ký tự
            if (string.IsNullOrWhiteSpace(postUpdateDto.Title) || postUpdateDto.Title.Length > 255)
            {
                throw new ArgumentException("Title is required and must not exceed 255 characters.");
            }

            // Ràng buộc 3: Nội dung bài viết không được trống
            if (string.IsNullOrWhiteSpace(postUpdateDto.Content))
            {
                throw new ArgumentException("Content is required.");
            }
            _mapper.Map(postUpdateDto, existingPost);
            // Kiểm tra và xử lý hình ảnh nếu có ảnh mới
            if (postUpdateDto.Image != null)
            {
                // Xóa hình ảnh cũ
                await _blobService.DeleteBlob(existingPost.Image.Split('/').Last(), SD.Storage_Container);

                // Tạo tên file mới cho hình ảnh và tải lên
                string fileNameImage = $"{Guid.NewGuid()}{Path.GetExtension(postUpdateDto.Image.FileName)}";
                existingPost.Image = await _blobService.UploadBlob(fileNameImage, SD.Storage_Container, postUpdateDto.Image);
            }
            
            // Ánh xạ DTO vào thực thể bài viết và cập nhật ngày sửa
           

            existingPost.DateModified = DateTime.Now;
            if (postUpdateDto.Image == null)
            {
                existingPost.Image = imageUrl;
            }
            existingPost.DateModified = DateTime.Now;

            // Cập nhật bài viết
            await _unitOfWork.Post.UpdateAsync(existingPost);
            await _unitOfWork.SaveChangeAsync();
        }


        public async Task<IEnumerable<PostDto>> GetPostsAsync()
        {
            var posts = await _unitOfWork.Post.GetAllAsync();
            // Sắp xếp các bài viết theo modifiedDate giảm dần
            posts = posts.OrderByDescending(post => post.DateModified);
            var postDtos = new List<PostDto>();
            postDtos = _mapper.Map<List<PostDto>>(posts);
            return postDtos;
        }

        public async Task<PostDto?> GetPostByIdAsync(int id)
        {
            var post = await _unitOfWork.Post.GetByIdAsync(id);

            if (post == null)
            {
                return null;
            }
            var UserCreated = await _unitOfWork.User.GetByIdAsync(post.CreatedBy);
            var UserUpdated = await _unitOfWork.User.GetByIdAsync(post.UpdatedBy);

            var postDto = _mapper.Map<PostDto>(post);
            postDto.UserCreated = UserCreated.UserName;
            postDto.UserUpdated = UserUpdated.UserName;

            return postDto;
        }

    }
}
