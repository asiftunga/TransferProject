using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniApp1Api.Data.Entities;

namespace MiniApp1Api.Data.Mappings;

public class ApprovedOrderMap : IEntityTypeConfiguration<ApprovedOrders>
{
    public void Configure(EntityTypeBuilder<ApprovedOrders> builder)
    {
        builder.ToTable("TemporaryOrders");

        builder.HasKey(x => x.Description);
    }
}