using SCCMS.Infrastucture.Context;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Infrastucture.Repository.Implements
{
    public class VolunteerRepository : GenericRepository<Volunteer>, IVolunteerRepository
    {
        public VolunteerRepository(AppDbContext context) : base(context)
        {
        }
    }
}
