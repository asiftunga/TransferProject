using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniApp1Api.Data.Entities;

namespace MiniApp1Api.Data.Mappings;

public class UserRefreshTokenConfiguration : IEntityTypeConfiguration<UserRefreshToken>
{
    public void Configure(EntityTypeBuilder<UserRefreshToken> builder)
    {
        builder.ToTable("UserRefreshTokens");

        builder.HasKey(x => x.UserId);

        builder.Property(x => x.Code).HasMaxLength(256).IsRequired();;

    }
}