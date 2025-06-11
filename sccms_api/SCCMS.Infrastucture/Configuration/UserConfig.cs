using Microsoft.EntityFrameworkCore;
using SCCMS.Infrastucture.Entities;

namespace SCCMS.Infrastucture.Configuration
{
    public class UserConfig : IEntityTypeConfiguration<User>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<User> builder)
        {
            builder.HasIndex(x => x.Email).IsUnique();
            builder.HasIndex(x => x.UserName).IsUnique();
            builder.HasIndex(x => x.PhoneNumber).IsUnique();
            builder.HasIndex(x => x.NationalId).IsUnique();


        }
    }
}
