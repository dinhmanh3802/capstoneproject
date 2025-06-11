// DashboardDtos/RegistrationOverTimeDto.cs
using System;

namespace SCCMS.Domain.DTOs.DashboardDtos
{
    public class RegistrationOverTimeDto
    {
        public DateTime Period { get; set; } // Thời gian (tháng hoặc năm)
        public int Count { get; set; } // Số lượng đăng ký
    }
}
