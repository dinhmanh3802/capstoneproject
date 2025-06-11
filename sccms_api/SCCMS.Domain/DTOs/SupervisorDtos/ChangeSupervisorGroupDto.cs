using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.DTOs.SupervisorDtos
{
    public class ChangeSupervisorGroupsDto
    {
        public List<int> SupervisorIds { get; set; }
        public int NewGroupId { get; set; }
    }
}
