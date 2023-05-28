using System.Text;
using Identity.Contracts;
using Identity.Domain.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Elastic.Transport;
using Elastic.Clients.Elasticsearch;

namespace Identity.Features.Security;

public class SecurityModule : IModule
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SecurityModule> _logger;
    private readonly ElasticsearchClient _elasticSearchClient;

    public SecurityModule(IConfiguration configuration,
     ILogger<SecurityModule> logger,
     ElasticsearchClient elasticSearchClient)
    {
        _configuration = configuration;
        _logger = logger;
        _elasticSearchClient = elasticSearchClient;
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
                _logger.LogError($"Error indexing document: {result.Errors}");
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
        async (UserManager<IdentityUser> userMgr) =>
        {
            var identityUser = new IdentityUser
            {
                Id = "664dc1c9-9b97-4fe3-97b1-98f2cafebfe7",
                UserName = "Farzad"
            };
            var response = await _elasticSearchClient.IndexAsync(identityUser, "identity-user-index");
            if (response.IsValidResponse)
            {
                return ($"Index document with ID {response.Id} succeeded.");
            }

            return "Mohsen";
        });

        return endpoints;
    }
}