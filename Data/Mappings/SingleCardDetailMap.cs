using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniApp1Api.Data.Entities;

namespace MiniApp1Api.Data.Mappings;

public class SingleCardDetailMap : IEntityTypeConfiguration<SingleCardDetail>
{
    public void Configure(EntityTypeBuilder<SingleCardDetail> builder)
    {
        builder.ToTable("SingleCardDetails");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderId).HasColumnType("uuid").IsRequired();
        builder.Property(x => x.UserId).HasColumnType("varchar(64)").IsRequired(); // Veya "text"
        builder.Property(x => x.Amount).HasColumnType("integer").IsRequired();
        builder.Property(x => x.OrderTypes).HasColumnType("integer").IsRequired();
        builder.Property(x => x.Currency).HasColumnType("integer").IsRequired();
        builder.Property(x => x.Payment).HasColumnType("integer").IsRequired();
        builder.Property(x => x.OrderStatus).HasColumnType("integer").IsRequired();
        builder.Property(x => x.CardName).HasColumnType("varchar(128)");
        builder.Property(x => x.CardNumber).HasColumnType("varchar(64)");
        builder.Property(x => x.CardDate).HasColumnType("timestamp");
        builder.Property(x => x.CVV).HasColumnType("varchar(4)");
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp").IsRequired();
        builder.Property(x => x.CreatedBy).HasColumnType("varchar(64)").IsRequired(); // Veya "text"
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamp").IsRequired();
        builder.Property(x => x.UpdatedBy).HasColumnType("varchar(64)").IsRequired();
    }
}