using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MiniApp1Api.Data.Entities;

namespace MiniApp1Api.Data;

public class TMMealDbContext : IdentityDbContext<UserApp, IdentityRole, string>
{
    public TMMealDbContext(DbContextOptions<TMMealDbContext> options) : base(options)
    {

    }

    public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
    public DbSet<Restourant> Restourants { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        base.OnModelCreating(builder);
    }
}