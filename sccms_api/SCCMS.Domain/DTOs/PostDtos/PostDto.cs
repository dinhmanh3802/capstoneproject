using SCCMS.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.DTOs.PostDtos
{
    public class PostDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [DataType(DataType.DateTime)]
        public string Image { get; set; }

        public PostType PostType { get; set; }

        public PostStatus Status { get; set; }


        public string UserCreated { get; set; }

        public string UserUpdated { get; set; }


        [DataType(DataType.Date)]
        public DateTime DateCreated { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateModified { get; set; }
    }
}
