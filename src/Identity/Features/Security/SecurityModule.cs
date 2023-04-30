using Identity.Contracts;
using Identity.Domain.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Identity.Features.Security;

public class SecurityModule : IModule
{
    public IEndpointRouteBuilder RegisterEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", () => "Hello World!");

        endpoints.MapPost("/api/security/createUser", async (AppIdentityDbContext ctx, User userModel) =>
        {
            var user = new User();
            user.UserName = userModel.UserName;
            
            ctx.Add(user);
            await ctx.SaveChangesAsync();

            return user;
        });

        //endpoints.MapPost("/minimalapi/security/createUser", 
        //[AllowAnonymous] 
        //async(UserManager<IdentityUser> userMgr, User user) =>
        //{
        //    var identityUser = new IdentityUser() {
        //        UserName = user.UserName,
        //        Email = user.UserName + "@example.com"
        //    };
//
        //    var result = await userMgr.CreateAsync(identityUser, user.PasswordHash);
//
        //    if(result.Succeeded)
        //    {
        //        return Results.Ok();
        //    }
        //    else
        //    {
        //        return Results.BadRequest();
        //    }
        //});

        return endpoints;
    }
}