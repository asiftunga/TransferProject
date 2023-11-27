using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniApp1Api.Data.Entities;

namespace MiniApp1Api.Data.Mappings;

public class RestourantMap : IEntityTypeConfiguration<Restourant>
{
    public void Configure(EntityTypeBuilder<Restourant> builder)
    {

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.City).HasMaxLength(32).IsRequired();
        builder.Property(x => x.City).HasMaxLength(32).IsRequired();
        builder.Property(x => x.District).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Neighborhood).HasMaxLength(32).IsRequired();
        builder.Property(x => x.DeliveryService).IsRequired();
        builder.Property(x => x.PassportOrTaxNumber).HasMaxLength(64).IsRequired();
        builder.Property(x => x.RestaurantName).HasMaxLength(64).IsRequired();
        builder.Property(x => x.CuisineType).HasMaxLength(32).IsRequired();
        builder.Property(x => x.ReferenceCode).HasMaxLength(32).IsRequired();
        builder.Property(x => x.IsDeleted).IsRequired();
    }
}