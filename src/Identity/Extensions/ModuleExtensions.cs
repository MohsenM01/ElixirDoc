using Identity.Contracts;

namespace Identity.Extensions;

public static class ModuleExtensions
{
    public static WebApplication RegisterEndpoints(this WebApplication app)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        var modules = app.Services.GetServices<IModule>();
        foreach (var module in modules)
        {
            module.RegisterEndpoints(app);
        }

        return app;
    }
}