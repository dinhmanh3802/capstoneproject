using Microsoft.EntityFrameworkCore;
using SCCMS.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Infrastucture.Configuration
{
    public class ReportConfig : IEntityTypeConfiguration<Report>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Report> builder)
        {
            builder.HasOne(x => x.SubmittedByUser).WithMany(x => x.Report).HasForeignKey(x => x.SubmissionBy);
        }
    }
}
