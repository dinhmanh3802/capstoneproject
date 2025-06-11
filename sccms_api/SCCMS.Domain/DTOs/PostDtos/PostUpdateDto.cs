using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.DTOs.PostDtos
{
    public class PostUpdateDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }
        public IFormFile? Image { get; set; }

        public PostType? PostType { get; set; }

        public PostStatus? Status { get; set; }
    }
}
