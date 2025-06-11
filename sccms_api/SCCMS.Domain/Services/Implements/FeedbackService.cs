using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SCCMS.Domain.DTOs.CourseDtos;
using SCCMS.Domain.DTOs.FeedbackDtos;
using SCCMS.Domain.DTOs.StudentDtos;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.Services.Implements
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FeedbackService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task CreateFeedbackAsync(FeedbackCreateDto feedbackCreateDto)
        {
            if (feedbackCreateDto.CourseId <= 0)
            {
                throw new Exception("CourseId phải lớn hơn 0.");
            }

            if (string.IsNullOrEmpty(feedbackCreateDto.StudentCode))
            {
                throw new Exception("Mã khóa sinh là trường bắt buộc.");
            }

            if (feedbackCreateDto.StudentCode.Length != 9)
            {
                throw new Exception("Mã khóa sinh phải có 9 ký tự.");
            }

            if (string.IsNullOrEmpty(feedbackCreateDto.Content))
            {
                throw new Exception("Nội dung không được để trống.");
            }

            // Kiểm tra xem StudentCode có tồn tại trong StudentCourse không
            var existingStudentCourse = await _unitOfWork.StudentCourse.GetByStudentCodeAsync(feedbackCreateDto.StudentCode);
            if (existingStudentCourse == null)
            {
                throw new Exception("Mã khóa sinh không tồn tại.");
            }

            // Kiểm tra xem StudentCode có thuộc về CourseId này không
            if (existingStudentCourse.CourseId != feedbackCreateDto.CourseId)
            {
                throw new Exception("Khóa sinh không tham gia khóa tu này.");
            }

            // Kiểm tra xem CourseId có tồn tại trong Course không
            var existingCourse = await _unitOfWork.Course.GetByIdAsync(feedbackCreateDto.CourseId);
            if (existingCourse == null)
            {
                throw new InvalidOperationException("Khóa tu này không tồn tại");
            }

            // Tạo và lưu phản hồi mới
            var feedback = _mapper.Map<Feedback>(feedbackCreateDto);
            feedback.SubmissionDate = DateTime.Now;
            await _unitOfWork.Feedback.AddAsync(feedback);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task DeleteFeedbackAsync(int id)
        {
            Feedback feedback = await _unitOfWork.Feedback.GetByIdAsync(id);
            if (feedback == null)
            {
                throw new ArgumentException("Phản hồi không tồn tại.");
            }
            await _unitOfWork.Feedback.DeleteAsync(feedback);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task<IEnumerable<FeedbackDto>> GetAllFeedbacksAsync(int courseId, DateTime? feedbackDateStart, DateTime? feedbackDateEnd)
        {
            // Validate course existence
            var existingCourse = await _unitOfWork.Course.GetByIdAsync(courseId);
            if (existingCourse == null)
            {
                throw new ArgumentException("Course ID does not exist in the system.");
            }

            // Filter feedbacks based on courseId and date range (ignoring time)
            var feedbacks = await _unitOfWork.Feedback.FindAsync(f =>
                f.CourseId == courseId &&
                (!feedbackDateStart.HasValue || f.SubmissionDate.Date >= feedbackDateStart.Value.Date) &&
                (!feedbackDateEnd.HasValue || f.SubmissionDate.Date <= feedbackDateEnd.Value.Date));

            return _mapper.Map<IEnumerable<FeedbackDto>>(feedbacks);
        }


        public async Task<FeedbackDto?> GetFeedbackByIdAsync(int id)
        {
            var feedback = await _unitOfWork.Feedback.GetByIdAsync(id);
            
            if (feedback == null)
            {
                throw new ArgumentException("Phản hồi không tồn tại.");
            }
            var feedbackDto = _mapper.Map<FeedbackDto>(feedback);

            return feedbackDto;
        }

        public Task UpdateFeedbackAsync(int feedbackId, FeedbackUpdateDto feedbackUpdateDto)
        {
            throw new NotImplementedException();
        }
        public async Task DeleteFeedbacksByIdsAsync(IEnumerable<int> ids)
        {
            // Fetch the feedbacks by IDs
            var feedbacks = await _unitOfWork.Feedback.GetByIdsAsync(ids);

            // Check if all IDs provided exist in the database
            if (feedbacks == null || !feedbacks.Any())
            {
                throw new ArgumentException("No feedbacks found for the provided IDs.");
            }

            // Delete the feedbacks
            await _unitOfWork.Feedback.DeleteRangeAsync(feedbacks);

            // Save changes to the database
            await _unitOfWork.SaveChangeAsync();
        }

    }
}
