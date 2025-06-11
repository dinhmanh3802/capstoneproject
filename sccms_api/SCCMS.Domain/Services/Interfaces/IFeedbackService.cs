using SCCMS.Domain.DTOs.FeedbackDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.Services.Interfaces
{
    public interface IFeedbackService
    {
        Task<IEnumerable<FeedbackDto>> GetAllFeedbacksAsync(int courseId, DateTime? feedbackDateStart, DateTime? feedbackDateEnd);
        Task<FeedbackDto?> GetFeedbackByIdAsync(int id);
        Task CreateFeedbackAsync(FeedbackCreateDto feedbackCreateDto);
        Task UpdateFeedbackAsync(int feedbackId, FeedbackUpdateDto feedbackUpdateDto);
        Task DeleteFeedbackAsync(int id);
        Task DeleteFeedbacksByIdsAsync(IEnumerable<int> ids);
    }
}
