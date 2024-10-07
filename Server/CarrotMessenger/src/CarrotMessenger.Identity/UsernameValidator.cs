using Microsoft.AspNetCore.Identity;

namespace CarrotMessenger.Identity;

public class UsernameValidator<TUser, TKey> : IUserValidator<TUser> where TUser : IdentityUser<TKey> where TKey : IEquatable<TKey>
{
    public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
    {
        if (string.IsNullOrWhiteSpace(user.UserName))
        {
            return IdentityResult.Failed(new IdentityError { Description = "Username is required" });
        }

        if (await manager.FindByNameAsync(user.UserName) != null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "Username already exists" });
        }

        return IdentityResult.Success;
    }
}