using AutoMapper;
using SCCMS.Domain.DTOs.TeamDtos;
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
    public class TeamService : ITeamService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TeamService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<IEnumerable<VolunteerInforInTeamDto>> GetVolunteersInTeamAsync(
    int teamId,
    string? volunteerCode,
    string? fullName,
    string? phoneNumber,
    Gender? gender,
    ProgressStatus? status)
        {
            // Kiểm tra team có tồn tại không và đảm bảo VolunteerTeam được tải
            var team = await _unitOfWork.Team.GetAsync(
                t => t.Id == teamId,
                includeProperties: "VolunteerTeam.Volunteer.VolunteerCourse");

            if (team == null)
            {
                throw new ArgumentException("Không tìm thấy team với ID này.");
            }

            var volunteers = GetTeamByIdAsync(teamId).Result?.Volunteers;
            if (volunteers == null || !volunteers.Any())
            {
                return new List<VolunteerInforInTeamDto>();
            }

            // Nếu VolunteerTeam là null, gán nó thành danh sách rỗng
            var volunteersQuery = (team.VolunteerTeam ?? new List<VolunteerTeam>()).AsQueryable();

            // Áp dụng các bộ lọc tìm kiếm nếu có
            if (!string.IsNullOrEmpty(volunteerCode))
            {
                volunteers = volunteers.Where(volunteers => volunteers.volunteerCode.Contains(volunteerCode)).ToList();
            }

            if (!string.IsNullOrEmpty(fullName))
            {
                volunteers = volunteers.Where(volunteers => volunteers.FullName.ToLower().Contains(fullName.ToLower())).ToList();
            }

            if (!string.IsNullOrEmpty(phoneNumber))
            {
                volunteers = volunteers.Where(volunteers => volunteers.PhoneNumber.Contains(phoneNumber)).ToList();
            }

            if (gender.HasValue)
            {
                volunteers = volunteers.Where(vt => vt.Gender == gender).ToList();
            }

            if (status.HasValue)
            {
                volunteers = volunteers.Where(vt => vt.Status == status).ToList();
            }

            // Sử dụng AutoMapper để ánh xạ kết quả
            return volunteers;
        }


        public async Task CreateTeamAsync(TeamCreateDto entity)
        {
            var course = await _unitOfWork.Course.GetByIdAsync(entity.CourseId);
            if (course == null)
            {
                throw new ArgumentException("CourseId không tham chiếu đến khóa học hợp lệ.");
            }
            if (entity.TeamName.Length > 100)
            {
                throw new ArgumentException("Tên ban không được vượt quá 100 ký tự.");
            }
            var leader = await _unitOfWork.User.GetByIdAsync(entity.LeaderId);
            if (leader == null)
            {
                throw new ArgumentException("Không tìm thấy trưởng ban.");
            }
            var teamsInCourse = await _unitOfWork.Team.GetAllAsync(t => t.CourseId == entity.CourseId);
            if (teamsInCourse.Any(team => team.LeaderId == entity.LeaderId))
            {
                throw new ArgumentException("Trưởng ban đã làm một ban khác. Vui lòng chọn trưởng ban khác!");
            }

            // Kiểm tra giới tính của leader
            if (entity.Gender.HasValue && entity.Gender != leader.Gender)
            {
                throw new ArgumentException("Giới tính của trưởng ban không phù hợp với giới tính yêu cầu của ban.");
            }
            // Kiểm tra tên ban không chứa ký tự đặc biệt
            var specialCharactersPattern = @"[^\p{L}\p{N}\s]";
            if (System.Text.RegularExpressions.Regex.IsMatch(entity.TeamName, specialCharactersPattern))
            {
                throw new ArgumentException("Tên ban không được chứa ký tự đặc biệt. Vui lòng chọn tên khác.");
            }
            var existingTeam = await _unitOfWork.Team
                .GetAsync(t => t.CourseId == entity.CourseId && t.TeamName.ToLower() == entity.TeamName.ToLower());

            if (existingTeam != null)
            {
                throw new ArgumentException("Tên ban đã tồn tại trong khóa học này. Vui lòng chọn tên khác.");
            }
            if(entity.ExpectedVolunteers <=0)
            {
                throw new ArgumentException("Số lượng tình nguyện viên dự kiến phải lớn hơn 0.");
            }
            var team = _mapper.Map<Team>(entity);
            await _unitOfWork.Team.AddAsync(team);
            await _unitOfWork.SaveChangeAsync();
        }



        public async Task<IEnumerable<TeamDto>> GetAllTeamsByCourseIdAsync(int courseId)
        {
            if (courseId < 1) throw new ArgumentException("CourseId không hợp lệ, phải lớn hơn 0.");

            var teams = await _unitOfWork.Team.GetAllAsync(
                t => t.CourseId == courseId,
                includeProperties: "Course,Leader,VolunteerTeam.Volunteer.VolunteerCourse");

            if (teams == null || !teams.Any()) return new List<TeamDto>();

            return _mapper.Map<List<TeamDto>>(teams);
        }

        public async Task<TeamDto?> GetTeamByIdAsync(int id)
        {
            if (id < 1) throw new ArgumentException("Id không hợp lệ, phải lớn hơn 0.");

            var team = await _unitOfWork.Team.GetAsync(
                t => t.Id == id,
                includeProperties: "Course,Leader,VolunteerTeam.Volunteer.VolunteerCourse");

            if (team == null) return null;

            return _mapper.Map<TeamDto>(team);
        }

        public async Task UpdateTeamAsync(int teamId, TeamUpdateDto entity)
        {
            var existingTeam = await _unitOfWork.Team.GetAsync(
                x => x.Id == teamId,
                includeProperties: "VolunteerTeam.Volunteer");

            if (existingTeam == null)
                throw new ArgumentException("Không tìm thấy ban.");

            var course = await _unitOfWork.Course.GetByIdAsync(entity.CourseId);
            if (course == null)
                throw new ArgumentException("CourseId không tham chiếu đến khóa học hợp lệ.");
            if (string.IsNullOrWhiteSpace(entity.TeamName))
            {
                throw new ArgumentException("Tên ban không được để trống hoặc null. Vui lòng chọn tên khác.");
            }
            if (entity.TeamName.Length > 100)
            {
                throw new ArgumentException("Tên ban không được vượt quá 100 ký tự.");
            }
            if (entity.LeaderId != null)
            {
                var leader = await _unitOfWork.User.GetByIdAsync(entity.LeaderId);
                if (leader == null)
                    throw new ArgumentException("Không tìm thấy trưởng ban.");
            }
            var teamsInCourse = await _unitOfWork.Team.GetAllAsync(t => t.CourseId == entity.CourseId);
            if (teamsInCourse.Any(team => team.LeaderId == entity.LeaderId && team.Id != teamId))
            {
                throw new ArgumentException("Trưởng ban đã làm một ban khác. Vui lòng chọn trưởng ban khác!");
            }
            // Kiểm tra tên ban không chứa ký tự đặc biệt
            var specialCharactersPattern = @"[^\p{L}\p{N}\s]";
            if (System.Text.RegularExpressions.Regex.IsMatch(entity.TeamName, specialCharactersPattern))
            {
                throw new ArgumentException("Tên ban không được chứa ký tự đặc biệt. Vui lòng chọn tên khác.");
            }
            // Kiểm tra tên đội có bị trùng trong khóa học này không
            var duplicateTeam = await _unitOfWork.Team
                .GetAsync(t => t.CourseId == entity.CourseId && t.TeamName.ToLower() == entity.TeamName.ToLower() && t.Id != teamId);

            if (duplicateTeam != null)
            {
                throw new ArgumentException("Tên ban đã tồn tại trong khóa học này. Vui lòng chọn tên khác.");
            }
            
            // Kiểm tra số lượng tình nguyện viên trong đội (phải lớn hơn 0)
            var volunteerCount = entity.ExpectedVolunteers; // Chuyển thành List trước khi tính số lượng
            if (volunteerCount <= 0)
            {
                throw new ArgumentException("Số lượng tình nguyện viên dự kiến trong ban phải lớn hơn 0.");
            }
            // Kiểm tra điều kiện cập nhật Gender
            if (entity.Gender != null && entity.Gender != existingTeam.Gender)
            {
                // Nếu Gender của Team được cập nhật thành Male hoặc Female
                if (entity.Gender == Gender.Male || entity.Gender == Gender.Female)
                {
                    // Kiểm tra tất cả các Volunteer trong Team
                    var conflictingVolunteer = existingTeam.VolunteerTeam
                        .Any(vt => vt.Volunteer.Gender != entity.Gender);

                    if (conflictingVolunteer)
                    {
                        throw new ArgumentException("Tất cả học sinh đều phải có giới tính " + (entity.Gender == Gender.Male ? "nam" : "nữ") + "!");
                    }
                }
            }

            _mapper.Map(entity, existingTeam);
            await _unitOfWork.Team.UpdateAsync(existingTeam);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task DeleteTeamAsync(int id)
        {
            var team = await _unitOfWork.Team.GetByIdAsync(id);
            if (team == null) throw new ArgumentException("Không tìm thấy ban.");

            await _unitOfWork.Team.DeleteAsync(team);
            await _unitOfWork.SaveChangeAsync();
        }
    }

}
