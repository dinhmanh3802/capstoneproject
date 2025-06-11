using SCCMS.Domain.DTOs.PostDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.Services.Interfaces
{
    public interface IPostService
    {
        // Lấy danh sách các bài đăng với các tiêu chí tìm kiếm
        Task<IEnumerable<PostDto>> GetAllPostsAsync(string? title = null,
                                                    string? content = null,
                                                    DateTime? postDateStart = null,
                                                    DateTime? postDateEnd = null,
                                                    PostStatus? status = null,
                                                    PostType? postType = null,
                                                    int? createdBy = null,
                                                    int pageNumber = 0, 
                                                    int pageSize = 0);

        // Lấy thông tin chi tiết của một bài đăng theo ID
        Task<PostDto?> GetPostByIdAsync(int id);

        // Tạo mới bài đăng
        Task CreatePostAsync(PostCreateDto postCreateDto);

        // Cập nhật bài đăng theo ID
        Task UpdatePostAsync(int postId, PostUpdateDto postUpdateDto);

        // Xóa bài đăng theo ID
        Task DeletePostAsync(int id);
    }
}
