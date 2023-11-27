using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniApp1Api.Data.Entities;

namespace MiniApp1Api.Data.Mappings;

public class UserAppMap : IEntityTypeConfiguration<UserApp>
{
    public void Configure(EntityTypeBuilder<UserApp> builder)
    {
        builder.Property(x => x.FirstName).HasMaxLength(32).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Address);
        builder.Property(x => x.IpAddress).HasMaxLength(16).IsRequired();
        builder.Property(x => x.IsDeleted).IsRequired();
    }
}