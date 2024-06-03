using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniApp1Api.Data.Entities;

namespace MiniApp1Api.Data.Mappings;

public class OrderMap : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderId).HasColumnType("uuid").IsRequired();
        builder.Property(x => x.UserId).HasColumnType("varchar(64)").IsRequired(); // Veya "text"
        builder.Property(x => x.Amount).HasColumnType("integer").IsRequired();
        builder.Property(x => x.OrderTypes).HasColumnType("integer").IsRequired();
        builder.Property(x => x.Currency).HasColumnType("integer").IsRequired();
        builder.Property(x => x.Payment).HasColumnType("integer").IsRequired();
        builder.Property(x => x.PaymentArea).HasColumnType("integer");
        builder.Property(x => x.OrderStatus).HasColumnType("integer").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz").IsRequired(); // Veya "timestamptz"
        builder.Property(x => x.CreatedBy).HasColumnType("varchar(64)").IsRequired(); // Veya "text"
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz").IsRequired(); // Veya "timestamptz"
        builder.Property(x => x.UpdatedBy).HasColumnType("varchar(64)").IsRequired(); // Veya "text"
    }
}