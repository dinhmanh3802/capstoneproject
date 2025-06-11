// DashboardDtos/DashboardDto.cs
using System.Collections.Generic;

namespace SCCMS.Domain.DTOs.DashboardDtos
{
    public class DashboardDto
    {
        public int? TotalRegisteredStudents { get; set; } // Số học sinh đã đăng ký cho khóa học
        public int? TotalStudents { get; set; } // Tổng số học sinh trong khóa học - đã được duyệt
        public int? TotalMaleStudents { get; set; } // Tổng số học sinh Nam trong khóa học
        public int? TotalRegisteredVolunteers { get; set; } // Số tình nguyện viên đã đăng ký cho khóa học
        public int? TotalVolunteers { get; set; } // Tổng số tình nguyện viên trong khóa học
        public int? TotalMaleVolunteers { get; set; } // Tổng số tình nguyện viên NAM trong khóa học - đã được duyệt
        public double? AttendanceRate { get; set; } // Tỷ lệ có mặt
        public double? GraduationRate { get; set; } // Tỷ lệ tốt nghiệp
        public int? TotalFeedbacks { get; set; } // Tổng số feedback đã nhận cho khóa học
    }
}
