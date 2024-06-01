using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MiniApp1Api.Data.Entities;
using MiniApp1Api.Data.Mappings;

namespace MiniApp1Api.Data;

public class TransferProjectDbContext : IdentityDbContext<UserApp, IdentityRole, string>
{
    public TransferProjectDbContext(DbContextOptions<TransferProjectDbContext> options) : base(options)
    {

    }

    public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }

    public DbSet<TemporaryOrder> TemporaryOrders { get; set; }

    public DbSet<Order> Orders { get; set; }

    public DbSet<ApprovedOrder> ApprovedOrders { get; set; }

    public DbSet<SingleCardDetail>  SingleCardDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        base.OnModelCreating(builder);
    }
}