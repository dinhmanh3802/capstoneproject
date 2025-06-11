using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.DTOs.UserDtos
{
    public class BulkChangeRoleDto
    {
        public List<int> UserIds { get; set; }
        public int NewRoleId { get; set; }
    }
}
