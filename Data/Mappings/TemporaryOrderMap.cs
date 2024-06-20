using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransferProject.Data.Entities;

namespace TransferProject.Data.Mappings;

public class TemporaryOrderMap : IEntityTypeConfiguration<TemporaryOrder>
{
    public void Configure(EntityTypeBuilder<TemporaryOrder> builder)
    {
        builder.ToTable("TemporaryOrders");

        builder.HasKey(x => x.OrderId);

        builder.Property(x => x.OrderType).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.OrderId).IsRequired();
    }
}