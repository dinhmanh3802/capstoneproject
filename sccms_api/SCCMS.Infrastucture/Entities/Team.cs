using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Infrastucture.Entities
{
    public class Team : BaseEntity
    {
        public int Id { get; set; }

        public int? CourseId { get; set; }  
        public Course Course { get; set; }

        public int LeaderId { get; set; }
        public User? Leader { get; set; }

        [Required]
        [StringLength(100)]
        public string TeamName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public Gender? Gender { get; set; }  

        public int ExpectedVolunteers { get; set; }
        public ICollection<VolunteerTeam>? VolunteerTeam { get; set; }
    }
}
