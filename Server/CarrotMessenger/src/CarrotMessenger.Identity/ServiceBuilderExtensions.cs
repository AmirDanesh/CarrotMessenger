using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CarrotMessenger.Infrastructure.Common.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CarrotMessenger.Identity;
public static class ServiceBuilderExtensions
{
    public static void AddApplicationDbConnection(this WebApplicationBuilder builder, string migrationAssemblyName)
    {
        builder.Services.AddDbContext<CarrotMessengerIdentityDbContext>(options => options.UseInMemoryDatabase("CarrotMessengerDb"));
    }

    public static void AddApplicationIdentity(this WebApplicationBuilder builder)
    {
        var identitySettings = builder.Configuration.GetSection("identity") ?? throw new ArgumentNullException("identitySettings");
        builder.Services.Configure<IdentitySettings>(identitySettings.Bind);
        var identityOptions = identitySettings.Get<IdentitySettings>() ?? throw new ArgumentNullException(nameof(identitySettings));

        builder.Services.AddAuthorization();

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = identityOptions.Save;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(identityOptions.Secret)),
                    ValidIssuer = identityOptions.Issuer,
                    ValidAudiences = identityOptions.Audiences,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true
                };
            });

        builder.Services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = false;
        })
            .AddRoles<IdentityRole<long>>()
            .AddEntityFrameworkStores<CarrotMessengerIdentityDbContext>()
            .AddUserValidator<UsernameValidator<ApplicationUser, long>>()
            .AddDefaultTokenProviders()
            .AddApiEndpoints();
    }

    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, CarrotMessengerIdentityDbContext>();
    }

    public static WebApplication UseApplicationIdentity(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapIdentityRoutes();
        return app;
    }

    public static void MapIdentityRoutes(this WebApplication app)
    {
        app.MapPost("/identity/signin", async ([FromBody] SignInRequest signInRequest, IHttpContextAccessor httpContextAccessor, IOptions<IdentitySettings> identityOptions, SignInManager<ApplicationUser> signInManager) =>
        {
            var user = await signInManager.UserManager.FindByNameAsync(signInRequest.Username);
            if (user == null)
                return Results.BadRequest("Invalid email or password.");

            var saltedPassword = signInRequest.Password + user.PasswordSalt;
            var result = await signInManager.CheckPasswordSignInAsync(user, saltedPassword, true);
            if (result.IsLockedOut)
                return Results.BadRequest("Invalid email or password.");
            if (!result.Succeeded)
                return Results.BadRequest("Invalid email or password.");

            var roles = await signInManager.UserManager.GetRolesAsync(user);
            var request = httpContextAccessor.HttpContext?.Request ?? throw new ArgumentNullException(nameof(IHttpContextAccessor.HttpContext));
            var options = identityOptions.Value;
            if (request.Headers.TryGetValue("Referer", out var referrer))
                referrer = new Uri(referrer!).Authority;
            options.Audiences = [referrer!];
            options.Issuer = request.Host.Value;
            return Results.Ok(CreateToken(options, user, roles));
        })
        .WithName("SignUserIn");

        app.MapPost("/identity/signup", async ([FromBody] SignUpRequest request, UserManager<ApplicationUser> userManager) =>
        {
            var salt = Guid.NewGuid().ToString();
            var saltedPassword = request.Password + salt;

            var user = new ApplicationUser
            {
                Email = request.Email,
                PasswordSalt = salt,
                UserName = request.Username
            };

            var result = await userManager.CreateAsync(user, saltedPassword);
            if (result.Succeeded)
                return Results.Created(string.Empty, result);
            else
                return Results.BadRequest(result.Errors);
        })
        .WithName("SignUserUp");
    }

    private static string CreateToken(IdentitySettings identityOptions, ApplicationUser user, IEnumerable<string> userRoles)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(identityOptions.Secret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.UserName ?? user.Email!),
                new Claim(ClaimTypes.Role, string.Join(',', userRoles))
            ]),
            Expires = DateTime.Now.Add(identityOptions.ExpiresIn),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature),
            Issuer = identityOptions.Issuer,
            Audience = identityOptions.Audiences[0]
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
