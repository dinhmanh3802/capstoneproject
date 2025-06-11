using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.DTOs.UserDtos
{
    public class BulkCreateUsersResultDto
    {
        public bool HasErrors { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
