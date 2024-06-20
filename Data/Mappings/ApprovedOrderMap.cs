using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransferProject.Data.Entities;

namespace TransferProject.Data.Mappings;

public class ApprovedOrderMap : IEntityTypeConfiguration<ApprovedOrder>
{
    public void Configure(EntityTypeBuilder<ApprovedOrder> builder)
    {
        builder.ToTable("ApprovedOrders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderId).HasColumnType("uuid").IsRequired();
        builder.Property(x => x.UserId).HasColumnType("varchar(64)").IsRequired();
        builder.Property(x => x.IsRead).HasColumnType("boolean").IsRequired();
    }
}