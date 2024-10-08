using CarrotMessenger.Domain;
using CarrotMessenger.Infrastructure.Common.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CarrotMessenger.Identity;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IEnumerable<User>> GetByQueryAsync(string query)
    {
        return await _userManager
            .Users
            .Where(x => x.UserName!.Contains(query))
            .Select(x => new User(x.Id, x.UserName))
            .ToListAsync();
    }
}