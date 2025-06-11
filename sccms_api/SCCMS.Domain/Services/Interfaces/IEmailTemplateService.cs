using SCCMS.Domain.DTOs.CourseDtos;
using SCCMS.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.Services.Interfaces
{
    public interface IEmailTemplateService
    {
        Task<EmailTemplate?> GetTemplateByNameAsync(string templateName);
        Task<EmailTemplate?> GetTemplate(int id);
        Task<IEnumerable<EmailTemplate>> GetAllEmailTemplateAsync();
        Task CreateOrUpdateTemplateAsync(EmailTemplate template);
    }
}
