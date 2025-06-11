using SCCMS.Domain.DTOs.CourseDtos;
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
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EmailTemplateService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EmailTemplate?> GetTemplateByNameAsync(string templateName)
        {
            return await _unitOfWork.EmailTemplate.GetAsync(t => t.Name == templateName);
        }

        public async Task CreateOrUpdateTemplateAsync(EmailTemplate template)
        {
            var existingTemplate = await _unitOfWork.EmailTemplate.GetAsync(t => t.Name == template.Name);
            if (existingTemplate != null)
            {
                // Cập nhật template
                existingTemplate.Subject = template.Subject;
                existingTemplate.Body = template.Body;
                await _unitOfWork.EmailTemplate.UpdateAsync(existingTemplate);
            }
            else
            {
                // Tạo mới template
                await _unitOfWork.EmailTemplate.AddAsync(template);
            }
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task<EmailTemplate?> GetTemplate(int id)
        {
            try
            {
                var email = await _unitOfWork.EmailTemplate.GetAsync(t => t.Id == id);

                if (email == null)
                {
                    return null;
                }
                return email;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while fetching the course with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<EmailTemplate>> GetAllEmailTemplateAsync()
        {
            return await _unitOfWork.EmailTemplate.FindAsync(x=> x.Id != 1 && x.Id!= 2);
        }
    }
}
