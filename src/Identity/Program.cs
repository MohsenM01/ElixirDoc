using NLog;
using NLog.Web;
using Identity.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationServices(builder);

builder.Logging.ClearProviders();
builder.Host.UseNLog();

var app = builder.Build();
app.ConfigureApplication();
app.RegisterEndpoints();

app.MapGet("/", () => "Hello World!");

app.Run();
