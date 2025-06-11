using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.DTOs.Auth.Login
{
    public class LoginResponseDto
    {
        public int Id { get; set; }
        public string Token { get; set; }
    }
}
