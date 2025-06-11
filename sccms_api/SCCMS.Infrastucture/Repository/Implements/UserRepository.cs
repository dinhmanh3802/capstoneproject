using Microsoft.EntityFrameworkCore;
using SCCMS.Infrastucture.Context;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Infrastucture.Repository.Implements
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {

        }

    }
}
