using SCCMS.Domain.DTOs.EmailDtos;
using System.Threading.Tasks;

namespace SCCMS.API.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string templateName, Dictionary<string, string> parameters);
        void SendBulkEmailAsync(SendBulkEmailRequestDto emailRequest);

    }
}
