using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniApp1Api.Data.Entities;

namespace MiniApp1Api.Data.Mappings;

public class ApprovedOrderMap : IEntityTypeConfiguration<ApprovedOrders>
{
    public void Configure(EntityTypeBuilder<ApprovedOrders> builder)
    {
        builder.ToTable("ApprovedOrders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderId).HasColumnType("uuid").IsRequired();
        builder.Property(x => x.UserId).HasColumnType("varchar(64)").IsRequired();
        builder.Property(x => x.IsRead).HasColumnType("boolean").IsRequired();
    }
}