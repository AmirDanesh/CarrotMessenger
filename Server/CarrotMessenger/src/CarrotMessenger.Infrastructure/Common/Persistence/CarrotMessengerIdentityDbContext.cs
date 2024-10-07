using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarrotMessenger.Infrastructure.Common.Persistence;

public class CarrotMessengerIdentityDbContext(DbContextOptions<CarrotMessengerIdentityDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<long>, long>(options), IUnitOfWork
{
    async Task IUnitOfWork.CommitChangesAsync()
    {
        await SaveChangesAsync();
    }
}

public class ApplicationUser : IdentityUser<long>
{
    public string? PasswordSalt { get; set; }
}