using Identity.Contracts;
using Identity.Domain.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Nest;
using Elasticsearch.Net;

namespace Identity.Features.Security;

public class SecurityModule : IModule
{
    private readonly IConfiguration _configuration;
    private readonly IElasticClient _elasticClient;
    private readonly ILogger<SecurityModule> _logger;

    public SecurityModule(IConfiguration configuration,
     ILogger<SecurityModule> logger, IElasticClient elasticClient)
    {
        _configuration = configuration;
        _logger = logger;
        _elasticClient = elasticClient;

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
                try
                {
                    var indexResponse = await _elasticClient.IndexDocumentAsync(user.Id);
                    if (indexResponse.IsValid == false)
                    {
                        _logger.LogError($"Failed to index document: {indexResponse.DebugInformation}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error indexing document: {ex.Message}");
                }
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
        async (UserManager<IdentityUser> userMgr) =>
        {

            var pool = new SingleNodeConnectionPool(new Uri("https://localhost:9200"));

            var connectionSettings = new ConnectionSettings(pool)
                .BasicAuthentication("elastic", "changeme");

            var client = new ElasticClient(connectionSettings);

            var settings = new IndexSettings();
            settings.NumberOfReplicas = 1;
            settings.NumberOfShards = 5;
            //settings.Settings.Add("merge.policy.merge_factor", "10");
            //settings.Settings.Add("search.slowlog.threshold.fetch.warn", "1s");

            var tweet = new Tweet
            {
                Id = 1,
                User = "stevejgordon",
                PostDate = new DateTime(2009, 11, 15),
                Message = "Trying out the client, so far so good?"
            };

            var response = await client.IndexDocumentAsync(tweet);

            if (response.IsValid)
            {
                Console.WriteLine($"Index document with ID {response.Id} succeeded.");
            }

            _logger.LogDebug(1, "NLog injected into HomeController");
            _logger.LogDebug("Debug message");
            _logger.LogTrace("Trace message");
            _logger.LogError("Error message");
            _logger.LogWarning("Warning message");
            _logger.LogCritical("Critical message");
            _logger.LogInformation("Information message");
            return "Mohsen";
        });

        return endpoints;
    }

    public class Tweet
    {
        public int Id { get; set; }
        public string User { get; set; }
        public DateTime PostDate { get; set; }
        public string Message { get; set; }
    }
}