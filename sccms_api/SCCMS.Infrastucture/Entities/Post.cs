using System.ComponentModel.DataAnnotations;
using Utility;

namespace SCCMS.Infrastucture.Entities
{
    public class Post : BaseEntity
    {
        public int Id { get; set; }
        [Required]
        [StringLength(255)]
        public string Title { get; set; }
        [Required]
        public string Image { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public PostType PostType { get; set; }
        [Required]
        public PostStatus Status { get; set; }

    }
}
