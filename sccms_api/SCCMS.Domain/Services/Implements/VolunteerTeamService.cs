using AutoMapper;
using SCCMS.Domain.DTOs.TeamDtos;
using SCCMS.Domain.DTOs.VolunteerTeamDtos;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.Services.Implements
{
    public class VolunteerTeamService : IVolunteerTeamService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public VolunteerTeamService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task AddVolunteersIntoTeamAsync(VolunteerTeamDto volunteerTeamDto)
        {
            // Lấy thông tin của ban
             var team = await _unitOfWork.Team.GetByIdAsync(volunteerTeamDto.TeamId);
            if (team == null)
            {
                throw new ArgumentException($"Không tìm thấy đội với ID {volunteerTeamDto.TeamId}.");
            }

            // Lấy tất cả các bản ghi phân công tình nguyện viên trong cùng khóa tu
            var existingAssignments = await _unitOfWork.VolunteerTeam
                .GetAllAsync(vt => volunteerTeamDto.VolunteerIds.Contains(vt.VolunteerId) && vt.Team.CourseId == team.CourseId);

            var newAssignments = new List<VolunteerTeam>();

            foreach (var volunteerId in volunteerTeamDto.VolunteerIds)
            {
                // Kiểm tra trạng thái của volunteer trong VolunteerCourse
                var volunteerCourse = await _unitOfWork.VolunteerApplication
                    .GetAsync(vc => vc.VolunteerId == volunteerId && vc.CourseId == team.CourseId);

                // Nếu không tồn tại hoặc chưa được phê duyệt, bỏ qua tình nguyện viên này
                if (volunteerCourse == null || volunteerCourse.Status != ProgressStatus.Approved)
                {
                    throw new ArgumentException("Vui lòng duyệt đơn trước khi thêm tình nguyện viên");
                }
                // Lấy thông tin của tình nguyện viên
                var volunteer = await _unitOfWork.Volunteer.GetByIdAsync(volunteerId);
                if (volunteer == null)
                {
                    throw new ArgumentException("Không tìm thấy tình nguyện viên");
                }

                // Kiểm tra điều kiện giới tính
                if (team.Gender != null && volunteer.Gender != team.Gender)
                {
                    throw new ArgumentException("Chỉ được chọn tình nguyện viên giới tính " + (team.Gender == Gender.Male ? "Nam" : "Nữ") + "!");
                }

            }

            foreach (var volunteerId in volunteerTeamDto.VolunteerIds)
            {
                // Lấy thông tin của tình nguyện viên
                var volunteer = await _unitOfWork.Volunteer.GetByIdAsync(volunteerId);
                if (volunteer == null)
                {
                    continue; // Nếu không tìm thấy tình nguyện viên, bỏ qua và tiếp tục
                }

                // Kiểm tra xem tình nguyện viên đã có trong một đội khác của cùng khóa học chưa
                var existingAssignment = existingAssignments.FirstOrDefault(vt => vt.VolunteerId == volunteerId);

                if (existingAssignment != null)
                {
                    // Xóa bản ghi cũ nếu tình nguyện viên đã ở trong đội khác của khóa học này
                    await _unitOfWork.VolunteerTeam.DeleteAsync(existingAssignment);
                }

                // Tạo bản ghi mới với TeamId đã cập nhật
                var volunteerTeamAssignment = new VolunteerTeam
                {
                    VolunteerId = volunteerId,
                    TeamId = volunteerTeamDto.TeamId
                };

                newAssignments.Add(volunteerTeamAssignment);
            }

            // Thêm tất cả tình nguyện viên vào đội
            if (newAssignments.Any())
            {
                await _unitOfWork.VolunteerTeam.AddRangeAsync(newAssignments);
            }

            // Lưu các thay đổi
            await _unitOfWork.SaveChangeAsync();
        }


        public async Task RemoveVolunteersFromTeamAsync(VolunteerTeamDto volunteerTeamDto)
        {
            foreach (var volunteerId in volunteerTeamDto.VolunteerIds)
            {
                // Kiểm tra xem tình nguyện viên có trong đội không
                var existingAssignment = await _unitOfWork.VolunteerTeam
                    .GetAsync(vt => vt.VolunteerId == volunteerId && vt.TeamId == volunteerTeamDto.TeamId);

                if (existingAssignment == null)
                {
                    throw new ArgumentException($"Tình nguyện viên {volunteerId} không có trong ban {volunteerTeamDto.TeamId}.");
                }

                // Xóa VolunteerTeam nếu tồn tại
                _unitOfWork.VolunteerTeam.DeleteAsync(existingAssignment);
            }

            // Lưu các thay đổi vào cơ sở dữ liệu
            await _unitOfWork.SaveChangeAsync();
        }


        public async Task AutoAssignVolunteersToTeamsAsync(int courseId)
        {
            // 1. Kiểm tra tất cả các đơn đăng ký đã được duyệt
            var pendingApplications = await _unitOfWork.VolunteerApplication.CountAsync(v => v.CourseId == courseId && v.Status == ProgressStatus.Pending);
            if (pendingApplications > 0)
            {
                throw new InvalidOperationException("Cần phải duyệt hết tất cả các đơn đăng ký trước khi phân team tự động.");
            }

            // 2. Lấy danh sách các volunteer đã được duyệt
            var approvedVolunteerApplications = await _unitOfWork.VolunteerApplication.FindAsync(
                v => v.CourseId == courseId && v.Status == ProgressStatus.Approved,
                includeProperties: "Volunteer"
            );

            var approvedVolunteers = approvedVolunteerApplications.Select(v => v.Volunteer).ToList();

            if (!approvedVolunteers.Any())
            {
                throw new InvalidOperationException("Không có tình nguyện viên nào được duyệt để phân vào team.");
            }

            // 3. Lấy danh sách các team trong khóa học
            var teams = await _unitOfWork.Team.FindAsync(
                t => t.CourseId == courseId,
                includeProperties: "VolunteerTeam.Volunteer"
            );

            if (!teams.Any())
            {
                throw new InvalidOperationException("Không có team nào trong khóa học này.");
            }

            // 4. Tính tổng số dự kiến của tất cả các team
            var totalExpectedVolunteers = teams.Sum(t => t.ExpectedVolunteers);

            if (totalExpectedVolunteers == 0)
            {
                throw new InvalidOperationException("Tổng số dự kiến của các team phải lớn hơn 0.");
            }

            // 5. Tính tỷ lệ phân bổ cho mỗi team
            var teamAllocation = teams.Select(t => new
            {
                Team = t,
                Ratio = (double)t.ExpectedVolunteers / totalExpectedVolunteers
            }).ToList();

            // 6. Tính số lượng volunteer cần phân bổ cho mỗi team
            var totalVolunteers = approvedVolunteers.Count;
            var teamVolunteerCounts = teamAllocation.Select(t => new
            {
                Team = t.Team,
                AllocateCount = (int)Math.Round(t.Ratio * totalVolunteers)
            }).ToList();

            // 7. Lấy danh sách volunteer đã được phân vào team
            var existingAssignments = await _unitOfWork.VolunteerTeam.FindAsync(
                vt => vt.Team.CourseId == courseId && approvedVolunteers.Select(v => v.Id).Contains(vt.VolunteerId),
                includeProperties: "Volunteer"
            );

            var assignedVolunteerIds = existingAssignments.Select(vt => vt.VolunteerId).ToHashSet();

            // 8. Lấy danh sách volunteer chưa được phân vào team
            var unassignedVolunteers = approvedVolunteers.Where(v => !assignedVolunteerIds.Contains(v.Id)).ToList();
            if(!unassignedVolunteers.Any())
            {
                throw new InvalidOperationException("Tất cả tình nguyện viên đã được phân vào ban.");
            }

            // 9. Phân loại volunteer theo giới tính
            var maleVolunteers = unassignedVolunteers.Where(v => v.Gender == Gender.Male).ToList();
            var femaleVolunteers = unassignedVolunteers.Where(v => v.Gender == Gender.Female).ToList();

            // 10. Phân bổ volunteer vào team dựa trên tỷ lệ và giới tính
            foreach (var allocation in teamVolunteerCounts)
            {
                var team = allocation.Team;
                var allocateCount = allocation.AllocateCount;

                // Lấy số volunteer hiện tại trong team
                var currentVolunteerCount = team.VolunteerTeam?.Count ?? 0;

                // Tính số volunteer cần thêm
                var needed = allocateCount - currentVolunteerCount;
                if (needed <= 0)
                {
                    continue; // Team đã đạt hoặc vượt số lượng dự kiến
                }

                // Lọc volunteer theo giới tính và điều kiện của team
                List<Volunteer> eligibleVolunteers;
                if (team.Gender == Gender.Male)
                {
                    eligibleVolunteers = maleVolunteers;
                }
                else if (team.Gender == Gender.Female)
                {
                    eligibleVolunteers = femaleVolunteers;
                }
                else
                {
                    // Team không yêu cầu giới tính cụ thể
                    eligibleVolunteers = unassignedVolunteers;
                }

                // Phân bổ volunteer
                var volunteersToAssign = eligibleVolunteers.Take(needed).ToList();

                foreach (var volunteer in volunteersToAssign)
                {
                    var volunteerTeamAssignment = new VolunteerTeam
                    {
                        VolunteerId = volunteer.Id,
                        TeamId = team.Id
                    };
                    await _unitOfWork.VolunteerTeam.AddAsync(volunteerTeamAssignment);

                    // Loại volunteer đã phân khỏi danh sách
                    unassignedVolunteers.Remove(volunteer);
                    if (volunteer.Gender == Gender.Male)
                    {
                        maleVolunteers.Remove(volunteer);
                    }
                    else if (volunteer.Gender == Gender.Female)
                    {
                        femaleVolunteers.Remove(volunteer);
                    }
                }
            }

            // 11. Phân bổ các volunteer còn lại vào các team có ít người nhất
            while (unassignedVolunteers.Any())
            {
                foreach (var volunteer in unassignedVolunteers.ToList())
                {
                    // Tìm team phù hợp theo giới tính và có ít volunteer nhất
                    List<Team> eligibleTeams;

                    if (volunteer.Gender == Gender.Male)
                    {
                        eligibleTeams = teams
                            .Where(t => t.Gender == Gender.Male || t.Gender == null)
                            .OrderBy(t => t.VolunteerTeam?.Count ?? 0)
                            .ToList();
                    }
                    else if (volunteer.Gender == Gender.Female)
                    {
                        eligibleTeams = teams
                            .Where(t => t.Gender == Gender.Female || t.Gender == null)
                            .OrderBy(t => t.VolunteerTeam?.Count ?? 0)
                            .ToList();
                    }
                    else
                    {
                        // Nếu giới tính không được xác định, có thể thêm logic xử lý khác nếu cần
                        eligibleTeams = teams
                            .OrderBy(t => t.VolunteerTeam?.Count ?? 0)
                            .ToList();
                    }

                    // Chọn team có ít volunteer nhất
                    var targetTeam = eligibleTeams.FirstOrDefault();

                    if (targetTeam == null)
                    {
                        throw new InvalidOperationException($"Không tìm thấy team phù hợp để phân tình nguyện viên ID {volunteer.Id}.");
                    }

                    // Tạo bản ghi mới phân bổ volunteer vào team
                    var newAssignment = new VolunteerTeam
                    {
                        VolunteerId = volunteer.Id,
                        TeamId = targetTeam.Id
                    };
                    await _unitOfWork.VolunteerTeam.AddAsync(newAssignment);

                    // Loại volunteer đã phân khỏi danh sách
                    unassignedVolunteers.Remove(volunteer);
                    if (volunteer.Gender == Gender.Male)
                    {
                        maleVolunteers.Remove(volunteer);
                    }
                    else if (volunteer.Gender == Gender.Female)
                    {
                        femaleVolunteers.Remove(volunteer);
                    }
                }
            }

            // 12. Lưu các thay đổi vào cơ sở dữ liệu
            await _unitOfWork.SaveChangeAsync();           
        }
    }
}
