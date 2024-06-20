using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TransferProject.Data.Entities;

namespace TransferProject.Data.Identity;

public class CustomUserManager<TUser> : UserManager<TUser> where TUser : IdentityUser
{
    private readonly TransferProjectDbContext _transferProjectDbContext;

    public async Task<UserApp?> FindByEmailAsync(string email, bool includeDeleted = false)
    {
        IQueryable<UserApp> query = _transferProjectDbContext.Users.Where(u => u.Email == email);
        if (!includeDeleted)
        {
            query = query.Where(u => !u.IsDeleted);
        }
        return await query.FirstOrDefaultAsync();
    }

    public CustomUserManager(IUserStore<TUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<TUser> passwordHasher, IEnumerable<IUserValidator<TUser>> userValidators, IEnumerable<IPasswordValidator<TUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<TUser>> logger, TransferProjectDbContext transferProjectDbContext) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
        _transferProjectDbContext = transferProjectDbContext;
    }
}