using MediatR;
using Identity.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Identity.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        WebApplicationBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var connectionString = builder.Configuration.GetConnectionString("AppIdentityDbContext");
        builder.Services.AddDbContext<AppIdentityDbContext>(o => o.UseSqlServer(connectionString));

        builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
           .AddEntityFrameworkStores<AppIdentityDbContext>();

        builder.Services.AddMediatR(typeof(Program));
        builder.Services.AddAutoMapper(typeof(Program));
        builder.Services.AddAllModules(typeof(Program));

        return services;
    }

    private static void AddAllModules(this IServiceCollection services, params Type[] types)
    {
        // Using the `Scrutor` NuGet Package to add all of the application's modules at once.
        services.Scan(scan =>
            scan.FromAssembliesOf(types)
                .AddClasses(classes => classes.AssignableTo<IModule>())
                .AsImplementedInterfaces()
                .WithSingletonLifetime());
    }
}