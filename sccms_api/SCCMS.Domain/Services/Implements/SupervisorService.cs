using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCCMS.Domain.DTOs.StudentDtos;
using SCCMS.Domain.DTOs.SupervisorDtos;
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
    public class SupervisorService : ISupervisorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public SupervisorService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<SupervisorDto>> GetSupervisorsByCourseIdAsync(
        int courseId,
        string? name = null,
        string? email = null,
        string? phoneNumber = null,
        UserStatus? status = null,
        Gender? gender = null)
        {
            if (courseId < 1)
            {
                throw new ArgumentException("CourseId không hợp lệ, phải lớn hơn 0.");
            }

            // Loại bỏ dấu cách dư thừa từ các tham số đầu vào
            name = name?.Trim();
            email = email?.Trim();
            phoneNumber = phoneNumber?.Trim();

            // Thực hiện truy vấn với điều kiện lọc không phân biệt dấu
            var supervisors = await _unitOfWork.User.FindAsync(u =>
                (string.IsNullOrEmpty(name) ||
                    EF.Functions.Collate(u.FullName, "Latin1_General_CI_AI").Contains(name)) && // Lọc theo tên không phân biệt dấu
                (string.IsNullOrEmpty(email) ||
                    EF.Functions.Collate(u.Email, "Latin1_General_CI_AI").Contains(email)) && // Lọc theo email không phân biệt dấu
                (string.IsNullOrEmpty(phoneNumber) || u.PhoneNumber.Contains(phoneNumber)) && // Số điện thoại thường không có dấu, nên không cần Collate
                (status == null || u.Status == status) && // Lọc theo trạng thái nếu có
                (gender == null || u.Gender == gender) && // Lọc theo giới tính nếu có
                u.RoleId == SD.RoleId_Supervisor, // Lọc theo vai trò Supervisor
                includeProperties: "SupervisorStudentGroup.StudentGroup");

            var supervisorDtos = new List<SupervisorDto>();

            foreach (var supervisor in supervisors)
            {
                var supervisorGroup = supervisor.SupervisorStudentGroup?
                    .Select(sg => sg.StudentGroup)
                    .FirstOrDefault(sg => sg.CourseId == courseId);

                var supervisorDto = _mapper.Map<SupervisorDto>(supervisor);
                if (supervisorGroup != null)
                {
                    supervisorDto.Group = _mapper.Map<GroupInfoDto>(supervisorGroup);
                }
                else
                {
                    supervisorDto.Group = null;
                }

                supervisorDtos.Add(supervisorDto);
            }

            return supervisorDtos;
        }


        public async Task<SupervisorDto?> GetSupervisorByIdAsync(int id)
        {
            if (id < 1)
            {
                throw new ArgumentException("ID của huynh trưởng phải lớn hơn 0.");
            }

            var supervisor = await _unitOfWork.User
                .FindAsync(u => u.Id == id && u.RoleId == SD.RoleId_Supervisor, includeProperties: "SupervisorStudentGroup.StudentGroup");

            var supervisorEntity = supervisor.FirstOrDefault();

            if (supervisorEntity == null)
            {
                return null;
            }

            var supervisorDto = _mapper.Map<SupervisorDto>(supervisorEntity);

            if (supervisorEntity.SupervisorStudentGroup != null && supervisorEntity.SupervisorStudentGroup.Any())
            {
                supervisorDto.Group = _mapper.Map<GroupInfoDto>(supervisorEntity.SupervisorStudentGroup.FirstOrDefault()?.StudentGroup);
            }
            else
            {
                supervisorDto.Group = null;
            }

            return supervisorDto;
        }

        public async Task ChangeSupervisorsGroupAsync(List<int> supervisorIds, int newGroupId)
        {
            if (supervisorIds == null || !supervisorIds.Any())
            {
                throw new ArgumentException("SupervisorIds không được để trống.");
            }

            if (newGroupId < 1)
            {
                throw new ArgumentException("NewGroupId phải lớn hơn 0.");
            }

            // Kiểm tra xem nhóm mới có tồn tại không
            var newGroup = await  _unitOfWork.StudentGroup.GetByIdAsync(newGroupId);
            var course = await _unitOfWork.Course.GetByIdAsync(newGroup.CourseId);
            if (course == null)
            {
                throw new ArgumentException("NewGroupId không tồn tại.");
            }

            var courseId = newGroup.CourseId;

            // Lấy danh sách Supervisor từ danh sách ID
            var supervisors = await _unitOfWork.User.FindAsync(u =>
                supervisorIds.Contains(u.Id) &&
                u.RoleId == SD.RoleId_Supervisor,
                includeProperties: "SupervisorStudentGroup.StudentGroup");

            if (supervisors == null || !supervisors.Any())
            {
                throw new ArgumentException("Không tìm thấy Supervisor nào trong danh sách đã cung cấp.");
            }

            // Kiểm tra tất cả Supervisor đều tồn tại
            var foundSupervisorIds = supervisors.Select(s => s.Id).ToList();
            var notFoundIds = supervisorIds.Except(foundSupervisorIds).ToList();
            if (notFoundIds.Any())
            {
                throw new ArgumentException($"Không tìm thấy Supervisor với các ID sau: {string.Join(", ", notFoundIds)}");
            }

            foreach (var supervisor in supervisors)
            {
                // Loại bỏ các nhóm hiện tại của Supervisor trong khóa tu hiện tại
                if (supervisor.SupervisorStudentGroup != null && supervisor.SupervisorStudentGroup.Any())
                {
                    var existingAssignments = supervisor.SupervisorStudentGroup
                        .Where(ssg => ssg.StudentGroup.CourseId == courseId)
                        .ToList();

                    foreach (var assignment in existingAssignments)
                    {
                        await _unitOfWork.SupervisorStudentGroup.DeleteAsync(assignment);
                        await _notificationService.NotifyUserAsync(supervisor.Id, $"Bạn bị xóa khỏi chánh '{assignment.StudentGroup.GroupName}' của khóa tu '{course.CourseName}'.", "student-groups");

                    }
                }

                // Thêm nhóm mới
                var newAssignment = new SupervisorStudentGroup
                {
                    SupervisorId = supervisor.Id,
                    StudentGroupId = newGroupId
                };
                await _unitOfWork.SupervisorStudentGroup.AddAsync(newAssignment);

                //Tạo thông báo: Lấy thông tin khóa tu và nhóm để gửi thông báo
                var groupName = newGroup.GroupName;
                string message = $"Bạn đã được thêm vào chánh '{groupName}' của khóa tu '{course.CourseName}'.";
                //TODO: Đổi thành màn chi tiết 
                string link = "student-groups/" + newGroupId;
                await _notificationService.NotifyUserAsync(supervisor.Id, message, link);
            }

            // Lưu thay đổi vào database
            await _unitOfWork.SaveChangeAsync();
        }



        public async Task<IEnumerable<SupervisorDto>> GetAvailableSupervisorsForCourseAsync(int courseId)
        {
            if (courseId < 1)
            {
                throw new ArgumentException("CourseId không hợp lệ.");
            }

            var supervisors = await _unitOfWork.User.FindAsync(
                u => u.RoleId == SD.RoleId_Supervisor && u.Status == UserStatus.Active,
                includeProperties: "SupervisorStudentGroup.StudentGroup"
            );

            var availableSupervisors = supervisors
                .Where(s => s.SupervisorStudentGroup == null || !s.SupervisorStudentGroup.Any(ssg => ssg.StudentGroup.CourseId == courseId))
                .ToList();

            return _mapper.Map<List<SupervisorDto>>(availableSupervisors);
        }



    }
}
