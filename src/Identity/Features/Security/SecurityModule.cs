using Identity.Contracts;
using Identity.Domain.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Identity.Features.Security;

public class SecurityModule : IModule
{
    private readonly IConfiguration _configuration;
     private readonly ILogger<SecurityModule> _logger;

    public SecurityModule(IConfiguration configuration, ILogger<SecurityModule> logger)
    {
        _configuration = configuration;
        _logger = logger;
       
    }

    public IEndpointRouteBuilder RegisterEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/security/register",
        [AllowAnonymous]
        async (UserManager<IdentityUser> userMgr, User user) =>
        {
            var identityUser = new IdentityUser()
            {
                UserName = user.UserName,
                Email = user.UserName + "@example.com"
            };

            var result = await userMgr.CreateAsync(identityUser, user.PasswordHash);

            if (result.Succeeded)
            {
                return result;
            }
            else
            {
                return result;
            }
        });

        endpoints.MapPost("/api/security/getToken",
        [AllowAnonymous]
        async (UserManager<IdentityUser> userMgr, User user) =>
        {
            var identityUsr = await userMgr.FindByNameAsync(user.UserName);
        
            if (await userMgr.CheckPasswordAsync(identityUsr, user.PasswordHash))
            {
                var issuer = "Identity";
                var audience = "Gateway";
                var securityKey = new SymmetricSecurityKey
                (Encoding.UTF8.GetBytes("JWTAuthenticationHIGHsecuredPasswordVVVp1OH7Xzyr"));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(issuer: issuer,
                    audience: audience,
                    signingCredentials: credentials);
                var tokenHandler = new JwtSecurityTokenHandler();
                var stringToken = tokenHandler.WriteToken(token);
                return Results.Ok(stringToken);
            }
            else
            {
                var failedAttempt = int.TryParse(_configuration["MaxFailedAccessAttemptsBeforeLockout"], out int defaultMaxFailed);
                await userMgr.AccessFailedAsync(identityUsr);
                if (identityUsr.AccessFailedCount >= defaultMaxFailed)
                {
                    var lockUserTask = await userMgr.SetLockoutEnabledAsync(identityUsr, true);
                    var lockDateTask = await userMgr.SetLockoutEndDateAsync(identityUsr, DateTime.Now.AddDays(3));
                    return Results.Text("userName or password is wrong");
                }
                return Results.Unauthorized();
            }
        });

        endpoints.MapPost("/api/security/lockout",
        [AllowAnonymous]
        async (UserManager<IdentityUser> userMgr, User user, bool enabled) =>
        {
            var userTask = await userMgr.FindByNameAsync(user.UserName);
            var lockUserTask = await userMgr.SetLockoutEnabledAsync(userTask, enabled);
            var lockDateTask = await userMgr.SetLockoutEndDateAsync(userTask, DateTime.Now.AddDays(3));
            return lockDateTask.Succeeded && lockUserTask.Succeeded;
        });

        endpoints.MapGet("/api/security/getUser",
        [AllowAnonymous]
         (UserManager<IdentityUser> userMgr) =>
        {
             _logger.LogDebug(1, "NLog injected into HomeController");
            return "Mohsen";
        });
        
        return endpoints;
    }
}