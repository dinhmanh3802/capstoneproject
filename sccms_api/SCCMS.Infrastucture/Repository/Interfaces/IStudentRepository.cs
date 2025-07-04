using SCCMS.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Infrastucture.Repository.Interfaces
{
    public interface IStudentRepository : IGenericRepository<Student>
    {
		Task<Student?> GetByNationalIdAsync(string nationalId);
	}
}
