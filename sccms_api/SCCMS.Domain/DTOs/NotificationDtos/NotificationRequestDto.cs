using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.DTOs.NotificationDtos
{
    public class NotificationRequestDTO
    {
        public int UserId { get; set; }
        public string Message { get; set; }
        public string Link { get; set; }
    }
}
