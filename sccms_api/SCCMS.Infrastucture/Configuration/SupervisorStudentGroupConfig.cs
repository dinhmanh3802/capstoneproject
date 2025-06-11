using Microsoft.EntityFrameworkCore;
using SCCMS.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Infrastucture.Configuration
{
    public class SupervisorStudentGroupConfig: IEntityTypeConfiguration<SupervisorStudentGroup>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<SupervisorStudentGroup> builder)
        {
            builder.HasKey(x => new { x.SupervisorId, x.StudentGroupId });
            builder.HasOne(x => x.Supervisor).WithMany(x => x.SupervisorStudentGroup).HasForeignKey(x => x.SupervisorId);
            builder.HasOne(x => x.StudentGroup).WithMany(x => x.SupervisorStudentGroup).HasForeignKey(x => x.StudentGroupId);
        }
    }
}
